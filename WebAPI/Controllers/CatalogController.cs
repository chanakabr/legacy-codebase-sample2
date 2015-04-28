using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.Clients.Utils;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [RoutePrefix("catalog")]
    public class CatalogController : ApiController
    {
        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="search_assets"></param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("search"), HttpPost]
        public AssetInfoWrapper Post([FromBody]SearchAssets search_assets)
        {
            int groupId = 215;            



            var res = ClientsManager.CatalogClient().SearchAssets(groupId, string.Empty, string.Empty, 0, 
                search_assets.page_index, search_assets.page_size, search_assets.filter, search_assets.order_by, search_assets.filter_types, search_assets.with);

            return res;
        }
    }
}