using ApiObjects.Response;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.DMS;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("report")]
    public class ReportController : IKalturaController
    {
        /// <summary>
        /// Return a device configuration retrieval log request for a specific device.
        /// </summary>
        /// <param name="udid">Device UDID</param>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.PartnerMismatch)]
        [Throws(StatusCode.MissingConfiguration)]
        static public KalturaReport Get(string udid)
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
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        [Throws(StatusCode.MissingConfiguration)]
        static public KalturaReportListResponse List(KalturaReportFilter filter, KalturaFilterPager pager = null)
        {
            KalturaReportListResponse response = null;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            try
            {
                KalturaDeviceReportFilter deviceReportFilter = filter as KalturaDeviceReportFilter;
                if (deviceReportFilter.LastAccessDateGreaterThanOrEqual < 0)
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "lastAccessDateGreaterThanOrEqual");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.GetDevicesReport(partnerId, deviceReportFilter.LastAccessDateGreaterThanOrEqual, pager.GetRealPageIndex() + 1, pager.PageSize.Value);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}