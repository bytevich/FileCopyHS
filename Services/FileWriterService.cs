using System.Collections.Concurrent;
using System.Threading.Channels;
using FileCopyHS.Interfaces;
using FileCopyHS.Models;
using Microsoft.Win32.SafeHandles;

namespace FileCopyHS.Services
{
    public class FileWriterService : IFileWriterService
    {
        private readonly IHashService _hashService;

        public FileWriterService(IHashService hashService)
        {
            _hashService = hashService; 
        }

        public async Task WriteFile(string destinationFile, ChannelReader<Chunk> reader)
        {
            try
            {
                using var destinationFileHandle = File.OpenHandle(
                    destinationFile,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    FileOptions.Asynchronous
                );

                var tasks = new ConcurrentBag<Task>();

                await foreach (var chunk in reader.ReadAllAsync())
                {
                    if (chunk.Data == null)
                    {
                        Console.WriteLine("Chunk data is null.");
                        Environment.Exit(1);
                    }

                    tasks.Add(WriteChunk(destinationFileHandle, chunk));
                }

                await Task.WhenAll(tasks);
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                // TODO: delete the file
                // TODO: avoid environment exit
                Environment.Exit(1);
            }
        }

        private async Task WriteChunk(SafeFileHandle destinationFileHandle, Chunk chunk)
        {
            await RandomAccess.WriteAsync(destinationFileHandle, chunk.Data, chunk.Position);
            var buffer = new byte[chunk.Size];
            await RandomAccess.ReadAsync(destinationFileHandle, buffer, chunk.Position);

            var destinationHash = _hashService.ComputeMd5Hash(buffer);
            if (!destinationHash.SequenceEqual(chunk.HashedData))
            {
                Console.WriteLine($"Chunk number {chunk.Number} failed hash verification. Re-submitting the chunk.");
                await VerifyChunk(destinationFileHandle, chunk, buffer);
            }
        }

        private async Task VerifyChunk(SafeFileHandle destinationFileHandle, Chunk chunk, byte[] buffer)
        {
            var maxRetryAttempts = 3;
            var destinationRetryHash = Array.Empty<byte>();
            for (var i = 0; i < maxRetryAttempts; i++)
            {
                await RandomAccess.WriteAsync(destinationFileHandle, chunk.Data, chunk.Position);
                await RandomAccess.ReadAsync(destinationFileHandle, buffer, chunk.Position);
                destinationRetryHash = _hashService.ComputeMd5Hash(buffer);

                if (destinationRetryHash.SequenceEqual(chunk.HashedData))
                    break;
            }

            if (!destinationRetryHash.SequenceEqual(chunk.HashedData))
            {
                throw new Exception($"Chunk number {chunk.Number} failed hash verification after {maxRetryAttempts} attempts. Aborting the process and deleting the file.");
            }
        }
    }
}