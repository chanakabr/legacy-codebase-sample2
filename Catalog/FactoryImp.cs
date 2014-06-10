using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

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
                return new ChannelRequestMultiFiltering();
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
                return new EpgSearchRequest();
            }
            if (m_oBaseRequest is EpgAutoCompleteRequest)
            {
                return new EpgAutoCompleteRequest();
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
                return new MediaAutoCompleteRequest();
            }
            if (m_oBaseRequest is BundleContainingMediaRequest)
            {
                return new BundleContainingMediaRequest();
            }
            if (m_oBaseRequest is AssetStatsRequest)
            {
                return new AssetStatsRequest();
            }
            if (m_oBaseRequest is EpgRequest)
            {
                return new EpgRequest();
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
                return new ChannelViewsRequest();
            }
            if (m_oBaseRequest is BuzzMeterRequest)
            {
                return new BuzzMeterRequest();
            }
            if (m_oBaseRequest is MediaLastPositionRequest)
            {
                return new MediaLastPositionRequest((MediaLastPositionRequest)this.m_oBaseRequest);
            }
            if (m_oBaseRequest is BundlesContainingMediaRequest)
            {
                return new BundlesContainingMediaRequest();
            }

            return null;
        }
    }
}

