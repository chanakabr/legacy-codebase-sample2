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
    [RoutePrefix("_service/report/action")]
    public class ReportController : ApiController
    {
        /// <summary>
        /// Return a device configuration retrieval log request for a specific device.
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
        public KalturaReport Get(string udid)
        {
            KalturaReport response = null;
            int partnerId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrWhiteSpace(udid))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");

            try
            {
                // call client               
                response = DMSClient.GetDeviceReport(partnerId, udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return device configurations retrieval log. Supports paging and can be filtered with the parameter "FromData".
        /// </summary>
        /// <param name="filter">Filter option for from date (sec)</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001 </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        public KalturaReportListResponse List(KalturaReportFilter filter, KalturaFilterPager pager = null)
        {
            KalturaReportListResponse response = null;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            try
            {
                if (filter.FromDateEqual < 0)
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "fromDate");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.GetDevicesReport(partnerId, filter.FromDateEqual, pager.getPageIndex() + 1, pager.getPageSize());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}