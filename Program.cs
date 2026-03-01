using System.Security.Cryptography;

namespace FileCopyHS;

internal class Program
{
    private static async Task Main()
    {
        Console.Write("Enter source file path: ");

        var sourcePath = Console.ReadLine();
        if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
        {
            Console.WriteLine("Invalid source path or file does not exist.");
            return;
        }

        Console.Write("Enter destination path: ");

        var destinationPath = Console.ReadLine();
        if (string.IsNullOrEmpty(destinationPath) || !Directory.Exists(destinationPath))
        {
            Console.WriteLine("Invalid destination path or directory does not exist in the destination path.");
            return;
        }
        
        try
        {
            var fileName = Path.GetFileName(sourcePath);

            // NOTE: validating if filename is valid only with an empty or null check, no check for special characters for different OS
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("An error occurred while obtaining the file name.");
                return;
            }

            destinationPath = Path.Combine(destinationPath, fileName);
            if (string.IsNullOrEmpty(destinationPath))
            {
                Console.WriteLine("An error occurred while setting up the destination path.");
                return;
            }

            await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            if (sourceStream.Length == 0)
            {
                Console.WriteLine("The source file is empty.");
                return;
            }

            var success = true;
            var buffer = new byte[1024 * 1024];
            var chunkCounter = 0;
            long totalBytesRead = 0;
            int currentBytesRead;
            var maxRetryAttempts = 3;
            using var md5 = MD5.Create();

            using var sourceHashInstance = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite);
            using var destinationHashInstance = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            while ((currentBytesRead = await sourceStream.ReadAsync(buffer)) > 0)
            {
                chunkCounter++;
                totalBytesRead += currentBytesRead;
                var chunkPosition = totalBytesRead - currentBytesRead;

                var sourceChunkHash = md5.ComputeHash(buffer[..currentBytesRead]);
                sourceHashInstance.AppendData(buffer[..currentBytesRead]);
                await destinationStream.WriteAsync(buffer[..currentBytesRead]);

                Console.WriteLine($"Chunk number: {chunkCounter}, position = {chunkPosition}, hash = {BitConverter.ToString(sourceChunkHash)}");

                destinationStream.Seek(chunkPosition, SeekOrigin.Begin);

                /* warning: Avoid inexact read with 'System.IO.FileStream.ReadAsync(System.Memory<byte>, System.Threading.CancellationToken)'
                    about not knowing if the stream will return the requested number of bytes in one read 
                    this won't be a problem for local file streams cause it reads from disk
                    for a real case scenario, there should be a mechanism for this */

                await destinationStream.ReadAsync(buffer[..currentBytesRead]);
                var destinationChunkHash = md5.ComputeHash(buffer[..currentBytesRead]);

                if (!destinationChunkHash.SequenceEqual(sourceChunkHash))
                {
                    Console.WriteLine($"Chunk number {chunkCounter} failed hash verification. Re-submitting the chunk.");

                    for (var i = 0; i < maxRetryAttempts; i++)
                    {
                        destinationStream.Seek(chunkPosition, SeekOrigin.Begin);
                        await destinationStream.WriteAsync(buffer[..currentBytesRead]);
                        destinationStream.Seek(chunkPosition, SeekOrigin.Begin);
                        await destinationStream.ReadAsync(buffer[..currentBytesRead]);
                        destinationChunkHash = md5.ComputeHash(buffer[..currentBytesRead]);

                        if (destinationChunkHash.SequenceEqual(sourceChunkHash))
                        {
                            break;
                        }
                    }

                    if (!destinationChunkHash.SequenceEqual(sourceChunkHash))
                    {
                        success = false;
                        Console.WriteLine($"Chunk number {chunkCounter} failed hash verification after {maxRetryAttempts} attempts. Aborting the process and deleting the file.");
                        break;
                    }
                }

                destinationHashInstance.AppendData(buffer[..currentBytesRead]);

                /* this is not needed because the stream will be at the end of the written chunk because we do a read after the write
                 was put as a test to ensure whether the stream position is correct or no. the hashes were the same even without this line
                 it is not an expensive operations so it is good to have this as a safe guard */
                destinationStream.Seek(0, SeekOrigin.End);
            }
            
            if (success)
            {
                var destinationFileHash = destinationHashInstance.GetHashAndReset();
                var sourceFileHash = sourceHashInstance.GetHashAndReset();
                var filesHashCheck = sourceFileHash.SequenceEqual(destinationFileHash) ? string.Empty : " not";

                Console.WriteLine($"Source file and destination file are{filesHashCheck} the same. " +
                                  $"Source file hash: {BitConverter.ToString(sourceFileHash)}, Destination file hash: {BitConverter.ToString(destinationFileHash)}");
            }

            else
            {
                File.Delete(destinationPath);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}