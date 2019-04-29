using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class GeoRuleUpdateData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_geo_rule_update";

        private long AssetRuleId;
        private List<int> CountriesToRemove;
        private bool RemoveBlocked;
        private bool RemoveAllowed;
        private bool UpdateKsql;

        public GeoRuleUpdateData(int groupId, long assetRuleId, List<int> countriesToRemove, bool removeBlocked, bool removeAllowed, bool updateKsql) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.AssetRuleId = assetRuleId;
            this.CountriesToRemove = countriesToRemove;
            this.RemoveBlocked = removeBlocked;
            this.RemoveAllowed = removeAllowed;
            this.UpdateKsql = updateKsql;

            this.args = new List<object>()
            {
                groupId,
                id,
                assetRuleId,
                countriesToRemove,
                removeBlocked,
                removeAllowed,
                updateKsql,
                base.RequestId
            };
        }
    }
}
