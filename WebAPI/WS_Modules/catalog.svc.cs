using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.Controller;
using Core.Catalog.Request;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Text;

namespace WS_Catalog
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Service : Iservice
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        internal static int LOG_THRESHOLD = Core.Catalog.Utils.DEFAULT_CATALOG_LOG_THRESHOLD_MILLISEC;

        public IngestResponse IngestTvinciData(IngestRequest request)
        {
            return IngestController.IngestData(request, eIngestType.Tvinci);
        }

        public IngestResponse IngestAdiData(IngestRequest request)
        {
            return IngestController.IngestData(request, eIngestType.Adi);
        }

        [ServiceKnownType(typeof(EpgIngestResponse))]
        public IngestResponse IngestKalturaEpg(IngestRequest request)
        {
            log.Topic = "EPGIngest";
            IngestResponse response = (IngestResponse)IngestController.IngestData(request, eIngestType.KalturaEpg);
            return response;
        }

        /* Get/Search  Channel/Medias */
        [ServiceKnownType(typeof(AssetsBookmarksRequest))]
        [ServiceKnownType(typeof(ChannelRequest))]
        [ServiceKnownType(typeof(ChannelRequestMultiFiltering))]
        [ServiceKnownType(typeof(EpgCommentRequest))]
        [ServiceKnownType(typeof(MediaCommentRequest))]
        [ServiceKnownType(typeof(EpgRequest))]
        [ServiceKnownType(typeof(AssetStatsRequest))]
        [ServiceKnownType(typeof(ChannelObjRequest))]
        [ServiceKnownType(typeof(CrowdsourceRequest))]
        [ServiceKnownType(typeof(MediaFilesRequest))]
        [ServiceKnownType(typeof(CategoryRequest))]        
        [ServiceKnownType(typeof(NPVRRetrieveRequest))]
        [ServiceKnownType(typeof(NPVRSeriesRequest))]
        [ServiceKnownType(typeof(UnifiedSearchRequest))]
        [ServiceKnownType(typeof(AssetInfoRequest))]
        [ServiceKnownType(typeof(WatchHistoryRequest))]
        [ServiceKnownType(typeof(BaseChannelRequest))]
        [ServiceKnownType(typeof(ExternalChannelRequest))]
        [ServiceKnownType(typeof(InternalChannelRequest))]
        [ServiceKnownType(typeof(AssetCommentsRequest))]
        [ServiceKnownType(typeof(AssetCommentAddRequest))]
        [ServiceKnownType(typeof(ScheduledRecordingsRequest))]
        public BaseResponse GetResponse(BaseRequest request)
        {
            if (request == null)
                return null;

            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, request.m_sSiteGuid != null ? request.m_sSiteGuid : "null");

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, request.m_nGroupID);

            IFactoryImp f = new FactoryImp(request);
            IRequestImp imp = f.GetTypeImp(request);

            // get action ID
            MonitorLogsHelper.SetContext(Constants.ACTION, imp.GetType());

            BaseResponse resp = imp.GetResponse(request);

            return resp;
        }

        //Complete all details by mediaID
        public MediaResponse GetMediasByIDs(MediasProtocolRequest mediaRequest)
        {
            return Core.Catalog.Module.GetMediasByIDs(mediaRequest);
        }

        //Complete all details by ProgramID
        public EpgProgramResponse GetProgramsByIDs(EpgProgramDetailsRequest programRequest)
        {
            return Core.Catalog.Module.GetProgramsByIDs(programRequest);
        }

        //Update Channel 
        public bool UpdateChannel(int nGroupId, int nChannelId)
        {
            return Core.Catalog.Module.UpdateChannel(nGroupId, nChannelId);
        }

        //Remove channel from Channel 
        public bool RemoveChannelFromCache(int nGroupId, int nChannelId)
        {
            return Core.Catalog.Module.RemoveChannelFromCache(nGroupId, nChannelId);
        }

        public bool UpdateIndex(List<int> lMediaIds, int nGroupId, eAction eAction)
        {
            return Core.Catalog.Module.UpdateIndex(lMediaIds, nGroupId, eAction);
        }

        /// <summary>
        /// Updates channel index AND CACHE as well
        /// </summary>
        /// <param name="channelIds"></param>
        /// <param name="groupId"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool UpdateChannelIndex(List<int> channelIds, int groupId, eAction action)
        {
            return Core.Catalog.Module.UpdateChannelIndex(channelIds, groupId, action);
        }

        public bool UpdateOperator(int nGroupID, int nOperatorID, int nSubscriptionID, long lChannelID, eOperatorEvent oe)
        {
            return Core.Catalog.Module.UpdateOperator(nGroupID, nOperatorID, nSubscriptionID, lChannelID, oe);
        }

        public bool UpdateEpgIndex(List<int> lEpgIds, int nGroupId, eAction eAction)
        {
            return Core.Catalog.Module.UpdateEpgIndex(lEpgIds, nGroupId, eAction);
        }

        public bool UpdateEpgChannelIndex(List<int> lEpgChannelIds, int nGroupId, eAction eAction)
        {
            return Core.Catalog.Module.UpdateEpgChannelIndex(lEpgChannelIds, nGroupId, eAction);
        }

        public bool RebuildIndex(int groupId, eObjectType type, bool switchIndexAlias, bool deleteOldIndices, DateTime? startDate, DateTime? endDate)
        {
            return Core.Catalog.Module.RebuildIndex(groupId, type, switchIndexAlias, deleteOldIndices, startDate, endDate);
        }

        public bool RebuildGroup(int nGroupId, bool rebuild)
        {
            return Core.Catalog.Module.RebuildGroup(nGroupId, rebuild);
        }

        public string GetGroup(int nGroupId)
        {
            return Core.Catalog.Module.GetGroup(nGroupId);
        }

        public bool UpdateRecordingsIndex(List<long> recordingsIds, int groupId, eAction action)
        {
            return Core.Catalog.Module.UpdateRecordingsIndex(recordingsIds, groupId, action);
        }

        public bool RebuildEpgChannel(int groupId, int epgChannelID, DateTime fromDate, DateTime toDate, bool duplicates)
        {
            return Core.Catalog.Module.RebuildEpgChannel(groupId, epgChannelID, fromDate, toDate, duplicates);
        }

        public bool RebaseIndex(int groupId, eObjectType type, DateTime startDate)
        {
            return Core.Catalog.Module.RebaseIndex(groupId, type, startDate);
        }

        #region Iservice Members


        public ApiObjects.Response.Status ClearStatistics(int groupId, DateTime until)
        {
            return Core.Catalog.Module.ClearStatistics(groupId, until);
        }

        #endregion
    }
}
