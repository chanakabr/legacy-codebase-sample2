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
            Status status = new Status((int)eResponseStatus.OK);

            int totalSeconds = ConditionalAccess.Utils.GetDomainDefaultQuota(groupId, domainId);
            int secondsLeft = GetDomainQuota(groupId, domainId);

            ApiObjects.TimeShiftedTv.DomainQuotaResponse response = new DomainQuotaResponse()
            {
                Status = status,
                TotalQuota = totalSeconds,
                AvailableQuota = secondsLeft
            };

            return response;
        }

        public int GetDomainQuota(int groupId, long domainId)
        {
            int domainQuota;
            // if the domain quota wasn't on CB
            if (!RecordingsDAL.GetDomainQuota(groupId, domainId, out domainQuota))
            {
                domainQuota = ConditionalAccess.Utils.GetDomainDefaultQuota(groupId, domainId);
            }

            return domainQuota;
        }

        public bool IncreaseDomainQuota(long domainId, int quotaToIncrease)
        {
            return RecordingsDAL.IncreaseDomainQuota(domainId, quotaToIncrease);
        }

        public bool DecreaseDomainQuota(int groupId, long domainId, int quotaToDecrease)
        {
            bool result = false;
            if (GetDomainQuota(groupId, domainId) >= quotaToDecrease)
            {
                int defaultDomainQuota = ConditionalAccess.Utils.GetDomainDefaultQuota(groupId, domainId);
                result = RecordingsDAL.DecreaseDomainQuota(domainId, quotaToDecrease, defaultDomainQuota);
            }

            return result;
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

        #endregion
    }
}
