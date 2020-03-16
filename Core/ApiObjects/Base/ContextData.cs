
namespace ApiObjects.Base
{
    public class ContextData
    {
        public int GroupId { get; private set; }
        public long? DomainId { get; set; }
        public long? UserId { get; set; }
        public string Udid { get; set; }

        public ContextData(int groupId)
        {
            this.GroupId = groupId;
        }

        public override string ToString()
        {
            return $"GroupId:{GroupId}, DomainId:{DomainId}, UserId:{UserId}, Udid:{Udid}.";
        }
    }
}
