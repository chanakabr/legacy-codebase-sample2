namespace ApiLogic.Notification
{
    internal class AddToThingShadowRequest
    {
        public string GroupId { get; }
        public string ThingArn { get; }
        public string Message { get; }
        public string Udid { get; }

        public AddToThingShadowRequest(string groupId, string thingArn, string message, string udid)
        {
            GroupId = groupId;
            ThingArn = thingArn;
            Message = message;
            Udid = udid;
        }
    }
}