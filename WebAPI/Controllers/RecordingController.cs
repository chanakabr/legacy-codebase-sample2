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
        /// Returns recording object by internal identifier
        /// </summary>
        /// <param name="id">Recording identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, RecordingNotFound = 3040</remarks>     
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Get(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().GetRecording(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Query record options for a program
        /// </summary>
        /// <param name="assetId">Internal identifier of the asset</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoDomain = 2024,
        /// ServiceNotAllowed = 3003, NotEntitled = 3032, AccountCdvrNotEnabled = 3033, AccountCatchUpNotEnabled = 3034, ProgramCdvrNotEnabled = 3035,
        /// ProgramCatchUpNotEnabled = 3036, CatchUpBufferLimitation = 3037, ProgramNotInRecordingScheduleWindow = 3039, InvalidAssetId = 4024</remarks>
        [Route("getContext"), HttpPost]
        [ApiAuthorize]
        public List<KalturaRecording> GetContext(long[] assetIds)
        {
            List<KalturaRecording> response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().QueryRecords(groupId, userId, assetIds);
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
        /// <param name="recording">Recording Object</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoDomain = 2024, ServiceNotAllowed = 3003, NotEntitled = 3032, AccountCdvrNotEnabled = 3033, AccountCatchUpNotEnabled = 3034,
        /// ProgramCdvrNotEnabled = 3035, ProgramCatchUpNotEnabled = 3036, CatchUpBufferLimitation = 3037, CdvrAdapterProviderFail = 3038,
        /// ProgramNotInRecordingScheduleWindow = 3039, InvalidAssetId = 4024</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Add(KalturaRecording recording)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().Record(groupId, userId, recording.AssetId);
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
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <param name="pager">Page size and index</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoDomain = 2024</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaRecordingListResponse List(KalturaRecordingFilter filter, KalturaFilterPager pager = null)
        {
            KalturaRecordingListResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                if (pager == null)
                {
                    pager = new KalturaFilterPager();                    
                }

                if (filter == null)
                {
                    filter = new KalturaRecordingFilter();
                }

                if (!string.IsNullOrEmpty(filter.FilterExpression) && filter.FilterExpression.Length > 1024)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter too long");
                }

                // call client                
                response = ClientsManager.ConditionalAccessClient().SearchRecordings(groupId, userId, domainId, filter.StatusIn.Select(x => x.status).ToList(),
                                                                                     filter.FilterExpression, pager.PageIndex, pager.PageSize, filter.OrderBy, string.Empty);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}