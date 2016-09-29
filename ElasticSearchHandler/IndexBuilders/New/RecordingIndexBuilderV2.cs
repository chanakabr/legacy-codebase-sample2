using ApiObjects;
using ApiObjects.TimeShiftedTv;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EpgBL;
using ElasticSearch.Common;

namespace ElasticSearchHandler.IndexBuilders
{
    public class RecordingIndexBuilderV2 : EpgIndexBuilderV2
    {
        #region Data Members

        public static readonly string RECORDING = "recording";
        
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected Dictionary<long, long> epgToRecordingMapping = null;
        
        #endregion

        #region Ctor

        public RecordingIndexBuilderV2(int groupId)
            : base(groupId)
        {
            serializer = new ESSerializerV2();
            epgToRecordingMapping = new Dictionary<long, long>();
            shouldAddRouting = false;
        }

        #endregion

        #region Override Methods

        protected override string GetNewIndexName()
        {
            return ElasticsearchTasksCommon.Utils.GetNewRecordingIndexStr(this.groupId);
        }

        protected override void PopulateIndex(string newIndexName, GroupsCacheManager.Group group)
        {
            List<int> statuses = new List<int>() { (int)RecordingInternalStatus.OK, (int)RecordingInternalStatus.Waiting,
            (int)RecordingInternalStatus.Canceled, (int)RecordingInternalStatus.Failed};

            // Get information about relevant recordings
            epgToRecordingMapping = DAL.RecordingsDAL.GetEpgToRecordingsMapByRecordingStatuses(this.groupId, statuses);
            List<string> epgIds = new List<string>();

            List<LanguageObj> languages = group.GetLangauges();
            List<EpgCB> epgs = new List<EpgCB>();
            EpgBL.TvinciEpgBL epgBL = new TvinciEpgBL(this.groupId);

            foreach (var programId in epgToRecordingMapping.Keys)
            {
                // for main language
                epgIds.Add(programId.ToString());

                //Build list of keys with language
                foreach (var language in languages)
                {
                    string docID = string.Format("epg_{0}_lang_{1}", programId, language.Code.ToLower());
                    epgIds.Add(docID);
                }

                // Work in bulks so we don't chocke the Couchbase. every time get only a bulk of EPGs
                if (epgIds.Count >= sizeOfBulk)
                {
                    // Get EPG objects
                    epgs.AddRange(epgBL.GetEpgs(epgIds));
                    epgIds.Clear();
                }
            }

            // Finish off what's left to get from CB
            if (epgIds.Count >= 0)
            {
                epgs.AddRange(epgBL.GetEpgs(epgIds));
            }

            Dictionary<ulong, Dictionary<string, EpgCB>> epgDictionary = BuildEpgsLanguageDictionary(epgs);

            this.AddEPGsToIndex(newIndexName, RECORDING, epgDictionary, group);
        }

        /// <summary>
        /// Do nothing when it comes to recordings
        /// </summary>
        /// <param name="groupManager"></param>
        /// <param name="group"></param>
        /// <param name="newIndexName"></param>
        protected override void InsertChannelsQueries(GroupsCacheManager.GroupManager groupManager, GroupsCacheManager.Group group, string newIndexName)
        {
            
        }

        protected override string GetAlias()
        {
            return ElasticsearchTasksCommon.Utils.GetRecordingGroupAliasStr(this.groupId);
        }

        protected override string SerializeEPGObject(ApiObjects.EpgCB epg, string suffix = null)
        {
            long recordingId = (long)(epgToRecordingMapping[(int)epg.EpgID]);

            return serializer.SerializeRecordingObject(epg, recordingId, suffix);
        }

        protected override ulong GetDocumentId(ulong epgId)
        {
            return (ulong)(epgToRecordingMapping[(int)epgId]);
        }

        protected override string GetIndexType(LanguageObj language)
        {
            return (language.IsDefault) ? RECORDING : string.Concat(RECORDING, "_", language.Code);
        }

        protected override string GetIndexType()
        {
            return RECORDING;
        }

        #endregion
    }
}
