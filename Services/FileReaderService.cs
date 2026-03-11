using FileCopyHS.Interfaces;
using FileCopyHS.Models;
using System.Security.Cryptography;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace FileCopyHS.Services
{
    public class FileReaderService : IFileReaderService
    {
        private readonly IHashService _hashService;
        private readonly ILogger<FileReaderService> _logger;

        public FileReaderService(IHashService hashService, ILogger<FileReaderService> logger)
        {
            _hashService = hashService;
            _logger = logger;
        }

        public async Task ReadFile(string sourceFile, ChannelWriter<Chunk> writer, IncrementalHash sourceHashInstance, CancellationToken ct)
        {
            try
            {
                await using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                if (sourceStream.Length == 0)
                {
                    throw new InvalidDataException("The source file is empty.");
                }

                var buffer = new byte[1024 * 1024];
                var chunkCounter = 0;
                long totalBytesRead = 0;
                int currentBytesRead;

                while ((currentBytesRead = await sourceStream.ReadAsync(buffer, ct)) > 0)
                {
                    totalBytesRead += currentBytesRead;
                    var chunkData = buffer[..currentBytesRead].ToArray();
                    sourceHashInstance.AppendData(chunkData);

                    var chunk = new Chunk
                    {
                        Number = chunkCounter++,
                        Position = totalBytesRead - currentBytesRead,
                        Data = chunkData,
                        HashedData = _hashService.ComputeMd5Hash(chunkData),
                        Size = currentBytesRead
                    };

                    await writer.WriteAsync(chunk, ct);
                }

                writer.Complete();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while reading the source file: {SourceFile}.", sourceFile);
                writer.Complete(ex);
                throw;
            }
        }
    }
}