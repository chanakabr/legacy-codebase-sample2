using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/ottCategory/action")]
    public class OttCategoryController : ApiController
    {
        /// <summary>
        /// Retrieve the list of categories (hierarchical) and their associated channels
        /// </summary>
        /// <param name="id">Category Identifier</param>
        /// <param name="language">Language Code</param>
        /// <remarks></remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaOTTCategory Get(int id)
        {
            KalturaOTTCategory response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Category ID is illegal");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                response = ClientsManager.CatalogClient().GetCategory(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language, id);

                // if no response - return not found status 
                if (response == null || response.Id == 0)
                {
                    throw new NotFoundException();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }

    [RoutePrefix("_service/category/action")]
    [Obsolete]
    public class CategoryController : OttCategoryController
    {
    }
}