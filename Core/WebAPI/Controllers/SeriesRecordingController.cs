using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("seriesRecording")]
    public class SeriesRecordingController : IKalturaController
    {

        /// <summary>        
        /// Cancel a previously requested series recording. Cancel series recording can be called for recording in status Scheduled or Recording Only 
        /// </summary>
        /// <param name="id">Series Recording identifier</param>       
        /// <returns></returns>
        [Action("cancel")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.SeriesRecordingNotFound)]
        static public KalturaSeriesRecording Cancel(long id)//, long epgId, long seasonNumber
        {
            KalturaSeriesRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS();
                response = ClientsManager.ConditionalAccessClient().CancelSeriesRecord(groupId, userId, domainId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>        
        /// Cancel EPG recording that was recorded as part of series
        /// </summary>
        /// <param name="id">Series Recording identifier</param>
        /// <param name="epgId">epg program identifier</param>
        /// <returns></returns>
        [Action("cancelByEpgId")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("epgId", MinLong = 1)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.SeriesRecordingNotFound)]
        [Throws(eResponseStatus.EpgIdNotPartOfSeries)]
        static public KalturaSeriesRecording CancelByEpgId(long id, long epgId)
        {
            KalturaSeriesRecording response = null;
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS();

                // call client                
                response = ClientsManager.ConditionalAccessClient().CancelSeriesRecord(groupId, userId, domainId, id, epgId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>        
        /// Cancel Season recording epgs that was recorded as part of series
        /// </summary>
        /// <param name="id">Series Recording identifier</param>
        /// <param name="seasonNumber">Season Number</param>
        /// <returns></returns>
        [Action("cancelBySeasonNumber")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("seasonNumber", MinLong = 1)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.SeriesRecordingNotFound)]
        [Throws(eResponseStatus.SeasonNumberNotMatch)]
        static public KalturaSeriesRecording CancelBySeasonNumber(long id, long seasonNumber)
        {
            KalturaSeriesRecording response = null;
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS();

                // call client                
                response = ClientsManager.ConditionalAccessClient().CancelSeriesRecord(groupId, userId, domainId, id, 0, seasonNumber);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }


        /// <summary>        
        /// Delete series recording(s). Delete series recording can be called recordings in any status
        /// </summary>
        /// <param name="id">Series Recording identifier</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.SeriesRecordingNotFound)]
        static public KalturaSeriesRecording Delete(long id)
        {
            KalturaSeriesRecording response = null;
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS();
                // call client                
                response = ClientsManager.ConditionalAccessClient().DeleteSeriesRecord(groupId, userId, domainId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>        
        /// Delete Season recording epgs that was recorded as part of series
        /// </summary>
        /// <param name="id">Series Recording identifier</param>
        /// <param name="seasonNumber">Season Number</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003,UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoDomain = 2024, RecordingNotFound = 3039,RecordingStatusNotValid = 3043, SeriesRecordingNotFound= 3048, SeasonNumberNotMatch = 3052  </remarks>
        [Action("deleteBySeasonNumber")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("seasonNumber", MinInteger = 1)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.SeriesRecordingNotFound)]
        [Throws(eResponseStatus.SeasonNumberNotMatch)]
        static public KalturaSeriesRecording DeleteBySeasonNumber(long id, int seasonNumber)
        {
            KalturaSeriesRecording response = null;
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS();

                // call client                
                response = ClientsManager.ConditionalAccessClient().DeleteSeriesRecord(groupId, userId, domainId, id, 0, seasonNumber);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Return a list of series recordings for the household with optional filter by status and KSQL.
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result - support order by only - START_DATE_ASC, START_DATE_DESC, ID_ASC,ID_DESC,SERIES_ID_ASC, SERIES_ID_DESC</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoDomain = 2024</remarks>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.SeriesRecordingNotFound)]
        static public KalturaSeriesRecordingListResponse List(KalturaSeriesRecordingFilter filter = null)
        {
            KalturaSeriesRecordingListResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS();

                if (filter == null)
                {
                    filter = new KalturaSeriesRecordingFilter();
                }

                // call client                
                var cloudFilter = filter as KalturaCloudSeriesRecordingFilter;
                if (cloudFilter == null)
                {
                    response = ClientsManager.ConditionalAccessClient().GetFollowSeries(groupId, userId, domainId, filter.OrderBy);
                }
                else
                {
                    Dictionary<string, string> adapterData = null;
                    if (cloudFilter.AdapterData != null)
                    {
                        adapterData =
                            cloudFilter.AdapterData.ToDictionary(x => x.Key.ToLower(), x => x.Value.value.ToLowerOrNull());
                    }


                    response = ClientsManager.ConditionalAccessClient().SearchCloudSeriesRecordings(groupId, userId, domainId, adapterData);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }


        /// <summary>
        /// Issue a record request for a complete season or series
        /// </summary>
        /// <param name="recording">SeriesRecording Object</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001,
        /// UserWithNoDomain = 2024, ServiceNotAllowed = 3003, NotEntitled = 3032, AccountCdvrNotEnabled = 3033, ProgramCdvrNotEnabled = 3035,
        /// AccountSeriesRecordingNotEnabled = 3046, AlreadyRecordedAsSeriesOrSeason = 3047, InvalidAssetId = 4024</remarks>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.NotEntitled)]
        [Throws(eResponseStatus.AccountCdvrNotEnabled)]
        [Throws(eResponseStatus.ProgramCdvrNotEnabled)]
        [Throws(eResponseStatus.AccountSeriesRecordingNotEnabled)]
        [Throws(eResponseStatus.AlreadyRecordedAsSeriesOrSeason)]
        [Throws(eResponseStatus.InvalidAssetId)]
        [Throws(eResponseStatus.ProgramNotInRecordingScheduleWindow)]
        [Throws(eResponseStatus.AccountCatchUpNotEnabled)]
        [Throws(eResponseStatus.ProgramCatchUpNotEnabled)]
        [Throws(eResponseStatus.CatchUpBufferLimitation)]
        [Throws(eResponseStatus.ExceededQuota)]
        static public KalturaSeriesRecording Add(KalturaSeriesRecording recording)
        {
            KalturaSeriesRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                // validate recording type
                if (recording.Type == KalturaRecordingType.SINGLE)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSeriesRecording.type", "KalturaRecordingType.SINGLE");
                }

                if (recording.SeriesRecordingOption != null)
                {
                    recording.SeriesRecordingOption.Validate();
                }

                // call client
                response = ClientsManager.ConditionalAccessClient().RecordSeasonOrSeries(groupId, userId, recording.EpgId, recording.Type, recording.SeriesRecordingOption);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Enable EPG recording that was canceled as part of series
        /// </summary>
        /// <param name="epgId">EPG program identifies</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: UserSuspended = 2001, UserWithNoDomain = 2024,
        /// RecordingNotFound = 3039, RecordingStatusNotValid = 3043, SeriesRecordingNotFound = 3048, EpgIdNotPartOfSeries = 3049.</remarks>
        [Action("rebookCanceledByEpgId")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("epgId", MinLong = 1)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.SeriesRecordingNotFound)]
        [Throws(eResponseStatus.EpgIdNotPartOfSeries)]
        public static KalturaSeriesRecording RebookCanceledByEpgId(long epgId)
        {
            KalturaSeriesRecording response = null;
            try
            {
                var groupId = KS.GetFromRequest().GroupId;
                var userId = Utils.Utils.GetUserIdFromKs();
                var domainId = HouseholdUtils.GetHouseholdIDByKS();

                response = ClientsManager.ConditionalAccessClient().RebookCanceledRecordByEpgId(groupId, userId, domainId, epgId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}