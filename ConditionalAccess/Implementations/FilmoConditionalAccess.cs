using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;
using KLogMonitor;
using System.Reflection;

namespace ConditionalAccess
{
    class FilmoConditionalAccess : TvinciConditionalAccess
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public FilmoConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public FilmoConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        internal override string GetLicensedLink(int nStreamingCompany, Dictionary<string, string> dParams)
        {
            string sBasicLink;

            dParams.TryGetValue("url", out sBasicLink);

            string retVal = string.Empty;

            if (!string.IsNullOrEmpty(sBasicLink) && sBasicLink.EndsWith("m3u8"))
            {
                retVal = sBasicLink;
            }

            try
            {

                //CDNetworksVault.MediaVault m = new CDNetworksVault.MediaVault("filmofvs", "guest", 7200);
                string decodedUrl = HttpUtility.UrlDecode(sBasicLink);
                Uri u = new Uri(decodedUrl);
                string decoded = HttpUtility.UrlDecode(u.Query);
                string url = HttpUtility.ParseQueryString(decoded).Get("url");
                Uri fileUrl = new Uri(url);
                string[] sFileSegments = fileUrl.Segments;
                string sPathWithoutFile = "";
                for (int i = 0; i < sFileSegments.Length - 1; i++)
                {
                    sPathWithoutFile += sFileSegments[i];
                }
                string sFile = sFileSegments[sFileSegments.Length - 1];
                string keyUrlStr = string.Empty;
                //m.GetURL(fileUrl.Scheme + "://" + fileUrl.Host + sPathWithoutFile + sFile + "_" + "04" + ".mp4");
                Uri keyUrl = new Uri(keyUrlStr);
                string keyStr = HttpUtility.ParseQueryString(keyUrl.Query).Get("key");
                if (!string.IsNullOrEmpty(keyStr))
                {
                    retVal = HttpUtility.UrlDecode(string.Format("{0}&key={1}", sBasicLink, keyStr));
                }

            }
            catch (Exception ex)
            {
                log.Error("LicensedLink - Exceptions :" + ex.Message, ex);
            }

            return retVal;

        }

        protected override string GetErrorLicensedLink(string sBasicLink)
        {
            return "";
        }



        private Int32 InsertMetaDataToTable(string sMetaData)
        {
            Int32 nPID = 0;

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("purchase_metadata");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("METADATA", "=", sMetaData);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select max(id) as id from purchase_metadata";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nPID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());

            }
            selectQuery.Finish();
            selectQuery = null;

            return nPID;
        }
    }
}
