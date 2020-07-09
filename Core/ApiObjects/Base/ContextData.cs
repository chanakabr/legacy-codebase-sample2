
namespace ApiObjects.Base
{
    public class ContextData
    {
        public int GroupId { get; private set; }
        public long? DomainId { get; set; }
        public long? UserId { get; set; }
        public string Udid { get; set; }
        public string UserIp { get; set; }
        public string Language { get; set; }
        public string Format { get; set; }
        public bool ManagementData 
        {
            get
            {
                return !string.IsNullOrEmpty(this.Format) && this.Format == "30" ? true : false;
            }
        }

        public ContextData(int groupId)
        {
            this.GroupId = groupId;
        }

        public override string ToString()
        {
            return $"GroupId:{GroupId}, DomainId:{DomainId}, UserId:{UserId}, Udid:{Udid}, UserIp:{UserIp}, Language:{Language}, Format:{Format}.";
        }
    }
}
