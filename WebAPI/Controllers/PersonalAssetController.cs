using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/personalAsset/action")]
    public class PersonalAssetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Lists personal enriched data for given assets
        /// </summary>
        /// <param name="assets">Assets and files which we want their data</param>
        /// <param name="with">Which data will be returned</param>
        /// <returns></returns>
        public KalturaPersonalAssetListResponse List(List<KalturaPersonalAssetRequest> assets, List<KalturaPersonalAssetWithHolder> with)
        {
            KalturaPersonalAssetListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (with == null)
            {
                with = new List<KalturaPersonalAssetWithHolder>();
            }

            try
            {

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}