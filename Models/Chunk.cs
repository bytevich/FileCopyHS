namespace FileCopyHS.Models
{
    public class Chunk
    {
        public int Number { get; set; }
        public long Position { get; set; }
        public byte[]? Data { get; set; }
        public byte[]? HashedData { get; set; }
    }
}