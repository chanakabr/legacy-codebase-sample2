using CDNTokenizers.Tokenizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CDNTokenizers.Tokenizers.AkamaiTokenizers;
using KLogMonitor;
using System.Reflection;

namespace CDNTokenizers
{
    public static class CDNTokenizerFactory
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static object tokenImplLocker = new object();
        private static Dictionary<int, ICDNTokenizer> dCDNTokenizerImpl = new Dictionary<int, ICDNTokenizer>();

        public static ICDNTokenizer GetTokenizerInstance(int nGroupID, int nStreamingCompanyID)
        {
            ICDNTokenizer tokenizer;

            if (!dCDNTokenizerImpl.ContainsKey(nStreamingCompanyID))
            {
                lock (tokenImplLocker)
                {
                    if (!dCDNTokenizerImpl.ContainsKey(nStreamingCompanyID))
                    {
                        ICDNTokenizer tempTokenizer = CreateTokenizer(nGroupID, nStreamingCompanyID);

                        if (tempTokenizer != null)
                        {
                            dCDNTokenizerImpl[nStreamingCompanyID] = tempTokenizer;
                        }
                    }
                }
            }

            dCDNTokenizerImpl.TryGetValue(nStreamingCompanyID, out tokenizer);

            return tokenizer;
        }

        private static ICDNTokenizer CreateTokenizer(int nGroupID, int nStreamingCompanyID)
        {
            BaseCDNTokenizer tokenizer;

            string sGetImplName = GetCDNTokenizer(nStreamingCompanyID).ToLower();

            try
            {
                switch (sGetImplName)
                {
                    case "swiftserve":
                        tokenizer = new SwiftServeTokenizer(nGroupID, nStreamingCompanyID);
                        break;
                    case "concurrent":
                        tokenizer = new ConcurrentCDNTokenizer(nGroupID, nStreamingCompanyID);
                        break;
                    case "limelight":
                        tokenizer = new MediaValutTokenizer(nGroupID, nStreamingCompanyID);
                        break;
                    case "akamai":
                        tokenizer = new AkamaiTokenizerTurner(nGroupID, nStreamingCompanyID);
                        break;
                    default:
                        tokenizer = new BaseCDNTokenizer(nGroupID, nStreamingCompanyID);
                        break;
                }

                tokenizer.Init();
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                tokenizer = null;
            }

            return tokenizer;
        }

        private static string GetCDNTokenizer(int nStreamingCompanyID)
        {
            string cdnTokenizerName = string.Empty;

            ODBCWrapper.StoredProcedure Get_StreamingCoImplName = new ODBCWrapper.StoredProcedure("Get_StreamingCoImplName");
            Get_StreamingCoImplName.SetConnectionKey("MAIN_CONNECTION_STRING");
            Get_StreamingCoImplName.AddParameter("@StreamingCoID", nStreamingCompanyID);

            System.Data.DataSet ds = Get_StreamingCoImplName.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                cdnTokenizerName = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "NAME");
            }

            return cdnTokenizerName;
        }
    }
}
