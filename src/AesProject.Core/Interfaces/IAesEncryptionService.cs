namespace AesProject.Core.Interfaces
{
    public record DecryptedFileResult(Stream FileStream, string ContentType, string FileName);

    public interface IAesEncryptionService
    {
        Task<(Stream FileStream, string FileName)> EncryptAsync(Stream fileStream, string fileName, int keySize, bool uploadToDrive);
        Task<DecryptedFileResult> DecryptAsync(Stream fileStream);
    }
}
