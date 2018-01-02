using System.Collections.Generic;
using System.Web.Http;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/promotion/action")]
    public class PromotionController : ApiController
    {
        /// <summary>
        /// Returns promotions list regarding to mediaId
        /// </summary>        
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaPromotionListResponse List()
        {
            KalturaPromotionListResponse response = new KalturaPromotionListResponse();
            response.Promotions = new List<KalturaPromotion>();
            response.Promotions.Add(new KalturaPromotion() { Link = "http://www.next.co.il/en/g222610s2#185058", Text = "Game over", StartTime = 5, EndTime=7 });
            return response;
        }
    }
}