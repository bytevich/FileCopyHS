using System.Security.Cryptography;
using System.Threading.Channels;
using FileCopyHS.Interfaces;
using FileCopyHS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileCopyHS.Services
{
    public class FileProcessorService : IFileProcessorService
    {
        private readonly IFileReaderService _fileReaderService;
        private readonly IFileWriterService _fileWriterService;
        private readonly IHashService _hashService;
        private readonly ILogger<FileProcessorService> _logger;
        private readonly IConfiguration _configuration;

        public FileProcessorService(IFileReaderService fileReaderService, IFileWriterService fileWriterService, 
            IHashService hashService, ILogger<FileProcessorService> logger, IConfiguration configuration)
        {
            _fileReaderService = fileReaderService;
            _fileWriterService = fileWriterService;
            _hashService = hashService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task CopyFile(string sourceFile, string destinationFile, CancellationToken ct)
        {
            var channel = Channel.CreateBounded<Chunk>(capacity: _configuration.GetValue<int>("ChannelCapacity"));
            using var sourceHashInstance = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            var producer = _fileReaderService.ReadFile(sourceFile, channel.Writer, sourceHashInstance, ct);
            var consumer = _fileWriterService.WriteFile(destinationFile, channel.Reader, ct);

            await Task.WhenAll(producer, consumer);

            var result = await _hashService.ComputeAndCompare(sourceHashInstance, destinationFile, ct);
            var negation = !result.Item1 ? " not" : string.Empty;

            _logger.LogInformation("Source file and destination file are{Negation} the same. Source file hash: {ResultItem2}, Destination file hash: {ResultItem3}", 
                negation, result.Item2, result.Item3);
        }
    }
}