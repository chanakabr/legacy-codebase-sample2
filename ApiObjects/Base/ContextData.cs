
namespace ApiObjects.Base
{
    public class ContextData
    {
        public int GroupId { get; private set; }
        public long? DomainId { get; set; }
        public long? UserId { get; set; }

        public ContextData(int groupId)
        {
            this.GroupId = groupId;
        }

        public override string ToString()
        {
            return string.Format("GroupId:{0}, DomainId:{1}, UserId:{2}.", GroupId, DomainId, UserId);
        }
    }
}
