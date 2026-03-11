using System.Security.Cryptography;

namespace FileCopyHS.Interfaces
{
    public interface IHashService
    {
        public byte[] ComputeMd5Hash(byte[] data);
        Task<Tuple<bool, string, string>> ComputeAndCompare(IncrementalHash sourceHashInstance, string destinationFile, CancellationToken ct);
    }
}