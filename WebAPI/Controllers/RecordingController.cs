using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
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
        [Throws(eResponseStatus.RecordingNotFound)]
        public KalturaRecording Get(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                // call client                
                response = ClientsManager.ConditionalAccessClient().GetRecord(groupId, domainId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /* 
        /// <summary>
        /// Return recording information and status for collection of program for a user.
        /// Specify per programs if it can be recorded or not.
        /// If program record request was already issued – return recording status
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInHousehold = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoHousehold = 2024,
        /// ServiceNotAllowed = 3003, NotEntitled = 3032, AccountCdvrNotEnabled = 3033, AccountCatchUpNotEnabled = 3034, ProgramCdvrNotEnabled = 3035,
        /// ProgramCatchUpNotEnabled = 3036, CatchUpBufferLimitation = 3037, ProgramNotInRecordingScheduleWindow = 3038, ExceededQuota = 3042,
        /// AccountSeriesRecordingNotEnabled = 3046, AlreadyRecordedAsSeriesOrSeason = 3047, InvalidAssetId = 4024</remarks>
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

                if (string.IsNullOrEmpty(filter.AssetIdIn))
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter ids cannot be empty");
                }          

                // call client                
                response = ClientsManager.ConditionalAccessClient().QueryRecords(groupId, userId, filter.getAssetIdIn());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
         **/

        /// <summary>
        /// Issue a record request for a program
        /// </summary>
        /// <param name="recording">Recording Object</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInHousehold = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoHousehold = 2024, ServiceNotAllowed = 3003, NotEntitled = 3032, AccountCdvrNotEnabled = 3033, AccountCatchUpNotEnabled = 3034,
        /// ProgramCdvrNotEnabled = 3035, ProgramCatchUpNotEnabled = 3036, CatchUpBufferLimitation = 3037, ProgramNotInRecordingScheduleWindow = 3038,
        /// ExceededQuota = 3042, AccountSeriesRecordingNotEnabled = 3046, AlreadyRecordedAsSeriesOrSeason = 3047, InvalidAssetId = 4024</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInHousehold)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.NotEntitled)]
        [Throws(eResponseStatus.AccountCdvrNotEnabled)]
        [Throws(eResponseStatus.AccountCatchUpNotEnabled)]
        [Throws(eResponseStatus.ProgramCdvrNotEnabled)]
        [Throws(eResponseStatus.ProgramCatchUpNotEnabled)]
        [Throws(eResponseStatus.CatchUpBufferLimitation)]
        [Throws(eResponseStatus.ProgramNotInRecordingScheduleWindow)]
        [Throws(eResponseStatus.ExceededQuota)]
        [Throws(eResponseStatus.AccountSeriesRecordingNotEnabled)]
        [Throws(eResponseStatus.AlreadyRecordedAsSeriesOrSeason)]
        [Throws(eResponseStatus.InvalidAssetId)]
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
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInHousehold = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoHousehold = 2024</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInHousehold)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
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
        /// <remarks>Possible status codes: BadRequest = 500003,UserNotInHousehold = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoHousehold = 2024, RecordingNotFound = 3039,RecordingStatusNotValid = 3043 </remarks>
        [Route("cancel"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInHousehold)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
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
        /// <remarks>Possible status codes: BadRequest = 500003,UserNotInHousehold = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoHousehold = 2024, RecordingNotFound = 3039,RecordingStatusNotValid = 3043 </remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInHousehold)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
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

        /// <summary>
        /// Protects an existing recording from the cleanup process for the defined protection period
        /// </summary>
        /// <param name="id">Recording identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInHousehold = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoHousehold = 2024,
        /// RecordingNotFound = 3039, RecordingStatusNotValid = 3043, HouseholdExceededProtectionQuota = 3044, AccountProtectRecordNotEnabled = 3045</remarks>     
        [Route("protect"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInHousehold)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.ExceededProtectionQuota)]
        [Throws(eResponseStatus.AccountProtectRecordNotEnabled)]
        public KalturaRecording Protect(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().ProtectRecord(groupId, userId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}