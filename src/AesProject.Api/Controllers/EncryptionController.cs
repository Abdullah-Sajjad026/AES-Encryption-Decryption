using AesProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AesProject.Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class EncryptionController : ControllerBase
    {
        private readonly IAesEncryptionService _encryptionService;
        private readonly IGoogleDriveService _driveService;

        public EncryptionController(IAesEncryptionService encryptionService, IGoogleDriveService driveService)
        {
            _encryptionService = encryptionService;
            _driveService = driveService;
        }

        [Authorize]
        [HttpPost("encrypt")]
        public async Task<IActionResult> Encrypt(IFormFile file, [FromQuery] int keySize, [FromQuery] bool uploadToDrive, [FromQuery] List<string>? shareToList)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            var userId = User.Identity?.Name;
            if (userId == null) return Unauthorized();

            // OpenReadStream returns a stream over the uploaded file (buffered or disk).
            using var inputStream = file.OpenReadStream();

            var (zipStream, zipFileName) = await _encryptionService.EncryptAsync(inputStream, file.FileName, keySize, uploadToDrive);

            if (uploadToDrive)
            {
                try
                {
                    // Ensure stream is at beginning
                    if (zipStream.Position != 0) zipStream.Position = 0;

                    // Upload
                    await _driveService.UploadFileAsync(zipStream, zipFileName, "application/zip", userId, shareToList);
                }
                catch (Exception ex)
                {
                    // If upload fails, should we fail the request or just return the file with a warning?
                    // Java code didn't handle exception explicitly in controller (SneakyThrows/Global handler).
                    // I'll return BadRequest with error message to be safe.
                    // But first dispose zipStream to delete the file.
                    await zipStream.DisposeAsync();
                    return BadRequest("Failed to upload to Drive: " + ex.Message);
                }
            }

            // Reset position for download
            if (zipStream.Position != 0) zipStream.Position = 0;

            return File(zipStream, "application/zip", zipFileName);
        }

        [HttpPost("decrypt")]
        public async Task<IActionResult> Decrypt(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            using var inputStream = file.OpenReadStream();

            try
            {
                var result = await _encryptionService.DecryptAsync(inputStream);
                return File(result.FileStream, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Decryption failed: " + ex.Message);
            }
        }
    }
}
