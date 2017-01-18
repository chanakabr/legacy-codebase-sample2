using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using ApiObjects.SearchObjects;

namespace Core.Catalog.Request
{
    /**************************************************************************************
  * return : Search Media with Searcher service 
  * *************************************************************************************/
    [DataContract]
    public class MediaSearchRequest : BaseMediaSearchRequest
    {
        [DataMember]
        public bool m_bAnd; //(cut_with)
        [DataMember]
        public string m_sName ;
        [DataMember]
        public string m_sDescription;
        [DataMember]
        public List<KeyValue> m_lMetas;
        [DataMember]
        public List<KeyValue> m_lTags;

       
        public MediaSearchRequest() : base()
        {
        }

        public MediaSearchRequest(bool bExact, bool bAnd, OrderObj oOrderObj, List<KeyValue> Metas, List<KeyValue> Tags,
            Int32 nPageSize, Int32 nPageIndex, string sName, string sDescription, List<Int32> nMediaTypes, Int32 nMediaID, Int32 nGroupID, string sUserIP, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)           
        {
            Initialize(bExact, bAnd, oOrderObj, Metas, Tags, sName, sDescription, nMediaTypes, nMediaID);
        }

        public MediaSearchRequest(MediaSearchRequest m)
            : base(m)
        {
            Initialize(m.m_bExact, m.m_bAnd, m.m_oOrderObj, m.m_lMetas, m.m_lTags, m.m_sName, m.m_sDescription, m.m_nMediaTypes, m.m_nMediaID);
        }

        private void Initialize(bool bExact, bool bAnd, OrderObj oOrderObj, List<KeyValue> Metas, List<KeyValue> Tags, string sName, string sDescription, List<Int32> nMediaTypes, Int32 nMediaID)
        {
            m_bExact = bExact;
            m_bAnd = bAnd;

            m_oOrderObj = oOrderObj;

            m_sName = sName;
            m_sDescription = sDescription;

            m_nMediaID = nMediaID;
            m_nMediaTypes = nMediaTypes;

            m_lMetas = new List<KeyValue>(Metas);
            m_lTags = new List<KeyValue>(Tags);
        }
    }  
}
