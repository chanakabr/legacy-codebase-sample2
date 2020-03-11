using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDNTokenizers.Tokenizers
{
    public class BaseCDNTokenizer : ICDNTokenizer
    {
        public static readonly string CDN_TOKENIZER_LOG = "CdnTokenizer";
        protected int m_nGroupID;
        protected int m_nStreamingCoID;
        protected int m_nTTL;
        protected string m_sSalt;

        public BaseCDNTokenizer(int nGroupID, int nStreamingCompanyID)
        {
            m_nGroupID = nGroupID;
            m_nStreamingCoID = nStreamingCompanyID;
            m_sSalt = string.Empty;
        }

        internal virtual void Init()
        {
            //Get streaming co TTL + SALT
            ODBCWrapper.StoredProcedure Get_StreamingCoBasicParams = new ODBCWrapper.StoredProcedure("Get_StreamingCoBasicParams");
            Get_StreamingCoBasicParams.SetConnectionKey("MAIN_CONNECTION_STRING");
            Get_StreamingCoBasicParams.AddParameter("@StreamingCoID", m_nStreamingCoID);

            System.Data.DataSet ds = Get_StreamingCoBasicParams.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables[0] != null  && ds.Tables[0].Rows.Count > 0)
            {
                m_nTTL = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "TTL");
                m_sSalt = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "SALT");
            }
        }

        public virtual string GenerateToken(Dictionary<string, string> dParams)
        {
            return GetUrl(dParams);
        }

        protected string GetUrl(Dictionary<string, string> dParams)
        {
            return dParams.ContainsKey(Constants.URL) ? dParams[Constants.URL] : string.Empty;
        }



    }
}
