
namespace ApiObjects.Notification
{
    public class FollowTemplate
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string DateFormat { get; set; }
        public eOTTAssetTypes AssetType { get; set; }

        public override string ToString()
        {
            if (this == null)
            {
                return string.Empty;
            }

            return string.Format("FollowTemplate: Id: {0}, message: {1}, DateFormat: {2}, AssetType: {3}",
                Id, Message, DateFormat, AssetType.ToString());
        }
    }
}
