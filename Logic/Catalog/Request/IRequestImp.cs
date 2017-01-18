using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using Core.Catalog.Response;
namespace Core.Catalog.Request
{
    [ServiceContract()]
    public interface IRequestImp
    {
        [ServiceKnownType(typeof(ChannelRequest))]
        [ServiceKnownType(typeof(ChannelRequestMultiFiltering))]
        [ServiceKnownType(typeof(MediaSearchRequest))]
        [ServiceKnownType(typeof(MediaRelatedRequest))]
        [ServiceKnownType(typeof(ChannelsListRequest))]
        [ServiceKnownType(typeof(PWWAWProtocolRequest))]
        [ServiceKnownType(typeof(PersonalLastWatchedRequest))]
        [ServiceKnownType(typeof(PersonalLasDeviceRequest))]
        [ServiceKnownType(typeof(AssetsBookmarksRequest))]
        [ServiceKnownType(typeof(PersonalRecommendedRequest))]
        [ServiceKnownType(typeof(CommentsListRequest))]
        [ServiceKnownType(typeof(PWLALProtocolRequest))]
        [ServiceKnownType(typeof(UserSocialMediasRequest))]
        [ServiceKnownType(typeof(BundleAssetsRequest))]
        [ServiceKnownType(typeof(BundleMediaRequest))]
        [ServiceKnownType(typeof(PicRequest))]
        [ServiceKnownType(typeof(BaseRequest))]
        [ServiceKnownType(typeof(EpgCommentRequest))]
        [ServiceKnownType(typeof(MediaCommentRequest))]
        [ServiceKnownType(typeof(EpgCommentsListRequest))]
        [ServiceKnownType(typeof(EpgSearchRequest))]
        [ServiceKnownType(typeof(EpgAutoCompleteRequest))]
        [ServiceKnownType(typeof(IsMediaExistsInSubscriptionRequest))]
        [ServiceKnownType(typeof(ChannelsContainingMediaRequest))]
        [ServiceKnownType(typeof(EPGProgramsByScidsRequest))]
        [ServiceKnownType(typeof(EPGProgramsByProgramsIdentefierRequest))]
        [ServiceKnownType(typeof(EPGSearchContentRequest))]
        [ServiceKnownType(typeof(ChannelViewsRequest))]        
        [ServiceKnownType(typeof(EpgProgramDetailsRequest))]
        [ServiceKnownType(typeof(MediaRelatedExternalRequest))]
        [ServiceKnownType(typeof(MediaSearchExternalRequest))]
        [ServiceKnownType(typeof(AssetCommentAddRequest))]
        [ServiceKnownType(typeof(ScheduledRecordingsRequest))]

        [OperationContract]
        BaseResponse GetResponse(BaseRequest oBaseRequest);
    }
}
