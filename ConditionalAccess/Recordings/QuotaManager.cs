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

namespace Recordings
{
    public class QuotaManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static CouchbaseSynchronizer synchronizer = null;

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
            synchronizer = new CouchbaseSynchronizer(1000, 60);
            synchronizer.SynchronizedAct += Quota_SynchronizedAct;
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

            // Check that we don't exceed amount of minutes

            int minutesLeft = quotaManagerModel.Minutes;

            status = DeductRecordings(currentRecordings, ref shouldContinue, ref minutesLeft);

            // If there wasn't an error previously
            if (shouldContinue)
            {
                status = CheckQuotaByAvailableMinutes(groupId, householdId, minutesLeft, newRecordings, isAggregative);
            }

            return status;
        }

        public Status CheckQuotaByAvailableMinutes(int groupId, long householdId, int availableMinutes, List<Recording> newRecordings, bool isAggregative)
        {
            Status status = new Status((int)eResponseStatus.OK);

            int minutesLeft = availableMinutes;

            // Now deduct the time of all the new/requested recordings
            foreach (var recording in newRecordings)
            {
                int currentEpgSeconds = 0;
                int initialMinutesLeft = minutesLeft;

                TimeSpan span = recording.EpgEndDate - recording.EpgStartDate;

                currentEpgSeconds = (int)Math.Round((span.TotalSeconds/60), MidpointRounding.ToEven);

                int tempMinutes = minutesLeft - currentEpgSeconds;

                // Mark this, current-specific, recording as failed
                if (tempMinutes < 0)
                {
                    log.DebugFormat("Requested EPG exceeds domain's quota. EPG duration is {0} minutes and there are {1} minutes available.", currentEpgSeconds, initialMinutesLeft);
                    recording.Status = new Status((int)eResponseStatus.ExceededQuota, eResponseStatus.ExceededQuota.ToString());
                }

                // If we check each recording individually or not
                if (isAggregative)
                {
                    minutesLeft = tempMinutes;
                }
            }

            return status;
        }

        public ApiObjects.TimeShiftedTv.DomainQuotaResponse GetDomainQuota(int groupId, long domainID, List<Recording> recordings)
        {
            Status status = new Status((int)eResponseStatus.OK);

            int totalMinutes = ConditionalAccess.Utils.GetQuota(groupId, domainID);
            int minutesLeft = totalMinutes;

            bool shouldContinue = false;
            status = DeductRecordings(recordings, ref shouldContinue, ref minutesLeft);

            // Available quota cannot be negative. Minimum is 0
            minutesLeft = Math.Max(0, minutesLeft);

            ApiObjects.TimeShiftedTv.DomainQuotaResponse response = new DomainQuotaResponse()
            {
                Status = status,
                TotalQuota = totalMinutes,
                AvailableQuota = minutesLeft
            };

            return response;
        }        

        public Status CheckQuotaByTotalMinutes(int groupId, long householdId, int totalMinutes, bool isAggregative, List<Recording> newRecordings, List<Recording> currentRecordings)
        {
            Status status = new Status((int)eResponseStatus.OK);
            bool shouldContinue = true;
            HashSet<long> currentEPGIds = new HashSet<long>();
            int minutesLeft = totalMinutes;

            if (currentRecordings != null)
            {
                foreach (var recording in currentRecordings)
                {
                    currentEPGIds.Add(recording.EpgId);
                }
            }

            // Remove all recordings that already exist for the household
            newRecordings.RemoveAll(recording => currentEPGIds.Contains(recording.EpgId));

            status = DeductRecordings(currentRecordings, ref shouldContinue, ref minutesLeft);

            // If there wasn't an error previously
            if (shouldContinue)
            {
                status = CheckQuotaByAvailableMinutes(groupId, householdId, minutesLeft, newRecordings, isAggregative);
            }

            return status;
        }

        public bool UpdateDomainQuota(long domainId, int currentQuota, int updatedQuota)
        {
            bool result = false;
            string syncKey = string.Format("QuotaManager_d{0}", domainId);
            Dictionary<string, object> syncParmeters = new Dictionary<string, object>();
            syncParmeters.Add("domainId", domainId);
            syncParmeters.Add("previousQuota", currentQuota);
            syncParmeters.Add("updatedQuota", updatedQuota);
            result = synchronizer.DoAction(syncKey, syncParmeters);

            return result;
        }

        #endregion

        #region Private Methods

        private static Status DeductRecordings(List<Recording> recordings, ref bool shouldContinue, ref int minutesLeft)
        {
            Status status = new Status((int)eResponseStatus.OK);

            if (recordings != null)
            {
                // Deduct minutes of EPGs that domain already previously recorded
                foreach (var recording in recordings)
                {
                    int currentEpgMinutes = 0;

                    TimeSpan span = recording.EpgEndDate - recording.EpgStartDate;

                    currentEpgMinutes = (int)span.TotalMinutes;

                    minutesLeft -= currentEpgMinutes;

                    // Mark the entire operation as failure, something here is completely wrong
                    if (minutesLeft < 0)
                    {
                        status = new Status((int)eResponseStatus.ExceededQuota, eResponseStatus.ExceededQuota.ToString());
                        shouldContinue = false;
                        break;
                    }
                }
            }

            return status;
        }

        private static bool Quota_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = true;

            if (parameters != null && parameters.Count > 0)
            {
                long domainId = (long)parameters["domainId"];
                int previousQuota = (int)parameters["previousQuota"];
                int UpdatedQuota = (int)parameters["updatedQuota"];

                int currentQuota = ConditionalAccess.Utils.GetDomainQuota(domainId);
                if (currentQuota == previousQuota)
                {
                    result = DAL.ConditionalAccessDAL.UpdateDomainQuota(domainId,UpdatedQuota);
                }
                else
                {
                    log.ErrorFormat("Failed updating domain quota, domainId: {0}, previousQuota: {1}, updatedQuota: {2}, currentQuota: {3}", domainId, previousQuota, UpdatedQuota, currentQuota);
                }
            }

            return result;

        }

        #endregion
    }
}
