using AesProject.Core.Entities;
using AesProject.Core.Interfaces;
using AesProject.Infrastructure.Data;
using AesProject.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text;
using Xunit;

namespace AesProject.Tests
{
    public class AesEncryptionServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly AesEncryptionService _service;
        private readonly string _tempDir;

        public AesEncryptionServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _fileService = new FileService();
            _service = new AesEncryptionService(_context, _fileService);

            _tempDir = Path.GetTempPath(); // We rely on system temp path same as service
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task EncryptAndDecrypt_ShouldReturnOriginalContent()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "Hello World This Is AES GCM";
            var contentBytes = Encoding.UTF8.GetBytes(content);
            using var fileStream = new MemoryStream(contentBytes);

            // Act - Encrypt
            var (zipStream, zipFileName) = await _service.EncryptAsync(fileStream, fileName, 128, false);

            // Assert Encrypt
            Assert.NotNull(zipStream);
            Assert.True(zipStream.Length > 0);

            // Verify DB has meta info
            var count = await _context.AesEncryptionMetaInfos.CountAsync();
            Assert.Equal(1, count);

            // Act - Decrypt
            // Reset zipStream position to beginning before reading
            zipStream.Position = 0;
            var decryptedResult = await _service.DecryptAsync(zipStream);

            // Assert Decrypt
            Assert.NotNull(decryptedResult);
            using var reader = new StreamReader(decryptedResult.FileStream);
            var decryptedContent = await reader.ReadToEndAsync();

            Assert.Equal(content, decryptedContent);
            Assert.StartsWith("Decrypted-", decryptedResult.FileName);
            Assert.Contains("test", decryptedResult.FileName);
            Assert.EndsWith(".txt", decryptedResult.FileName);
        }
    }
}
