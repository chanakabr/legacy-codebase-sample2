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
            ApiObjects.TimeShiftedTv.DomainQuotaResponse response = new DomainQuotaResponse();
            Status status = new Status((int)eResponseStatus.OK);

            int defaultTotal = ConditionalAccess.Utils.GetDomainDefaultQuota(groupId, domainId); // group total quota 
            DomainQuota domainQuota = GetDomainQuotaObject(groupId, domainId, defaultTotal);
            if (domainQuota != null)
            {
                int totalQuota = domainQuota.Total == 0 ? defaultTotal : domainQuota.Total;

                int secondsLeft = totalQuota - domainQuota.Used;

                response = new DomainQuotaResponse()
                {
                    Status = status,
                    TotalQuota = totalQuota,
                    AvailableQuota = secondsLeft < 0 ? 0 : secondsLeft
                };
            }
            return response;
        }

        private DomainQuota GetDomainQuotaObject(int groupId, long domainId, int defaultTotal)
        {            
            DomainQuota domainQuota;
                     
            // if the domain quota wasn't on CB
            if (!RecordingsDAL.GetDomainQuota(groupId, domainId, out domainQuota, defaultTotal))
            {                
                domainQuota = new DomainQuota(0);
            }

            return domainQuota;
        }
         
        public int GetDomainAvailableQuota(int groupId, long domainId)
        {
            DomainQuota domainQuota;
            int defaultQuota = ConditionalAccess.Utils.GetDomainDefaultQuota(groupId, domainId);
            // if the domain quota wasn't on CB -  quota in seconds !
            if (!RecordingsDAL.GetDomainQuota(groupId, domainId, out domainQuota, defaultQuota))
            {
                return defaultQuota;
            }
            else
            {
                return (domainQuota.Total - domainQuota.Used);
            }
        }

        public bool IncreaseDomainQuota(int groupId, long domainId, int quotaToIncrease)
        {
            int defaultTotal = ConditionalAccess.Utils.GetDomainDefaultQuota(groupId, domainId); // group total quota 
            DomainQuota domainQuota = GetDomainQuotaObject(groupId, domainId, defaultTotal);
            if (domainQuota != null)
            {
                domainQuota.Used -= quotaToIncrease;

                return RecordingsDAL.UpdateDomainQuota(domainId, domainQuota);
            }
            return false;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <param name="quotaToDecrease"></param>
        /// <param name="shouldForceDecrease">If true - decrease the quota to 0 if not enough quota</param>
        /// <returns></returns>
        public bool DecreaseDomainQuota(int groupId, long domainId, int quotaToDecrease, bool shouldForceDecrease = false)
        {
            int defaultTotal = ConditionalAccess.Utils.GetDomainDefaultQuota(groupId, domainId); // group total quota 
            DomainQuota domainQuota = GetDomainQuotaObject(groupId, domainId, defaultTotal);

            int totalQuota = domainQuota.Total == 0 ? defaultTotal : domainQuota.Total;            
            if (totalQuota - domainQuota.Used >= quotaToDecrease || shouldForceDecrease)
            {
                domainQuota.Used += quotaToDecrease;
                return RecordingsDAL.UpdateDomainQuota(domainId, domainQuota);
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
