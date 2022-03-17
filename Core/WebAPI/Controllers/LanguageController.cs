using Phx.Lib.Log;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("language")]
    public class LanguageController : IKalturaController
    {
         private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
         /// Get the list of languages for the partner with option to filter by language codes
        /// </summary>
        /// <param name="filter">language filter</param>
        /// <remarks></remarks>
         [Action("list")]
         [ApiAuthorize]
         static public KalturaLanguageListResponse List(KalturaLanguageFilter filter)
         {
             KalturaLanguageListResponse response = null;
             int groupId = KS.GetFromRequest().GroupId;

            filter.Validate();

            try
            {
                if (filter.ExcludePartner.HasValue && filter.ExcludePartner.Value)
                {
                    response = ClientsManager.ApiClient().GetLanguageList(groupId, filter.OrderBy);
                }
                else
                {
                    response = ClientsManager.ApiClient().GetLanguageList(groupId, filter.GetCodeIn(), filter.OrderBy);
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