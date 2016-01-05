using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.Models.Catalog;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/personalAsset/action")]
    public class PersonalAssetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public KalturaPersonalAssetListResponse List(List<KalturaPersonalAssetRequest> assets, List<KalturaPersonalAssetWithHolder> with)
        {
            KalturaPersonalAssetListResponse response = null;

            return response;
        }
    }
}