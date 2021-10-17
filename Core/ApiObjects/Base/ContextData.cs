namespace ApiObjects.Base
{
    public class ContextData
    {
        public int GroupId { get; }
        public long? DomainId { get; set; }
        public long? UserId { get; set; }
        public long? OriginalUserId { get; set; }
        public string Udid { get; set; }
        public string UserIp { get; set; }
        public string Language { get; set; }
        public string Format { get; set; }
        public bool ManagementData => !string.IsNullOrEmpty(Format) && Format == "30";
        public string SessionCharacteristicKey { get; set; }

        public ContextData(int groupId)
        {
            GroupId = groupId;
        }

        public override string ToString()
        {
            return
                $"GroupId:{GroupId}, DomainId:{DomainId}, UserId:{UserId}, Udid:{Udid}, UserIp:{UserIp}, Language:{Language}, Format:{Format}, SessionCharacteristicKey:{SessionCharacteristicKey}.";
        }
    }
}
