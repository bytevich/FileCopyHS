using FileCopyHS.Interfaces;
using FileCopyHS.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileCopyHS;

internal class Program
{
    private static async Task Main()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddTransient<IFileReaderService, FileReaderService>();
        builder.Services.AddTransient<IFileWriterService, FileWriterService>();
        builder.Services.AddTransient<IHashService, HashService>();
        builder.Services.AddTransient<IUserInputService, UserInputService>();
        builder.Services.AddTransient<IFileProcessorService, FileProcessorService>();

        using var host = builder.Build();

        var userInputService = host.Services.GetRequiredService<IUserInputService>();
        var paths = userInputService.ValidateUserInput();
        var fileProcessorService = host.Services.GetRequiredService<IFileProcessorService>();
        await fileProcessorService.CopyFile(paths.Item1, paths.Item2);
    }
}