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

        using var host = builder.Build();

        var userInputService = host.Services.GetRequiredService<IUserInputService>();
        userInputService.ValidateUserInput();
    }
}