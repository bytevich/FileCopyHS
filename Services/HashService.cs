using System.Security.Cryptography;
using FileCopyHS.Interfaces;

namespace FileCopyHS.Services
{
    public class HashService : IHashService
    {
        public byte[] ComputeMd5Hash(byte[] data)
        {
            return MD5.HashData(data);
        }

        public async Task<Tuple<bool, string, string>> ComputeAndCompare(IncrementalHash sourceHashInstance, string destinationFile, CancellationToken ct)
        {
            var sourceFileHash = sourceHashInstance.GetHashAndReset();
            await using var destStream = File.OpenRead(destinationFile);
            var destinationFileHash = await SHA256.HashDataAsync(destStream, ct);

            var areEqual = sourceFileHash.SequenceEqual(destinationFileHash);

            return new Tuple<bool, string, string>(areEqual, BitConverter.ToString(sourceFileHash), BitConverter.ToString(destinationFileHash));
        }
    }
}