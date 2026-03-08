using FileCopyHS.Interfaces;

namespace FileCopyHS.Services
{
    public class UserInputService : IUserInputService
    {
        public Tuple<string, string> ValidateUserInput()
        {
            var sourceFile = string.Empty;
            var destinationFile = string.Empty;

            try
            {
                Console.Write("Enter source file path: ");

                sourceFile = Console.ReadLine();
                if (!File.Exists(sourceFile))
                {
                    Console.WriteLine("Invalid source path or file does not exist.");
                    Environment.Exit(1);
                }

                Console.Write("Enter destination path: ");

                var destinationPath = Console.ReadLine();
                if (string.IsNullOrEmpty(destinationPath) || !Directory.Exists(destinationPath))
                {
                    Console.WriteLine("Invalid destination path or directory does not exist in the destination path.");
                    Environment.Exit(1);
                }

                if (!string.IsNullOrEmpty(sourceFile) && !string.IsNullOrEmpty(destinationPath) &&
                    Path.GetFullPath(sourceFile).Equals(Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Source and destination paths cannot be the same.");
                    Environment.Exit(1);
                }

                var fileName = Path.GetFileName(sourceFile);
                if (string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine("An error occurred while obtaining the file name.");
                    Environment.Exit(1);
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
                        Console.WriteLine("Invalid input, keeping original filename.");
                        break;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }

            return new Tuple<string, string>(sourceFile, destinationFile);
        }

        private static string FileNameCheck(string destinationPath, string extension, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("Filename cannot be empty.");
                Environment.Exit(1);
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