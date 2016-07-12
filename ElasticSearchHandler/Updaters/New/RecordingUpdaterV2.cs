using ApiObjects.TimeShiftedTv;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class RecordingUpdaterV2 : EpgUpdaterV2
    {
        #region Consts

        public static readonly string RECORDING = "recording";

        #endregion

        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected Dictionary<int, Recording> epgToRecordingMapping = null;

        #endregion

        #region Ctor

        public RecordingUpdaterV2(int groupId)
            : base (groupId)
        {
            epgToRecordingMapping = new Dictionary<int, Recording>();
        }

        #endregion

        #region Override Methods

        public override bool Start()
        {
            bool result = false;
            log.Debug("Info - Start recordings update");
            if (IDs == null || IDs.Count == 0)
            {
                log.Debug("Info - recordings Id list empty");
                result = true;

                return result;
            }

            if (!esApi.IndexExists(GetAlias()))
            {
                log.Error("Error - " + string.Format("Index of type recordings for group {0} does not exist", groupId));
                return result;
            }

            var recordingIds = this.IDs;

            // Get information about relevant recordings
            List<Recording> recordings = DAL.RecordingsDAL.GetRecordings(this.groupId, recordingIds.Select(i => (long)i).ToList());

            // Map EPGs to original recordings,
            // Get all program IDs

            List<int> epgIds = new List<int>();

            foreach (Recording recording in recordings)
            {
                epgIds.Add((int)recording.EpgId);
                epgToRecordingMapping[(int)recording.EpgId] = recording;
            }

            // Call to methods in EPG Updater with the EPG IDs we "collected"
            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                {
                    result = DeleteEpg(epgIds);
                }
                break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                {
                    result = UpdateEpg(epgIds);
                    break;
                }
                default:
                result = true;
                break;
            }

            return result;
        }

        /// <summary>
        /// Alias will be recording_{group_id} and not epg_{group_id}
        /// </summary>
        /// <returns></returns>
        protected override string GetAlias()
        {
            return ElasticsearchTasksCommon.Utils.GetRecordingGroupAliasStr(this.groupId);
        }

        protected override string GetDocumentType()
        {
            return RECORDING;
        }

        /// <summary>
        /// Document ID will be the recording ID and not the EPG ID
        /// </summary>
        /// <param name="epg"></param>
        /// <returns></returns>
        protected override ulong GetDocumentId(ApiObjects.EpgCB epg)
        {
            ulong result = base.GetDocumentId(epg);

            result = (ulong)(epgToRecordingMapping[(int)epg.EpgID].Id);

            return result;
        }

        /// <summary>
        /// Document ID will be the recording ID and not the EPG ID
        /// </summary>
        /// <param name="epgId"></param>
        /// <returns></returns>
        protected override ulong GetDocumentId(int epgId)
        {
            return (ulong)(epgToRecordingMapping[epgId].Id);
        }

        /// <summary>
        /// Serialize the recording ID as well as the EPG
        /// </summary>
        /// <param name="epg"></param>
        /// <returns></returns>
        protected override string SerializeEPG(ApiObjects.EpgCB epg)
        {
            long recordingId = (long)(epgToRecordingMapping[(int)epg.EpgID].Id);

            return esSerializer.SerializeRecordingObject(epg, recordingId);
        }

        #endregion
    }
}
