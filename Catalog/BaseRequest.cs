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

namespace Catalog
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
    [KnownType(typeof(SubscriptionMediaRequest))]
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
    [KnownType(typeof(SubscriptionContainingMediaRequest))]    
    [KnownType(typeof(MediaChannelsRequest))]
    [KnownType(typeof(MediaAutoCompleteRequest))]
    [KnownType(typeof(EpgRequest))]
    [KnownType(typeof(AssetStatsRequest))]
    [KnownType(typeof(EPGProgramsByScidsRequest))]
    [KnownType(typeof(EPGProgramsByProgramsIdentefierRequest))]
    [KnownType(typeof(EPGSearchContentRequest))]
    [KnownType(typeof(MediaLastPositionRequest))]
    [KnownType(typeof(MediaLastPositionResponse))]
  

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

        public BaseRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString, string sSiteGuid)
        {
            m_nGroupID = nGroupID;
            m_sUserIP = sUserIP;
            m_nPageSize = nPageSize;
            m_nPageIndex = nPageIndex;
            m_oFilter = oFilter;
            m_sSignature = sSignature;
            m_sSignString = sSignString;
            m_sSiteGuid = sSiteGuid;
        }

        public BaseRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString)
        {
            m_nGroupID = nGroupID;
            m_sUserIP = sUserIP;
            m_nPageSize = nPageSize;
            m_nPageIndex = nPageIndex;
            m_oFilter = oFilter;
            m_sSignature = sSignature;
            m_sSignString = sSignString;
            m_sSiteGuid = string.Empty;
        }
        public BaseRequest()
        {
        }


        public String SerializeToXML<T>(T objectToSerialize)
        {
            StringBuilder sb = new StringBuilder();

            //XmlWriterSettings settings =
            //    new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };

            //using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
            //{
            //    if (xmlWriter != null)
            //    {
            //        new XmlSerializer(typeof(T), new Type[] { typeof(MediaObj), typeof(PicObj) }).Serialize(xmlWriter, objectToSerialize);
            //    }
            //}

            return sb.ToString();
        }

        public List<SearchResult> GetMediaUpdateDate(List<int> mediaIds, int groupID = 0)
        {
            List<SearchResult> lMediaRes = null;
            int nGroupID = 0;
            if (groupID != 0)
            {
                nGroupID = groupID;
            }
            else
            {
                nGroupID = m_nGroupID;
            }
            Group group = GroupsCache.Instance.GetGroup(nGroupID);
            if (group != null)
            {
                lMediaRes = Utils.GetMediaUpdateDate(group.m_nParentGroupID, mediaIds);
            }

            return lMediaRes;
        }

        protected virtual void CheckSignature(BaseRequest oBaseRequest)
        {
            string sCheckSignature = Utils.GetSignature(oBaseRequest.m_sSignString, oBaseRequest.m_nGroupID);
            if (sCheckSignature != oBaseRequest.m_sSignature)
                throw new Exception("Signatures don't match");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("User IP: ", m_sUserIP));
            sb.Append(String.Concat(" Page Size: ", m_nPageSize));
            sb.Append(String.Concat(" Page Index: ", m_nPageIndex));
            sb.Append(String.Concat(" Site Guid: ", m_sSiteGuid));
            sb.Append(String.Concat(" Group ID: ", m_nGroupID));
            sb.Append(String.Concat(" this is: ", this.GetType().Name));

            return sb.ToString();
        }
    }
}
