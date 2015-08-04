using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/category/action")]
    public class CategoryController : ApiController
    {
        /// <summary>
        /// Returns category by category identifier        
        /// </summary>
        /// <param name="category_id">Category Identifier</param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="language">Language Code</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("get"), HttpPost]
        public KalturaCategory Get(string partner_id, int category_id, string language = null, string user_id = null, int household_id = 0)
        {
            KalturaCategory response = null;
            
            int groupId = int.Parse(partner_id);

            if (category_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "category_id cannot be 0");
            }

            try
            {
                response = ClientsManager.CatalogClient().GetCategory(groupId, user_id, household_id, language, category_id);

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
}