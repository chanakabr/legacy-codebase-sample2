
namespace ApiObjects
{
    public class KalturaObjectDeletedEvent : KalturaObjectActionEvent
    {
        public long Id { get; set; }

        public KalturaObjectDeletedEvent(string userIp, int groupId = 0, long id = 0, string type = null, CoreObject coreObject = null, eKalturaEventTime time = eKalturaEventTime.After) : 
            base(userIp, groupId, coreObject, eKalturaEventActions.Deleted, time, type)
        {
            this.Id = id;
        }
    }
}
