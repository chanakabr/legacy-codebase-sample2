using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlements filter 
    /// </summary>
    public partial class KalturaBaseEntitlementFilter : KalturaFilter<KalturaEntitlementOrderBy>
    {
        public override KalturaEntitlementOrderBy GetDefaultOrderByValue()
        {
            return KalturaEntitlementOrderBy.PURCHASE_DATE_ASC;
        }
    }
}