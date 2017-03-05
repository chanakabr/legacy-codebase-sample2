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
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

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
        /// <param name="groupBy">Group by field</groupBy>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetCountListResponse List(KalturaSearchAssetFilter filter = null, KalturaAssetGroupBy groupBy = null)
        {
            KalturaAssetCountListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (filter == null)
            {
                filter = new KalturaSearchAssetFilter();
            }
            else
            {
                filter.Validate();
            }

            List<string> groupByValuesList = null;

            if (groupBy == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupBy");
            }
            else
            {
                groupByValuesList  = groupBy.getValues();

                if (groupByValuesList == null || groupByValuesList.Count == 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupBy");
                }
            }

            try
            {
                KalturaSearchAssetFilter regularAssetFilter = (KalturaSearchAssetFilter)filter;
                response = ClientsManager.CatalogClient().GetAssetCount(groupId, userID, domainId, udid, language, regularAssetFilter.KSql,
                    regularAssetFilter.OrderBy, regularAssetFilter.getTypeIn(), regularAssetFilter.getEpgChannelIdIn(), groupByValuesList);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}