using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog.Response
{
    
    [DataContract]    
    [KnownType(typeof(AssetsBookmarksResponse))]
    [KnownType(typeof(ChannelResponse))]
    [KnownType(typeof(MediaIdsResponse))]
    [KnownType(typeof(MediaResponse))]
    [KnownType(typeof(ChannelDetailsResponse))]
    [KnownType(typeof(PersonalLastDeviceResponse))]    
    [KnownType(typeof(CommentsListResponse))]
    [KnownType(typeof(PicResponse))]
    [KnownType(typeof(MediaMarkResponse))]
    [KnownType(typeof(MediaHitResponse))]
    [KnownType(typeof(CommentResponse))]
    [KnownType(typeof(EpgSearchResponse))]
    [KnownType(typeof(EpgAutoCompleteResponse))]
    [KnownType(typeof(EpgProgramResponse))]
    [KnownType(typeof(IsMediaExistsInSubscriptionResponse))]                      
    [KnownType(typeof(ChannelsContainingMediaResponse))]
    [KnownType(typeof(MediaChannelsResponse))]
    [KnownType(typeof(MediaAutoCompleteResponse))]
    [KnownType(typeof(ContainingMediaResponse))]
    [KnownType(typeof(EpgResponse))]
	[KnownType(typeof(AssetStatsResponse))]
    [KnownType(typeof(EpgProgramsResponse))]
    [KnownType(typeof(ChannelViewsResponse))]
    [KnownType(typeof(BuzzMeterResponse))]
    [KnownType(typeof(ChannelObjResponse))]
    [KnownType(typeof(CrowdsourceResponse))]
    [KnownType(typeof(BundlesContainingMediaResponse))]    
    [KnownType(typeof(UnifiedSearchResponse))]
    [KnownType(typeof(UnifiedSearchExternalResponse))]    
    [KnownType(typeof(AssetInfoResponse))]
    [KnownType(typeof(MediaIdsStatusResponse))]
    [KnownType(typeof(ExtendedSearchResult))]
    [KnownType(typeof(AssetCommentsListResponse))]
    [KnownType(typeof(AssetCommentResponse))]
    public class BaseResponse
    {  
        [DataMember]
        public Int32 m_nTotalItems;

        [DataMember]
        public List<BaseObject> m_lObj;
     
        public BaseResponse()
        {
            m_lObj = new List<BaseObject>();
        }
    }
}
