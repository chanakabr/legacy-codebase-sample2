using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;

namespace Core.Catalog.Request
{
    /**************************************************************************
   * Get Channel + It's Medias List
   * return :
   * Channel with it's media List
   * ************************************************************************/
    [Serializable]
    [DataContract]
    public class ChannelRequestMultiFiltering : ChannelRequest, IRequestImp
    {
        [DataMember]
        public List<KeyValue> m_lFilterTags { get; set; }
        [DataMember]
        public CutWith m_eFilterCutWith { get; set; }

        public ChannelRequestMultiFiltering()
            : base()
        {
            m_lFilterTags = new List<KeyValue>();
            m_eFilterCutWith = CutWith.OR; // Set as default. This value will be provided by request
        }

        public ChannelRequestMultiFiltering(Int32 nChannelID, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString, ApiObjects.SearchObjects.OrderObj oOrderObj)
            : base(nChannelID, nGroupID, nPageSize, nPageIndex, sUserIP, oFilter, sSignature, sSignString, oOrderObj)
        {
            m_nChannelID = nChannelID;
            m_lFilterTags = new List<KeyValue>();
            m_eFilterCutWith = CutWith.OR; // Set as default. This value will be provided by request
        }

        public ChannelRequestMultiFiltering(ChannelRequest c)
            : base(c.m_nChannelID, c.m_nGroupID, c.m_nPageSize, c.m_nPageIndex, c.m_sUserIP, c.m_oFilter, c.m_sSignature, c.m_sSignString, c.m_oOrderObj)
        {
            m_lFilterTags = new List<KeyValue>();
            m_eFilterCutWith = CutWith.OR; // Set as default. This value will be provided by request
        }

        protected override MediaSearchObj GetManualSearchObject(GroupsCacheManager.Channel channel, ChannelRequest request, int nParentGroupID, ApiObjects.LanguageObj oLanguage, List<string> lPermittedWatchRules)
        {
            MediaSearchObj channelSearchObject = base.GetManualSearchObject(channel, request, nParentGroupID, oLanguage, lPermittedWatchRules);
            if (channelSearchObject != null)
            {
                CatalogLogic.AddChannelMultiFiltersToSearchObject(ref channelSearchObject, (ChannelRequestMultiFiltering)request);
            }

            return channelSearchObject;
        }
    }
}