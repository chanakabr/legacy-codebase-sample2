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
    [RoutePrefix("_service/configurationGroup/action")]
    public class ConfigurationGroupController : ApiController
    {
        /// <summary>
        /// Return the configuration group details, including group identifiers, tags, and number of associated devices, and list of device configuration
        /// </summary>
        /// <param name="configurationGroupId"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, NotExist = 12003, PartnerMismatch = 12004</remarks>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        public KalturaConfigurationGroup Get(string configurationGroupId)
        {
            KalturaConfigurationGroup response = null;
            int partnerId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrWhiteSpace(configurationGroupId))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "configurationGroupId");

            try
            {
                // call client               
                response = DMSClient.GetConfigurationGroup(partnerId, configurationGroupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return the list of configuration groups
        /// </summary>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001</remarks>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        public KalturaConfigurationGroupListResponse List()
        {
            KalturaConfigurationGroupListResponse response = null;

            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.GetConfigurationGroupList(partnerId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Add a new configuration group
        /// </summary>
        /// <param name="configurationGroup"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, IllegalPostData = 12002</remarks>        
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.IllegalPostData)]
        public KalturaConfigurationGroup Add(KalturaConfigurationGroup configurationGroup)
        {
            KalturaConfigurationGroup response = null;

            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.AddConfigurationGroup(partnerId, configurationGroup);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Update configuration group name
        /// </summary>
        /// <param name="configurationGroup"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, IllegalPostData = 12002, NotExist = 12003</remarks>        
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.IllegalPostData)]
        public KalturaConfigurationGroup Update(string id, KalturaConfigurationGroup configurationGroup) 
        {
            KalturaConfigurationGroup response = null;

            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                
                response = DMSClient.UpdateConfigurationGroup(partnerId, configurationGroup);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Remove a configuration group, including its tags, device configurations and devices associations
        /// </summary>
        /// <param name="configurationGroupId"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, NotExist = 12003, PartnerMismatch = 12004</remarks>        
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        public bool Delete(string configurationGroupId)
        {
            bool response = false;

            try
            {
                if (string.IsNullOrWhiteSpace(configurationGroupId))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupId");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.DeleteConfigurationGroup(partnerId, configurationGroupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}