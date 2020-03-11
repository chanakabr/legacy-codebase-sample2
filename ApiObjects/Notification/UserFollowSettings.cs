
namespace ApiObjects.Notification
{
    public class UserFollowSettings
    {
        public bool? EnablePush { get; set; }

        public bool? EnableMail { get; set; }

        public bool? EnableSms { get; set; }

        public UserFollowSettings()
        {
            EnablePush = true;            
            EnableMail = true;
            EnableSms = true;
        }
    }
}
