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
    [RoutePrefix("_service/language/action")]
    public class LanguageController : ApiController
    {
         private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
         /// Get the list of languages for the partner with option to filter by language codes
        /// </summary>
        /// <param name="filter">language filter</param>
        /// <remarks></remarks>
         [Route("list"), HttpPost]
         [ApiAuthorize]
         public KalturaLanguageListResponse List(KalturaLanguageFilter filter)
         {
             KalturaLanguageListResponse response = null;
             int groupId = KS.GetFromRequest().GroupId;

             try
             {                   
                 response = ClientsManager.ApiClient().GetLanguageList(groupId, filter.GetCodeIn(), filter.OrderBy);                 
             }
             catch (ClientException ex)
             {
                 ErrorUtils.HandleClientException(ex);
             }

             return response;
         }
    }
}