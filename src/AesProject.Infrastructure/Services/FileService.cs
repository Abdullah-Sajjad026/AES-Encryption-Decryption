using System.IO.Compression;
using AesProject.Core.Interfaces;

namespace AesProject.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _tempPath = Path.GetTempPath();

        public async Task<string> ZipFilesAsync(IEnumerable<string> filePaths, string zipFileName)
        {
            var uniqueZipName = await CreateUniqueFileNameAsync(zipFileName);
            var zipPath = Path.Combine(_tempPath, uniqueZipName);

            using (var zipToOpen = new FileStream(zipPath, FileMode.Create))
            using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (var filePath in filePaths)
                {
                    var entryName = Path.GetFileName(filePath);
                    archive.CreateEntryFromFile(filePath, entryName);
                }
            }

            return zipPath;
        }

        public async Task<List<string>> UnzipFileAsync(Stream zipStream, string destinationDirectory)
        {
            var extractedFiles = new List<string>();

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    // Prevent Zip Slip
                    var destinationPath = Path.GetFullPath(Path.Combine(destinationDirectory, entry.FullName));
                    if (!destinationPath.StartsWith(Path.GetFullPath(destinationDirectory), StringComparison.Ordinal))
                    {
                        throw new IOException("Zip Slip vulnerability detected");
                    }

                    if (string.IsNullOrEmpty(entry.Name)) // Directory
                    {
                        Directory.CreateDirectory(destinationPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                        entry.ExtractToFile(destinationPath, overwrite: true);
                        extractedFiles.Add(destinationPath);
                    }
                }
            }

            return await Task.FromResult(extractedFiles);
        }

        public Task<string> CreateUniqueFileNameAsync(string originalFileName)
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var ext = Path.GetExtension(originalFileName);
            var randomString = Guid.NewGuid().ToString("N").Substring(0, 9);
            return Task.FromResult($"{fileNameWithoutExt}-{randomString}{ext}");
        }
    }
}
