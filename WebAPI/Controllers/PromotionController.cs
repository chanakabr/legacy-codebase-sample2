using System.Collections.Generic;
using System.Web.Http;
using WebAPI.Models;
using System.Linq;
using WebAPI.Managers.Scheme;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/promotion/action")]
    public class PromotionController : ApiController
    {
        private static List<KalturaPromotion> promotionsList = new List<KalturaPromotion>()
        {
            new KalturaPromotion()
            {
                Id = 1,
                Link = "https://www.ebay.com/itm/The-Night-King-Javelin-Spear-Dragon-Game-of-Thrones-T-Shirt-Mens-Unisex-Tee/282620622056?hash=item41cd8074e8:m:mhTzfbCx-OuU0i5R4Nkhn1Q",
                Text = "The Night King Javelin Spear Dragon Game of Thrones T-Shirt ",
                StartTime = new System.TimeSpan(0, 0, 3),
                EndTime = new System.TimeSpan(0, 0, 7)
            },
            new KalturaPromotion()
            {
                Id = 2,
                Link = "https://www.ebay.com/itm/The-Night-King-Javelin-Spear-Dragon-Game-of-Thrones-T-Shirt-Mens-Unisex-Tee/282620622056?hash=item41cd8074e8:m:mhTzfbCx-OuU0i5R4Nkhn1Q",
                Text = "The Night King Javelin Spear Dragon Game of Thrones T-Shirt ",
                StartTime = new System.TimeSpan(0, 0, 8),
                EndTime = new System.TimeSpan(0, 0, 15),
                Thumbnail = "https://vignette.wikia.nocookie.net/helmet-heroes/images/1/17/Wooden_Spear.png"
            },
            new KalturaPromotion()
            {
                Id = 3,
                Link = "https://www.ebay.com/itm/Funko-POP-Game-of-Thrones-Viserion-6-Action-Figure/112626795098?epid=2254454525&hash=item1a3914825a:g:ut8AAOSwWflZ-zPc",
                Text = "Viserion Action Figure",
                StartTime = new System.TimeSpan(0, 0, 40),
                EndTime = new System.TimeSpan(0, 0, 49)
            },
            new KalturaPromotion()
            {
                Id = 4,
                Link = "https://www.amazon.com/1stvital-Knights-Cosplay-Halloween-Costume/dp/B01G3JEEAS/ref=pd_sim_193_3?_encoding=UTF8&psc=1&refRID=B37GKFMD48XKHPCBJEMZ",
                Text = "Jon Snow Knights Watch Costume",
                StartTime = new System.TimeSpan(0, 1, 7),
                EndTime = new System.TimeSpan(0, 1, 16)
            }
        };


        /// <summary>
        /// Returns promotions list regarding to mediaId
        /// </summary>        
        /// <param name="filter">Filter</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaPromotionListResponse List(KalturaPromotionFilter filter = null)
        {
            KalturaPromotionListResponse response = new KalturaPromotionListResponse();
            if (filter != null && filter.SavedEqual)
            {
                response.Promotions = promotionsList.Where(p => p.Saved).ToList();
            }
            else
            {
                response.Promotions = promotionsList;
            }
            return response;
        }

        /// <summary>
        /// Saves a Promotion for later
        /// </summary>        
        /// <param name="id">Promotion ID</param>
        /// <remarks></remarks>
        [Route("save"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public void Save(long id)
        {
            promotionsList.Where(p => p.Id == id).FirstOrDefault().Saved = true;
        }
    }
}