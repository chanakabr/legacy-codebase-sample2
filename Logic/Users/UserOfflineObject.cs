using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace Core.Users
{
    [Serializable]
    public class UserOfflineObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        //Site User ID
        public string m_SiteUserGUID;
        //Group ID
        public Int32 m_GroupID;
        //Media ID
        public string m_MediaID;
        //Media File ID
        //public string m_MediaFileID;
        //Create Date
        public string m_CreateDate;
        //Update Date
        public string m_UpdateDate;

        public UserOfflineObject()
        {
            m_MediaID = "";
            m_SiteUserGUID = "";
            m_GroupID = 0;
            //m_MediaFileID = "";
            m_CreateDate = "";
            m_UpdateDate = "";
        }

        /// <summary>
        /// Get user offline items
        /// </summary>
        /// <param name="nGroupID">Set group ID value.</param>
        /// <param name="sSiteGuid">Set site user GUID value.</param>
        /// <param name="sFileType">Set File Type value.</param>
        /// <returns>return array of UserOfflineObject of all the media id which have file type</returns>
        public static UserOfflineObject[] GetUserOfflineMedia(Int32 nGroupID, string sSiteGuid)
        {

            UserOfflineObject[] res = null;

            //define selecct query to retrive all media file id
            ODBCWrapper.DataSetSelectQuery selectquery = new ODBCWrapper.DataSetSelectQuery();
            selectquery += "select uoi.Group_id, uoi.Media_ID as Media_ID, uoi.Create_Date as Create_Date, uoi.Update_Date as Update_Date ";
            selectquery += "from Users.dbo.users_offline_items uoi ";
            selectquery += "where uoi.status=1 and";
            selectquery += "GROUP_ID in (select id from TVinci.dbo.groups g where ";
            selectquery += ODBCWrapper.Parameter.NEW_PARAM("g.COMMERCE_GROUP_ID", "=", nGroupID);
            selectquery += ") and ";
            selectquery += ODBCWrapper.Parameter.NEW_PARAM("uoi.Site_User_Guid", "=", sSiteGuid);



            //Execute select query
            if (selectquery.Execute("query", true) != null)
            {

                Int32 nCount = selectquery.Table("query").DefaultView.Count;
                res = new UserOfflineObject[nCount];
                //
                for (int i = 0; i < nCount; i++)
                {
                    //create new user offline object and add to res array
                    UserOfflineObject offlineitem = new UserOfflineObject();

                    offlineitem.m_SiteUserGUID = sSiteGuid;
                    offlineitem.m_GroupID = int.Parse(selectquery.Table("query").DefaultView[i].Row["Group_id"].ToString());
                    offlineitem.m_MediaID = selectquery.Table("query").DefaultView[i].Row["Media_ID"].ToString();
                    offlineitem.m_CreateDate = selectquery.Table("query").DefaultView[i].Row["Create_Date"].ToString();
                    offlineitem.m_UpdateDate = selectquery.Table("query").DefaultView[i].Row["Update_Date"].ToString();
                    res[i] = offlineitem;
                }


            }

            selectquery.Finish();
            selectquery = null;

            //Write Log
            log.Debug("Get User Offline Items - Get All Media file ID by SiteGuid " + sSiteGuid + "and GroupID " + nGroupID + " to status=1");

            return res;
        }
        /// <summary>
        /// Get user offline items
        /// </summary>
        /// <param name="nGroupID">Set group ID value.</param>
        /// <param name="sSiteGuid">Set site user GUID value.</param>
        /// <param name="sFileType">Set File Type value.</param>
        /// <returns>return array of UserOfflineObject of all the media id which have file type</returns>
        //public static UserOfflineObject[] GetUserOfflineItemsByFileType(Int32 nGroupID, string sSiteGuid, string sFileType)
        //{

        //    UserOfflineObject[] res = null;

        //    //define selecct query to retrive all media file id
        //    ODBCWrapper.DataSetSelectQuery selectquery = new ODBCWrapper.DataSetSelectQuery();
        //    selectquery += "select distinct uoi.Group_id, uoi.Media_ID as Media_ID, mf.ID as Media_File_ID, uoi.Create_Date as Create_Date, uoi.Update_Date as Update_Date ";
        //    selectquery += "from Users.dbo.users_offline_items uoi ";
        //    selectquery += "inner join TVinci.dbo.media_files mf ";
        //    selectquery += "on uoi.Media_ID = mf.MEDIA_ID ";
        //    selectquery += "inner join TVinci.dbo.groups_media_type gmt ";
        //    selectquery += "on mf.MEDIA_TYPE_ID = gmt.MEDIA_TYPE_ID ";
        //    selectquery += "where uoi.status=1 and mf.status=1 and gmt.status=1 and mf.IS_ACTIVE=1 and mf.IS_ACTIVE=1 and ";
        //    selectquery += "gmt.GROUP_ID in (select id from TVinci.dbo.groups g where ";
        //    selectquery += ODBCWrapper.Parameter.NEW_PARAM("g.COMMERCE_GROUP_ID", "=", nGroupID);
        //    selectquery += ") and ";
        //    selectquery += ODBCWrapper.Parameter.NEW_PARAM("uoi.Site_User_Guid", "=", sSiteGuid);

        //    if (!string.IsNullOrEmpty(sFileType))
        //    {
        //        selectquery += "and ";
        //        selectquery += ODBCWrapper.Parameter.NEW_PARAM("gmt.DESCRIPTION", "=", sFileType);
        //    }

        //    //Execute select query
        //    if (selectquery.Execute("query", true) != null)
        //    {

        //        Int32 nCount = selectquery.Table("query").DefaultView.Count;
        //        res = new UserOfflineObject[nCount];
        //        //
        //        for (int i = 0; i < nCount; i++)
        //        {
        //            //create new user offline object and add to res array
        //            UserOfflineObject offlineitem = new UserOfflineObject();

        //            offlineitem.m_SiteUserGUID = sSiteGuid;
        //            offlineitem.m_GroupID = int.Parse(selectquery.Table("query").DefaultView[0].Row["Group_id"].ToString());
        //            offlineitem.m_MediaID = selectquery.Table("query").DefaultView[0].Row["Media_ID"].ToString();
        //            offlineitem.m_MediaFileID = selectquery.Table("query").DefaultView[0].Row["Media_File_ID"].ToString();
        //            offlineitem.m_CreateDate = selectquery.Table("query").DefaultView[0].Row["Create_Date"].ToString();
        //            offlineitem.m_UpdateDate = selectquery.Table("query").DefaultView[0].Row["Update_Date"].ToString();
        //            res[i] = offlineitem;
        //        }


        //    }

        //    selectquery.Finish();
        //    selectquery = null;

        //    //Write Log

        //    return res;
        //}

        /// <summary>
        /// Add user offline items
        /// </summary>
        /// <param name="nGroupID">Set group ID value.</param>
        /// <param name="sSiteGuid">Set site user GUID value.</param>
        /// <param name="sMediaID">Set media ID value.</param>
        /// <returns>add user insert new offline media ID</returns>
        public static bool AddUserOfflineItems(Int32 nGroupID, string sSiteGuid, string sMediaID)
        {
            bool res = false;

            nGroupID = GetGroupIDByMediaId(sMediaID);

            if (nGroupID != 0 && !string.IsNullOrEmpty(sSiteGuid) && !string.IsNullOrEmpty(sMediaID))
            {
                //Check if MediaID offline exist
                ODBCWrapper.DataSetSelectQuery selectquery = new ODBCWrapper.DataSetSelectQuery();
                selectquery += "select * from users_offline_items where ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", nGroupID);
                selectquery += " and ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("Site_User_Guid", "=", sSiteGuid);
                selectquery += " and ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("Media_ID", "=", sMediaID);

                if (selectquery.Execute("query", true) != null)
                {
                    Int32 count = selectquery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        //define update 
                        ODBCWrapper.UpdateQuery updatequery = new ODBCWrapper.UpdateQuery("users_offline_items");
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Status", "=", "1");
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Update_Date", "=", DateTime.Now);
                        updatequery += " where ";
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", nGroupID);
                        updatequery += " and ";
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Site_User_Guid", "=", sSiteGuid);
                        updatequery += " and ";
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Media_ID", "=", sMediaID);

                        //Execute update query
                        res = updatequery.Execute();
                        updatequery.Finish();
                        updatequery = null;

                        //Write Log
                        log.Debug("Add User Offline Items - Update Exist MediaID " + sMediaID + ", SiteGuid " + sSiteGuid + "and GroupID " + nGroupID + " to status=1");


                    }
                    else
                    {
                        //define Insert
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_offline_items");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Site_User_Guid", "=", sSiteGuid);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Media_ID", "=", sMediaID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", nGroupID);

                        //Execute insert query
                        res = insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;

                        //Write Log
                        log.Debug("Add User Offline Items - Add MediaID " + sMediaID + ", SiteGuid " + sSiteGuid + "and GroupID " + nGroupID);
                    }
                }
            }

            return res;
        }
        /// <summary>
        /// Remove user offline items
        /// </summary>
        /// <param name="nGroupID">Set group ID value.</param>
        /// <param name="sSiteGuid">Set site user GUID value.</param>
        /// <param name="sMediaID">Set media ID value.</param>
        /// <returns>Return true if success update specific MediaID offline item to status equal 2 else return false </returns>
        public static bool RemoveUserOfflineItems(Int32 nGroupID, string sSiteGuid, string sMediaID)
        {
            bool res = false;

            nGroupID = GetGroupIDByMediaId(sMediaID);

            if (nGroupID != 0 && !string.IsNullOrEmpty(sSiteGuid) && !string.IsNullOrEmpty(sMediaID))
            {
                //Check if MediaID offline exist
                ODBCWrapper.DataSetSelectQuery selectquery = new ODBCWrapper.DataSetSelectQuery();
                selectquery += "select * from users_offline_items where ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", nGroupID);
                selectquery += " and ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("Site_User_Guid", "=", sSiteGuid);
                selectquery += " and ";
                selectquery += ODBCWrapper.Parameter.NEW_PARAM("Media_ID", "=", sMediaID);

                if (selectquery.Execute("query", true) != null)
                {
                    Int32 count = selectquery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        //update 
                        ODBCWrapper.UpdateQuery updatequery = new ODBCWrapper.UpdateQuery("users_offline_items");
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Status", "=", "2");
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Update_Date", "=", DateTime.Now);
                        updatequery += " where ";
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", nGroupID);
                        updatequery += " and ";
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Site_User_Guid", "=", sSiteGuid);
                        updatequery += " and ";
                        updatequery += ODBCWrapper.Parameter.NEW_PARAM("Media_ID", "=", sMediaID);

                        //Execute
                        res = updatequery.Execute();
                        updatequery.Finish();
                        updatequery = null;

                        //Write Log
                        log.Debug("Remove User Offline Items - Update Exist MediaID " + sMediaID + ", SiteGuid " + sSiteGuid + "and GroupID " + nGroupID + " to status=2");
                    }
                }
            }
            return res;
        }
        /// <summary>
        /// Clear user offline items
        /// </summary>
        /// <param name="nGroupID">Set group ID value.</param>
        /// <param name="sSiteGuid">Set site user GUID value.</param>
        /// <returns>return true if success update ALL offline items status equal 2 else return false.</returns>
        public static bool ClearUserOfflineItems(Int32 nGroupID, string sSiteGuid)
        {
            bool res = false;

            if (nGroupID != 0 && !string.IsNullOrEmpty(sSiteGuid))
            {

                //remove all media id for SiteGuid 
                ODBCWrapper.UpdateQuery updatequery = new ODBCWrapper.UpdateQuery("users_offline_items");
                updatequery += ODBCWrapper.Parameter.NEW_PARAM("Status", "=", "2");
                updatequery += ODBCWrapper.Parameter.NEW_PARAM("Update_Date", "=", DateTime.Now);
                updatequery += " where ";
                updatequery += ODBCWrapper.Parameter.NEW_PARAM("Site_User_Guid", "=", sSiteGuid);

                //Execute
                res = updatequery.Execute();
                updatequery.Finish();
                updatequery = null;
                //TO DO
                //
                //Write to LOG
                log.Debug("Clear User Offline Items - Update Exist All MediaID, SiteGuid " + sSiteGuid + "and GroupID " + nGroupID + " to status=2");
            }

            return res;
        }

        private static Int32 GetGroupIDByMediaId(string sMediaID)
        {
            int res = 0;
            ODBCWrapper.DataSetSelectQuery selectquery = new ODBCWrapper.DataSetSelectQuery();
            selectquery += "select group_id from TVinci.dbo.media where is_active=1 and status=1 and ";
            selectquery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", sMediaID);

            if (selectquery.Execute("query", true) != null)
            {
                Int32 count = selectquery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    res = int.Parse(selectquery.Table("query").DefaultView[0]["group_id"].ToString());
                }
            }
            return res;

        }

        /// <summary>
        /// Get site user GUID.
        /// </summary>
        public string SiteUserGUID { get { return m_SiteUserGUID; } }
        /// <summary>
        /// Get group ID.
        /// </summary>
        public Int32 GroupID { get { return m_GroupID; } }
        /// <summary>
        /// Get media ID.
        /// </summary>
        public string MediaID { get { return m_MediaID; } }
        /// <summary>
        /// Get create date.
        /// </summary>
        public string CreateDate { get { return m_CreateDate; } }
        /// <summary>
        /// Get update date.
        /// </summary>
        public string UpdateDate { get { return m_UpdateDate; } }
    }
}
