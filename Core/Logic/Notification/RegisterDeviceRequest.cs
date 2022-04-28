namespace ApiLogic.Notification
{
    internal class RegisterDeviceRequest
    {
        public string GroupId { get; }
        public string Udid { get; }

        public RegisterDeviceRequest(string groupId, string udid)
        {
            GroupId = groupId;
            Udid = udid;
        }
    }
}