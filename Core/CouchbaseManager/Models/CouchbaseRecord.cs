namespace CouchbaseManager.Models
{
    public class CouchbaseRecord<T>
    {
        public string Key { get; set; }
        
        public T Content { get; set; }

        public Compression.Compression Compression { get; set; } = global::CouchbaseManager.Compression.Compression.None;

        public uint Expiration { get; set; }

        public ulong? Version { get; set; }
    }
}