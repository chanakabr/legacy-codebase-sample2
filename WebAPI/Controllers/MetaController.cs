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
        /// <param name="filter">Meta filter</param>
        /// <remarks></remarks>
         [Route("list"), HttpPost]
         [ApiAuthorize]
         public KalturaMetaListResponse List(KalturaMetaFilter filter = null)
         {
             KalturaMetaListResponse response = null;

             if (filter == null)
             {
                 filter = new KalturaMetaFilter();
             }

             filter.validate();
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

         /// <summary>
         /// Update meta's user interest
         /// </summary>
         /// <param name="meta">Meta</param>           
         /// <returns></returns>
         /// <remarks>
         /// Possible status codes: 
         /// </remarks>
         [Route("update"), HttpPost]
         [ApiAuthorize]
         [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
         public KalturaMeta Update(KalturaMeta meta)
         {
             KalturaMeta response = null;

             int groupId = KS.GetFromRequest().GroupId;

             try
             {
                 if (string.IsNullOrEmpty(meta.Name))
                 {
                     throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaMeta.Name");
                 }

                 // call client
                 response = ClientsManager.CatalogClient().UpdateGroupMeta(groupId, meta);
             }
             catch (ClientException ex)
             {
                 ErrorUtils.HandleClientException(ex);
             }

             return response;
         }





    }
}