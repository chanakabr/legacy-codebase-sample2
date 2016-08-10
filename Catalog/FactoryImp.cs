using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Catalog.Request;

namespace Catalog
{
    [DataContract]
    public class FactoryImp : IFactoryImp
    {
        [DataMember]
        private BaseRequest m_oBaseRequest;

        public FactoryImp(BaseRequest oBaseRequest)
        {
            this.m_oBaseRequest = oBaseRequest;
        }

        public IRequestImp GetTypeImp(BaseRequest oIRequest)
        {
            //internal logic on which Type to return
            if (m_oBaseRequest is ChannelRequestMultiFiltering)
            {
                return new ChannelRequestMultiFiltering((ChannelRequestMultiFiltering)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is ChannelRequest)
            {
                return new ChannelRequest((ChannelRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is MediaRelatedRequest)
            {
                return new MediaRelatedRequest((MediaRelatedRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is MediaSearchRequest)
            {
                return new MediaSearchRequest((MediaSearchRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is MediaUpdateDateRequest)
            {
                return new MediaUpdateDateRequest();
            }
            if (m_oBaseRequest is PWWAWProtocolRequest)
            {
                return new PWWAWProtocolRequest();
            }
            if (m_oBaseRequest is ChannelsListRequest)
            {
                return new ChannelsListRequest();
            }
            if (m_oBaseRequest is PersonalLastWatchedRequest)
            {
                return new PersonalLastWatchedRequest();
            }
            if (m_oBaseRequest is PersonalLasDeviceRequest)
            {
                return new PersonalLasDeviceRequest();
            }
            if (m_oBaseRequest is PersonalRecommendedRequest)
            {
                return new PersonalRecommendedRequest();
            }
            if (m_oBaseRequest is CommentsListRequest)
            {
                return new CommentsListRequest();
            }
            if (m_oBaseRequest is PWLALProtocolRequest)
            {
                return new PWLALProtocolRequest();
            }
            if (m_oBaseRequest is UserSocialMediasRequest)
            {
                return new UserSocialMediasRequest();
            }
            if (m_oBaseRequest is BundleAssetsRequest)
            {
                return new BundleAssetsRequest();
            }
            if (m_oBaseRequest is BundleMediaRequest)
            {
                return new BundleMediaRequest();
            }
            if (m_oBaseRequest is PicRequest)
            {
                return new PicRequest();
            }
            if (m_oBaseRequest is MediaMarkRequest)
            {
                return new MediaMarkRequest((MediaMarkRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is MediaHitRequest)
            {
                return new MediaHitRequest((MediaHitRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is EpgCommentRequest)
            {
                return new EpgCommentRequest();
            }
            if (m_oBaseRequest is EpgCommentsListRequest)
            {
                return new EpgCommentsListRequest();
            }
            if (m_oBaseRequest is EpgSearchRequest)
            {
                return (EpgSearchRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is EpgAutoCompleteRequest)
            {
                return (EpgAutoCompleteRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is MediaSearchFullRequest)
            {
                return new MediaSearchFullRequest((MediaSearchFullRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is MediaCommentRequest)
            {
                return new MediaCommentRequest();
            }
            if (m_oBaseRequest is IsMediaExistsInSubscriptionRequest)
            {
                return new IsMediaExistsInSubscriptionRequest();
            }
            if (m_oBaseRequest is ChannelsContainingMediaRequest)
            {
                return new ChannelsContainingMediaRequest();
            }
            if (m_oBaseRequest is MediaChannelsRequest)
            {
                return new MediaChannelsRequest();
            }
            if (m_oBaseRequest is MediaAutoCompleteRequest)
            {
                return (MediaAutoCompleteRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is BundleContainingMediaRequest)
            {
                return new BundleContainingMediaRequest();
            }
            if (m_oBaseRequest is AssetStatsRequest)
            {
                return (AssetStatsRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is EpgRequest)
            {
                return (EpgRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is EPGProgramsByScidsRequest)
            {
                return new EPGProgramsByScidsRequest();
            }
            if (m_oBaseRequest is EPGProgramsByProgramsIdentefierRequest)
            {
                return new EPGProgramsByProgramsIdentefierRequest();
            }
            if (m_oBaseRequest is EPGSearchContentRequest)
            {
                return new EPGSearchContentRequest();
            }
            if (m_oBaseRequest is ChannelViewsRequest)
            {
                return (ChannelViewsRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is BuzzMeterRequest)
            {
                return new BuzzMeterRequest();
            }
            if (m_oBaseRequest is MediaLastPositionRequest)
            {
                return new MediaLastPositionRequest((MediaLastPositionRequest)this.m_oBaseRequest);
            } 
            if (m_oBaseRequest is ChannelObjRequest)
            {
                return new ChannelObjRequest((ChannelObjRequest)this.m_oBaseRequest);
            } 
            if (m_oBaseRequest is CrowdsourceRequest)
            {
                return (CrowdsourceRequest)this.m_oBaseRequest;
            }
            if (m_oBaseRequest is BundlesContainingMediaRequest)
            {
                return (BundlesContainingMediaRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is MediaFilesRequest)
            {
                return (MediaFilesRequest) m_oBaseRequest;
            }
            if (m_oBaseRequest is CategoryRequest)
            {
                return (CategoryRequest) m_oBaseRequest;
            }
            if (m_oBaseRequest is AssetsBookmarksRequest)
            {
                return (AssetsBookmarksRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is EpgProgramDetailsRequest)
            {
                return (EpgProgramDetailsRequest) m_oBaseRequest;
            }
            if (m_oBaseRequest is NPVRRetrieveRequest)
            {
                return (NPVRRetrieveRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is NPVRSeriesRequest)
            {
                return (NPVRSeriesRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is UnifiedSearchRequest)
            {
                return (UnifiedSearchRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is AssetInfoRequest)
            {
                return (AssetInfoRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is WatchHistoryRequest)
            {
                return (WatchHistoryRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is ExternalChannelRequest)
            {
                return (ExternalChannelRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is BaseChannelRequest)
            {
                return (BaseChannelRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is InternalChannelRequest)
            {
                return (InternalChannelRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is MediaRelatedExternalRequest)
            {
                return new MediaRelatedExternalRequest((MediaRelatedExternalRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is MediaSearchExternalRequest)
            {
                return new MediaSearchExternalRequest((MediaSearchExternalRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is CountryRequest)
            {
                return (CountryRequest)m_oBaseRequest;
            }
            if (m_oBaseRequest is AssetCommentsRequest)
            {
                return (AssetCommentsRequest)m_oBaseRequest;
            }

            return null;
        }
    }
}

