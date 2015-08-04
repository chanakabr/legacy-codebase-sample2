using ApiObjects.MediaIndexingObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class CeleryIndexingData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_update_index";
        
        #endregion

        #region Data Members

        private List<int> assetIds;
        private eAssetType assetType;
        private eAction action;
        private DateTime date;

        #endregion

        /// <summary>
        /// Initialize a new instance of an indexing data object that will be used by a celery instance
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="assetIds"></param>
        /// <param name="asset_type"></param>
        /// <param name="action"></param>
        /// <param name="date"></param>
        public CeleryIndexingData(int groupId, List<int> assetIds = null,
            eAssetType assetType = eAssetType.UNKNOWN, eAction action = eAction.On, DateTime date = default(DateTime))
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK, 
                // No ETA and no expires
                null, 
                null,
                groupId, 
                assetIds,
                assetType.ToString(),
                action.ToString(),
                date.Ticks)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.assetIds = assetIds;
            this.assetType = assetType;
            this.action = action;
            this.date = date;
        }
    }
}
