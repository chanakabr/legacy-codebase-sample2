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
        private List<int> asset_ids;        

        public FreeItemUpdateData(int groupId, eObjectType type, List<int> asset_ids, DateTime updateIndexDate) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;

            this.type = type;
            this.asset_ids = asset_ids;            

            this.ETA = updateIndexDate;

            this.args = new List<object>()
            {
                groupId,
                type.ToString(),
                asset_ids                
            };
        }
    }
}
