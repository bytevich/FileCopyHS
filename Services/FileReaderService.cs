using FileCopyHS.Interfaces;
using FileCopyHS.Models;

namespace FileCopyHS.Services
{
    public class FileReaderService : IFileReaderService
    {
        private readonly IHashService _hashService;

        public FileReaderService(IHashService hashService)
        {
            _hashService = hashService;
        }

        public async Task<Chunk> ReadFile(string sourceFile)
        {
            if (string.IsNullOrEmpty(sourceFile))
            {
                Console.WriteLine("Source file path is null or empty while attempting to read the file.");
                Environment.Exit(1);
            }

            var chunk = new Chunk();

            //try
            //{
            //    await using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            //    if (sourceStream.Length == 0)
            //    {
            //        Console.WriteLine("The source file is empty.");
            //        Environment.Exit(1);
            //    }

            //    var buffer = new byte[1024 * 1024];
            //    var chunkCounter = 0;
            //    long totalBytesRead = 0;
            //    int currentBytesRead;

            //    while ((currentBytesRead = await sourceStream.ReadAsync(buffer)) > 0)
            //    {
            //        totalBytesRead += currentBytesRead;
            //        var chunkData = buffer[..currentBytesRead].ToArray();

            //        chunk = new Chunk
            //        {
            //            Number = chunkCounter++,
            //            Position = totalBytesRead - currentBytesRead,
            //            Data = chunkData,
            //            HashedData = _hashService.ComputeMd5Hash(chunkData)
            //        };
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    Environment.Exit(1);
            //}
            
            return chunk;
        }
    }
}