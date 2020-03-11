
namespace EventManager
{
    public abstract class KalturaEvent
    {
        #region Properties

        public int PartnerId { get; set; }
        
        #endregion

        #region Ctor

        public KalturaEvent(int groupId)
        {
            this.PartnerId = groupId;
        } 

        #endregion
    }
}
