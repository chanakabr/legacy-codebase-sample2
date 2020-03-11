using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    [Serializable]
    public class PartialUpdateData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_partial_update";

        #endregion

        #region Data Members

        private AssetsPartialUpdate assetsPartialUpdate;

        #endregion

        /// <summary>
        /// Initialize a new instance of a partial indexing data object that will be used by a celery instance
        /// </summary>
        public PartialUpdateData(int groupId, AssetsPartialUpdate assetsPartialUpdate = null)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK,
                groupId,
                assetsPartialUpdate)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.assetsPartialUpdate = assetsPartialUpdate;

            this.args.Add(base.RequestId);
        }
    }
}
