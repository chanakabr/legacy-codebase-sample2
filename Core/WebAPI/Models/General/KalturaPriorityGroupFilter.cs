using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// It's just a pure fabrication filter not intended to filter smth.
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaPriorityGroupFilter : KalturaFilter<KalturaPriorityGroupOrderByDummy>, KalturaRelatedObjectFilter
    {
        public override KalturaPriorityGroupOrderByDummy GetDefaultOrderByValue()
        {
            return KalturaPriorityGroupOrderByDummy.NONE;
        }
    }
}
