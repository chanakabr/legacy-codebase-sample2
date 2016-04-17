using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class RecordingUpdater : EpgUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public RecordingUpdater(int groupId)
            : base (groupId)
        {

        }

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

            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                result = DeleteEpg(IDs);
                break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                result = UpdateEpg(IDs);
                break;
                default:
                result = true;
                break;
            }

            return result;
        }
        protected override string GetAlias()
        {
            return ElasticsearchTasksCommon.Utils.GetRecordingGroupAliasStr(this.groupId);
        }
    }
}
