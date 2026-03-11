using System.Threading.Channels;
using FileCopyHS.Models;

namespace FileCopyHS.Interfaces
{
    public interface IFileWriterService
    {
        Task WriteFile(string destinationFile, ChannelReader<Chunk> reader, CancellationToken ct);
    }
}