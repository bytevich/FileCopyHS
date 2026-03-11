using FileCopyHS.Interfaces;
using FileCopyHS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace FileCopyHS.Services
{
    public class FileWriterService : IFileWriterService
    {
        private readonly IHashService _hashService;
        private readonly ILogger<FileWriterService> _logger;
        private readonly IConfiguration _configuration;

        public FileWriterService(IHashService hashService, ILogger<FileWriterService> logger, IConfiguration configuration)
        {
            _hashService = hashService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task WriteFile(string destinationFile, ChannelReader<Chunk> reader, CancellationToken ct)
        {
            using var destinationFileHandle = File.OpenHandle(
                destinationFile,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                FileOptions.Asynchronous
            );

            var tasks = new ConcurrentBag<Task>();
            var semaphore = new SemaphoreSlim(_configuration.GetValue<int>("SemaphoreInitialCount"));
            await foreach (var chunk in reader.ReadAllAsync(ct))
            {
                await semaphore.WaitAsync(ct);

                if (chunk.Data == null || chunk.HashedData == null)
                {
                    semaphore.Release();
                    throw new InvalidDataException("Chunk data is null.");
                }

                Task task;
                try
                {
                    task = WriteChunk(destinationFileHandle, chunk, ct);
                }
                finally 
                {
                    semaphore.Release();
                }

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task WriteChunk(SafeFileHandle destinationFileHandle, Chunk chunk, CancellationToken ct)
        {
            await RandomAccess.WriteAsync(destinationFileHandle, chunk.Data, chunk.Position, ct);
            Console.WriteLine($"Chunk number: {chunk.Number}, position = {chunk.Position}, hash = {BitConverter.ToString(chunk.HashedData!)}");

            var buffer = new byte[chunk.Size];
            await RandomAccess.ReadAsync(destinationFileHandle, buffer, chunk.Position, ct);

            var destinationHash = _hashService.ComputeMd5Hash(buffer);
            if (!destinationHash.SequenceEqual(chunk.HashedData))
            {
                _logger.LogWarning("Chunk number {ChunkNumber} failed hash verification. Re-submitting the chunk.", 
                    chunk.Number);
                await VerifyChunk(destinationFileHandle, chunk, buffer, ct);
            }
        }

        private async Task VerifyChunk(SafeFileHandle destinationFileHandle, Chunk chunk, byte[] buffer, CancellationToken ct)
        {
            var maxRetryAttempts = _configuration.GetValue<int>("MaxRetryAttempts");
            byte[]? destinationRetryHash = null;

            for (var i = 0; i < maxRetryAttempts; i++)
            {
                await RandomAccess.WriteAsync(destinationFileHandle, chunk.Data, chunk.Position, ct);
                await RandomAccess.ReadAsync(destinationFileHandle, buffer, chunk.Position, ct);
                destinationRetryHash = _hashService.ComputeMd5Hash(buffer);

                if (destinationRetryHash.SequenceEqual(chunk.HashedData))
                    break;
            }

            if (!destinationRetryHash.SequenceEqual(chunk.HashedData))
            {
                throw new IOException($"Chunk number {chunk.Number} failed hash verification after {maxRetryAttempts} attempts. Aborting the process and deleting the file.");
            }
        }
    }
}