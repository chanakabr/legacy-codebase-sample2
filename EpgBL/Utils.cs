using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ApiObjects;

namespace EpgBL
{
    public static class Utils
    {

        #region CONST
        private const int YES_REGULAR = 154;
        private const int YES = 153;
        #endregion

        public static BaseEpgBL GetInstance(int nGroupID)
        {
            switch (nGroupID)
            {
                case YES_REGULAR:
                    {
                        return new TvinciEpgBL(nGroupID);
                        //return new YesEpgBL(YES);                        
                    }
                case YES:
                    {
                        return new TvinciEpgBL(nGroupID);
                        //return new YesEpgBL(YES_REGULAR);
                    }
                default:
                    {
                        return new TvinciEpgBL(nGroupID);
                    }
            }
        }

        public static string GenerateDocID(int nGroupID, int nEpgID)
        {
            return string.Format("{0}_{1}", nGroupID, nEpgID);
        }

        //create a ConcurrentDictionary per channel ID
        public static ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> createDic(List<int> lChannelIDs)
        {
            ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> dChannelEpgList = new ConcurrentDictionary<int, List<EPGChannelProgrammeObject>>();
            if (lChannelIDs != null && lChannelIDs.Count > 0)
            {
                for (int i = 0; i < lChannelIDs.Count; i++)
                {
                    int nChannel = lChannelIDs[i];
                    dChannelEpgList.TryAdd(nChannel, new List<EPGChannelProgrammeObject>());
                }
            }
            return dChannelEpgList;
        }
    }
}
