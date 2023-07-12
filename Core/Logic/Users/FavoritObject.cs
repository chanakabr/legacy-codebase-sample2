using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class FavoritObject
    {
        private const int INSERT_NUM_OF_TRIES = 2;

        public FavoritObject()
        {
            m_sDeviceName = "";
            m_sType = "";
            m_sItemCode = "";
            m_dUpdateDate = new DateTime(2000, 1, 1);
            m_dCreateDate = new DateTime(2000, 1, 1);
            m_sSiteUserGUID = "";
            m_sExtraData = "";
            m_nID = 0;
        }

        public void Initialize(Int32 nID, string sUserGUID, int domainID, string sDeviceName, string sDeviceUDID, string sType, string sItemCode, string sExtraData, DateTime dUpdate, DateTime dCreate)
        {
            m_sDeviceName = sDeviceName;
            m_sDeviceUDID = sDeviceUDID;
            m_sType = sType;
            m_sItemCode = sItemCode;
            m_sSiteUserGUID = sUserGUID;
            m_sExtraData = sExtraData;
            m_nID = nID;
            m_dUpdateDate = dUpdate;
            m_dCreateDate = dCreate;
            m_nDomainID = domainID;
            m_is_channel = 0;
        }
        public void Initialize(Int32 nID, string sUserGUID, int domainID, string sDeviceName, string sDeviceUDID, string sType, string sItemCode, string sExtraData, DateTime dUpdate, DateTime dCreate, int isChannel)
        {
            m_sDeviceName = sDeviceName;
            m_sDeviceUDID = sDeviceUDID;
            m_sType = sType;
            m_sItemCode = sItemCode;
            m_sSiteUserGUID = sUserGUID;
            m_sExtraData = sExtraData;
            m_nID = nID;
            m_dUpdateDate = dUpdate;
            m_dCreateDate = dCreate;
            m_nDomainID = domainID;
            m_is_channel = isChannel;
        }
        public void Initialize(Int32 nID, string sUserGUID, int domainID, string sDeviceUDID, string sType, string sItemCode, string sExtraData, DateTime dUpdate, int groupID)
        {
            //m_sDeviceName = sDeviceName;
            if (!string.IsNullOrEmpty(sDeviceUDID))
            {
                Device device = DeviceRepository.Get(sDeviceUDID, domainID, groupID);
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
            m_dCreateDate = dUpdate;
            m_is_channel = 0;
        }
        public void Initialize(Int32 nID, string sUserGUID, int domainID, string sDeviceUDID, string sType, string sItemCode, string sExtraData, DateTime dUpdate, int groupID, int isChannel)
        {
            //m_sDeviceName = sDeviceName;
            if (!string.IsNullOrEmpty(sDeviceUDID))
            {
                Device device = DeviceRepository.Get(sDeviceUDID, domainID, groupID);
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
        public static FavoriteResponse GetFavorites(Int32 nGroupID, string sUserGUID, int domainID, string sUDID, string sType, FavoriteOrderBy orderBy = 0)
        {
            FavoriteResponse response = new FavoriteResponse();

            //TODO: consider change to  IsUserValid ( Should move to Util.. ) ANat
            if (string.IsNullOrEmpty(sUserGUID))
            {
                return new FavoriteResponse()
                {
                    Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error"),
                    Favorites = null
                };
            }

            int nType = 0;
            if (!string.IsNullOrEmpty(sType))
                if (!int.TryParse(sType, out nType))
                {
                    return new FavoriteResponse()
                    {
                        Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error"),
                        Favorites = null
                    };
                }

            List<FavoritObject> favorits = new List<FavoritObject>();

            #region Get Single Media's user Favorit
            var table = DAL.UsersDal.Get_UserFavorites(sUserGUID, sUDID, nType, (int)orderBy);

            if (table != null && table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    int nID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                    string sDeviceUDID = ODBCWrapper.Utils.GetSafeStr(row, "DEVICE_UDID");
                    string sIType = ODBCWrapper.Utils.GetSafeStr(row, "TYPE_CODE");
                    string sItemCode = ODBCWrapper.Utils.GetSafeStr(row, "ITEM_CODE");
                    string sExtraData = ODBCWrapper.Utils.GetSafeStr(row, "EXTRA_DATA");
                    string sDeviceName = ODBCWrapper.Utils.GetSafeStr(row, "DEVICE_NAME");
                    DateTime dUpdate = ODBCWrapper.Utils.GetDateSafeVal(row, "UPDATE_DATE");
                    DateTime dCreate = ODBCWrapper.Utils.GetDateSafeVal(row, "CREATE_DATE");

                    FavoritObject fo = new FavoritObject();
                    fo.Initialize(nID, sUserGUID, domainID, sDeviceName, sDeviceUDID, sIType, sItemCode, sExtraData, dUpdate, dCreate);
                    favorits.Add(fo);
                }
            }
            #endregion

            #region Get Channel Media's User Favorit
            // Get Channel Media 
            ODBCWrapper.DataSetSelectQuery selectchannelquery = new ODBCWrapper.DataSetSelectQuery();
            selectchannelquery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectchannelquery += "select * from users_favorites where is_active=1 and status=1 and IS_CHANNEL=1";
            selectchannelquery += "and";
            selectchannelquery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sUserGUID);

            if (sUDID != "")
            {
                selectchannelquery += " and ";
                selectchannelquery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);
            }

            selectchannelquery += "order by update_date desc";
            if (selectchannelquery.Execute("query", true) != null)
            {
                Int32 nCount = selectchannelquery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nID = ODBCWrapper.Utils.GetIntSafeVal(selectchannelquery, "ID", i);
                    string sDeviceUDID = ODBCWrapper.Utils.GetStrSafeVal(selectchannelquery, "DEVICE_UDID", i);
                    string sIType = ODBCWrapper.Utils.GetStrSafeVal(selectchannelquery, "TYPE_CODE", i);
                    int sChannelCode = ODBCWrapper.Utils.GetIntSafeVal(selectchannelquery, "ITEM_CODE", i);
                    string sExtraData = ODBCWrapper.Utils.GetStrSafeVal(selectchannelquery, "EXTRA_DATA", i);
                    string sDeviceName = ODBCWrapper.Utils.GetStrSafeVal(selectchannelquery, "DEVICE_NAME", i);
                    DateTime dUpdate = ODBCWrapper.Utils.GetDateSafeVal(selectchannelquery, "UPDATE_DATE", i);
                    DateTime dCreate = ODBCWrapper.Utils.GetDateSafeVal(selectchannelquery, "CREATE_DATE", i);

                    int nDeviceUDID = 0;
                    int.TryParse(sDeviceUDID, out nDeviceUDID);

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
                            FavoritObject fo = new FavoritObject();
                            fo.Initialize(nID, sUserGUID, domainID, sDeviceName, sDeviceUDID, sIType, MediaIDsArray[j], sExtraData, dUpdate, dCreate, 1);
                            favorits.Add(fo);
                        }
                    }
                }
            }
            selectchannelquery.Finish();
            selectchannelquery = null;
            #endregion

            response = new FavoriteResponse();
            response.Favorites = favorits.Count > 0 ? favorits.ToArray<FavoritObject>() : new FavoritObject[0];
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Ok");
            return response;
        }

        public static void RemoveFavorit(string sSiteGUID, Int32 nGroupID, long nID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_favorites");
            updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
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

        static public bool RemoveFavorit(string sSiteGUID, int nGroupID, long[] nMediaIDs)
        {
            if (nMediaIDs != null)
            {
                if (nMediaIDs.Length > 0)
                {
                    var ids = nMediaIDs.ToList().ConvertAll(x => (int)x);
                    var favorites = GetFavoriteList(sSiteGUID, ids);

                    if (favorites.Count != ids.Count)
                    {
                        return false;
                    }
                }

                var sb = new StringBuilder();
                var counter = 0;
                foreach (int mediaID in nMediaIDs)
                {
                    if (counter != 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(mediaID.ToString());
                    counter++;
                }

                var isExecuteDone = false;
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_favorites");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
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
                isExecuteDone = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                return isExecuteDone;
            }
            return false;
        }

        public static void RemoveChannelMediaFavorit(string sSiteGUID, int nGroupID, int[] nChannelMediaIDs)
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
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
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
            var numOfTries = 0;
            while (true)
            {
                Int32 nRowID = 0;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "select id from users_favorites where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", m_sSiteUserGUID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ITEM_CODE", "=", m_sItemCode);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TYPE_CODE", "=", m_sType);


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
                    updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTRA_DATA", "=", m_sExtraData);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_CHANNEL", "=", m_is_channel);
                    updateQuery += "where";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nRowID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;

                    return true;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_favorites");
                    insertQuery.SetConnectionKey("USERS_CONNECTION_STRING");
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
                    var insertQueryResult = insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;

                    if (insertQueryResult)
                    {
                        return true;
                    }

                    if (numOfTries >= INSERT_NUM_OF_TRIES)
                    {
                        return false;
                    }

                    numOfTries++;
                }
            }
        }

        static public List<FavoritObject> GetFavoriteList(string userId, List<int> mediaIds, string mediaType = null)
        {
            var response = new List<FavoritObject>();
            DataTable dt = DAL.UsersDal.Get_FavoriteMediaIds(userId, mediaIds, null, mediaType, 0);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    var favorites = new FavoritObject[dt.Rows.Count];
                    FavoritObject favorite;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        favorite = new FavoritObject()
                        {
                            m_sItemCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["ID"]),
                            m_sExtraData = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["EXTRA_DATA"]),
                            m_dCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[i]["CREATE_DATE"])
                        };
                        response.Add(favorite);
                    }
                    return response;
                }
            }
            return response;
        }

        public string m_sDeviceUDID;
        public string m_sType;
        public string m_sItemCode;
        public string m_sSiteUserGUID;
        public DateTime m_dUpdateDate;
        public DateTime m_dCreateDate;
        public string m_sExtraData;
        public Int32 m_nID;
        public string m_sDeviceName;
        public int m_nDomainID;
        public int m_is_channel;
    }
}
