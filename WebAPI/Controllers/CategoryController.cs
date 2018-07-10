using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    public class BaseCategoryController
    {
        [NonAction]
        [SchemeArgument("id", MinInteger = 1)]
        static public KalturaOTTCategory Get(int id)
        {
            KalturaOTTCategory response = null;

            int groupId = KS.GetFromRequest().GroupId;
            
            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                response = ClientsManager.CatalogClient().GetCategory(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language, id);

                // if no response - return not found status 
                if (response == null || response.Id == 0)
                {
                    throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "OTT-Category");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }

    [Service("ottCategory")]
    public class OttCategoryController : IKalturaController
    {
        /// <summary>
        /// Retrieve the list of categories (hierarchical) and their associated channels
        /// </summary>
        /// <param name="id">Category Identifier</param>
        /// <remarks></remarks>
        [Action("get")]
        [ApiAuthorize]
        static public KalturaOTTCategory Get(int id)
        {
            return BaseCategoryController.Get(id);
        }
    }

    [Service("category")]
    [Obsolete]
    public class CategoryController : IKalturaController
    {
        /// <summary>
        /// Retrieve the list of categories (hierarchical) and their associated channels
        /// </summary>
        /// <param name="id">Category Identifier</param>
        /// <remarks></remarks>
        [Action("get")]
        [ApiAuthorize]
        static public KalturaOTTCategory Get(int id)
        {
            return BaseCategoryController.Get(id);
        }
    }
}