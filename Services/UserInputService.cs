using FileCopyHS.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileCopyHS.Services
{
    public class UserInputService : IUserInputService
    {
        private readonly ILogger<UserInputService> _logger;

        public UserInputService(ILogger<UserInputService> logger)
        {
            _logger = logger;
        }

        public Tuple<string, string> ValidateUserInput()
        {
            string destinationFile;
            Console.Write("Enter source file path: ");

            var sourceFile = Console.ReadLine();
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException("Invalid source path or file does not exist.");
            }

            Console.Write("Enter destination path: ");

            var destinationPath = Console.ReadLine();
            if (string.IsNullOrEmpty(destinationPath) || !Directory.Exists(destinationPath))
            {
                throw new DirectoryNotFoundException("Invalid destination path or directory does not exist in the destination path.");
            }

            if (!string.IsNullOrEmpty(sourceFile) && !string.IsNullOrEmpty(destinationPath) &&
                Path.GetFullPath(sourceFile).Equals(Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Source and destination paths cannot be the same.");
            }

            var fileName = Path.GetFileName(sourceFile);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("An error occurred while obtaining the file name.");
            }

            var extension = Path.GetExtension(fileName);

            Console.Write("Keep original filename? (y/n)");
            var keepFilename = Console.ReadLine();

            switch (keepFilename)
            {
                case "y": destinationFile = FileNameCheck(destinationPath, extension, Path.GetFileNameWithoutExtension(fileName));
                    break;
                case "n": Console.WriteLine("Enter new filename: ");
                    var newFileName = Console.ReadLine();
                    destinationFile = FileNameCheck(destinationPath, extension, Path.GetFileNameWithoutExtension(newFileName));
                    break;
                default: destinationFile = FileNameCheck(destinationPath, extension, Path.GetFileNameWithoutExtension(fileName));
                    _logger.LogWarning("Invalid input, keeping original filename.");
                    break;
            }
            
            return new Tuple<string, string>(sourceFile, destinationFile);
        }

        private static string FileNameCheck(string destinationPath, string extension, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Filename cannot be empty.");
            }

            var counter = 0;
            var originalName = fileName;

            while (File.Exists(Path.Combine(destinationPath, $"{fileName}{extension}")))
            {
                counter++;
                fileName = $"{originalName}_{counter}";
            }

            return Path.Combine(destinationPath, $"{fileName}{extension}");
        }
    }
}