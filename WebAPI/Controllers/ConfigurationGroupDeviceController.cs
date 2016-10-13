using ApiObjects.Response;
using System.Web.Http;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.DMS;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/configurationGroupDevice/action")]
    public class ConfigurationGroupDeviceController : ApiController
    {
        /// <summary>
        /// Return the configuration group to which a specific device is associated to
        /// </summary>
        /// <param name="udid">Device UDID</param>        
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, NotExist = 12003, PartnerMismatch = 12004</remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        public KalturaConfigurationGroupDevice Get(string udid)
        {
            KalturaConfigurationGroupDevice response = null;
            int partnerId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrWhiteSpace(udid))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");

            try
            {
                // call client               
                response = DMSClient.GetConfigurationGroupDevice(partnerId, udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return the list of associated devices for a given configuration group
        /// </summary>
        /// <param name="filter">Filter option for configuration group identifier</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001 </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        public KalturaConfigurationGroupDeviceListResponse List(KalturaConfigurationGroupDeviceFilter filter, KalturaFilterPager pager = null)
        {
            KalturaConfigurationGroupDeviceListResponse response = null;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(filter.ConfigurationGroupIdEqual))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "configurationGroupIdEqual");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.GetConfigurationGroupDeviceList(partnerId, filter.ConfigurationGroupIdEqual, pager.getPageIndex() + 1, pager.getPageSize());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Associate a collection of devices to a configuration group. If a device is already associated to another group – old association is replaced 
        /// </summary>
        /// <param name="configurationGroupDevice">Configuration group device</param>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, IllegalPostData = 12002, NotExist = 12003 </remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.IllegalPostData)]
        [Throws(eResponseStatus.NotExist)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public bool Add(KalturaConfigurationGroupDevice configurationGroupDevice)
        {
            bool response = false;

            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.AddConfigurationGroupDevice(partnerId, configurationGroupDevice.ConfigurationGroupId, configurationGroupDevice.Udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Remove a device association
        /// </summary>
        /// <param name="configurationGroupId">Configuration group identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, NotExist = 12003 </remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public bool Delete(string udid)
        {
            bool response = false;

            try
            {
                if (string.IsNullOrWhiteSpace(udid))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.DeleteConfigurationGroupDevice(partnerId, udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}