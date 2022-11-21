using System;
using System.Collections.Generic;

namespace ApiObjects.Pricing.Dto
{
    public class PpvDTO
    {
        public int Id { get; set; }
        public int PriceCode { get; set; }
        public long UsageModuleCode { get; set; }
        public long DiscountCode { get; set; }
        public long CouponsGroupCode { get; set; }
        public LanguageContainer[] Descriptions { get; set; }
        public string Name { get; set; }
        public string AdsParam { get; set; }
        public bool SubscriptionOnly { get; set; }
        public string ProductCode { get; set; }
        public bool FirstDeviceLimitation { get; set; }
        public bool IsActive { get; set; }
        public string alias { get; set; }
        public List<int> FileTypesIds { get; set; }
        public AdsPolicy? AdsPolicy { get; set; }
        
        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }
        
        public long? VirtualAssetId { get; set; }
        public long? AssetUserRuleId { get; set; }
    }
}
