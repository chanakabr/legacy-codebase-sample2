
namespace ApiObjects.Notification
{
    public class MessageTemplate
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string DateFormat { get; set; }
        public eOTTAssetTypes AssetType { get; set; }
        public string Sound { get; set; }
        public string Action { get; set; }
        public string URL { get; set; }

        public override string ToString()
        {
            if (this == null)
            {
                return string.Empty;
            }

            return string.Format("MessageTemplate: Id: {0}, message: {1}, DateFormat: {2}, AssetType: {3}, Sound: {4}, Action: {5}, URL: {6}",
                Id, Message, DateFormat, AssetType.ToString(), Sound, Action, URL);
        }
    }
}
