namespace AesProject.Core.Interfaces
{
    public interface IGoogleDriveService
    {
        string GetAuthorizationUrl(string redirectUri);
        Task ExchangeCodeForTokenAsync(string code, string userId, string redirectUri);
        Task UploadFileAsync(Stream fileStream, string fileName, string contentType, string userId, List<string>? shareWith = null);
        bool IsDriveAuthorized(string userId);
    }
}
