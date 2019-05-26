
namespace EventManager
{
    public abstract class KalturaEvent
    {
        public const string USER_IP = "USER_IP";

        #region Properties

        public int PartnerId { get; set; }

        public string UserIp { get; set; }

        #endregion

        #region Ctor

        public KalturaEvent(int groupId, string userIp)
        {
            this.PartnerId = groupId;
            this.UserIp = userIp;
        } 

        #endregion
    }
}
