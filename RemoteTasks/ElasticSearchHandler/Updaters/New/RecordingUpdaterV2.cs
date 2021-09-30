using ApiObjects;
using ApiObjects.Catalog;
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

        protected Dictionary<long, long> epgToRecordingMapping = null;

        #endregion

        #region Ctor

        public RecordingUpdaterV2(int groupId)
            : base (groupId)
        {
            epgToRecordingMapping = new Dictionary<long, long>();
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

            var recordingIds = this.IDs;

            // Get information about relevant recordings
            epgToRecordingMapping = DAL.RecordingsDAL.GetEpgToRecordingsMap(this.groupId, recordingIds.Select(i => (long)i).ToList());            

            // Map EPGs to original recordings,
            // Get all program IDs

            List<long> epgIds = epgToRecordingMapping.Keys.Select(x => (long)x).ToList();

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
                    result = UpdateEpg(epgIds, this.UpdateEpgs);
                    break;
                }
                default:
                result = true;
                break;
            }

            return result;
        }

        protected override bool UpdateEpgs(List<EpgCB> epgObjects)
        {
            return _indexManager.UpdateEpgs(epgObjects, true, epgToRecordingMapping);
        }

        #endregion
    }
}
