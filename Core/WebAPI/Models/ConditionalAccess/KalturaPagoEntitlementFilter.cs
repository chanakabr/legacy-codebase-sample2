namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Program asset group offer Entitlements filter 
    /// </summary>
    public partial class KalturaProgramAssetGroupOfferEntitlementFilter : KalturaBaseEntitlementFilter
    {
       
        public override KalturaEntitlementOrderBy GetDefaultOrderByValue()
        {
            return KalturaEntitlementOrderBy.PURCHASE_DATE_ASC;
        }
    }
}