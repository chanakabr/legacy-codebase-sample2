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
        
        public Status CheckQuotaByModel(int groupId, int quotaManagerModelId, long householdId, 
            List<Recording> newRecordings, List<Recording> currentRecordings)
        {
            Status status = new Status();
            bool shouldContinue = true;

            // Get model by Id/default of group
            QuotaManagementModel quotaManagerModel = ConditionalAccessDAL.GetQuotaManagementModel(groupId, quotaManagerModelId);

            // Check that we don't exceed amount of minutes

            int minutesLeft = quotaManagerModel.Minutes;

            if (currentRecordings != null)
            {
                // Deduct minutes of EPGs that domain already previously recorded
                foreach (var recording in currentRecordings)
                {
                    int currentEpgMinutes = 0;

                    TimeSpan span = recording.EpgEndDate - recording.EpgStartDate;

                    currentEpgMinutes = (int)span.TotalMinutes;

                    minutesLeft -= currentEpgMinutes;

                    // Mark the entire operation as failure, something here is completely wrong
                    if (minutesLeft < 0)
                    {
                        status = new Status((int)eResponseStatus.DomainExceededQuota);
                        shouldContinue = false;
                        break;
                    }
                }
            }
            
            // If there wasn't an error previously
            if (shouldContinue)
            {
                status = CheckQuotaByAvailableMinutes(groupId, householdId, minutesLeft, newRecordings);
            }

            return status;
        }

        public Status CheckQuotaByAvailableMinutes(int groupId, long householdId, int availableMinutes, List<Recording> newRecordings)
        {
            Status status = null;

            int minutesLeft = availableMinutes;

            // Now deduct the time of all the new/requested recordings
            foreach (var recording in newRecordings)
            {
                int currentEpgMinutes = 0;

                TimeSpan span = recording.EpgEndDate - recording.EpgStartDate;

                currentEpgMinutes = (int)span.TotalMinutes;

                minutesLeft -= currentEpgMinutes;

                // Mark this, current-specific, recording as failed
                if (minutesLeft < 0)
                {
                    recording.Status = new Status((int)eResponseStatus.DomainExceededQuota);
                }
            }

            return status;
        }

        public int GetDomainRemainingQuota(int groupId, int totalMinutes, long domainID, List<Recording> recordings)
        {
            int minutesLeft = totalMinutes;

            // Now deduct the time of all the new/requested recordings
            foreach (var recording in recordings)
            {
                int currentEpgMinutes = 0;

                TimeSpan span = recording.EpgEndDate - recording.EpgStartDate;

                currentEpgMinutes = (int)span.TotalMinutes;

                minutesLeft -= currentEpgMinutes;
            }

            return minutesLeft;
        }

        #endregion
        
    }
}
