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
    [RoutePrefix("_service/recording/action")]
    public class RecordingController : ApiController
    {
        /// <summary>
        /// Returns recording object by internal identifier
        /// </summary>
        /// <param name="id">Recording identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, RecordingNotFound = 3039</remarks>     
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Get(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                // call client                
                response = ClientsManager.ConditionalAccessClient().GetRecording(groupId, domainId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Return recording information and status for collection of program for a user.
        /// Specify per programs if it can be recorded or not.
        /// If program record request was already issued – return recording status
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoDomain = 2024,
        /// ServiceNotAllowed = 3003, NotEntitled = 3032, AccountCdvrNotEnabled = 3033, AccountCatchUpNotEnabled = 3034, ProgramCdvrNotEnabled = 3035,
        /// ProgramCatchUpNotEnabled = 3036, CatchUpBufferLimitation = 3037, ProgramNotInRecordingScheduleWindow = 3038, ExceededQuota = 3042, InvalidAssetId = 4024</remarks>
        [Route("getContext"), HttpPost]
        [ApiAuthorize]
        [WebAPI.Managers.Schema.ValidationException(WebAPI.Managers.Schema.SchemaValidationType.ACTION_NAME)]
        public KalturaRecordingContextListResponse GetContext(KalturaRecordingContextFilter filter)
        {
            KalturaRecordingContextListResponse response = null;

            try
            {                        
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (filter == null)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
                }

                if (filter.AssetIdIn == null || filter.AssetIdIn.Count == 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter ids cannot be empty");
                }          

                // call client                
                response = ClientsManager.ConditionalAccessClient().QueryRecords(groupId, userId, filter.AssetIdIn.Select(x => x.value).ToArray());
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
        /// ProgramCdvrNotEnabled = 3035, ProgramCatchUpNotEnabled = 3036, CatchUpBufferLimitation = 3037, ProgramNotInRecordingScheduleWindow = 3038,
        /// ExceededQuota = 3042, InvalidAssetId = 4024</remarks>
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
        public KalturaRecordingListResponse List(KalturaRecordingFilter filter = null, KalturaFilterPager pager = null)
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
                    filter = new KalturaRecordingFilter() { StatusIn = string.Empty };
                    
                }

                if (!filter.OrderBy.HasValue)
                {
                    filter.OrderBy = (KalturaRecordingOrderBy)filter.GetDefaultOrderByValue();
                }

                if (!string.IsNullOrEmpty(filter.FilterExpression) && filter.FilterExpression.Length > 1024)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter too long");
                }

                // call client                
                response = ClientsManager.ConditionalAccessClient().SearchRecordings(groupId, userId, domainId, filter.ConvertStatusIn(), filter.FilterExpression,
                                                                                     pager.getPageIndex(), pager.PageSize, filter.OrderBy);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }


        /// <summary>        
        /// Cancel a previously requested recording. Cancel recording can be called for recording in status Scheduled or Recording Only 
        /// </summary>
        /// <param name="id">Recording identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003,UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoDomain = 2024, RecordingNotFound = 3039,RecordingStatusNotValid = 3043 </remarks>
        [Route("cancel"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Cancel(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                // call client                
                response = ClientsManager.ConditionalAccessClient().CancelRecord(groupId, userId, domainId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>        
        /// Delete one or more user recording(s). Delete recording can be called only for recordings in status Recorded
        /// </summary>
        /// <param name="id">Recording identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003,UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoDomain = 2024, RecordingNotFound = 3039,RecordingStatusNotValid = 3043 </remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Delete(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                // call client                
                response = ClientsManager.ConditionalAccessClient().DeleteRecord(groupId, userId, domainId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }



    }
}