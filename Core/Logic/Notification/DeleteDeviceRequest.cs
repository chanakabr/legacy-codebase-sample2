namespace ApiLogic.Notification
{
    internal class DeleteDeviceRequest
    {
        public string GroupId { get; }
        public string Udid { get; }

        public DeleteDeviceRequest(string groupId, string udid)
        {
            GroupId = groupId;
            Udid = udid;
        }
    }
}