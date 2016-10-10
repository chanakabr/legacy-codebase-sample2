using ApiObjects.Response;
using System.Web.Http;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.DMS;
using WebAPI.Models.Renderers;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/configuration/action")]
    public class ConfigurationController : ApiController
    {
        [Route("serve"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.Registered)]
        [Throws(eResponseStatus.VersionNotFound)]
        public KalturaStringRenderer Serve(string applicationName, string configurationVersion, string platform, string udid, string tag)
        {
            KalturaStringRenderer response = null;

            int partnerId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrWhiteSpace(applicationName))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "applicationName");

            if (string.IsNullOrWhiteSpace(configurationVersion))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "configurationVersion");

            if (string.IsNullOrWhiteSpace(platform))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "platform");

            if (string.IsNullOrWhiteSpace(udid))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "UDID");

            try
            {
                // call client               
                response = DMSClient.GetConfiguration(partnerId, applicationName, configurationVersion, platform, udid, tag);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}