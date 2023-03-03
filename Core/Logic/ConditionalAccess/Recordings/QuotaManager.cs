using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.Response;
using ApiObjects;
using ApiObjects.TimeShiftedTv;
using DAL;
using Phx.Lib.Log;
using System.Reflection;
using Synchronizer;
using Core.ConditionalAccess;
using System.Threading;
using ApiObjects.Base;
using TVinciShared;

namespace Core.Recordings
{
    public class QuotaManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 3;

        #region SingleTon

        private static object locker = new object();
        private static QuotaManager instance;

        public static QuotaManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new QuotaManager();
                        }
                    }
                }

                return instance;
            }
        }

        static QuotaManager()
        {
        }

        #endregion

        #region Public Methods

        public Status CheckQuotaByModel(int groupId, int quotaManagerModelId, long householdId, bool isAggregative,
            List<Recording> newRecordings, List<Recording> currentRecordings)
        {
            Status status = new Status((int)eResponseStatus.OK);
            bool shouldContinue = true;

            // Get model by Id/default of group
            QuotaManagementModel quotaManagerModel =
                ConditionalAccessDAL.GetQuotaManagementModel(groupId, quotaManagerModelId);

            // Check that we don't exceed amount of seconds

            int secondsLeft = quotaManagerModel.Minutes * 60;

            status = DeductRecordings(currentRecordings, groupId, ref shouldContinue, ref secondsLeft);

            // If there wasn't an error previously
            if (shouldContinue)
            {
                status = CheckQuotaByAvailableSeconds(groupId, householdId, secondsLeft, newRecordings, isAggregative);
            }

            return status;
        }

        public Status CheckQuotaByAvailableSeconds(int groupId, long householdId, int availableSeconds,
            List<Recording> newRecordings, bool isAggregative)
        {
            Status status = new Status((int)eResponseStatus.OK);

            int SecondsLeft = availableSeconds; //available Seconds

            // Now deduct the time of all the new/requested recordings
            foreach (var recording in newRecordings)
            {
                int currentEpgSeconds = 0;
                int initialSecondsLeft = SecondsLeft;
                currentEpgSeconds = GetRecordingDurationSeconds(groupId, recording);
                int tempSeconds = SecondsLeft - currentEpgSeconds;
                // Mark this, current-specific, recording as failed
                if (tempSeconds < 0)
                {
                    log.DebugFormat(
                        "Requested EPG exceeds domain's quota. EPG duration is {0} seconds and there are {1} seconds available.",
                        currentEpgSeconds, initialSecondsLeft);
                    recording.Status = new Status((int)eResponseStatus.ExceededQuota,
                        eResponseStatus.ExceededQuota.ToString());
                }

                // If we check each recording individually or not
                if (isAggregative)
                {
                    SecondsLeft = tempSeconds;
                }
            }

            return status;
        }

        public static int GetRecordingDurationSeconds(int groupId, Recording recording, bool decreaseDefaultPadding = true, long domainId = 0)
        {
            DateTime start = recording.EpgStartDate;
            DateTime end = recording.EpgEndDate;

            int seconds = Math.Abs((int)(end - start).TotalSeconds);

            TimeShiftedTvPartnerSettings accountSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);
            if (accountSettings != null && accountSettings.PersonalizedRecordingEnable == true)
            {
                var contetData = new ContextData(groupId) { DomainId = domainId };
                if (decreaseDefaultPadding && IsHasDefaultPadding(contetData, accountSettings, recording))
                {
                    return seconds;
                }

                if (recording.AbsoluteStartTime.HasValue && recording.AbsoluteEndTime.HasValue)
                {
                    seconds = PaddedRecordingsManager.Instance.GetImmediateRecordingTimeSpanSeconds(
                        recording.AbsoluteStartTime, recording.AbsoluteEndTime);
                }
                else
                {
                    if (recording.EndPadding.HasValue)
                        seconds += 60 * recording.EndPadding.Value;

                    if (recording.StartPadding.HasValue)
                        seconds += 60 * recording.StartPadding.Value;
                }
            }

            return seconds > 0 ? seconds : 0; 
        }

        private static bool IsHasDefaultPadding(ContextData contextData, TimeShiftedTvPartnerSettings accountSettings, Recording recording)
        {
            if (recording.AbsoluteEndTime.HasValue)
            {
                return false;
            }

            bool shouldRemoveStartPadding, shouldRemoveEndPadding;
            if (contextData.DomainId > 0)
            {
                var domainRecording =
                    PaddedRecordingsManager.Instance.GetHouseholdRecordingByRecordingId(contextData.GroupId, recording.Id, contextData.DomainId.Value);
                if (domainRecording != null && domainRecording.Id > 0)
                {
                    shouldRemoveStartPadding = !domainRecording.IsStartPaddingDefault;
                    shouldRemoveEndPadding = !domainRecording.IsEndPaddingDefault;   
                }
                else
                {
                    shouldRemoveStartPadding = false;
                    shouldRemoveEndPadding = false;
                }
            }
            else
            {
                var defaultStart = Utils.ConvertSecondsToMinutes((int)(accountSettings.PaddingBeforeProgramStarts ?? 0));
                var defaultEnd = Utils.ConvertSecondsToMinutes((int)(accountSettings.PaddingAfterProgramEnds ?? 0));    
                shouldRemoveStartPadding = defaultStart.Equals(recording.StartPadding ?? 0);
                shouldRemoveEndPadding = defaultEnd.Equals(recording.EndPadding ?? 0);
            }
            
            return shouldRemoveStartPadding && shouldRemoveEndPadding;
        }

        // public static long GetRecordingDurationWithPaddingSeconds(int groupId, Recording recording,
        //     TimeShiftedTvPartnerSettings accountSettings)
        // {
        //     var result = GetRecordingDurationSeconds(groupId, recording);
        //
        //     var isStartPaddingNeeded = !recording.AbsoluteStartTime.HasValue &&
        //                                (!recording.StartPadding.HasValue || recording.StartPadding.Value == 0);
        //     var isEndPaddingNeeded = !recording.AbsoluteStartTime.HasValue &&
        //                              (!recording.EndPadding.HasValue || recording.EndPadding.Value == 0);
        //
        //     if (isStartPaddingNeeded && accountSettings.PaddingBeforeProgramStarts.HasValue)
        //         result += (int)accountSettings.PaddingBeforeProgramStarts.Value;
        //     if (isEndPaddingNeeded && accountSettings.PaddingAfterProgramEnds.HasValue)
        //         result += (int)accountSettings.PaddingAfterProgramEnds.Value;
        //
        //     return result;
        // }

        public ApiObjects.TimeShiftedTv.DomainQuotaResponse GetDomainQuotaResponse(int groupId, long domainId)
        {
            ApiObjects.TimeShiftedTv.DomainQuotaResponse response = new DomainQuotaResponse();

            int defaultQuota = 0;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            if (domainQuota != null)
            {
                var used = GetDomainUsedQuota(groupId, domainId);
                response = new DomainQuotaResponse()
                {
                    Status = new Status((int)eResponseStatus.OK),
                    TotalQuota = domainQuota.Total,
                    AvailableQuota = Math.Max(0, domainQuota.Total - used),
                    Used = used
                };
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        public int GetDomainAvailableQuota(int groupId, long domainId, out int used)
        {
            int defaultQuota = 0;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            used = GetDomainUsedQuota(groupId, domainId);
            if (domainQuota.Total == -1)
            {
                return domainQuota.Total;
            }

            return domainQuota.Total - used;
        }

        [Obsolete("Not using cb document for used Quota")]
        public bool DecreaseDomainUsedQuota(int groupId, long domainId, int quotaToDecrease)
        {
            int defaultQuota = 0;
            quotaToDecrease = Math.Abs(quotaToDecrease);
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            if (domainQuota != null)
            {
                return RecordingsDAL.UpdateDomainUsedQuota(domainId, (-1) * quotaToDecrease, defaultQuota);
            }

            return false;
        }

        /// <summary>
        /// Set cb quota usage document for hh
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <param name="quotaToIncrease"></param>
        /// <param name="shouldForceIncrease">If true - decrease the quota to 0 if not enough quota</param>
        /// <returns></returns>
        public bool SetDomainUsedQuota(int groupId, long domainId, int quotaToIncrease,
            bool shouldForceIncrease = false)
        {
            int defaultQuota = 0;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            if (domainQuota != null)
            {
                return RecordingsDAL.UpdateDomainUsedQuota(domainId, quotaToIncrease, defaultQuota,
                    shouldForceIncrease);
            }

            return false;
        }

        public int GetDomainUsedQuota(int groupId, long domainId)
        {
            var response = 0;
            var recordingStatuses = new List<TstvRecordingStatus>()
            {
                TstvRecordingStatus.OK,
                TstvRecordingStatus.Recorded,
                TstvRecordingStatus.Recording,
                TstvRecordingStatus.Scheduled
            };

            var allRecords = Utils.GetDomainRecordingsByTstvRecordingStatuses(groupId, domainId, recordingStatuses);
            if (allRecords == null || allRecords.Count == 0)
            {
                return response;
            }

            response = allRecords.Values.Where(x => x.isExternalRecording == false)
                .Select(r => GetRecordingDurationSeconds(groupId, r, true, domainId)).Sum();

            return response;
        }

        internal Status CheckQuotaByTotalSeconds(int groupId, long householdId, int totalSeconds, bool isAggregative,
            List<Recording> newRecordings, List<Recording> currentRecordings)
        {
            Status status = new Status((int)eResponseStatus.OK);
            bool shouldContinue = true;
            HashSet<long> currentEPGIds = new HashSet<long>();
            int secondsLeft = totalSeconds;

            if (currentRecordings != null)
            {
                foreach (var recording in currentRecordings)
                {
                    currentEPGIds.Add(recording.EpgId);
                }
            }

            // Remove all recordings that already exist for the household
            newRecordings.RemoveAll(recording => currentEPGIds.Contains(recording.EpgId));

            status = DeductRecordings(currentRecordings, groupId, ref shouldContinue, ref secondsLeft);

            // If there wasn't an error previously
            if (shouldContinue)
            {
                status = CheckQuotaByAvailableSeconds(groupId, householdId, secondsLeft, newRecordings, isAggregative);
            }

            return status;
        }


        [Obsolete("Not using hh quota document")]
        public ApiObjects.Response.Status SetDomainTotalQuota(int groupId, long domainId, long totalQuota)
        {
            ApiObjects.Response.Status status =
                new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            int defaultQuota = 0;
            bool result = false;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            if (domainQuota != null)
            {
                domainQuota.Total = (int)totalQuota;
                result = RecordingsDAL.SetDomainQuota(domainId, domainQuota);
            }

            if (result)
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return status;
        }

        public List<Recording> HandleDomainAutoDelete(int groupId, long domainId, int recordingDuration,
            DomainRecordingStatus domainRecordingStatus = DomainRecordingStatus.Deleted)
        {
            List<Recording> deletedRecordings = null;
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();

            try
            {
                while (limitRetries > 0)
                {
                    deletedRecordings =
                        DeleteDomainOldestRecordings(groupId, domainId, recordingDuration, domainRecordingStatus);
                    if (deletedRecordings?.Count > 0)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(r.Next(50));
                        limitRetries--;
                    }
                }

                if (deletedRecordings == null) // fail to delete and free some quota 
                {
                    log.ErrorFormat("Failed HandleDomainAutoDelete groupID: {0}, domainID: {1}, recordingDuration: {2}",
                        groupId, domainId, recordingDuration);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "Failed HandleDomainAutoDelete groupID: {0}, domainID: {1}, recordingDuration: {2}, ex: {4}",
                    groupId, domainId, recordingDuration, ex);
            }

            return deletedRecordings;
        }

        public ApiObjects.Response.Status HandleDomainRecoveringRecording(int groupId, long domainId,
            int totalRecordingDuration)
        {
            ApiObjects.Response.Status bRes = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            List<long> domainRecordingIds = new List<long>();
            int recordingDuration = 0;
            int tempRecordingDuration = 0;
            try
            {
                // get all deletePending recordings related to domain sort by epgStartDate
                var accountSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);
                Dictionary<long, Recording> recordings = null;
                if (accountSettings != null && accountSettings.PersonalizedRecordingEnable == true)
                {
                    recordings = PaddedRecordingsManager.Instance.GetHouseholdRecordingsToRecover(groupId, domainId);
                }
                else
                {
                    recordings = Utils.GetDomainRecordingsToRecover(groupId, domainId);
                }

                if (recordings != null && recordings.Count > 0)
                {
                    foreach (KeyValuePair<long, Recording> recording in recordings)
                    {
                        recordingDuration =
                            (int)(recording.Value.EpgEndDate - recording.Value.EpgStartDate).TotalSeconds;
                        tempRecordingDuration += recordingDuration;

                        if (tempRecordingDuration > totalRecordingDuration)
                        {
                            tempRecordingDuration -= recordingDuration;
                            break;
                        }

                        domainRecordingIds.Add(recording.Key);
                    }

                    if (domainRecordingIds.Count > 0)
                    {
                        if (accountSettings != null && accountSettings.PersonalizedRecordingEnable == true)
                        {
                            PaddedRecordingsManager.Instance.UpdateHouseholdRecordingsStatus(groupId,
                                domainRecordingIds,
                                DomainRecordingStatus.OK.ToString());
                        }
                        else
                        {
                            // update all these domain recording ids to status OK  and update used quota
                            if (RecordingsDAL.RecoverDomainRecordings(domainRecordingIds, DomainRecordingStatus.OK))
                            {
                                bRes = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                            }
                        }
                    }
                }
                else
                {
                    log.DebugFormat("fount 0 domainRecordingIds to recover groupID: {0}, domainID: {1}", groupId,
                        domainId);
                }

                if (bRes.Code != (int)eResponseStatus.OK) // fail to recover recordings
                {
                    log.ErrorFormat(
                        "Failed HandleDominQuotaOvarge groupID: {0}, domainID: {1}, recordingDuration: {2}, status = {3}",
                        groupId, domainId, totalRecordingDuration, bRes.Message);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "Failed HandleDomainRecoveringRecording groupID: {0}, domainID: {1}, recordingDuration: {2}, status = {3}, ex: {4}",
                    groupId, domainId, totalRecordingDuration, bRes.Message, ex);
            }

            return bRes;
        }

        private List<Recording> DeleteDomainOldestRecordings(int groupId, long domainId, int newRecordingDuration,
            DomainRecordingStatus domainRecordingStatus = DomainRecordingStatus.Deleted)
        {
            List<Recording> deletedRecordings = null;
            try
            {
                List<TstvRecordingStatus> recordingStatuses = new List<TstvRecordingStatus>()
                    { TstvRecordingStatus.Recorded };
                Dictionary<long, Recording> domainRecordingIdToRecordingMap =
                    Utils.GetDomainRecordingsByTstvRecordingStatuses(groupId, domainId, recordingStatuses);
                if (domainRecordingIdToRecordingMap != null && domainRecordingIdToRecordingMap.Count > 0)
                {
                    var accountSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);
                    long quotaClearanceSeconds = 0;
                    long currentUtcTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                    var ordered = domainRecordingIdToRecordingMap.OrderBy(x => x.Value.ViewableUntilDate ?? 0)
                        .Where(rec => !IsRecordingProtected(rec.Value, currentUtcTime));

                    List<long> domainRecordingIds = new List<long>();

                    if (ordered != null)
                    {
                        // build list of domain recordings id - Least as needed   
                        Dictionary<long, Recording> recordings =
                            ordered.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
                        long recordingDuration = 0;

                        foreach (KeyValuePair<long, Recording> recording in recordings)
                        {
                            domainRecordingIds.Add(recording.Key);
                            if (deletedRecordings == null)
                            {
                                deletedRecordings = new List<Recording>();
                            }

                            deletedRecordings.Add(recording.Value);

                            recordingDuration =
                                GetRecordingDurationSeconds(groupId, recording.Value, false);
                            quotaClearanceSeconds += recordingDuration;

                            if (quotaClearanceSeconds >= newRecordingDuration)
                            {
                                break;
                            }
                        }
                    }

                    if (quotaClearanceSeconds >= newRecordingDuration)
                    {
                        // delete all and DecreaseDomainUsedQuota
                        if (domainRecordingIds != null && domainRecordingIds.Count > 0)
                        {
                            log.DebugFormat("try to deleted domainRecordingIds: {0}, QuotaOverageDuration : {1}",
                                string.Join(",", domainRecordingIds), quotaClearanceSeconds);

                            if (accountSettings != null && accountSettings.PersonalizedRecordingEnable == true)
                            {
                                PaddedRecordingsManager.Instance.UpdateHouseholdRecordingsStatus(groupId,
                                    domainRecordingIds,
                                    domainRecordingStatus.ToString());
                            }
                            else
                            {
                                if (!RecordingsDAL.DeleteDomainRecording(domainRecordingIds, domainRecordingStatus))
                                {
                                    deletedRecordings = null;
                                    log.ErrorFormat(
                                        "Fail in DeleteDomainOldestRecordings to perform delete domainRecordingID = {0}",
                                        string.Join(",", domainRecordingIds));
                                }
                            }
                        }
                    }
                    else
                    {
                        deletedRecordings = null;
                    }
                }
            }
            catch (Exception ex)
            {
                deletedRecordings = null;
                log.Error(string.Format("Error in 'DeleteDomainOldestRecordings' for domainId = {0}", domainId), ex);
            }

            return deletedRecordings;
        }

        private bool IsRecordingProtected(Recording recording, long currentUtcTime)
        {
            if (recording == null) return false;

            // check if record is protected - ignore it 
            return (!recording.ViewableUntilDate.HasValue || recording.ViewableUntilDate.Value == 0 ||
                    (recording.ProtectedUntilDate.HasValue && recording.ProtectedUntilDate.Value > currentUtcTime));
        }

        #endregion

        #region Private Methods

        private static Status DeductRecordings(List<Recording> recordings, int groupId, ref bool shouldContinue,
            ref int secondsLeft)
        {
            Status status = new Status((int)eResponseStatus.OK);

            if (recordings != null)
            {
                // Deduct seconds of EPGs that domain already previously recorded
                foreach (var recording in recordings)
                {
                    secondsLeft -= GetRecordingDurationSeconds(groupId, recording);

                    // Mark the entire operation as failure, something here is completely wrong
                    if (secondsLeft < 0)
                    {
                        status = new Status((int)eResponseStatus.ExceededQuota,
                            eResponseStatus.ExceededQuota.ToString());
                        shouldContinue = false;
                        break;
                    }
                }
            }

            return status;
        }

        private DomainQuota GetDomainQuota(int groupId, long domainId, ref int defaultQuota)
        {
            DomainQuota domainQuota;
            defaultQuota = ConditionalAccess.Utils.GetDomainDefaultQuota(groupId, domainId);

            if (!RecordingsDAL.GetDomainQuota(groupId, domainId, out domainQuota, defaultQuota))
            {
                return new DomainQuota(defaultQuota, 0, true);
            }

            return domainQuota;
        }

        #endregion
    }
}