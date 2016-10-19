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

        /// <summary>
        /// Return a device configuration applicable for a specific device (UDID), app name, software version, platform and optionally a configuration group’s tag
        /// </summary>
        /// <param name="partnerId">Partner Id</param>
        /// <param name="applicationName">Application name</param>
        /// <param name="clientVersion">Client version</param>
        /// <param name="platform">platform</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="tag">Tag</param>
        /// <returns></returns>
        /// <remarks> Possible status codes: IllegalQueryParams = 12001, Registered = 12006, VersionNotFound = 12007</remarks>        
        [Route("serveByDevice"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.Registered)]
        [Throws(eResponseStatus.VersionNotFound)]
        public KalturaStringRenderer ServeByDevice(int partnerId, string applicationName, string clientVersion, string platform, string udid, string tag)
        {
            string response = null;

            if (partnerId <= 0)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "partnerId");

            if (string.IsNullOrWhiteSpace(applicationName))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "applicationName");

            if (string.IsNullOrWhiteSpace(clientVersion))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "clientVersion");

            if (string.IsNullOrWhiteSpace(platform))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "platform");

            if (string.IsNullOrWhiteSpace(udid))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "UDID");

            try
            {
                // call client               
                response = DMSClient.Serve(partnerId, applicationName, clientVersion, platform, udid, tag);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaStringRenderer(this.Configuration, response);
        }

        /// <summary>
        /// Return the device configuration
        /// </summary>
        /// <param name="id">Configuration identifier</param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, NotExist = 12003, PartnerMismatch = 12004</remarks>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        public KalturaConfiguration Get(string id)
        {
            KalturaConfiguration response = null;

            int partnerId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrWhiteSpace(id))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");

            try
            {
                // call client               
                response = DMSClient.GetConfiguration(partnerId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return a list of device configurations of a configuration group 
        /// </summary>
        /// <param name="filter">Filter option for configuration group id.</param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001</remarks>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        public KalturaConfigurationListResponse List(KalturaConfigurationFilter filter)
        {
            KalturaConfigurationListResponse response = null;

            if (string.IsNullOrWhiteSpace(filter.ConfigurationGroupIdEqual))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "configurationGroupIdEqual");

            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.GetConfigurationList(partnerId, filter.ConfigurationGroupIdEqual);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Add a new device configuration to a configuration group
        /// </summary>
        /// <param name="configuration">Device configuration</param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, IllegalPostData = 12002, AlreadyExist = 12008</remarks>        
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.IllegalPostData)]
        [Throws(eResponseStatus.AlreadyExist)]
        public KalturaConfiguration Add(KalturaConfiguration configuration)
        {
            KalturaConfiguration response = null;

            if (string.IsNullOrWhiteSpace(configuration.ConfigurationGroupId))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "configurationGroupId");

            if (string.IsNullOrWhiteSpace(configuration.AppName))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "appName");

            if (string.IsNullOrWhiteSpace(configuration.ClientVersion))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "clientVersion");


            if (string.IsNullOrWhiteSpace(configuration.Platform.ToString()))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "platform");

            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.AddConfiguration(partnerId, configuration);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Update device configuration
        /// </summary>
        /// <param name="id">Configuration identifier</param>
        /// <param name="configuration">configuration to update</param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, IllegalPostData = 12002, NotExist = 12003, PartnerMismatch = 12004, AlreadyExist = 12008</remarks>        
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.IllegalPostData)]
        [Throws(eResponseStatus.PartnerMismatch)]
        [Throws(eResponseStatus.AlreadyExist)]
        public KalturaConfiguration Update(string id, KalturaConfiguration configuration)
        {
            KalturaConfiguration response = null;

            if (string.IsNullOrWhiteSpace(configuration.ConfigurationGroupId))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "configurationGroupId");

            if (string.IsNullOrWhiteSpace(configuration.AppName))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "appName");

            if (string.IsNullOrWhiteSpace(configuration.ClientVersion))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "clientVersion");


            if (string.IsNullOrWhiteSpace(configuration.Platform.ToString()))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "platform");


            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client                        
                response = DMSClient.UpdateConfiguration(partnerId, id, configuration);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Delete a device configuration
        /// </summary>
        /// <param name="id">Configuration identifier</param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, NotExist = 12003, PartnerMismatch = 12004</remarks>        
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        public bool Delete(string id)
        {
            bool response = false;

            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.DeleteConfiguration(partnerId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}