using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/Recording/action")]
    public class RecordingController : ApiController
    {
        /// <summary>
        /// Query record options for a program
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>The recording availability and status for for the requested program</returns>
        /// 
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Get(long epg_id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().QueryRecord(groupId, userId, epg_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Issue a record request for a program
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>The recording ID and status for options for the requested program</returns>
        /// 
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Add(long epg_id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().Record(groupId, userId, epg_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Return a list of recordings for the household with optional filter by status and KSQL.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>the</returns>
        /// 
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaRecordingListResponse List(KalturaRecordingInfoFilter filter, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null, KalturaFilterPager pager = null)
        {
            KalturaRecordingListResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                if (pager == null)
                    pager = new KalturaFilterPager();

                if (with == null)
                    with = new List<KalturaCatalogWithHolder>();

                if (filter == null)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
                }

                if (filter.RecordingStatuses == null || filter.RecordingStatuses.Count == 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter recording_statuses cannot be empty");
                }

                // call client                
                response.Objects = ClientsManager.ConditionalAccessClient().GetRecordings(groupId, userId, domainId, KalturaRecordingInfoFilter.KalturaCutWith.and, filter.RecordingStatuses, filter.filter_expression);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}