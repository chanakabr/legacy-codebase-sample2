using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class FreeItemUpdateData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_free_item_update";

        private eObjectType type;
        private List<int> assetIds;
        private List<int> moduleIds;

        public FreeItemUpdateData(int groupId, eObjectType type, List<int> assetIds, List<int> moduleIds, DateTime date) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;

            this.type = type;
            this.assetIds = assetIds;
            this.moduleIds = moduleIds;

            this.ETA = date;

            this.args = new List<object>()
            {
                type.ToString(),
                assetIds,
                moduleIds
            };
        }
    }
}
