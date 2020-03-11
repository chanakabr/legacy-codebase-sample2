using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;

namespace EpgFeeder
{
    public class EPGLogic
    {
        private const int MEDIA_CORP_GROUP_ID = 148;
        private const int YES_REGULAR = 154;
        private const int YES = 153;

        private static EPGImplementor GetEpgImplemnter(int groupID)
        {
            EPGImplementor ret = null;

            switch (groupID)
            {
                case MEDIA_CORP_GROUP_ID:
                    {
                        ret = new EPGMediaCorp(groupID.ToString());
                        break;
                    }
                case YES_REGULAR:
                    {
                        ret = new EPGYes(groupID.ToString());
                        break;
                    }
                case YES:
                    {
                        ret = new EPGYes(YES_REGULAR.ToString());
                        break;
                    }
                default:
                    {
                        ret = new EPGGeneral(groupID.ToString());
                        break;
                    }
            }
            return ret;
        }

        public static List<EPGChannelProgrammeObject> GetEPGChannelProgramsByDates(Int32 groupID, string sEPGChannelID, string sPicSize, DateTime fromDay, DateTime toDay, double nUTCOffset)
        {
            EPGImplementor epgIplementer = GetEpgImplemnter(groupID);
            List<EPGChannelProgrammeObject> retList = epgIplementer.GetEPGChannelProgramsByDates(groupID, sEPGChannelID, sPicSize, fromDay, toDay, nUTCOffset);
            return retList;
        }

        public static List<EPGChannelProgrammeObject> GetEPGChannelPrograms(Int32 groupID, string sEPGChannelID, string sPicSize, EPGUnit unit, int nFromOffsetUnit, int nToOffsetUnit, int nUTCOffset)
        {
            EPGImplementor epgIplementer = GetEpgImplemnter(groupID);
            List<EPGChannelProgrammeObject> retList = epgIplementer.GetEPGChannelPrograms(groupID, sEPGChannelID, sPicSize, unit, nFromOffsetUnit, nToOffsetUnit, nUTCOffset);
            return retList;
        }

        public static List<EPGChannelProgrammeObject> GetEPGMultiChannelPrograms(Int32 groupID, string[] sEPGChannelIDs, string sPicSize, EPGUnit unit, int nFromOffsetUnit, int nToOffsetUnit, int nUTCOffset)
        {
            EPGImplementor epgIplementer = GetEpgImplemnter(groupID);
            List<EPGChannelProgrammeObject> retList = epgIplementer.GetEPGMultiChannelPrograms(groupID, sEPGChannelIDs, sPicSize, unit, nFromOffsetUnit, nToOffsetUnit, nUTCOffset);
            return retList;
        }

        public static List<EPGChannelProgrammeObject> SearchEPGContent(Int32 groupID, string sSearchValue, int nPageIndex, int nPageSize)
        {
            EPGImplementor epgIplementer = GetEpgImplemnter(groupID);
            List<EPGChannelProgrammeObject> retList = epgIplementer.SearchEPGContent(groupID, sSearchValue, nPageIndex, nPageSize);
            return retList;
        }

        public static List<EPGChannelProgrammeObject> GetEPGProgramsByScids(Int32 groupID, string[] scids, Language eLang, int duration)
        {
            EPGImplementor epgIplementer = GetEpgImplemnter(groupID);
            List<EPGChannelProgrammeObject> retList = epgIplementer.GetEPGProgramsByScids(groupID, scids, eLang, duration);
            return retList;
        }

        public static int GetParentGroupId(int nGroupId, string sTentativeParentGroupId)
        {
            int nParentGroupId;
            if (nGroupId == 1)
            {
                nParentGroupId = int.Parse(sTentativeParentGroupId);
            }
            else
            {
                nParentGroupId = nGroupId;
            }

            return nParentGroupId;
        }
    }
}
