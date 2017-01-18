using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class UpdateCacheData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_update_cache";
        
        #endregion

        #region Data Members

        private string bucket;
        private string[] keys;

        #endregion

        public UpdateCacheData(int groupId, string bucket, string[] keys)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.bucket = bucket;
            this.keys = keys;

            this.args = new List<object>()
            {
                groupId,
                bucket,
                keys
            };
        }
    }
}
