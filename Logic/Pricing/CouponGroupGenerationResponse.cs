using System.Collections.Generic;

namespace Core.Pricing
{
    public class CouponGroupGenerationResponse
    {
        public List<string> Codes{ get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
