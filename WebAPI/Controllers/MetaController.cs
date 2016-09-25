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
    [RoutePrefix("_service/meta/action")]
    public class MetaController : ApiController
    {
         private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
         /// Get the list of meta mappings for the partner
        /// </summary>
        /// <param name="filter">Country filter</param>
        /// <remarks></remarks>
         [Route("list"), HttpPost]
         [ApiAuthorize]
         public KalturaMetaListResponse List(KalturaMetaFilter filter)
         {
             KalturaMetaListResponse response = null;

             int groupId = KS.GetFromRequest().GroupId;

             try
             {
                 response = ClientsManager.ApiClient().GetGroupMeta(groupId, filter.AssetTypeEqual, filter.TypeEqual, filter.FieldNameEqual, filter.FieldNameNotEqual);
             }
             catch (ClientException ex)
             {
                 ErrorUtils.HandleClientException(ex);
             }

             return response;
         }
    }
}