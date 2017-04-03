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
    [RoutePrefix("_service/currency/action")]
    public class CurrencyController : ApiController
    {
         private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
         /// Get the list of currencies for the partner with option to filter by currency codes
        /// </summary>
        /// <param name="filter">currency filter</param>
        /// <remarks></remarks>
         [Route("list"), HttpPost]
         [ApiAuthorize]
         public KalturaCurrencyListResponse List(KalturaCurrencyFilter filter)
         {
             KalturaCurrencyListResponse response = null;
             int groupId = KS.GetFromRequest().GroupId;

             try
             {                   
                 response = ClientsManager.ApiClient().GetCurrencyList(groupId, filter.GetCodeIn(), filter.OrderBy);                 
             }
             catch (ClientException ex)
             {
                 ErrorUtils.HandleClientException(ex);
             }

             return response;
         }
    }
}