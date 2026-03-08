namespace FileCopyHS.Interfaces
{
    public interface IFileReaderService
    {
        Task ReadFile(string sourceFile, string destinationFile);
    }
}