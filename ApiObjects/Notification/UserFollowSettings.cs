
namespace ApiObjects.Notification
{
    public class UserFollowSettings
    {
        public bool? EnablePush { get; set; }

        public bool? EnableInbox { get; set; }

        public bool? EnableMail { get; set; }

        public UserFollowSettings()
        {
            EnablePush = true;
            EnableInbox = true;
            EnableMail = true;
        }
    }
}
