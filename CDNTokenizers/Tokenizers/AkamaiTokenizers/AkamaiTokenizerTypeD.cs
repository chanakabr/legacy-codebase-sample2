using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDNTokenizers.Tokenizers.AkamaiTokenizers
{
    public class AkamaiTokenizerTypeD : AbstractAkamaiTokenizer
    {
        protected string m_sToRemoveFromUrl;
        protected string m_sAifp;

        public AkamaiTokenizerTypeD(int nGroupID, int nStreamingCompanyID)
            :base(nGroupID, nStreamingCompanyID)
        {
        }

        internal override void Init()
        {
            base.Init();
            
            string sConfig = "";
            object oConfig = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_SECRET_CODE", m_nGroupID);
            if (oConfig != null && oConfig != DBNull.Value)
                sConfig = oConfig.ToString();
            string[] sSep = { "|" };
            string[] sConfigs = sConfig.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
            string sRefferer = "";
            string sProfile = "";
            string sAifp = "";
            string sSecretCode = "";
            string sToRemove = "";

            //In new code the secret code will be part of streaming companies table
            if (sConfigs.Length == 4)
            {
                m_sPath = sConfigs[0];
                m_sAifp = sConfigs[1];
                m_sSecretCode = sConfigs[2];
                m_sToRemoveFromUrl = sConfigs[3];
            }

            //Need to understand if window, duration and payload are all part of group implementation or are specific to each call
        }

        public override string GenerateToken(Dictionary<string, string> dParams)
        {

            return null;
        }
    }
}
