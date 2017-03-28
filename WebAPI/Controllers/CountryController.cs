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
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/country/action")]
    public class CountryController : ApiController
    {
         private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
         /// Get the list of countries for the partner with option to filter by countries identifiers
        /// </summary>
        /// <param name="filter">Country filter</param>
        /// <remarks></remarks>
         [Route("list"), HttpPost]
         [ApiAuthorize]
         public KalturaCountryListResponse List(KalturaCountryFilter filter)
         {
             KalturaCountryListResponse response = null;

             int groupId = KS.GetFromRequest().GroupId;

             try
             {                 
                 List<int> countryIds = filter.GetIdIn();
                 bool isIpEqualExists = !string.IsNullOrEmpty(filter.IpEqual);
                 if (countryIds.Count > 0)
                 {
                     if ((isIpEqualExists || (filter.IpEqualCurrent.HasValue && filter.IpEqualCurrent.Value)))
                     {
                         throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "IdIn", isIpEqualExists ? "IpEqual" : "IpEqualCurrent");
                     }
                     else
                     {
                         response = ClientsManager.ApiClient().GetCountryList(groupId, countryIds, filter.OrderBy);
                     }
                 }
                 else if (isIpEqualExists)
                 {
                     if (filter.IpEqualCurrent.HasValue && filter.IpEqualCurrent.Value)
                     {
                         throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "IpEqual", "IpEqualCurrent");
                     }
                     else
                     {
                         response = ClientsManager.ApiClient().GetCountryListByIp(groupId, filter.IpEqual, filter.IpEqualCurrent, filter.OrderBy);
                     }
                 }
                 else if (filter.IpEqualCurrent.HasValue && filter.IpEqualCurrent.Value)
                 {
                     response = ClientsManager.ApiClient().GetCountryListByIp(groupId, filter.IpEqual, filter.IpEqualCurrent, filter.OrderBy);
                 }
                 else
                 {
                     response = ClientsManager.ApiClient().GetCountryList(groupId, countryIds, filter.OrderBy);
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