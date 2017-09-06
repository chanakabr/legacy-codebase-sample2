using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetStruct/action")]
    public class AssetStructController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Return a list of asset structs for the account with optional filter
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetStructListResponse List(KalturaAssetStructFilter filter = null)
        {
            if (filter == null)
            {
                filter = new KalturaAssetStructFilter();
            }
            else
            {
                filter.Validate();
            }

            KalturaAssetStructListResponse response = new KalturaAssetStructListResponse();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (!string.IsNullOrEmpty(filter.MetaIdContains))
                {
                    // call client
                    response = ClientsManager.CatalogClient().GetAssetStructs(groupId, filter.GetMetaIdContains(), filter.OrderBy, true);
                }
                else
                {
                    // call client
                    response = ClientsManager.CatalogClient().GetAssetStructs(groupId, filter.GetIdIn(), filter.OrderBy);
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}