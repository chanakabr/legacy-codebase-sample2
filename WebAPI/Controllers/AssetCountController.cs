using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    public class AssetCountController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /*
         Like list of AssetController
         KalturaAssetInfoFilter
         KalturaAssetGroupBy
         * KalturaAssetMetaGroupBy
         * 
         Returns: List KalturaAssetCount
         * Count
         * String - the value of the meta
         * 
         */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <param name="pager">Paging the request</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetCountListResponse List(KalturaAssetFilter filter = null, KalturaFilterPager pager = null, KalturaAssetMetaGroupBy groupBy = null)
        {
            return null;
        }
    }
}