using ApiObjects;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TVinciShared
{
    public class RabbitHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected const string ROUTING_KEY_PROCESS_FREE_ITEM_UPDATE = "PROCESS_FREE_ITEM_UPDATE\\{0}";

        public static bool InsertFreeItemsIndexUpdate(int groupID, eObjectType type, List<int> assetIDs, DateTime updateIndexDate)
        {
            bool result = false;
            int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupID);

            try
            {
                // validate assets and updateIndexDate
                if (assetIDs == null || assetIDs.Count == 0)
                {
                    return result;
                }

                GenericCeleryQueue queue = new GenericCeleryQueue();
                FreeItemUpdateData data = new FreeItemUpdateData(parentGroupId, type, assetIDs, updateIndexDate);
                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_FREE_ITEM_UPDATE, parentGroupId));
                if (enqueueSuccessful)
                {
                    log.DebugFormat("New free item index update task created. Next update date: {0}, data: {1}", updateIndexDate, data);
                    result = true;
                }
                else
                {
                    log.ErrorFormat("Failed queuing free item index update {0}", data);
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed Inserting remote task index update: ", ex);
            }

            return result;
        }

        public static bool IsFutureIndexUpdate(DateTime? previousDate, DateTime? currentDate)
        {
            return currentDate.HasValue 
                    && (currentDate > DateTime.UtcNow && currentDate.Value <= DateTime.UtcNow.AddYears(2))
                    && (!previousDate.HasValue || currentDate.Value != previousDate.Value);
        }

    }
}
