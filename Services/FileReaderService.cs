using FileCopyHS.Interfaces;

namespace FileCopyHS.Services
{
    public class FileReaderService : IFileReaderService
    {
        public FileReaderService()
        {

        }

        public Task ReadFile(string sourceFile, string destinationFile)
        {
            throw new NotImplementedException();
        }
    }
}