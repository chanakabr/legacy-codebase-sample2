using ApiObjects;
using ApiObjects.SearchObjects;
using Phx.Lib.Appconfig;
using DAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EpgBL
{
    public abstract class BaseEpgBL
    {
        private static readonly string EPG_SEQUENCE_DOCUMENT = "epg_sequence_document";

        public int m_nGroupID { get; protected set; }

        public abstract EPGChannelProgrammeObject GetEpg(ulong nProgramID);
        public abstract List<EPGChannelProgrammeObject> GetEpgs(List<int> lIds, bool isOpcAccount);
        public abstract List<EPGChannelProgrammeObject> GetEpgChannelProgrammeObjects(List<string> lIds, bool isOpcAccount);
        public abstract List<EpgCB> GetEpgs(List<string> lIds, bool isRecordings = false);


        public abstract EpgCB GetEpgCB(ulong nProgramID, bool includeRecordingFallback = false);
        public abstract EpgCB GetEpgCB(ulong nProgramID, out ulong cas);
        public abstract EpgCB GetEpgCB(string ProgramID, out ulong cas);
        public abstract List<EpgCB> GetEpgCB(ulong nProgramID, List<string> languages, bool isAddAction = false);
        public abstract List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, string language, bool isOpcAccount);
        public abstract List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, List<LanguageObj> language, bool isOpcAccount);

        public abstract ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDic(int nPageSize, int nStartIndex, List<int> lChannelIDs, DateTime fromDate, DateTime toDate);
        public abstract ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDicCurrent(int nNextTop, int nPrevTop, List<int> lChannelIDs);

        public abstract List<EPGChannelProgrammeObject> SearchEPGContent(int groupID, string searchValue, int pageIndex, int pageSize);
        public abstract List<EPGChannelProgrammeObject> GetEPGProgramsByScids(int groupID, string[] scids, Language eLang, int duration);
        public abstract List<EPGChannelProgrammeObject> GetEPGProgramsByProgramsIdentefier(int groupID, string[] pids, Language eLang, int duration);
        public abstract List<EPGChannelProgrammeObject> GetEPGPrograms(int groupID, string[] externalids, Language eLang, int duration, bool isOpcAccount);

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

                        var multilingualMetasLookup = multilingualProgrammeObject.EPG_Meta.ToLookup(x => x.Key);
                        // set Multilingual Metas
                        for (int i = 0; i < programmeObject.EPG_Meta.Count; i++)
                        {
                            EPGDictionary currMeta = programmeObject.EPG_Meta[i];
                            if (!multilingualMetasLookup.Contains(currMeta.Key))
                            {
                                continue;
                            }

                            Array.ForEach(multilingualMetasLookup[currMeta.Key].ToArray(), x =>
                            {
                                currMeta.Values = SetLanguageContainer(currMeta.Values, languageObj, x.Value);
                            });

                            programmeObject.EPG_Meta[i] = currMeta;
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

                var metasLookup = programmeObject.EPG_Meta.ToLookup(x => x.Key);
                var resultMetas = new List<EPGDictionary>();
                foreach (var metaLookupValue in metasLookup)
                {
                    var metaValues = metaLookupValue.ToArray();
                    var resultMeta = metaValues.First();
                    Array.ForEach(metaValues, x =>
                    {
                        resultMeta.Values = SetLanguageContainer(resultMeta.Values, languageObj, x.Value);
                    });

                    resultMetas.Add(resultMeta);
                }

                programmeObject.EPG_Meta = resultMetas;
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
            lastNewEpgId += ApplicationConfiguration.Current.EpgInitialId.Value;
            var firstNewEpgId = (lastNewEpgId - (ulong)countOfIds) + 1;

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

        public abstract List<EPGChannelProgrammeObject> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate, bool isOpcAccount, List<ESOrderObj> esOrderObj = null);
    }
}
