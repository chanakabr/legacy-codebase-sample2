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
using ConfigurationManager;

namespace EpgBL
{
    public abstract class BaseEpgBL
    {
        private static readonly string EPG_SEQUENCE_DOCUMENT = "epg_sequence_document";

        public int m_nGroupID { get; protected set; }

        public abstract EPGChannelProgrammeObject GetEpg(ulong nProgramID);
        public abstract List<EPGChannelProgrammeObject> GetEpgs(List<int> lIds);
        public abstract List<EPGChannelProgrammeObject> GetEpgChannelProgrammeObjects(List<string> lIds);
        public abstract List<EpgCB> GetEpgs(List<string> lIds);

        public abstract EpgCB GetEpgCB(ulong nProgramID, bool includeRecordingFallback = false);
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
            if (resultForMultilingual != null && resultForMultilingual.Count > 0)
            {
                foreach (var programmeObject in result)
                {
                    // find epg_id at resultForMultilingual2
                    EPGChannelProgrammeObject multilingualProgrammeObject = resultForMultilingual.FirstOrDefault(x => x.EPG_ID == programmeObject.EPG_ID);
                    if (multilingualProgrammeObject != null)
                    {
                        programmeObject.ProgrammeName = SetLanguageContainer(programmeObject.ProgrammeName, languageObj, multilingualProgrammeObject.NAME);
                        programmeObject.ProgrammeDescription = SetLanguageContainer(programmeObject.ProgrammeDescription, languageObj, multilingualProgrammeObject.DESCRIPTION);

                        // set Multilingual tags
                        for (int i = 0; i < programmeObject.EPG_TAGS.Count; i++)
                        {
                            EPGDictionary currTag = programmeObject.EPG_TAGS[i];
                            EPGDictionary currMultilingualTag = multilingualProgrammeObject.EPG_TAGS.FirstOrDefault(x => currTag.Key.Equals(x.Key));
                            if (!currMultilingualTag.Equals(default(EPGDictionary)))
                            {
                                currTag.Values = SetLanguageContainer(currTag.Values, languageObj, currMultilingualTag.Value);
                                programmeObject.EPG_TAGS[i] = currTag;
                            }
                        }

                        // set Multilingual Metas
                        for (int i = 0; i < programmeObject.EPG_Meta.Count; i++)
                        {
                            EPGDictionary currMeta = programmeObject.EPG_Meta[i];
                            EPGDictionary currMultilingualMeta = multilingualProgrammeObject.EPG_Meta.FirstOrDefault(x => currMeta.Key.Equals(x.Key));
                            if (!currMultilingualMeta.Equals(default(EPGDictionary)))
                            {
                                currMeta.Values = SetLanguageContainer(currMeta.Values, languageObj, currMultilingualMeta.Value);
                                programmeObject.EPG_Meta[i] = currMeta;
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
                m_sLanguageCode3 = languageObj.Code,
                m_sValue = value
            });

            return langContainers.ToArray();
        }

        public virtual IList<ulong> GetNewEpgIds(int countOfIds)
        {
            if (countOfIds <= 0) { throw new ArgumentException("Count should be greater than zero", nameof(countOfIds)); }

            var couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.EPG);
            var lastNewEpgId = couchbaseManager.Increment(EPG_SEQUENCE_DOCUMENT, (ulong)countOfIds);
            var firstNewEpgId = (lastNewEpgId - (ulong)countOfIds) + 1;

            firstNewEpgId += (ulong)ApplicationConfiguration.EpgInitialId.LongValue;
            var listOfIds = new List<ulong>();

            for (var epgId = firstNewEpgId; epgId <= lastNewEpgId; epgId++)
            {
                listOfIds.Add(epgId);
            }

            return listOfIds;
        }

        public virtual ulong GetNewEpgId()
        {
            return GetNewEpgIds(1).First();
        }

        public abstract List<EPGChannelProgrammeObject> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate);

    }
}
