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
    [RoutePrefix("_service/configurationgrouptag/action")]
    public class ConfigurationGroupTagController : ApiController
    {
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