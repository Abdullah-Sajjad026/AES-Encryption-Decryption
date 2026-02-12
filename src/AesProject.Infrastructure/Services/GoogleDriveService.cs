using AesProject.Core.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace AesProject.Infrastructure.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly GoogleAuthorizationCodeFlow _flow;

        public GoogleDriveService(IConfiguration configuration)
        {
            var googleConfig = configuration.GetSection("Google");
            var clientId = googleConfig["ClientId"];
            var clientSecret = googleConfig["ClientSecret"];
            var credentialsPath = googleConfig["CredentialsPath"] ?? "token_store";

            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            _flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new FileDataStore(credentialsPath, true)
            });
        }

        public string GetAuthorizationUrl(string redirectUri)
        {
            return _flow.CreateAuthorizationCodeRequest(redirectUri).Build().ToString();
        }

        public async Task ExchangeCodeForTokenAsync(string code, string userId, string redirectUri)
        {
            var token = await _flow.ExchangeCodeForTokenAsync(userId, code, redirectUri, CancellationToken.None);
            // Token is stored automatically by flow via DataStore.
        }

        public bool IsDriveAuthorized(string userId)
        {
            var token = _flow.DataStore.GetAsync<TokenResponse>(userId).Result;
            return token != null;
        }

        public async Task UploadFileAsync(Stream fileStream, string fileName, string contentType, string userId, List<string>? shareWith = null)
        {
            var token = await _flow.DataStore.GetAsync<TokenResponse>(userId);
            if (token == null) throw new InvalidOperationException("User not authorized for Google Drive.");

            var credential = new UserCredential(_flow, userId, token);

            // Refresh token is handled automatically by the library when making requests.

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "AES Project"
            });

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName
            };

            var request = service.Files.Create(fileMetadata, fileStream, contentType);
            request.Fields = "id, webViewLink";

            var progress = await request.UploadAsync();

            if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
            {
                throw new Exception("Upload failed: " + progress.Exception.Message);
            }

            var fileId = request.ResponseBody?.Id;
            if (string.IsNullOrEmpty(fileId)) return; // Should not happen on success

            if (shareWith != null && shareWith.Any())
            {
                var batch = new BatchRequest(service);
                foreach (var email in shareWith)
                {
                    var permission = new Permission
                    {
                        Type = "user",
                        Role = "writer",
                        EmailAddress = email
                    };
                    var permissionRequest = service.Permissions.Create(permission, fileId);
                    batch.Queue<Permission>(permissionRequest, (content, error, index, message) =>
                    {
                        // Log error?
                    });
                }
                await batch.ExecuteAsync();
            }
        }
    }
}
