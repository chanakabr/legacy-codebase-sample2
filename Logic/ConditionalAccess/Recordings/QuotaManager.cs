using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.Response;
using ApiObjects;
using ApiObjects.TimeShiftedTv;
using DAL;
using KLogMonitor;
using System.Reflection;
using Synchronizer;
using Core.ConditionalAccess;
using System.Threading;

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
            QuotaManagementModel quotaManagerModel = ConditionalAccessDAL.GetQuotaManagementModel(groupId, quotaManagerModelId);

            // Check that we don't exceed amount of seconds

            int secondsLeft = quotaManagerModel.Minutes * 60;

            status = DeductRecordings(currentRecordings, ref shouldContinue, ref secondsLeft);

            // If there wasn't an error previously
            if (shouldContinue)
            {
                status = CheckQuotaByAvailableSeconds(groupId, householdId, secondsLeft, newRecordings, isAggregative);
            }

            return status;
        }

        public Status CheckQuotaByAvailableSeconds(int groupId, long householdId, int availableSeconds, List<Recording> newRecordings, bool isAggregative)
        {
            Status status = new Status((int)eResponseStatus.OK);

            int SecondsLeft = availableSeconds;

            // Now deduct the time of all the new/requested recordings
            foreach (var recording in newRecordings)
            {
                int currentEpgSeconds = 0;
                int initialSecondsLeft = SecondsLeft;
                TimeSpan span = recording.EpgEndDate - recording.EpgStartDate;
                currentEpgSeconds = (int)span.TotalSeconds;
                int tempSeconds = SecondsLeft - currentEpgSeconds;
                // Mark this, current-specific, recording as failed
                if (tempSeconds < 0)
                {
                    log.DebugFormat("Requested EPG exceeds domain's quota. EPG duration is {0} seconds and there are {1} seconds available.", currentEpgSeconds, initialSecondsLeft);
                    recording.Status = new Status((int)eResponseStatus.ExceededQuota, eResponseStatus.ExceededQuota.ToString());
                }

                // If we check each recording individually or not
                if (isAggregative)
                {
                    SecondsLeft = tempSeconds;
                }
            }

            return status;
        }

        public ApiObjects.TimeShiftedTv.DomainQuotaResponse GetDomainQuotaResponse(int groupId, long domainId)
        {
            ApiObjects.TimeShiftedTv.DomainQuotaResponse response = new DomainQuotaResponse();

            int defaultQuota = 0;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            if (domainQuota != null)
            {
                response = new DomainQuotaResponse()
                {
                    Status = new Status((int)eResponseStatus.OK),
                    TotalQuota = domainQuota.Total,
                    AvailableQuota = Math.Max(0, domainQuota.Total - domainQuota.Used)
                };
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }
         
        public int GetDomainAvailableQuota(int groupId, long domainId)
        {
            int defaultQuota = 0;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            return domainQuota.Total - domainQuota.Used;
        }

        public bool DecreaseDomainUsedQuota(int groupId, long domainId, int quotaToDecrease)
        {
            int defaultQuota = 0;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            if (domainQuota != null)
             {
                 return RecordingsDAL.UpdateDomainUsedQuota(domainId, (-1) * quotaToDecrease, defaultQuota);
            }
            return false;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <param name="quotaToIncrease"></param>
        /// <param name="shouldForceIncrease">If true - decrease the quota to 0 if not enough quota</param>
        /// <returns></returns>
        public bool IncreaseDomainUsedQuota(int groupId, long domainId, int quotaToIncrease, bool shouldForceIncrease = false)
        { 
            int defaultQuota = 0;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            if (domainQuota != null)
            {
                return RecordingsDAL.UpdateDomainUsedQuota(domainId, quotaToIncrease, defaultQuota, shouldForceIncrease);
            }
            return false;
        }

        internal Status CheckQuotaByTotalSeconds(int groupId, long householdId, int totalSeconds, bool isAggregative, List<Recording> newRecordings, List<Recording> currentRecordings)
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

            status = DeductRecordings(currentRecordings, ref shouldContinue, ref secondsLeft);

            // If there wasn't an error previously
            if (shouldContinue)
            {
                status = CheckQuotaByAvailableSeconds(groupId, householdId, secondsLeft, newRecordings, isAggregative);
            }

            return status;
        }


        public bool SetDomainTotalQuota(int groupId, long domainId, long totalQuota)
        {
            int defaultQuota = 0;
            bool result = false;
            DomainQuota domainQuota = GetDomainQuota(groupId, domainId, ref defaultQuota);
            if (domainQuota != null)
            {
                domainQuota.Total = (int)totalQuota;
                result = RecordingsDAL.SetDomainQuota(domainId, domainQuota);
            }
            return result;
        }

        public ApiObjects.Response.Status HandleDominQuotaOvarge(int groupId, long domainId, int recordingDuration, DomainRecordingStatus domainRecordingStatus = DomainRecordingStatus.Deleted)
        {
            ApiObjects.Response.Status bRes = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();

            try
            {
                while (limitRetries > 0)
                {
                    bRes = DeleteDomainOldestRecordings(groupId, domainId, recordingDuration, domainRecordingStatus);
                    if (bRes != null && bRes.Code == (int)eResponseStatus.OK)
                    {                      
                        break;
                    }
                    else
                    {
                        Thread.Sleep(r.Next(50));
                        limitRetries--;
                    }
                }

                if (bRes.Code != (int)eResponseStatus.OK) // fail to delete and free some quota 
                {
                    log.ErrorFormat("Failed HandleDominQuotaOvarge groupID: {0}, domainID: {1}, recordingDuration: {2}, status = {3}", groupId, domainId, recordingDuration, bRes.Message);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed HandleDominQuotaOvarge groupID: {0}, domainID: {1}, recordingDuration: {2}, status = {3}, ex: {4}", groupId, domainId, recordingDuration, bRes.Message, ex);
            }
            return bRes;
        }

        private ApiObjects.Response.Status DeleteDomainOldestRecordings(int groupId, long domainId, int recordingQuota, DomainRecordingStatus domainRecordingStatus = DomainRecordingStatus.Deleted)
        {
            ApiObjects.Response.Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                List<long> domainRecordingIds = new List<long>();
                List<TstvRecordingStatus> recordingStatuses = new List<TstvRecordingStatus>() { TstvRecordingStatus.Recorded };
                Dictionary<long, Recording> domainRecordingIdToRecordingMap = Utils.GetDomainRecordingsByTstvRecordingStatuses(groupId, domainId, recordingStatuses);
                if (domainRecordingIdToRecordingMap != null && domainRecordingIdToRecordingMap.Count > 0)
                {
                    var ordered = domainRecordingIdToRecordingMap.OrderBy(x => x.Value.ViewableUntilDate.HasValue ? x.Value.ViewableUntilDate.Value : 0);
                    Dictionary<long, Recording> recordings = ordered.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

                    // build list of domain recordings id - Least as needed   
                    int tempQuotaOverage = 0;
                    int recordingDuration = 0;
                    long currentUtcTime = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                    foreach (KeyValuePair<long, Recording> recording in recordings)
                    {
                        // check if record is protected - ignore it 
                        if (recording.Value.ProtectedUntilDate.HasValue && recording.Value.ProtectedUntilDate.Value > currentUtcTime)
                        {
                            continue;
                        }

                        recordingDuration = (int)(recording.Value.EpgEndDate - recording.Value.EpgStartDate).TotalSeconds;
                        domainRecordingIds.Add(recording.Key);
                        tempQuotaOverage += recordingDuration;

                        if (tempQuotaOverage >= recordingQuota)
                        {
                            break;
                        }
                    }

                    if (tempQuotaOverage >= recordingQuota)
                    {
                        if (domainRecordingIds != null && domainRecordingIds.Count > 0) // delete all and DecreaseDomainUsedQuota
                        {
                            if (QuotaManager.Instance.DecreaseDomainUsedQuota(groupId, domainId, tempQuotaOverage))
                            {
                                log.DebugFormat("try to deleted domainRecordingIds: {0}, QuotaOverageDuration : {1}", string.Join(",", domainRecordingIds), tempQuotaOverage);
                                if (!RecordingsDAL.DeleteDomainRecording(domainRecordingIds, domainRecordingStatus))
                                {
                                    QuotaManager.Instance.IncreaseDomainUsedQuota(groupId, domainId, tempQuotaOverage);
                                    log.ErrorFormat("fail in DeleteDomainOldestRecordings to perform delete domainRecordingID = {0}", string.Join(",", domainRecordingIds));
                                }
                                else
                                {
                                    response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        // not enough quota to free
                        response = new Status((int)eResponseStatus.ExceededQuota, eResponseStatus.ExceededQuota.ToString());
                        log.DebugFormat("try to GetDomainRecordingsByTstvRecordingStatuses by : {0} status return no results", TstvRecordingStatus.Recorded.ToString());
                    }
                }
                else
                {
                    response = new Status((int)eResponseStatus.ExceededQuota, eResponseStatus.ExceededQuota.ToString());
                    log.DebugFormat("try to GetDomainRecordingsByTstvRecordingStatuses by : {0} status return no results", TstvRecordingStatus.Recorded.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in 'DeleteDomainOldestRecordings' for domainId = {0}", domainId), ex);
            }

            return response;
        }

        #endregion

        #region Private Methods

        private static Status DeductRecordings(List<Recording> recordings, ref bool shouldContinue, ref int secondsLeft)
        {
            Status status = new Status((int)eResponseStatus.OK);

            if (recordings != null)
            {
                // Deduct seconds of EPGs that domain already previously recorded
                foreach (var recording in recordings)
                {
                    int currentEpgSeconds = 0;

                    TimeSpan span = recording.EpgEndDate - recording.EpgStartDate;

                    currentEpgSeconds = (int)span.TotalSeconds;

                    secondsLeft -= currentEpgSeconds;

                    // Mark the entire operation as failure, something here is completely wrong
                    if (secondsLeft < 0)
                    {
                        status = new Status((int)eResponseStatus.ExceededQuota, eResponseStatus.ExceededQuota.ToString());
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
