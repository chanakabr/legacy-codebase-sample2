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
    public class MediaSearchFullRequest : BaseMediaSearchRequest
    {
        [DataMember]
        public List<KeyValue> m_AndList;
        [DataMember]
        public List<KeyValue> m_OrList;

        public MediaSearchFullRequest()
            : base()
        {
        }

        public MediaSearchFullRequest(bool bExact, List<KeyValue> andList, List<KeyValue> orList, OrderObj oOrderObj, List<int> nMediaTypes, int nMediaID,
             int nPageSize, int nPageIndex,  int nGroupID, string sUserIP, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            Initialize(bExact, andList, orList, oOrderObj, m_nMediaTypes, nMediaID);
        }

        public MediaSearchFullRequest(MediaSearchFullRequest m)
            : base(m)
        {
            Initialize(m.m_bExact, m.m_AndList, m.m_OrList, m.m_oOrderObj, m.m_nMediaTypes, m.m_nMediaID);
        }

        private void Initialize(bool bExact, List<KeyValue> andList, List<KeyValue> orList, OrderObj oOrderObj, List<int> nMediaTypes, int nMediaID)
        {
            m_bExact = bExact;
            m_AndList = andList;
            m_OrList = orList;
            m_oOrderObj = oOrderObj;
            m_nMediaTypes = nMediaTypes;
            m_nMediaID = nMediaID; 
        }
    }

}
