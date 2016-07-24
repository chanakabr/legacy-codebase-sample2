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
    public class RecordingIndexBuilderV1 : EpgIndexBuilderV1
    {
        #region Data Members

        public static readonly string RECORDING = "recording";
        
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected Dictionary<long, long> epgToRecordingMapping = null;

        #endregion

        #region Ctor

        public RecordingIndexBuilderV1(int groupId) : base(groupId)
        {
            epgToRecordingMapping = new Dictionary<long, long>();
            serializer = new ESSerializerV1();
        }

        #endregion

        #region Override Methods

        protected override string GetNewIndexName()
        {
            return ElasticsearchTasksCommon.Utils.GetNewRecordingIndexStr(this.groupId);
        }

        protected override void PopulateIndex(string newIndexName)
        {
            List<int> statuses = new List<int>() { (int)RecordingInternalStatus.OK, (int)RecordingInternalStatus.Waiting,
            (int)RecordingInternalStatus.Canceled, (int)RecordingInternalStatus.Failed};

            // Get information about relevant recordings
            epgToRecordingMapping = DAL.RecordingsDAL.GetEpgToRecordingsMapByRecordingStatuses(this.groupId, statuses);
            List<string> epgIds = epgToRecordingMapping.Select(x => x.ToString()).ToList();                        

            EpgBL.TvinciEpgBL epgBL = new TvinciEpgBL(this.groupId);

            // Get EPG objects
            List<EpgCB> epgs = epgBL.GetEpgs(epgIds);

            Dictionary<ulong, EpgCB> epgDictionary = new Dictionary<ulong, EpgCB>();

            foreach (var epg in epgs)
            {
                epgDictionary.Add(epg.EpgID, epg);
            }

            this.AddEPGsToIndex(newIndexName, RECORDING, epgDictionary);
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

        protected override string SerializeEPGObject(ApiObjects.EpgCB epg)
        {
            long recordingId = (long)(epgToRecordingMapping[(int)epg.EpgID]);

            return serializer.SerializeRecordingObject(epg, recordingId);
        }

        protected override ulong GetDocumentId(ulong epgId)
        {
            return (ulong)(epgToRecordingMapping[(int)epgId]);
        }

        protected override string GetIndexType(LanguageObj language)
        {
            return (language.IsDefault) ? RECORDING : string.Concat(RECORDING, "_", language.Code);
        }

        #endregion
    }
}
