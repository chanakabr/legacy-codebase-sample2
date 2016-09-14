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
         /// Get the list of countries for the group with option to filter by countries identifiers
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
                 response = ClientsManager.ApiClient().GetCountryList(groupId, filter.GetIdIn());
             }
             catch (ClientException ex)
             {
                 ErrorUtils.HandleClientException(ex);
             }

             return response;
         }
    }
}