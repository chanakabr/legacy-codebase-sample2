namespace ApiObjects
{
    public class EncryptionKey
    {
        public long Id { get; }
        public int GroupId { get; }
        public byte[] Value { get; }
        public EncryptionType Type { get; }

        public EncryptionKey(long id, int groupId, byte[] value, EncryptionType type)
        {
            Id = id;
            GroupId = groupId;
            Value = value;
            Type = type;
        }
    }
}
