using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner  base configuration
    /// </summary>
    public abstract partial class KalturaPartnerConfiguration : KalturaOTTObject
    {
        protected abstract KalturaPartnerConfigurationType ConfigurationType { get; }
        internal abstract bool Update(int groupId);
        public virtual void ValidateForUpdate() { }
    }
}