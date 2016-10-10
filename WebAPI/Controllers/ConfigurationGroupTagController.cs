using ApiObjects.Response;
using System.Web.Http;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.DMS;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/configurationGroupTag/action")]
    public class ConfigurationGroupTagController : ApiController
    {
        /// <summary>
        /// Return the configuration group the tag is associated to
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, NotExist = 12003, PartnerMismatch = 12004</remarks>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        public KalturaConfigurationGroupTag Get(string tag)
        {
            KalturaConfigurationGroupTag response = null;
            int partnerId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrWhiteSpace(tag))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "tag");

            try
            {
                // call client               
                response = DMSClient.GetConfigurationGroupTag(partnerId, tag);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return list of tags for a configuration group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001</remarks>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        public KalturaConfigurationGroupTagListResponse List(string groupId)
        {
            KalturaConfigurationGroupTagListResponse response = null;

            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupId");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.GetConfigurationGroupTagList(partnerId, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Add a new tag to a configuration group. If this tag is already associated to another group, request fails
        /// </summary>
        /// <param name="configurationGroupTag"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001,  NotExist = 12003, AlreadyExist = 12008</remarks>        
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.AlreadyExist)]
        [Throws(eResponseStatus.NotExist)]
        public KalturaConfigurationGroupTag Add(KalturaConfigurationGroupTag configurationGroupTag)
        {
            KalturaConfigurationGroupTag response = null;

            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.AddConfigurationGroupTag(partnerId, configurationGroupTag);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Remove a tag association from configuration group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001,  NotExist = 12003</remarks>        
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        public bool Delete(string groupId, string tag)
        {
            bool response = false;

            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupId");

                if (string.IsNullOrWhiteSpace(tag))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "tag");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.DeleteConfigurationGroupTag(partnerId, groupId, tag);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}