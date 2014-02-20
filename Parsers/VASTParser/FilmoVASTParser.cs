using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;

namespace VASTParser
{
    public class FilmoVASTParser : VASTParser
    {

        public FilmoVASTParser()
            : base()
        {
        }

        public FilmoVASTParser(int mediaID, int groupID, string adType) : base(mediaID, groupID, adType)
        {
            
            
        }

        protected override string GetVASTXml()
        {
            string url = string.Format(@"http://admatcher.videostrip.com/?categories={0}&puid={1}&host={2}&fmt=vast20&purpose={3}", m_category, m_playerID, m_hostName, m_adType);
            string retXml = WS_Utils.SendXMLHttpReq(url, string.Empty, string.Empty);
            return retXml;
        }

        public override string GetVastURL()
        {
            string url = string.Format(@"http://admatcher.videostrip.com/?categories={0}&puid={1}&host={2}&fmt=vast20&purpose={3}", m_category, m_playerID, m_hostName, m_adType);
            return url;
        }

        protected override void Initialize(int mediaID, int groupID, string adType)
        {
            //Todo - get player ID by Genre (?)
            m_playerID = "23941324";
            m_hostName = "ximon.nl";
            m_mediaID = mediaID;
            m_groupID = groupID;
            m_adType = adType;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select t.value from tags t, media_tags_types mtt, media_tags mt where ";
            selectQuery += "t.TAG_TYPE_ID = mtt.ID ";
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mtt.name", "=", "Advertisement category");
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.MEDIA_ID", "=", mediaID);
            selectQuery += " and mt.TAG_ID = t.ID and mtt.status = 1 and mt.status=1";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    m_category = selectQuery.Table("query").DefaultView[0].Row["value"].ToString();
                }
                    
            }
            selectQuery.Finish();
            selectQuery = null;

        }

        protected override bool IsMediaWithAds()
        {
            bool retVal = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select COMMERCIAL_TYPE_PRE_ID,COMMERCIAL_TYPE_POST_ID, COMMERCIAL_TYPE_OVERLAY_ID, COMMERCIAL_TYPE_BREAK_ID from media_files where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", m_mediaID);
            selectQuery += " and is_active = 1 and status = 1";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        object oPreProvider = selectQuery.Table("query").DefaultView[i].Row["COMMERCIAL_TYPE_PRE_ID"];
                        object oPostProvider = selectQuery.Table("query").DefaultView[i].Row["COMMERCIAL_TYPE_POST_ID"];
                        object oBreakProvider = selectQuery.Table("query").DefaultView[i].Row["COMMERCIAL_TYPE_BREAK_ID"];
                        object oOverlayProvider = selectQuery.Table("query").DefaultView[i].Row["COMMERCIAL_TYPE_OVERLAY_ID"];
                        if ((oPreProvider != null && oPreProvider != System.DBNull.Value && !string.IsNullOrEmpty(oPreProvider.ToString()) && int.Parse(oPreProvider.ToString()) != 0) ||
                            (oPostProvider != null && oPostProvider != System.DBNull.Value && !string.IsNullOrEmpty(oPostProvider.ToString()) && int.Parse(oPostProvider.ToString()) != 0) ||
                            (oBreakProvider != null && oBreakProvider != System.DBNull.Value && !string.IsNullOrEmpty(oBreakProvider.ToString()) && int.Parse(oBreakProvider.ToString()) != 0) ||
                            (oOverlayProvider != null && oOverlayProvider != System.DBNull.Value && !string.IsNullOrEmpty(oOverlayProvider.ToString()) && int.Parse(oOverlayProvider.ToString()) != 0))
                        {
                            retVal = true;
                            break;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }
    }
}
