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

        public async Task Run()
        {
            var destinationPath = string.Empty;
            try
            {
                var paths = _userInputService.ValidateUserInput();
                destinationPath = paths.Item2;

                await _fileProcessorService.CopyFile(paths.Item1, paths.Item2);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "File transfer failed.");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the file copy process.");
            }
        }
    }
}
