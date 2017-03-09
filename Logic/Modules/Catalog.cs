using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.Controller;
using Core.Catalog.Request;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog
{
    public class Module
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        internal static int LOG_THRESHOLD = Utils.DEFAULT_CATALOG_LOG_THRESHOLD_MILLISEC;

        public static IngestResponse IngestTvinciData(IngestRequest request)
        {
            return IngestController.IngestData(request, eIngestType.Tvinci);
        }

        public static IngestResponse IngestAdiData(IngestRequest request)
        {
            return IngestController.IngestData(request, eIngestType.Adi);
        }

        public static IngestResponse IngestKalturaEpg(IngestRequest request)
        {
            log.Topic = "EPGIngest";
            IngestResponse response = (IngestResponse)IngestController.IngestData(request, eIngestType.KalturaEpg);
            return response;
        }

        /* Get/Search  Channel/Medias */
        public static BaseResponse GetResponse(BaseRequest request)
        {
            if (request == null)
                return null;

            // add siteguid to logs/monitor
            OperationContext.Current.IncomingMessageProperties[Constants.USER_ID] = request.m_sSiteGuid != null ? request.m_sSiteGuid : "null";

            // get group ID
            OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = request.m_nGroupID;

            IFactoryImp f = new FactoryImp(request);
            IRequestImp imp = f.GetTypeImp(request);

            // get action ID
            OperationContext.Current.IncomingMessageProperties[Constants.ACTION] = imp.GetType();

            BaseResponse resp = imp.GetResponse(request);

            return resp;
        }

        //Complete all details by mediaID
        public static MediaResponse GetMediasByIDs(MediasProtocolRequest mediaRequest)
        {
            if (mediaRequest != null)
            {
                // add siteguid to logs/monitor
                OperationContext.Current.IncomingMessageProperties[Constants.USER_ID] = mediaRequest.m_sSiteGuid != null ? mediaRequest.m_sSiteGuid : "null";

                // get group ID
                OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = mediaRequest.m_nGroupID;

                return mediaRequest.GetMediasByIDs(mediaRequest);
            }
            throw new ArgumentException("Request object is null.");
        }

        //Complete all details by ProgramID
        public static EpgProgramResponse GetProgramsByIDs(EpgProgramDetailsRequest programRequest)
        {
            if (programRequest != null)
            {
                // add siteguid to logs/monitor
                OperationContext.Current.IncomingMessageProperties[Constants.USER_ID] = programRequest.m_sSiteGuid != null ? programRequest.m_sSiteGuid : "null";

                // get group ID
                OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = programRequest.m_nGroupID;

                return programRequest.GetProgramsByIDs(programRequest);
            }
            return null;
        }

        //Update Channel 
        public static bool UpdateChannel(int nGroupId, int nChannelId)
        {
            // get group ID
            OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = nGroupId;

            bool isChannelUpdatingSucceeded = false;

            if (nGroupId > 0 && nChannelId > 0)
            {
                GroupManager groupManager = new GroupManager();

                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(nGroupId);

                isChannelUpdatingSucceeded = groupManager.RemoveChannel(nParentGroupID, nChannelId);
            }

            return isChannelUpdatingSucceeded;
        }

        //Remove channel from Channel 
        public static bool RemoveChannelFromCache(int nGroupId, int nChannelId)
        {
            // get group ID
            OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = nGroupId;

            bool isChannelUpdatingSucceeded = false;

            if (nGroupId > 0 && nChannelId > 0)
            {
                GroupManager groupManager = new GroupManager();

                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(nGroupId);

                isChannelUpdatingSucceeded = groupManager.RemoveChannel(nParentGroupID, nChannelId);
            }

            return isChannelUpdatingSucceeded;
        }

        public static bool UpdateIndex(List<int> lMediaIds, int nGroupId, eAction eAction)
        {
            var groupId = nGroupId;
            var objectIDs = lMediaIds;
            var action = eAction;

            // get group ID
            OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = groupId;

            bool bIsUpdateIndexSucceeded = false;

            if (objectIDs != null && objectIDs.Count > 0 && groupId > 0)
            {
                try
                {
                    bIsUpdateIndexSucceeded = CatalogLogic.UpdateIndex(objectIDs.ConvertAll<long>(i => (long)i), groupId, action);
                }
                catch
                {
                    bIsUpdateIndexSucceeded = false;
                }
            }

            return bIsUpdateIndexSucceeded;
        }

        /// <summary>
        /// Updates channel index AND CACHE as well
        /// </summary>
        /// <param name="channelIds"></param>
        /// <param name="groupId"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool UpdateChannelIndex(List<int> channelIds, int groupId, eAction action)
        {
            // get group ID
            OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = groupId;

            bool bIsUpdateIndexSucceeded = false;

            if (channelIds != null && channelIds.Count > 0 && groupId > 0)
            {
                try
                {
                    bIsUpdateIndexSucceeded = CatalogLogic.UpdateChannelIndex(channelIds.ConvertAll<long>(i => (long)i),
                        groupId, action);

                    foreach (int id in channelIds)
                    {
                        RemoveChannelFromCache(groupId, id);
                    }
                }
                catch
                {
                    bIsUpdateIndexSucceeded = false;
                }
            }

            return bIsUpdateIndexSucceeded;
        }

        public static bool UpdateOperator(int nGroupID, int nOperatorID, int nSubscriptionID, long lChannelID, eOperatorEvent oe)
        {
            // get group ID
            OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = nGroupID;

            try
            {
                GroupManager groupManager = new GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(nGroupID);

                return groupManager.HandleOperatorEvent(nParentGroupID, nOperatorID, nSubscriptionID, lChannelID, oe);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception. Group ID: ", nGroupID));
                sb.Append(String.Concat(" Operator ID: ", nOperatorID));
                sb.Append(String.Concat(" Subscription ID: ", nSubscriptionID));
                sb.Append(String.Concat(" Channel ID: ", lChannelID));
                sb.Append(String.Concat(" Operator Event: ", oe.ToString()));
                sb.Append(String.Concat(" Exception msg: ", ex.Message));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));
                log.Error("UpdateOperator - " + sb.ToString(), ex);
                return false;
            }
        }

        public static bool UpdateEpgIndex(List<int> lEpgIds, int nGroupId, eAction eAction)
        {
            // get group ID
            OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = nGroupId;

            bool bIsUpdateIndexSucceeded = false;

            if (lEpgIds != null && lEpgIds.Count > 0 && nGroupId > 0)
            {
                try
                {
                    bIsUpdateIndexSucceeded = CatalogLogic.UpdateEpgIndex(lEpgIds.ConvertAll<long>(i => (long)i), nGroupId, eAction);
                }
                catch
                {
                    bIsUpdateIndexSucceeded = false;
                }
            }

            return bIsUpdateIndexSucceeded;
        }

        public static bool UpdateEpgChannelIndex(List<int> lEpgChannelIds, int nGroupId, eAction eAction)
        {
            // get group ID
            OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = nGroupId;

            bool bIsUpdateIndexSucceeded = false;

            if (lEpgChannelIds != null && lEpgChannelIds.Count > 0 && nGroupId > 0)
            {
                try
                {
                    bIsUpdateIndexSucceeded = CatalogLogic.UpdateEpgChannelIndex(lEpgChannelIds.ConvertAll<long>(i => (long)i), nGroupId, eAction);
                }
                catch
                {
                    bIsUpdateIndexSucceeded = false;
                }
            }

            return bIsUpdateIndexSucceeded;
        }

        public static bool RebuildIndex(int groupId, eObjectType type, bool switchIndexAlias, bool deleteOldIndices, DateTime? startDate, DateTime? endDate)
        {
            bool result = false;

            result = CatalogLogic.SendRebuildIndexMessage(groupId, type, switchIndexAlias, deleteOldIndices, startDate, endDate);

            return result;
        }

        public static bool RebuildGroup(int nGroupId, bool rebuild)
        {
            return CatalogLogic.RebuildGroup(nGroupId, rebuild);
        }

        public static string GetGroup(int nGroupId)
        {
            return CatalogLogic.GetGroup(nGroupId);
        }

        public static bool UpdateRecordingsIndex(List<long> recordingsIds, int groupId, eAction action)
        {
            // get group ID

            if (OperationContext.Current != null && OperationContext.Current.IncomingMessageProperties != null)
            {
                OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID] = groupId;
            }

            bool updateSuccess = false;

            if (recordingsIds != null && recordingsIds.Count > 0 && groupId > 0)
            {
                try
                {
                    updateSuccess = CatalogLogic.UpdateRecordingsIndex(recordingsIds, groupId, action);
                }
                catch
                {
                    updateSuccess = false;
                }
            }

            return updateSuccess;
        }

        public static bool RebuildEpgChannel(int groupId, int epgChannelID, DateTime fromDate, DateTime toDate, bool duplicates)
        {
            return CatalogLogic.RebuildEpgChannel(groupId, epgChannelID, fromDate, toDate, duplicates);
        }

        public static bool RebaseIndex(int groupId, eObjectType type, DateTime startDate)
        {
            bool result = false;

            result = CatalogLogic.SendRebaseIndexMessage(groupId, type, startDate);

            return result;
        }

        #region Iservice Members


        public static ApiObjects.Response.Status ClearStatistics(int groupId, DateTime until)
        {
            ApiObjects.Response.Status status = null;

            status = CatalogLogic.ClearStatistics(groupId, until);

            return status;
        }

        #endregion
    }
}
