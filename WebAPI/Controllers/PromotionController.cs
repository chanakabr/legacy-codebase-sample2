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
            response.UiConfId = 41400451;
            response.EntryId= "1_06ex551x";
            response.PartnerId = 2091671;
            response.Promotions = new List<KalturaPromotion>();
            response.Promotions.Add(new KalturaPromotion() {
                Link = "https://www.ebay.com/itm/The-Night-King-Javelin-Spear-Dragon-Game-of-Thrones-T-Shirt-Mens-Unisex-Tee/282620622056?hash=item41cd8074e8:m:mhTzfbCx-OuU0i5R4Nkhn1Q",
                Text ="The Night King Javelin Spear Dragon Game of Thrones T-Shirt ",
                StartTime = 8,
                EndTime = 15 });
            response.Promotions.Add(new KalturaPromotion()
            {
                Link = "https://www.ebay.com/itm/Funko-POP-Game-of-Thrones-Viserion-6-Action-Figure/112626795098?epid=2254454525&hash=item1a3914825a:g:ut8AAOSwWflZ-zPc",
                Text = "Viserion Action Figure",
                StartTime = 40,
                EndTime = 49
            });
            response.Promotions.Add(new KalturaPromotion()
            {
                Link = "https://www.amazon.com/1stvital-Knights-Cosplay-Halloween-Costume/dp/B01G3JEEAS/ref=pd_sim_193_3?_encoding=UTF8&psc=1&refRID=B37GKFMD48XKHPCBJEMZ",
                Text = "Jon Snow Knights Watch Costume",
                StartTime = 67,
                EndTime = 76
            });
            return response;
        }
    }
}