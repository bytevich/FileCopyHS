using System.Security.Cryptography;

namespace FileCopyHS;

internal class Program
{
    private static async Task Main()
    {
        Console.Write("Enter source file path: ");

        var sourcePath = Console.ReadLine();
        //TODO: check the methods
        if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
        {
            Console.WriteLine("Invalid source path.");
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

            // validating if filename is valid only with an empty or null check, no check for special characters for different OS
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

            var file = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            if (file.Length == 0)
            {
                Console.WriteLine("The source file is empty.");
                return;
            }

            // TODO: last chunk may be smaller than the buffer size, handle that case

            var buffer = new byte[1024 * 1024];
            var chunkCounter = 0;
            var totalBytesRead = 0;
            int currentBytesRead;
            await using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            using var md5 = MD5.Create();

            while ((currentBytesRead = await file.ReadAsync(buffer)) > 0)
            {
                chunkCounter++;
                totalBytesRead += currentBytesRead;
                var hashedChunk = md5.ComputeHash(buffer);

                await destinationStream.WriteAsync(buffer);
                Console.WriteLine($"Chunk number: {chunkCounter}, position = {totalBytesRead - currentBytesRead}, hash = {BitConverter.ToString(hashedChunk)}");

                // TODO: verify each chunk at the destination by comparing the hash of the chunk with the hash sent from the source, if one of the chunks does not match, re-submit the whole file
            }

            // TODO: hash the whole file and then compare the hash of the whole file in destination and display both
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}