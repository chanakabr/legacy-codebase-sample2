using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Catalog.Services;
using ApiObjects.Base;
using ApiObjects.TimeShiftedTv;
using AutoMapper;
using Core.Recordings;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
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
        public static KalturaRecording Get(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS();
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
        /// ExceededQuota = 3042, AlreadyRecordedAsSeriesOrSeason = 3047, InvalidAssetId = 4024, InvalidParameters = 7010</remarks>
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
        [Throws(eResponseStatus.InvalidParameters)]
        [Throws(eResponseStatus.CanOnlyAddRecordingBeforeRecordingStart)]
        [Throws(eResponseStatus.RecordingExceededConcurrency)]
        [Throws(StatusCode.ArgumentCannotBeEmpty)]
        public static KalturaRecording Add(KalturaRecording recording)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();
                if (recording is KalturaExternalRecording)
                    // external recording implementation
                {
                    KalturaExternalRecording externalRecording = recording as KalturaExternalRecording;
                    if (string.IsNullOrEmpty(externalRecording.ExternalId))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
                    }

                    response = ClientsManager.ConditionalAccessClient()
                        .AddExternalRecording(groupId, externalRecording, userId);
                }
                else
                    // regular recording implementation
                {
                    recording.ValidateForAdd(groupId);
                    switch (recording)
                    {
                        case KalturaPaddedRecording rec:
                            response = ClientsManager.ConditionalAccessClient()
                                .Record(groupId, userId.ToString(), rec.AssetId, rec.StartPadding, rec.EndPadding,
                                    true);
                            break; 
                        case KalturaRecording rec:
                            response = ClientsManager.ConditionalAccessClient()
                                .Record(groupId, userId.ToString(), rec.AssetId, 0, 0);
                            break;
                        default:
                            throw new NotImplementedException($"Add for {recording.objectType} is not implemented");
                    }
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
        public static KalturaRecordingListResponse List(KalturaRecordingFilter filter = null,
            KalturaFilterPager pager = null)
        {
            KalturaRecordingListResponse response = null;

            try
            {
                if (pager == null)
                {
                    pager = new KalturaFilterPager();
                }

                if (filter == null)
                {
                    filter = new KalturaRecordingFilter() { StatusIn = string.Empty };
                }

                var contextData = KS.GetContextData();
                response = filter.SearchRecordings(contextData, pager);
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
        [Throws(eResponseStatus.CanOnlyUpdatePaddingBeforeRecordingBeforeRecordingStart)]
        public static KalturaRecording Cancel(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client
                var timeShiftedSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
                if (timeShiftedSettings.PersonalizedRecordingEnable == true)
                {
                    var canceledRecording = PaddedRecordingsManager.Instance.CancelHouseholdRecordings(groupId, id, userId);

                    if (canceledRecording == null)
                    {
                        throw new ClientException(StatusCode.Error);
                    }

                    if (!canceledRecording.IsOkStatusCode())
                    {
                        throw new ClientException(canceledRecording.Status);
                    }
                    
                    if (canceledRecording.Object.AbsoluteStartTime.HasValue)
                    {
                        response = Mapper.Map<KalturaImmediateRecording>(canceledRecording.Object);
                    }
                    else
                    {
                        response = Mapper.Map<KalturaPaddedRecording>(canceledRecording.Object);
                    }
                }
                else
                {
                    response = ClientsManager.ConditionalAccessClient().CancelRecord(groupId, userId, id);    
                }
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
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.CanOnlyDeleteRecordingAfterRecordingEnd)]
        static public KalturaRecording Delete(long id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client     
                var timeShiftedSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
                if (timeShiftedSettings.PersonalizedRecordingEnable == true)
                {
                    var deletedRecording = PaddedRecordingsManager.Instance.DeleteHouseholdRecordings(groupId, id, userId);
                    if (deletedRecording == null)
                    {
                        throw new ClientException(StatusCode.Error);
                    }

                    if (!deletedRecording.IsOkStatusCode())
                    {
                        throw new ClientException(deletedRecording.Status);
                    }
                    
                    if (deletedRecording.Object.AbsoluteStartTime.HasValue)
                    {
                        response = Mapper.Map<KalturaImmediateRecording>(deletedRecording.Object);
                    }
                    else
                    {
                        response = Mapper.Map<KalturaPaddedRecording>(deletedRecording.Object);
                    }
                }
                else
                {
                    response = ClientsManager.ConditionalAccessClient().DeleteRecord(groupId, userId, id);
                }           
               
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete list of user's recordings. Recording can be deleted only in status Recorded.
        /// Possible error codes for each recording: RecordingNotFound = 3039, RecordingStatusNotValid = 3043, Error = 1
        /// </summary>
        /// <param name="recordingIds">Recording identifiers. Up to 40 private copies and up to 100 shared copies can be deleted withing a call.</param>
        /// <returns>List of recordings with result of action in status.</returns>
        [Action("bulkdelete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.RecordingIdsExceededLimit)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static List<KalturaActionResult> BulkDelete(string recordingIds)
        {
            List<KalturaActionResult> response = null;
            try
            {
                if (string.IsNullOrEmpty(recordingIds))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(recordingIds));
                }

                var ids = new HashSet<long>();
                foreach (var value in recordingIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (long.TryParse(value, out var recordingId))
                    {
                        ids.Add(recordingId);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, nameof(recordingIds));
                    }
                }

                var groupId = KS.GetFromRequest().GroupId;
                var userId = Utils.Utils.GetUserIdFromKs();

                response = ClientsManager.ConditionalAccessClient().DeleteRecordings(groupId, userId, ids.ToArray());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Deprecated, please use recording.update instead
        /// Protects an existing recording from the cleanup process for the defined protection period
        /// </summary>
        /// <param name="id">Recording identifier</param>
        /// <returns></returns>
        [Action("protect")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.RecordingFailed)]
        [Throws(eResponseStatus.InvalidParameters)]
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

        /// <summary>
        /// Update an existing recording with is protected field
        /// </summary>
        /// <param name="recording">recording to update</param>
        /// <param name="id">recording identifier</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.ExceededProtectionQuota)]
        [Throws(eResponseStatus.RecordingFailed)]
        [Throws(eResponseStatus.AccountProtectRecordNotEnabled)]
        [Throws(eResponseStatus.InvalidParameters)]
        [Throws(eResponseStatus.CanOnlyUpdatePaddingAfterRecordingBeforeRecordingEnd)]
        [Throws(eResponseStatus.CanOnlyUpdatePaddingBeforeRecordingBeforeRecordingStart)]
        public static KalturaRecording Update(long id, KalturaRecording recording)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                recording.ValidateForUpdate(groupId);

                // call client
                switch (recording)
                {
                    case KalturaPaddedRecording rec:
                        response = ClientsManager.ConditionalAccessClient()
                            .UpdateRecording(groupId, userId, id, rec);
                        break; //TODO - Separate logic to 2 different flows?
                     case KalturaImmediateRecording rec:
                            response = ClientsManager.ConditionalAccessClient()
                                .UpdateRecording(groupId, userId, id, rec);
                                         break;
                    case KalturaRecording rec:
                        response = ClientsManager.ConditionalAccessClient().UpdateRecording(groupId, userId, id, rec);
                        break;
                    default: throw new NotImplementedException($"Update for {recording.objectType} is not implemented");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Stop ongoing household recording
        /// </summary>
        /// <param name="assetId">asset identifier</param>
        /// <param name="id">household recording identifier</param>
        /// <returns></returns>
        [Action("stop")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.RecordingFailed)]
        [Throws(eResponseStatus.InvalidParameters)]
        [Throws(eResponseStatus.NotAllowed)]
        public static KalturaRecording Stop(long assetId, long id)
        {
            KalturaRecording response = null;

            try
            {
                var groupId = KS.GetFromRequest().GroupId;
                var userId = Utils.Utils.GetUserIdFromKs();
                var domainId = HouseholdUtils.GetHouseholdIDByKS();
                var ctx = new ContextData(groupId) { UserId = userId, DomainId = domainId };

                var timeShiftedSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
                if (timeShiftedSettings.PersonalizedRecordingEnable == true)
                {
                    Func<GenericResponse<Recording>> stopRecordingFunc = () =>
                        PaddedRecordingsManager.Instance.StopRecord(ctx, assetId, id);

                    response = ClientUtils.GetResponseFromWS<KalturaImmediateRecording, Recording>(stopRecordingFunc);
                }
                else
                {
                    throw new ClientException((int)eResponseStatus.NotAllowed, "Stop action is not allowed");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        
        /// <summary>
        /// Immediate Record
        /// </summary>
        /// <param name="assetId">asset identifier</param>
        /// <param name="endPadding">end padding offset</param>
        /// <returns></returns>
        [Action("immediateRecord")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.RecordingFailed)]
        [Throws(eResponseStatus.InvalidParameters)]
        [Throws(eResponseStatus.RecordingExceededConcurrency)]
        [Throws(eResponseStatus.NotAllowed)]
        public static KalturaImmediateRecording ImmediateRecord(long assetId, int? endPadding = null)
        {
            KalturaImmediateRecording response = null;

            try
            {
                var groupId = KS.GetFromRequest().GroupId;
                var domainId = HouseholdUtils.GetHouseholdIDByKS();
                var userId = Utils.Utils.GetUserIdFromKs();

                var timeShiftedSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
                if (timeShiftedSettings.PersonalizedRecordingEnable == true)
                {
                    Func<GenericResponse<Recording>> immediateRecordingFunc = () =>
                        PaddedRecordingsManager.Instance.ImmediateRecord(groupId, userId, domainId, assetId, endPadding);

                    response = ClientUtils.GetResponseFromWS<KalturaImmediateRecording, Recording>(immediateRecordingFunc);
                }
                else
                {
                    throw new ClientException((int)eResponseStatus.NotAllowed, "PersonalizedRecording isn't Enabled");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
             
            return response;
        }
    }
}