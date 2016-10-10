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
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        public KalturaConfigurationGroup Get(string groupId)
        {
            KalturaConfigurationGroup response = null;
            int partnerId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrWhiteSpace(groupId))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupId");

            try
            {
                // call client               
                response = DMSClient.GetConfigurationGroup(partnerId, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

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

        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.IllegalPostData)]
        public KalturaConfigurationGroup Update(int groupId, KalturaConfigurationGroup configurationGroup) //TODO: Anat why need 2 args?
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

        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        public bool Delete(string groupId)
        {
            bool response = false;

            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupId");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.DeleteConfigurationGroup(partnerId, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}