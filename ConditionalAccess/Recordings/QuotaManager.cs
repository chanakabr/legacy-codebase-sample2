using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.Response;
using ApiObjects;
using ApiObjects.TimeShiftedTv;
using DAL;

namespace Recordings
{
    public class QuotaManager
    {
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
                int currentEpgMinutes = 0;
                int initialMinutesLeft = minutesLeft;

                TimeSpan span = recording.EpgEndDate - recording.EpgStartDate;

                currentEpgMinutes = (int)span.TotalMinutes;

                int tempMinutes = minutesLeft - currentEpgMinutes;

                // Mark this, current-specific, recording as failed
                if (tempMinutes < 0)
                {
                    recording.Status = new Status((int)eResponseStatus.ExceededQuota,
                        string.Format("Requested EPG exceeds domain's quota. EPG duration is {0} minutes and there are {1} minutes available.",
                        currentEpgMinutes, initialMinutesLeft));
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

        internal Status CheckQuotaByTotalMinutes(int groupId, long householdId, int totalMinutes, bool isAggregative, List<Recording> newRecordings, List<Recording> currentRecordings)
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
                        status = new Status((int)eResponseStatus.ExceededQuota);
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
