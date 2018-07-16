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
    [Service("recording")]
    public class RecordingController : IKalturaController
    {
        /// <summary>
        /// Returns recording object by internal identifier
        /// </summary>
        /// <param name="id">Recording identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, RecordingNotFound = 3039</remarks>     
        [Action("get")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RecordingNotFound)]
        static public KalturaRecording Get(long id)
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
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoDomain = 2024,
        /// ServiceNotAllowed = 3003, NotEntitled = 3032, AccountCdvrNotEnabled = 3033, AccountCatchUpNotEnabled = 3034, ProgramCdvrNotEnabled = 3035,
        /// ProgramCatchUpNotEnabled = 3036, CatchUpBufferLimitation = 3037, ProgramNotInRecordingScheduleWindow = 3038, ExceededQuota = 3042,
        /// AccountSeriesRecordingNotEnabled = 3046, AlreadyRecordedAsSeriesOrSeason = 3047, InvalidAssetId = 4024</remarks>
        [Action("getContext")]
        [ApiAuthorize]
        [WebAPI.Managers.Schema.ValidationException(WebAPI.Managers.Schema.SchemaValidationType.ACTION_NAME)]
        static public KalturaRecordingContextListResponse GetContext(KalturaRecordingContextFilter filter)
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
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoDomain = 2024, ServiceNotAllowed = 3003, NotEntitled = 3032, AccountCdvrNotEnabled = 3033, AccountCatchUpNotEnabled = 3034,
        /// ProgramCdvrNotEnabled = 3035, ProgramCatchUpNotEnabled = 3036, CatchUpBufferLimitation = 3037, ProgramNotInRecordingScheduleWindow = 3038,
        /// ExceededQuota = 3042, AlreadyRecordedAsSeriesOrSeason = 3047, InvalidAssetId = 4024</remarks>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.NotEntitled)]
        [Throws(eResponseStatus.AccountCdvrNotEnabled)]
        [Throws(eResponseStatus.AccountCatchUpNotEnabled)]
        [Throws(eResponseStatus.ProgramCdvrNotEnabled)]
        [Throws(eResponseStatus.ProgramCatchUpNotEnabled)]
        [Throws(eResponseStatus.CatchUpBufferLimitation)]
        [Throws(eResponseStatus.ProgramNotInRecordingScheduleWindow)]
        [Throws(eResponseStatus.ExceededQuota)]        
        [Throws(eResponseStatus.AlreadyRecordedAsSeriesOrSeason)]
        [Throws(eResponseStatus.InvalidAssetId)]
        static public KalturaRecording Add(KalturaRecording recording)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();
                Type kalturaRecording = typeof(KalturaRecording);
                Type kalturaExternalRecording = typeof(KalturaExternalRecording);
                if (kalturaExternalRecording.IsAssignableFrom(recording.GetType()))
                // external recording implementation
                {
                    KalturaExternalRecording externalRecording = recording as KalturaExternalRecording;
                    if (string.IsNullOrEmpty(externalRecording.ExternalId))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
                    }

                    response = ClientsManager.ConditionalAccessClient().AddExternalRecording(groupId, externalRecording, userId);
                }
                else
                // regular recording implementation
                {
                    response = ClientsManager.ConditionalAccessClient().Record(groupId, userId.ToString(), recording.AssetId);
                }
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
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        static public KalturaRecordingListResponse List(KalturaRecordingFilter filter = null, KalturaFilterPager pager = null)
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

                filter.Validate();

                // call client                
                response = ClientsManager.ConditionalAccessClient().SearchRecordings(groupId, userId, domainId, filter.ConvertStatusIn(), filter.Ksql, filter.GetExternalRecordingIds(),
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
        [Action("cancel")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        static public KalturaRecording Cancel(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().CancelRecord(groupId, userId, id);
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
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        static public KalturaRecording Delete(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().DeleteRecord(groupId, userId, id);
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
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoDomain = 2024,
        /// RecordingNotFound = 3039, RecordingStatusNotValid = 3043, HouseholdExceededProtectionQuota = 3044, AccountProtectRecordNotEnabled = 3045</remarks>     
        [Action("protect")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.ExceededProtectionQuota)]
        [Throws(eResponseStatus.AccountProtectRecordNotEnabled)]
        static public KalturaRecording Protect(long id)
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