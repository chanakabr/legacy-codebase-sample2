using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class CeleryIndexBuildingData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_build_index";

        #endregion

        #region Data Members

        private eObjectType type;
        private bool switchIndexAlias;
        private bool deleteOldIndices;
        private DateTime? startDate;
        private DateTime? endDate;

        #endregion

        /// <summary>
        ///  Initialize a new instance of an index building data object that will be used by a celery instance
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="assetType"></param>
        /// <param name="switchIndexAlias"></param>
        /// <param name="deleteOldIndices"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public CeleryIndexBuildingData(int groupId,
            eObjectType type = eObjectType.Media, bool switchIndexAlias = false, bool deleteOldIndices = false,
            DateTime? startDate = null, DateTime? endDate = null)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK,
                groupId,
                switchIndexAlias,
                deleteOldIndices,
                type.ToString())
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.type = type;
            this.switchIndexAlias = switchIndexAlias;
            this.deleteOldIndices = deleteOldIndices;
            this.startDate = startDate;
            this.endDate = endDate;

            string startDateString = null;
            string endDateString = null;
            
            if (startDate.HasValue)
            {
                startDateString = startDate.Value.ToString("yyyyMMddHHmmss");
            }

            if (endDate.HasValue)
            {
                endDateString = endDate.Value.ToString("yyyyMMddHHmmss");
            }

            this.args.Add(startDateString);
            this.args.Add(endDateString);
        }
    }
}
