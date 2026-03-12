using System.Security.Cryptography;
using System.Threading.Channels;
using FileCopyHS.Models;

namespace FileCopyHS.Interfaces
{
    public interface IFileReaderService
    {
        Task ReadFile(string sourceFile, ChannelWriter<Chunk> writer, IncrementalHash sourceHashInstance, CancellationToken ct);
    }
}