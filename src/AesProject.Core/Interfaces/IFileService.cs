namespace AesProject.Core.Interfaces
{
    public interface IFileService
    {
        Task<string> ZipFilesAsync(IEnumerable<string> filePaths, string zipFileName);
        Task<List<string>> UnzipFileAsync(Stream zipStream, string destinationDirectory);
        Task<string> CreateUniqueFileNameAsync(string originalFileName);
    }
}
