using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.Clients.Exceptions;
using WebAPI.Clients.Utils;
using WebAPI.Filters;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [RoutePrefix("catalog")]
    public class CatalogController : ApiController
    {
        /// <summary>
        /// Unified search across – VOD: Movies, TV Series/episodes, EPG content
        /// </summary>
        /// <param name="search_assets">The search asset request parameter</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("search"), HttpPost]
        public AssetInfoWrapper Post([FromBody]SearchAssets search_assets)
        {
            AssetInfoWrapper response = null;
            //TODO: remove later
            int groupId = 215;

            try
            {
                response = ClientsManager.CatalogClient().SearchAssets(groupId, string.Empty, string.Empty, 0, 
                search_assets.PageIndex, search_assets.PageSize, search_assets.Filter, search_assets.OrderBy, search_assets.FilterTypes, search_assets.With);
            }
            catch (ClientException ex)
            {
                if (ex.Code == (int)WebAPI.Models.StatusCode.BadRequest)
                {
                    throw new BadRequestException(ex.Code, ex.Message);
                }

                throw new InternalServerErrorException(ex.Code, ex.Message);
            }

            return response;
        }
    }
}