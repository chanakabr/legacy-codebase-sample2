using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/tag/action")]
    public class TagController : ApiController
    {
        /// <summary>
        /// Returns all regions for the partner
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Page size and index</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaTagListResponse List (KalturaTagFilter filter = null, KalturaFilterPager pager =null)
        {
            KalturaTagListResponse response = null;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            if (filter == null)
            {
                filter = new KalturaTagFilter();
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.ApiClient().SearchTags(groupId, language, filter.TagEqual, filter.TopicIdEqual, filter.TagStartsWith, pager.getPageIndex(), pager.getPageSize());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}