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
        public abstract EpgCB GetEpgCB(string ProgramID, out ulong cas);
        public abstract List<EpgCB> GetEpgCB(ulong nProgramID, List<string> languages);
        public abstract List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, string language);
        public abstract List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, List<LanguageObj> language);

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
        public abstract bool UpdateEpg(EpgCB newEpgItem, bool isMainLang, out string docID, ulong? cas = null);

        public abstract void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate);

        public abstract void RemoveGroupPrograms(List<DateTime> lDates, int channelID);

        public abstract void RemoveGroupPrograms(List<int> lprogramIDs);

        public abstract void RemoveGroupPrograms(List<string> docIds);

        public abstract void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate, int channelID);

        public static void UpdateProgrammeWithMultilingual(ref List<EPGChannelProgrammeObject> result, LanguageObj languageObj, List<EPGChannelProgrammeObject> resultForMultilingual)
        {
            EPGChannelProgrammeObject multilingualProgrammeObject = null;
            EPGDictionary multilingualEpgDictionary;
            EPGDictionary epgDictionary;
            int epgIndex = 0;

            if (resultForMultilingual != null && resultForMultilingual.Count > 0)
            {
                foreach (var programmeObject in result)
                {
                    // find epg_id at resultForMultilingual2
                    multilingualProgrammeObject = resultForMultilingual.Where(x => x.EPG_ID == programmeObject.EPG_ID).First();
                    if (multilingualProgrammeObject != null)
                    {
                        programmeObject.ProgrammeName = SetLanguageContainer(programmeObject.ProgrammeName, languageObj, multilingualProgrammeObject.NAME);
                        programmeObject.ProgrammeDescription = SetLanguageContainer(programmeObject.ProgrammeDescription, languageObj, multilingualProgrammeObject.DESCRIPTION);

                        if (programmeObject.EPG_TAGS.Count == multilingualProgrammeObject.EPG_TAGS.Count)
                        {
                            for (epgIndex = 0; epgIndex < programmeObject.EPG_TAGS.Count; epgIndex++)
                            {
                                epgDictionary = programmeObject.EPG_TAGS[epgIndex];
                                multilingualEpgDictionary = multilingualProgrammeObject.EPG_TAGS[epgIndex];
                                epgDictionary.Values = SetLanguageContainer(epgDictionary.Values, languageObj, multilingualEpgDictionary.Value);
                                programmeObject.EPG_TAGS[epgIndex] = epgDictionary;
                            }
                        }

                        if (programmeObject.EPG_Meta.Count == multilingualProgrammeObject.EPG_Meta.Count)
                        {
                            for (epgIndex = 0; epgIndex < programmeObject.EPG_Meta.Count; epgIndex++)
                            {
                                epgDictionary = programmeObject.EPG_Meta[epgIndex];
                                multilingualEpgDictionary = multilingualProgrammeObject.EPG_Meta[epgIndex];
                                epgDictionary.Values = SetLanguageContainer(epgDictionary.Values, languageObj, multilingualEpgDictionary.Value);
                                programmeObject.EPG_Meta[epgIndex] = epgDictionary;
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateProgrammeWithMultilingual(ref List<EPGChannelProgrammeObject> result, LanguageObj languageObj)
        {
            EPGDictionary epgDictionary;
            int epgIndex = 0;

            foreach (var programmeObject in result)
            {
                programmeObject.ProgrammeName = SetLanguageContainer(programmeObject.ProgrammeName, languageObj, programmeObject.NAME);
                programmeObject.ProgrammeDescription = SetLanguageContainer(programmeObject.ProgrammeDescription, languageObj, programmeObject.DESCRIPTION);

                for (epgIndex = 0; epgIndex < programmeObject.EPG_TAGS.Count; epgIndex++)
                {
                    epgDictionary = programmeObject.EPG_TAGS[epgIndex];
                    epgDictionary.Values = SetLanguageContainer(epgDictionary.Values, languageObj, epgDictionary.Value);
                    programmeObject.EPG_TAGS[epgIndex] = epgDictionary;
                }

                for (epgIndex = 0; epgIndex < programmeObject.EPG_Meta.Count; epgIndex++)
                {
                    epgDictionary = programmeObject.EPG_Meta[epgIndex];
                    epgDictionary.Values = SetLanguageContainer(epgDictionary.Values, languageObj, epgDictionary.Value);
                    programmeObject.EPG_Meta[epgIndex] = epgDictionary;
                }
            }
        }

        private static LanguageContainer[] SetLanguageContainer(LanguageContainer[] sourceLanguageContainer, LanguageObj languageObj, string value)
        {
            List<LanguageContainer> langContainers = new List<LanguageContainer>();

            if (sourceLanguageContainer == null)
                langContainers = new List<LanguageContainer>();
            else
                langContainers = sourceLanguageContainer.Cast<LanguageContainer>().ToList();

            langContainers.Add(new LanguageContainer()
            {
                LanguageCode = languageObj.Code,
                Value = value
            });

            return langContainers.ToArray();
        }

    }
}
