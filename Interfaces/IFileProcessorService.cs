namespace FileCopyHS.Interfaces
{
    public interface IFileProcessorService
    {
        Task CopyFile(string sourceFile, string destinationFile);
    }
}