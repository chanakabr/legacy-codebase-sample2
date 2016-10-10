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
    [RoutePrefix("_service/configurationgroupdevice/action")]
    public class ConfigurationGroupDeviceController : ApiController
    {
        /// <summary>
        /// Returns device association by a given UDID
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
        /// Returns a list of device associations by a given group configuration ID.
        /// </summary>
        /// <param name="groupId">Group configuration ID</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001 </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        public KalturaConfigurationGroupDeviceListResponse List(string groupId, KalturaFilterPager pager = null)
        {
            KalturaConfigurationGroupDeviceListResponse response = null;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupId");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.GetConfigurationGroupDeviceList(partnerId, groupId, pager.getPageIndex(), pager.getPageSize());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Adds a new device association.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="udids"></param>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, IllegalPostData = 12002, NotExist = 12003 </remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.IllegalPostData)]
        [Throws(eResponseStatus.NotExist)]
        public KalturaConfigurationGroupDevice Add(string groupId, KalturaStringValueArray udids)
        {
            KalturaConfigurationGroupDevice response = null;

            try
            {
                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.AddConfigurationGroupDevice(partnerId, groupId, udids);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Delete a device association
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="udid"></param>
        /// <returns></returns>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001, NotExist = 12003 </remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        public bool Delete(string groupId, string udid)
        {
            bool response = false;

            try
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupId");

                if (string.IsNullOrWhiteSpace(udid))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.DeleteConfigurationGroupDevice(partnerId, groupId, udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}