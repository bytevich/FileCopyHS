using System.Security.Cryptography;
using System.Threading.Channels;
using FileCopyHS.Interfaces;
using FileCopyHS.Models;

namespace FileCopyHS.Services
{
    public class FileProcessorService : IFileProcessorService
    {
        private readonly IFileReaderService _fileReaderService;
        private readonly IFileWriterService _fileWriterService;
        private readonly IHashService _hashService;

        public FileProcessorService(IFileReaderService fileReaderService, IFileWriterService fileWriterService, IHashService hashService)
        {
            _fileReaderService = fileReaderService;
            _fileWriterService = fileWriterService;
            _hashService = hashService;
        }

        public async Task CopyFile(string sourceFile, string destinationFile)
        {
            var channel = Channel.CreateBounded<Chunk>(capacity: 5);
            using var sourceHashInstance = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            var producer = _fileReaderService.ReadFile(sourceFile, channel.Writer, sourceHashInstance);
            var consumer = _fileWriterService.WriteFile(destinationFile, channel.Reader);

            await Task.WhenAll(producer, consumer);

            var result = await _hashService.ComputeAndCompare(sourceHashInstance, destinationFile);

            if (!string.IsNullOrEmpty(result.Item2) && !string.IsNullOrEmpty(result.Item3))
            {
                var negation = !result.Item1 ? " not" : string.Empty;

                Console.WriteLine($"Source file and destination file are{negation} the same. " +
                                  $"Source file hash: {result.Item2}, Destination file hash: {result.Item3}");
            }

            else
            {
                Console.WriteLine("An error occured while checking the hashed values. Hash returned an empty string.");
            }
        }
    }
}