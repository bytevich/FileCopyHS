using FileCopyHS.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileCopyHS
{
    public class AppRunner
    {
        private readonly IUserInputService _userInputService;
        private readonly IFileProcessorService _fileProcessorService;
        private readonly ILogger<AppRunner> _logger;

        public AppRunner(IUserInputService userInputService, IFileProcessorService fileProcessorService, ILogger<AppRunner> logger)
        {
            _userInputService = userInputService;
            _fileProcessorService = fileProcessorService;
            _logger = logger;
        }

        public async Task Run(CancellationToken ct)
        {
            var destinationPath = string.Empty;
            try
            {
                var paths = _userInputService.ValidateUserInput();
                destinationPath = paths.Item2;

                await _fileProcessorService.CopyFile(paths.Item1, paths.Item2, ct);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "The file was not found.");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "File transfer failed.");
                DeleteFile(destinationPath);
                
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("The operation was cancelled.");
                DeleteFile(destinationPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the file copy process.");
            }
        }

        private void DeleteFile(string destinationPath)
        {
            Console.Write("Do you want to proceed to delete the file? (y/n)");
            var deleteFile = Console.ReadLine();

            if (!string.IsNullOrEmpty(deleteFile) && deleteFile == "y")
            {
                try
                {
                    File.Delete(destinationPath);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to delete partial file: {Path}", destinationPath);
                }
            }
        }
    }
}
