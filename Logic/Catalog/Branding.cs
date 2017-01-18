using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog
{
    [DataContract]
    public class Branding : FileMedia
    {   
        [DataMember]
        public Int32 m_nBrandHeight;
        [DataMember]
        public Int32 m_nRecurringTypeId;
        

        public Branding() : base()
        {
            m_nBrandHeight = 0;
            m_nRecurringTypeId = 0;
          
        }

        public Branding(Int32 nFileId, double nDuration, string sFormatFile, string sUrl, Int32 nBrandHeight, Int32 nRecurringTypeId, string sBillingType, int nCdnID)
            : base(nFileId, nDuration, sFormatFile, sUrl,sBillingType, nCdnID, string.Empty)
            
        {
            m_nBrandHeight = nBrandHeight;
            m_nRecurringTypeId = nRecurringTypeId;
           
        }
    }
}
