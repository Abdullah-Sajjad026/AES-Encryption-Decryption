using AesProject.Core.Entities;
using AesProject.Core.Interfaces;
using AesProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AesProject.Infrastructure.Services
{
    public class AesEncryptionService : IAesEncryptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private const int TagLengthBytes = 16; // 128 bits

        public AesEncryptionService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<(Stream FileStream, string FileName)> EncryptAsync(Stream fileStream, string fileName, int keySize, bool uploadToDrive)
        {
            if (keySize != 128 && keySize != 192 && keySize != 256)
                throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

            byte[] key = new byte[keySize / 8];
            RandomNumberGenerator.Fill(key);

            byte[] iv = new byte[12];
            RandomNumberGenerator.Fill(iv);

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var cipherText = new byte[fileBytes.Length];
            var tag = new byte[TagLengthBytes];

            using (var aes = new AesGcm(key, TagLengthBytes))
            {
                aes.Encrypt(iv, fileBytes, cipherText, tag);
            }

            // Save Meta Info
            var metaInfo = new AesEncryptionMetaInfo
            {
                SecretKey = key,
                Iv = iv,
                TagLength = TagLengthBytes * 8
            };
            _context.AesEncryptionMetaInfos.Add(metaInfo);
            await _context.SaveChangesAsync();

            // Prepare files for zipping
            var tempDir = Path.GetTempPath();
            var encryptedContent = new byte[cipherText.Length + tag.Length];
            Buffer.BlockCopy(cipherText, 0, encryptedContent, 0, cipherText.Length);
            Buffer.BlockCopy(tag, 0, encryptedContent, cipherText.Length, tag.Length);

            var uniqueEncryptedFileName = await _fileService.CreateUniqueFileNameAsync("Encrypted-" + fileName);
            var encryptedFilePath = Path.Combine(tempDir, uniqueEncryptedFileName);
            await File.WriteAllBytesAsync(encryptedFilePath, encryptedContent);

            var metaFilePath = Path.Combine(tempDir, "meta.id"); // Standard name for simplicity in new system
            // Encode ID in Base64 as per legacy, or just string? Legacy used Base64 of ID.
            var idBytes = Encoding.UTF8.GetBytes(metaInfo.Id);
            var idBase64 = Convert.ToBase64String(idBytes);
            await File.WriteAllTextAsync(metaFilePath, idBase64);

            var filesToZip = new List<string> { encryptedFilePath, metaFilePath };
            var zipPath = await _fileService.ZipFilesAsync(filesToZip, fileName); // Zip name based on original file

            // Clean up temp files (except zip which is returned)
            File.Delete(encryptedFilePath);
            File.Delete(metaFilePath);

            // We return a FileStream of the zip. The controller should dispose it, which closes the file.
            // But we need to delete the zip file after it's sent.
            // FileStream with DeleteOnClose is useful here.
            var zipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            return (zipStream, Path.GetFileName(zipPath));
        }

        public async Task<DecryptedFileResult> DecryptAsync(Stream fileStream)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var extractedFiles = await _fileService.UnzipFileAsync(fileStream, tempDir);

                var metaFilePath = extractedFiles.FirstOrDefault(f => Path.GetFileName(f) == "meta.id");
                if (metaFilePath == null)
                {
                    // Fallback for legacy compatibility?
                    // Legacy zip has "EncryptionMetaInfo(DO_NOT_DELETE)-....txt".
                    metaFilePath = extractedFiles.FirstOrDefault(f => Path.GetFileName(f).StartsWith("EncryptionMetaInfo") && Path.GetFileName(f).EndsWith(".txt"));
                }

                if (metaFilePath == null)
                    throw new InvalidOperationException("Meta info file not found in the uploaded zip.");

                var encryptedFilePath = extractedFiles.FirstOrDefault(f => f != metaFilePath);
                if (encryptedFilePath == null)
                    throw new InvalidOperationException("Encrypted file not found.");

                var idBase64 = await File.ReadAllTextAsync(metaFilePath);
                // Legacy used Base64 of ID.
                var id = Encoding.UTF8.GetString(Convert.FromBase64String(idBase64));

                var metaInfo = await _context.AesEncryptionMetaInfos.FindAsync(id);
                if (metaInfo == null)
                    throw new KeyNotFoundException("Encryption meta info not found for the given ID.");

                var encryptedBytesWithTag = await File.ReadAllBytesAsync(encryptedFilePath);

                // Extract tag
                var tag = new byte[TagLengthBytes];
                var cipherTextLength = encryptedBytesWithTag.Length - TagLengthBytes;
                var cipherText = new byte[cipherTextLength];

                Buffer.BlockCopy(encryptedBytesWithTag, cipherTextLength, tag, 0, TagLengthBytes);
                Buffer.BlockCopy(encryptedBytesWithTag, 0, cipherText, 0, cipherTextLength);

                var plainText = new byte[cipherTextLength];

                using (var aes = new AesGcm(metaInfo.SecretKey, TagLengthBytes))
                {
                    aes.Decrypt(metaInfo.Iv, cipherText, tag, plainText);
                }

                // Determine file name (remove "Encrypted-" prefix if present)
                var originalFileName = Path.GetFileName(encryptedFilePath);
                if (originalFileName.StartsWith("Encrypted-"))
                    originalFileName = originalFileName.Substring("Encrypted-".Length);

                // Remove unique random string suffix if added by CreateUniqueFileName
                // format: Name-Random.ext
                // This is hard to reverse perfectly without knowing format.
                // But typically we can just return it.
                // Or we can try to find the last dash and check length.
                // Let's just return the decrypted file name as is, maybe replacing "Encrypted" with "Decrypted".
                var decryptedFileName = "Decrypted-" + originalFileName;

                var decryptedStream = new MemoryStream(plainText);

                // Probe content type? .NET doesn't have built-in probe.
                var contentType = "application/octet-stream"; // Default

                return new DecryptedFileResult(decryptedStream, contentType, decryptedFileName);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
