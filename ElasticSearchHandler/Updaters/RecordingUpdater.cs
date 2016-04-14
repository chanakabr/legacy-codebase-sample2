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

        protected override string GetAlias()
        {
            return ElasticsearchTasksCommon.Utils.GetRecordingGroupAliasStr(this.groupId);
        }
    }
}
