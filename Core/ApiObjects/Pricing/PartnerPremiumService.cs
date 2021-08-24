using System.Collections.Generic;

namespace ApiObjects.Pricing
{
    public class PartnerPremiumService
    {
        public long Id { get; set; }
        public string Name{ get; set; }
        public bool IsApplied{ get; set; }
    }

    public class PartnerPremiumServices
    {
        public List<PartnerPremiumService> Services{ get; set; }
    }
}