using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.DMS;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    public class ReportController
    {
        /// <summary>
        /// Gets a single device information by a given UDID
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
        public KalturaDevice Get(string udid)
        {
            KalturaDevice response = null;
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
        /// Return a list of device information. Supports paging and can be filtered with the parameter "FromData".
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="pager">Page size and index</param>
        /// <remarks> Possible status codes: Forbidden = 12000, IllegalQueryParams = 12001 </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.Forbidden)]
        [Throws(eResponseStatus.IllegalQueryParams)]
        public KalturaDeviceListResponse List(long fromDate, KalturaFilterPager pager = null)
        {
            KalturaDeviceListResponse response = null;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            try
            {
                if (fromDate < 0)
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "fromDate");

                int partnerId = KS.GetFromRequest().GroupId;

                // call client        
                response = DMSClient.GetDevicesReport(partnerId, fromDate, pager.getPageIndex(), pager.getPageSize());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}