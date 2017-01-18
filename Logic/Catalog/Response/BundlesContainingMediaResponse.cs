using ApiObjects.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Core.Catalog.Response
{
    [DataContract]
    public class BundlesContainingMediaResponse : BaseResponse
    {
        [DataMember]
        public List<BundleTriple> m_oBundles;

        [XmlIgnore]
        internal Dictionary<int, int> m_oSubsToIndexMapping;

        [XmlIgnore]
        internal Dictionary<int, int> m_oColsToIndexMapping;

        public BundlesContainingMediaResponse()
            : base()
        {
            m_oBundles = new List<BundleTriple>();
            m_oSubsToIndexMapping = new Dictionary<int, int>();
            m_oColsToIndexMapping = new Dictionary<int, int>();
        }

        public BundlesContainingMediaResponse(BundleKeyValue[] oIncomingBundles) : base()
        {
            m_oBundles = new List<BundleTriple>(oIncomingBundles.Length);
            m_oSubsToIndexMapping = new Dictionary<int,int>();
            m_oColsToIndexMapping = new Dictionary<int,int>();
            for (int i = 0; i < oIncomingBundles.Length; i++)
            {
                m_oBundles.Add(new BundleTriple(oIncomingBundles[i].m_nBundleCode, oIncomingBundles[i].m_eBundleType, false));
                if (oIncomingBundles[i].m_eBundleType == eBundleType.SUBSCRIPTION)
                {
                    if (!m_oSubsToIndexMapping.ContainsKey(oIncomingBundles[i].m_nBundleCode))
                    {
                        m_oSubsToIndexMapping.Add(oIncomingBundles[i].m_nBundleCode, i);
                    }
                }
                else
                {
                    if (!m_oColsToIndexMapping.ContainsKey(oIncomingBundles[i].m_nBundleCode))
                    {
                        m_oColsToIndexMapping.Add(oIncomingBundles[i].m_nBundleCode, i);
                    }
                }
            }
        }
    }
}
