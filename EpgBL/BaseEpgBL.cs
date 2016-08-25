using ApiObjects;
using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DalCB;
using System.Collections.Concurrent;

namespace EpgBL
{
    public abstract class BaseEpgBL
    {
        public int m_nGroupID { get; protected set; }

        public abstract EPGChannelProgrammeObject GetEpg(ulong nProgramID);
        public abstract List<EPGChannelProgrammeObject> GetEpgs(List<int> lIds);        
        public abstract List<EpgCB> GetEpgs(List<string> lIds);

        public abstract EpgCB GetEpgCB(ulong nProgramID);
        public abstract EpgCB GetEpgCB(ulong nProgramID, out ulong cas);
        public abstract List<EpgCB> GetEpgCB(ulong nProgramID, List<string> languages);
        public abstract List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, string language);

        public abstract ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDic(int nPageSize, int nStartIndex, List<int> lChannelIDs, DateTime fromDate, DateTime toDate);
        public abstract ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDicCurrent(int nNextTop, int nPrevTop, List<int> lChannelIDs);
      
        public abstract List<EPGChannelProgrammeObject> SearchEPGContent(int groupID, string searchValue, int pageIndex, int pageSize);
        public abstract List<EPGChannelProgrammeObject> GetEPGProgramsByScids(int groupID, string[] scids, Language eLang, int duration);
        public abstract List<EPGChannelProgrammeObject> GetEPGProgramsByProgramsIdentefier(int groupID, string[] pids, Language eLang, int duration);
        public abstract List<EPGChannelProgrammeObject> GetEPGPrograms(int groupID, string[] externalids, Language eLang, int duration);

        public abstract bool InsertEpg(EpgCB newEpgItem, out ulong epgID, ulong? cas = null);
        public abstract bool InsertEpg(EpgCB newEpgItem, bool isMainLang, out string docID, ulong? cas = null);

        public abstract bool SetEpg(EpgCB newEpgItem, out ulong epgID, ulong? cas = null);

        public abstract bool UpdateEpg(EpgCB newEpgItem, ulong? cas = null);

        public abstract void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate);

        public abstract void RemoveGroupPrograms(List<DateTime> lDates, int channelID);

        public abstract void RemoveGroupPrograms(List<int> lprogramIDs);

        public abstract void RemoveGroupPrograms(List<string> docIds);

        public abstract void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate, int channelID);
    }
}
