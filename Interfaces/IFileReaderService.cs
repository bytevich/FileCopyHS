using FileCopyHS.Models;

namespace FileCopyHS.Interfaces
{
    public interface IFileReaderService
    {
        Task<Chunk> ReadFile(string sourceFile);
    }
}