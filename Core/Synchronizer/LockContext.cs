namespace Synchronizer
{
    public sealed class LockContext
    {
        public LockContext(int groupId) : this(groupId, 0)
        {
        }

        public LockContext(int groupId, long userId)
        {
            GroupId = groupId;
            UserId = userId;
        }
        
        public int GroupId { get; }

        public long UserId { get; }
    }
}
