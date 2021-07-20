using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ApiObjects;
using ApiObjects.BulkUpload;

namespace EpgBL
{
    public static class Utils
    {
        private const int YES_REGULAR = 154;
        private const int YES = 153;

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

        public static List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> GetOverlappingPrograms(List<EpgProgramBulkUploadObject> listOfPrograms)
        {
            return GetOverlappingPrograms(listOfPrograms, listOfPrograms);
        }

        /// <summary>
        /// Get list of overlapping programs between tow lists
        /// </summary>
        /// <param name="listOfPrograms">1st list</param>
        /// <param name="otherListOfPrograms">2nd list</param>
        /// <returns>List of overlapping pairs, item1 is from 1st list item2 is from 2nd</returns>
        public static List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> GetOverlappingPrograms(List<EpgProgramBulkUploadObject> listOfPrograms,
            List<EpgProgramBulkUploadObject> otherListOfPrograms)
        {
            var overlappingPairs = new List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>>();
            var uniqueListOfPrograms = listOfPrograms.ToHashSet().ToList();
            foreach (var prog in uniqueListOfPrograms)
            {
                var allOtherPrograms = otherListOfPrograms.Where(p => p.EpgExternalId != prog.EpgExternalId);
                foreach (var otherProg in allOtherPrograms)
                {
                    if (IsOverlappingPrograms(prog, otherProg))
                    {
                        overlappingPairs.Add(Tuple.Create(prog, otherProg));
                    }
                }
            }

            // make sure we return a unique list of overlaps
            return overlappingPairs.ToHashSet().ToList();
        }

        private static bool IsOverlappingPrograms(EpgProgramBulkUploadObject prog, EpgProgramBulkUploadObject otherProg)
        {
            // if prog starts befor other ends AND other start before the prog ends
            return prog.StartDate < otherProg.EndDate && otherProg.StartDate < prog.EndDate;
        }
    }
}
