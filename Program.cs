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
        builder.Services.AddTransient<AppRunner>();

        using var host = builder.Build();
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await host.Services.GetRequiredService<AppRunner>().Run(cts.Token);
    }
}