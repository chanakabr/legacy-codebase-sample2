using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using ApiObjects.CrowdsourceItems.Base;
using System.ServiceModel.Activation;
using ApiObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Catalog;

namespace WS_Catalog
{
    
    [ServiceContract(Namespace = "")]
    public interface Iservice
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "InjestAdiData", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestAdiData(IngestRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "InjestTvinciData", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestTvinciData(IngestRequest request);

        [OperationContract]
        [ServiceKnownType(typeof(EpgIngestResponse))]
        [WebInvoke(Method = "POST", UriTemplate = "IngestKalturaEpg", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestKalturaEpg(IngestRequest request);

        [OperationContract]
        [ServiceKnownType(typeof(CountryRequest))]
        [ServiceKnownType(typeof(Core.Catalog.Response.CountryResponse))] 
        [ServiceKnownType(typeof(AssetsBookmarksRequest))]
        [ServiceKnownType(typeof(AssetsBookmarksResponse))]        
        [ServiceKnownType(typeof(ChannelRequest))]
        [ServiceKnownType(typeof(MediaSearchRequest))]
        [ServiceKnownType(typeof(MediaRelatedRequest))]
        [ServiceKnownType(typeof(MediaUpdateDateRequest))]
        [ServiceKnownType(typeof(PWWAWProtocolRequest))]
        [ServiceKnownType(typeof(ChannelsListRequest))]
        [ServiceKnownType(typeof(PersonalLastWatchedRequest))]
        [ServiceKnownType(typeof(PersonalLasDeviceRequest))]
        [ServiceKnownType(typeof(PersonalRecommendedRequest))]
        [ServiceKnownType(typeof(CommentsListRequest))]
        [ServiceKnownType(typeof(PWLALProtocolRequest))]
        [ServiceKnownType(typeof(UserSocialMediasRequest))]
        [ServiceKnownType(typeof(BundleAssetsRequest))]
        [ServiceKnownType(typeof(BundleMediaRequest))]
        [ServiceKnownType(typeof(PicRequest))]
        [ServiceKnownType(typeof(MediaMarkRequest))]
        [ServiceKnownType(typeof(MediaHitRequest))]
        [ServiceKnownType(typeof(ChannelRequestMultiFiltering))]
        [ServiceKnownType(typeof(EpgCommentsListRequest))]
        [ServiceKnownType(typeof(EpgSearchRequest))]
        [ServiceKnownType(typeof(EpgAutoCompleteRequest))]
        [ServiceKnownType(typeof(MediaCommentRequest))]
        [ServiceKnownType(typeof(IsMediaExistsInSubscriptionRequest))]
        [ServiceKnownType(typeof(ChannelsContainingMediaRequest))]
        [ServiceKnownType(typeof(BundleContainingMediaRequest))]
        [ServiceKnownType(typeof(MediaChannelsRequest))]
        [ServiceKnownType(typeof(MediaAutoCompleteRequest))]
        [ServiceKnownType(typeof(EpgRequest))]
        [ServiceKnownType(typeof(AssetStatsRequest))]
        [ServiceKnownType(typeof(MediaSearchFullRequest))]
        [ServiceKnownType(typeof(BaseMediaSearchRequest))]
        [ServiceKnownType(typeof(EpgCommentRequest))]
        [ServiceKnownType(typeof(MediaCommentRequest))]
        [ServiceKnownType(typeof(EpgProgramDetailsRequest))]
        [ServiceKnownType(typeof(EPGProgramsByProgramsIdentefierRequest))]
        [ServiceKnownType(typeof(EPGProgramsByScidsRequest))]
        [ServiceKnownType(typeof(EPGSearchContentRequest))]
        [ServiceKnownType(typeof(BuzzMeterRequest))]
        [ServiceKnownType(typeof(BaseCrowdsourceItem))]
        [ServiceKnownType(typeof(ChannelObjRequest))]
        [ServiceKnownType(typeof(CrowdsourceRequest))]
        [ServiceKnownType(typeof(BundlesContainingMediaRequest))]
        [ServiceKnownType(typeof(MediaFilesRequest))]
        [ServiceKnownType(typeof(MediaFilesResponse))]
        [ServiceKnownType(typeof(CategoryRequest))]
        [ServiceKnownType(typeof(CategoryResponse))]        
        [ServiceKnownType(typeof(NPVRRetrieveRequest))]
        [ServiceKnownType(typeof(NPVRSeriesRequest))]
        [ServiceKnownType(typeof(NPVRRetrieveResponse))]
        [ServiceKnownType(typeof(NPVRSeriesResponse))]
        [ServiceKnownType(typeof(UnifiedSearchRequest))]
        [ServiceKnownType(typeof(AssetInfoRequest))]
        [ServiceKnownType(typeof(WatchHistoryRequest))]
        [ServiceKnownType(typeof(WatchHistoryResponse))]
        [ServiceKnownType(typeof(BaseChannelRequest))]
        [ServiceKnownType(typeof(ExternalChannelRequest))]
        [ServiceKnownType(typeof(InternalChannelRequest))]
        [ServiceKnownType(typeof(MediaRelatedExternalRequest))]
        [ServiceKnownType(typeof(MediaSearchExternalRequest))]
        [ServiceKnownType(typeof(ExtendedSearchRequest))]
        [ServiceKnownType(typeof(AssetCommentsRequest))]
        [ServiceKnownType(typeof(AssetCommentAddRequest))]
        [ServiceKnownType(typeof(ScheduledRecordingsRequest))]
        [WebInvoke(Method = "POST", UriTemplate = "GetResponse", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        BaseResponse GetResponse(BaseRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetMediasByIDs", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        MediaResponse GetMediasByIDs(MediasProtocolRequest mediaRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "UpdateChannel", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool UpdateChannel(int nGroupId, int nChannelId);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "RemoveChannelFromCache", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool RemoveChannelFromCache(int nGroupId, int nChannelId);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "UpdateIndex", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool UpdateIndex(List<int> lMediaIds, int nGroupId, eAction eAction);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "UpdateChannelIndex", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool UpdateChannelIndex(List<int> lChannelIds, int nGroupId, eAction eAction);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetProgramsByIDs", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        [ServiceKnownType(typeof(EpgProgramDetailsRequest))]
        EpgProgramResponse GetProgramsByIDs(EpgProgramDetailsRequest programRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "UpdateOperator", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool UpdateOperator(int nGroupID, int nOperatorID, int nSubscriptionID, long lChannelID, eOperatorEvent oe);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "UpdateEpgIndex", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool UpdateEpgIndex(List<int> lEpgIds, int nGroupId, eAction eAction);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "UpdateEpgChannelIndex", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool UpdateEpgChannelIndex(List<int> lEpgChannelIds, int nGroupId, eAction eAction);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "RebuildIndex", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool RebuildIndex(int groupId, eObjectType type, bool switchIndexAlias, bool deleteOldIndices, DateTime? startDate, DateTime? endDate);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "RebuildGroup", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool RebuildGroup(int nGroupId, bool rebuild);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetGroup", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        string GetGroup(int nGroupId);


        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "UpdateRecordingsIndex", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool UpdateRecordingsIndex(List<long> recordingsIds, int groupId, eAction action);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "RebuildEpgChannel", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool RebuildEpgChannel(int groupId, int epgChannelID, DateTime fromDate, DateTime toDate, bool duplicates);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "ClearStatistics", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        ApiObjects.Response.Status ClearStatistics(int groupId, DateTime until);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "RebaseIndex", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        bool RebaseIndex(int groupId, eObjectType type, DateTime startDate);

    }
}
