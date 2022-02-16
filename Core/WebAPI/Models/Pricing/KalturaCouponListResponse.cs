using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    [ListResponse("Coupons")]
    public partial class KalturaCouponListResponse : KalturaListResponse<KalturaCoupon>
    {
    }
}
