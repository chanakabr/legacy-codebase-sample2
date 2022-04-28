namespace ApiLogic.Notification
{
    internal class CreateIotEnvironmentRequest
    {
        public string GroupId { get; }

        public CreateIotEnvironmentRequest(string groupId)
        {
            GroupId = groupId;
        }
    }
}