using ApiObjects.Response;
using ConfigurationManager;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("assetFilePpv")]
    public class AssetFilePpvController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Return a list of asset files ppvs for the account with optional filter
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaAssetFilePpvListResponse List(KalturaAssetFilePpvFilter filter)
        {
            KalturaAssetFilePpvListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "filter");
            }

            filter.Validate();

            try
            {
                response = ClientsManager.PricingClient().GetAssetFilePPVList(groupId, filter.AssetIdEqual, filter.AssetFileIdEqual);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}