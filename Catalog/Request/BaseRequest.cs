using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;
using Catalog.Cache;
using DAL;
using Catalog.Response;

namespace Catalog.Request
{
    [KnownType(typeof(ChannelRequest))]
    [KnownType(typeof(MediaSearchRequest))]
    [KnownType(typeof(MediaRelatedRequest))]
    [KnownType(typeof(MediaUpdateDateRequest))]
    [KnownType(typeof(PWWAWProtocolRequest))]
    [KnownType(typeof(ChannelsListRequest))]
    [KnownType(typeof(PersonalLastWatchedRequest))]
    [KnownType(typeof(PersonalLasDeviceRequest))]
    [KnownType(typeof(PersonalRecommendedRequest))]
    [KnownType(typeof(CommentsListRequest))]
    [KnownType(typeof(PWLALProtocolRequest))]
    [KnownType(typeof(UserSocialMediasRequest))]
    [KnownType(typeof(BundleAssetsRequest))]
    [KnownType(typeof(PicRequest))]
    [KnownType(typeof(MediaMarkRequest))]
    [KnownType(typeof(MediaHitRequest))]
    [KnownType(typeof(ChannelRequestMultiFiltering))]
    [KnownType(typeof(EpgCommentsListRequest))]
    [KnownType(typeof(EpgSearchRequest))]
    [KnownType(typeof(EpgAutoCompleteRequest))]
    [KnownType(typeof(MediaCommentRequest))]
    [KnownType(typeof(IsMediaExistsInSubscriptionRequest))]
    [KnownType(typeof(ChannelsContainingMediaRequest))]
    [KnownType(typeof(BundleContainingMediaRequest))]    
    [KnownType(typeof(MediaChannelsRequest))]
    [KnownType(typeof(MediaAutoCompleteRequest))]
    [KnownType(typeof(EpgRequest))]
    [KnownType(typeof(AssetStatsRequest))]
    [KnownType(typeof(EPGProgramsByScidsRequest))]
    [KnownType(typeof(EPGProgramsByProgramsIdentefierRequest))]
    [KnownType(typeof(EPGSearchContentRequest))]
    [KnownType(typeof(ChannelViewsRequest))]
    [KnownType(typeof(BuzzMeterRequest))]
    [KnownType(typeof(MediaLastPositionRequest))]
    [KnownType(typeof(MediaLastPositionResponse))]
    [KnownType(typeof(AssetsBookmarksResponse))]
    [KnownType(typeof(CategoryRequest))]
    [KnownType(typeof(BaseChannelRequest))]
    [KnownType(typeof(InternalChannelRequest))]
    [KnownType(typeof(ExternalChannelRequest))]
    [KnownType(typeof(MediaRelatedExternalRequest))]
    [KnownType(typeof(MediaSearchExternalRequest))]
    [ServiceKnownType(typeof(ChannelObjRequest))]
    [ServiceKnownType(typeof(CrowdsourceRequest))]
    [ServiceKnownType(typeof(AssetsBookmarksRequest))]    
    [ServiceKnownType(typeof(EpgProgramDetailsRequest))]
    [ServiceKnownType(typeof(ExternalChannelRequest))]
    [ServiceKnownType(typeof(BaseChannelRequest))]
    [ServiceKnownType(typeof(InternalChannelRequest))]
    [ServiceKnownType(typeof(AssetCommentsRequest))]
    [ServiceKnownType(typeof(AssetCommentAddRequest))]
    [DataContract]
    public class BaseRequest
    {
        [DataMember]
        public string m_sUserIP;
        [DataMember]
        public string m_sSignature;
        [DataMember]
        public string m_sSignString;
        [DataMember]
        public Int32 m_nPageSize;
        [DataMember]
        public Int32 m_nPageIndex;
        [DataMember]
        public Int32 m_nGroupID;
        [DataMember]
        public Filter m_oFilter;
        [DataMember]
        public string m_sSiteGuid;
        [DataMember]
        public DateTime m_dServerTime;
        [DataMember]
        public int domainId;

        /// <summary>
        /// Full constructor, including user Id and domain Id
        /// </summary>
        /// <param name="nPageSize"></param>
        /// <param name="nPageIndex"></param>
        /// <param name="sUserIP"></param>
        /// <param name="nGroupID"></param>
        /// <param name="oFilter"></param>
        /// <param name="sSignature"></param>
        /// <param name="sSignString"></param>
        /// <param name="sSiteGuid"></param>
        /// <param name="nDomainId"></param>
        public BaseRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID, Filter oFilter, 
            string sSignature, string sSignString, string sSiteGuid, int nDomainId)
        {
            m_nGroupID = nGroupID;
            m_sUserIP = sUserIP;
            m_nPageSize = nPageSize;
            m_nPageIndex = nPageIndex;
            m_oFilter = oFilter;
            m_sSignature = sSignature;
            m_sSignString = sSignString;
            m_sSiteGuid = sSiteGuid;
            domainId = nDomainId;
        }

        /// <summary>
        /// Constructor with requests that are not related to a specific user
        /// </summary>
        /// <param name="nPageSize"></param>
        /// <param name="nPageIndex"></param>
        /// <param name="sUserIP"></param>
        /// <param name="nGroupID"></param>
        /// <param name="oFilter"></param>
        /// <param name="sSignature"></param>
        /// <param name="sSignString"></param>
        public BaseRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString)
            : this(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString, string.Empty, 0)
        {
        }

        public BaseRequest()
        {
        }


        public String SerializeToXML<T>(T objectToSerialize)
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }

        public List<SearchResult>  GetMediaUpdateDate(List<int> mediaIds, int groupID = 0)
        {
            List<SearchResult> lMediaRes = null;
           
            if (groupID == 0)
            {
                groupID = m_nGroupID;
            }

            lMediaRes = Utils.GetMediaUpdateDate(groupID, mediaIds);
           
            return lMediaRes;
        }

        protected virtual void CheckSignature(BaseRequest oBaseRequest)
        {
            string sCheckSignature = Utils.GetSignature(oBaseRequest.m_sSignString, oBaseRequest.m_nGroupID);
            if (sCheckSignature != oBaseRequest.m_sSignature)
                throw new Exception("Signatures don't match");
        }     
      

        protected virtual void CheckRequestValidness()
        {
            /*
             * To be overriden in the inheriting class
             * 
             */ 
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("this is: ", this.GetType().Name));
            sb.Append(String.Concat(" Page Size: ", m_nPageSize));
            sb.Append(String.Concat(" Page Index: ", m_nPageIndex));
            sb.Append(String.Concat(" Site Guid: ", m_sSiteGuid));
            sb.Append(String.Concat(" Group ID: ", m_nGroupID));
            sb.Append(String.Concat(" User IP: ", m_sUserIP));

            return sb.ToString();
        }
    }
}
