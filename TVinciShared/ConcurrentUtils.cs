using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public class ConcurrentUtils
    {
        
        public const int MILLISEC_THRESHOLD = 65000;
        
        static public bool IsConcurrent(string sSiteGUID, string sUDID, int nGroupID)
        {
            bool isConcurrent = false;

            if (string.IsNullOrEmpty(sSiteGUID) || string.IsNullOrEmpty(sUDID))
            {
                return false;
            }

            // Check global group limitation    
            int nDeviceLimit = 0;
            int nUserLimit = 0;
            int nStrongConcurrentLimit = 0;
            int nGroupConcurrentLimit = 0;
            int nDeviceLimitationModuleID = 
                DAL.DomainDal.GetDomainDefaultLimitsID(nGroupID, ref nDeviceLimit, ref nUserLimit, ref nStrongConcurrentLimit, ref nGroupConcurrentLimit);

            // Check if device limitation module is not set or module's concurrent limitation is not set 
            if ((nGroupConcurrentLimit <= 0) && (nDeviceLimitationModuleID <= 0 || nStrongConcurrentLimit <= 0))
            {
                return false;
            }

            
            int nConcurrent = 0;
            int nFamilyConcurrentCount = 0;
            int nDomainID = DAL.UsersDal.GetUserDomainID(sSiteGUID);

            // Check if Group device limits were defined
            if (nDeviceLimitationModuleID > 0 && nStrongConcurrentLimit > 0)
            {
                // Get device limitations per family if defined
                List<string[]> dbDeviceFamilies = DAL.DomainDal.InitializeDeviceFamilies(nDeviceLimitationModuleID, nGroupID);

                // Get current device family
                int nDeviceBrandID = 0;
                int nDbDeviceFamilyID = DAL.DeviceDal.GetDeviceFamilyID(nGroupID, sUDID, ref nDeviceBrandID);

                // Find the family's limit
                int nFamilyConcurrentLimit = 0;
                for (int i = 0; i < dbDeviceFamilies.Count; i++)
                {
                    string[] currentDeviceFamily = dbDeviceFamilies[i];

                    int nFamilyID = -1;
                    if ((!int.TryParse(currentDeviceFamily[0], out nFamilyID)) || (nFamilyID != nDbDeviceFamilyID))
                    {
                        continue;
                    }

                    int.TryParse(currentDeviceFamily[2], out nFamilyConcurrentLimit);
                    break;
                }
            
                //int nFamilyConcurrentCount = 0;
                nConcurrent = GetConcurrentCount(nGroupID, nDomainID, sUDID, ref nFamilyConcurrentCount, nDbDeviceFamilyID);

                isConcurrent = (nStrongConcurrentLimit > 0 && nConcurrent >= nStrongConcurrentLimit) ||
                               (nFamilyConcurrentLimit > 0 && nFamilyConcurrentCount >= nFamilyConcurrentLimit);
                
                return isConcurrent;
            }

            // Group limitation module not defined, check Group concurrent setting

            nConcurrent = GetConcurrentCount(nGroupID, nDomainID, sUDID, ref nFamilyConcurrentCount);

            isConcurrent = (nGroupConcurrentLimit > 0 && nConcurrent >= nGroupConcurrentLimit);

            return isConcurrent;
        }


        static public int GetConcurrentCount(int nGroupID, int nDomainID, string sUDID, ref int nFamilyConcurrentCount, int nDeviceFamilyID = 0)
        {
            int nConcurrent = 0;
            nFamilyConcurrentCount = 0;

            if (nDomainID <= 0)
            {
                return nConcurrent;
            }

            Dictionary<string, TimeSpan> dictDeviceUpdateDate = DAL.ProtocolsFuncsDal.GetLastMediaMarks(nDomainID, MILLISEC_THRESHOLD);

            if (dictDeviceUpdateDate == null || dictDeviceUpdateDate.Count == 0)
            {
                return nConcurrent;
            }

            for (int i = 0; i < dictDeviceUpdateDate.Keys.Count; i++)
            {
                string sLastUDID = dictDeviceUpdateDate.Keys.ElementAt<string>(i);
                TimeSpan ts = dictDeviceUpdateDate[sLastUDID];

                if (string.Compare(sUDID, sLastUDID, true) != 0 && ts.TotalMilliseconds < MILLISEC_THRESHOLD)
                {
                    nConcurrent++;

                    int nDeviceBrandID = 0;
                    int nCurrentDeviceFamilyID = DAL.DeviceDal.GetDeviceFamilyID(nGroupID, sLastUDID, ref nDeviceBrandID);

                    if ((nDeviceFamilyID > 0) && (nDeviceFamilyID == nCurrentDeviceFamilyID))
                    {
                        nFamilyConcurrentCount++;
                    }
                }
            }

            return nConcurrent;
        }
    }
}
