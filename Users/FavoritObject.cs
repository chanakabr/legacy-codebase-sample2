using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class FavoritObject
    {
        public FavoritObject()
        {
            m_sDeviceName = "";
            m_sType = "";
            m_sItemCode = "";
            m_dUpdateDate = new DateTime(2000, 1, 1);
            m_sSiteUserGUID = "";
            m_sExtraData = "";
            m_nID = 0;
        }

        public void Initialize(Int32 nID , string sUserGUID, int domainID, string sDeviceName, string sDeviceUDID, string sType, string sItemCode , string sExtraData , DateTime dUpdate)
        {
            m_sDeviceName = sDeviceName;
            m_sDeviceUDID = sDeviceUDID;
            m_sType = sType;
            m_sItemCode = sItemCode;
            m_sSiteUserGUID = sUserGUID;
            m_sExtraData = sExtraData;
            m_nID = nID;
            m_dUpdateDate = dUpdate;
            m_nDomainID = domainID;
            m_is_channel = 0;
        }
        public void Initialize(Int32 nID, string sUserGUID, int domainID, string sDeviceName, string sDeviceUDID, string sType, string sItemCode, string sExtraData, DateTime dUpdate, int isChannel)
        {
            m_sDeviceName = sDeviceName;
            m_sDeviceUDID = sDeviceUDID;
            m_sType = sType;
            m_sItemCode = sItemCode;
            m_sSiteUserGUID = sUserGUID;
            m_sExtraData = sExtraData;
            m_nID = nID;
            m_dUpdateDate = dUpdate;
            m_nDomainID = domainID;
            m_is_channel = isChannel;
        }
        public void Initialize(Int32 nID, string sUserGUID, int domainID, string sDeviceUDID, string sType, string sItemCode, string sExtraData, DateTime dUpdate, int groupID)
        {
            //m_sDeviceName = sDeviceName;
            if (!string.IsNullOrEmpty(sDeviceUDID))
            {
                Device device = new Device(groupID);
                device.Initialize(sDeviceUDID, domainID);
                m_sDeviceName = device.m_deviceName;
                m_sDeviceUDID = sDeviceUDID;
            }
            else
            {
                m_sDeviceName = "PC";
                m_sDeviceUDID = string.Empty;
            }
            m_sType = sType;
            m_sItemCode = sItemCode;
            m_sSiteUserGUID = sUserGUID;
            m_sExtraData = sExtraData;
            m_nID = nID;
            m_dUpdateDate = dUpdate;
            m_is_channel = 0;
        }
        public void Initialize(Int32 nID, string sUserGUID, int domainID, string sDeviceUDID, string sType, string sItemCode, string sExtraData, DateTime dUpdate, int groupID, int isChannel)
        {
            //m_sDeviceName = sDeviceName;
            if (!string.IsNullOrEmpty(sDeviceUDID))
            {
                Device device = new Device(groupID);
                device.Initialize(sDeviceUDID, domainID);
                m_sDeviceName = device.m_deviceName;
                m_sDeviceUDID = sDeviceUDID;
            }
            else
            {
                m_sDeviceName = "PC";
                m_sDeviceUDID = string.Empty;
            }
            m_sType = sType;
            m_sItemCode = sItemCode;
            m_sSiteUserGUID = sUserGUID;
            m_sExtraData = sExtraData;
            m_nID = nID;
            m_dUpdateDate = dUpdate;
            m_is_channel = isChannel;
        }
        static public FavoritObject[] GetFavorites(Int32 nGroupID , string sUserGUID, int domainID, string sUDID, string sType)
        {
            FavoritObject[] ret = new FavoritObject[0];
            
            #region Get Single Media's user Favorit
            ODBCWrapper.DataSetSelectQuery selectquery = new ODBCWrapper.DataSetSelectQuery();
            selectquery += "select * from users_favorites where is_active=1 and status=1 and IS_CHANNEL=0";
            selectquery += " and ";
            selectquery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);
            if (sUDID != "")
            {
                selectquery += " and ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);
            }
            if (sType != "")
            {
                selectquery += " and ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("TYPE_CODE", "=", sType);
            }
            selectquery += "order by update_date desc";
            if (selectquery.Execute("query", true) != null)
            {
                Int32 nCount = selectquery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nID = int.Parse(selectquery.Table("query").DefaultView[i].Row["ID"].ToString());
                    string sIUserGUID = selectquery.Table("query").DefaultView[i].Row["SITE_USER_GUID"].ToString();
                    string sDeviceUDID = selectquery.Table("query").DefaultView[i].Row["DEVICE_UDID"].ToString();
                    string sIType = selectquery.Table("query").DefaultView[i].Row["TYPE_CODE"].ToString();
                    string sItemCode = selectquery.Table("query").DefaultView[i].Row["ITEM_CODE"].ToString();
                    string sExtraData = selectquery.Table("query").DefaultView[i].Row["EXTRA_DATA"].ToString();
                    string sDeviceName = string.Empty;
                    if (selectquery.Table("query").DefaultView[i].Row["DEVICE_NAME"] != System.DBNull.Value && selectquery.Table("query").DefaultView[i].Row["DEVICE_NAME"] != null)
                    {
                        sDeviceName = selectquery.Table("query").DefaultView[i].Row["DEVICE_NAME"].ToString();
                    }
                    DateTime dUpdate = (DateTime)(selectquery.Table("query").DefaultView[i].Row["UPDATE_DATE"]);
                    ret = (FavoritObject[])(TVinciShared.ProtocolsFuncs.ResizeArray(ret, ret.Length + 1));
                    //Device device = new Device();
                    //device.Initialize(sDeviceUDID, domainID, nGroupID);
                    ret[ret.Length - 1] = new FavoritObject();
                    ret[ret.Length - 1].Initialize(nID, sIUserGUID, domainID, sDeviceName, sDeviceUDID, sIType, sItemCode, sExtraData, dUpdate);
                }
            }
            selectquery.Finish();
            selectquery = null;
            #endregion
            
            #region Get Channel Media's User Favorit
            // Get Channel Media 
            ODBCWrapper.DataSetSelectQuery selectchannelquery = new ODBCWrapper.DataSetSelectQuery();
            selectchannelquery += "select * from users_favorites where is_active=1 and status=1 and IS_CHANNEL=1";
            selectchannelquery += "and"; 
            selectchannelquery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);
            
            if (sUDID != "")
            {
                selectquery += " and ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);
            }
            
            selectchannelquery += "order by update_date desc";
            if (selectchannelquery.Execute("query", true) != null)
            {
                Int32 nCount = selectchannelquery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int isChannel = int.Parse(selectchannelquery.Table("query").DefaultView[i].Row["IS_Channel"].ToString());
                    Int32 nID = int.Parse(selectchannelquery.Table("query").DefaultView[i].Row["ID"].ToString());
                    string sIUserGUID = selectchannelquery.Table("query").DefaultView[i].Row["SITE_USER_GUID"].ToString();
                    string sDeviceUDID = selectchannelquery.Table("query").DefaultView[i].Row["DEVICE_UDID"].ToString();
                    string sIType = selectchannelquery.Table("query").DefaultView[i].Row["TYPE_CODE"].ToString();
                    int sChannelCode = int.Parse(selectchannelquery.Table("query").DefaultView[i].Row["ITEM_CODE"].ToString());
                    string sExtraData = selectchannelquery.Table("query").DefaultView[i].Row["EXTRA_DATA"].ToString();
                    string sDeviceName = string.Empty;
                    if (selectchannelquery.Table("query").DefaultView[i].Row["DEVICE_NAME"] != System.DBNull.Value && selectchannelquery.Table("query").DefaultView[i].Row["DEVICE_NAME"] != null)
                    {
                        sDeviceName = selectchannelquery.Table("query").DefaultView[i].Row["DEVICE_NAME"].ToString();
                    }
                    DateTime dUpdate = (DateTime)(selectchannelquery.Table("query").DefaultView[i].Row["UPDATE_DATE"]);
                    
                    int nDeviceUDID = 0;
                    int.TryParse(sDeviceUDID,out nDeviceUDID);

                    Int32 nChannelGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("channels", "group_id", sChannelCode, "MAIN_CONNECTION_STRING").ToString());
                    TVinciShared.Channel ch = new TVinciShared.Channel(sChannelCode, false, 0, true, 0, nDeviceUDID, nChannelGroupID, "MAIN_CONNECTION_STRING");

                   
                    string MediaIDs = "";
                    if (!string.IsNullOrEmpty(sType))
                    {
                        int[] nFileType = new int[1];
                        nFileType[0] = 0;
                        int.TryParse(sType, out nFileType[0]);
                        MediaIDs = ch.GetChannelMediaIDs(0, nFileType, true, true);
                    }
                    else
                    {
                        MediaIDs = ch.GetChannelMediaIDs();
                    }
                    if (!string.IsNullOrEmpty(MediaIDs))
                    {
                        string[] MediaIDsArray = MediaIDs.Split(',');

                        for (int j = 0; j < MediaIDsArray.Length; j++)
                        {
                            ret = (FavoritObject[])(TVinciShared.ProtocolsFuncs.ResizeArray(ret, ret.Length + 1));
                            ret[ret.Length - 1] = new FavoritObject();
                            ret[ret.Length - 1].Initialize(nID, sIUserGUID, domainID, sDeviceName, sDeviceUDID, sType, MediaIDsArray[j], sExtraData, dUpdate, isChannel);
                        }
                    }
                }
            }
            selectchannelquery.Finish();
            selectchannelquery = null;
            #endregion

            return ret;
        }

        static public void RemoveFavorit(string sSiteGUID, Int32 nGroupID , Int32 nID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_favorites");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
            updateQuery += " where ";
            if (nID != 0)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                updateQuery += "and";
            }
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            updateQuery += "and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static public void RemoveFavorit(string sSiteGUID, int nGroupID, int[] nMediaIDs)
        {
            if (nMediaIDs != null)
            {
                StringBuilder sb = new StringBuilder();
                int counter = 0;
                foreach (int mediaID in nMediaIDs)
                {
                    if (counter != 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(mediaID.ToString());
                    counter++;
                }
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_favorites");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                updateQuery += " where ";
                if (!string.IsNullOrEmpty(sb.ToString()))
                {
                    updateQuery += "item_code in (";
                    updateQuery += sb.ToString();
                    updateQuery += ") and ";
                }
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                updateQuery += "and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }

        static public void RemoveChannelMediaFavorit(string sSiteGUID, int nGroupID, int[] nChannelMediaIDs)
        {
            if (nChannelMediaIDs != null)
            {
                StringBuilder sb = new StringBuilder();
                int counter = 0;
                foreach (int channelID in nChannelMediaIDs)
                {
                    if (counter != 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(channelID.ToString());
                    counter++;
                }
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_favorites");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                updateQuery += " where ";
                if (!string.IsNullOrEmpty(sb.ToString()))
                {
                    updateQuery += "item_code in (";
                    updateQuery += sb.ToString();
                    updateQuery += ") and ";
                }
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                updateQuery += "and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                updateQuery += "and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_Channel", "=", 1);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }
        public bool Save(Int32 nGroupID)
        {
            if (m_sType == "" || m_sItemCode == "" || m_sSiteUserGUID == "")
                return false;
            Int32 nRowID = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from users_favorites where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", m_sSiteUserGUID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ITEM_CODE", "=", m_sItemCode);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TYPE_CODE", "=", m_sType);
            if (string.IsNullOrEmpty(m_sDeviceUDID))
            {

                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", m_sDeviceUDID);
            }
          
           
          
           
           
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRowID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nRowID != 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_favorites");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTRA_DATA", "=", m_sExtraData);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_CHANNEL", "=", m_is_channel);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nRowID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            else
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_favorites");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTRA_DATA", "=", m_sExtraData);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                if (!string.IsNullOrEmpty(m_sDeviceUDID))
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", m_sDeviceUDID);
                }
                if (!string.IsNullOrEmpty(m_sDeviceName))
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", m_sDeviceName);
                }
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TYPE_CODE", "=", m_sType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ITEM_CODE", "=", m_sItemCode);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", m_sSiteUserGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_Channel", "=", m_is_channel);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            return true;
        }

        public string m_sDeviceUDID;
        public string m_sType;
        public string m_sItemCode;
        public string m_sSiteUserGUID;
        public DateTime m_dUpdateDate;
        public string m_sExtraData;
        public Int32 m_nID;
        public string m_sDeviceName;
        public int m_nDomainID;
        public int m_is_channel;
    }
}
