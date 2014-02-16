using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Core.DAL;


namespace DAL
{
    public class ProtocolsFuncsDal : BaseDal
    {
        private static void HandleException(Exception ex)
        {
            //throw new NotImplementedException();
        }


        public static Dictionary<string, TimeSpan> GetLastMediaMarks(int nDomainID, int nMillisec)
        {
            Dictionary<string, TimeSpan> dictDeviceUpdateDate = null;

            if (nDomainID <= 0)
            {
                return null;
            }

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetWritable(true);
                selectQuery += "select TOP 100 GETDATE() as dNow, update_date, device_udid from users_media_mark WITH (nolock) where ";
                selectQuery += "site_user_guid in (select distinct user_id from Users..users_domains WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", nDomainID);
                selectQuery += ") order by update_date desc";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sSiteGuid);
                
                selectQuery.SetCachedSec(0);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        dictDeviceUpdateDate = new Dictionary<string, TimeSpan>();

                        for (int i = 0; i < nCount; i++)
                        {
                            string sLastUDID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "device_udid", i);

                            DateTime dLast  = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "update_date", i);
                            DateTime dNow   = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "dNow", i);
                            TimeSpan ts     = dNow.Subtract(dLast);

                            // If the current (and thus next) time span is larger than given threshold, break and return dictionary
                            if (ts.TotalMilliseconds > nMillisec)
                            {
                                return dictDeviceUpdateDate;
                            }

                            dictDeviceUpdateDate[sLastUDID] = ts;

                            //string sLastSessionID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "session_id", 0);
                        }    
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);    
            }

            return dictDeviceUpdateDate;
        }


        public static bool InsertWatchersMediaAction(int nGroupID, string sSiteGUID, int nWatcherID, int nCountryID, int nPlayerID, int nLoc, int nMediaFileID, int nMediaID, int nOwnerGroupID, int nQualityID, int nFormatID, int nBrowser, int nPlatform, int nActionID, int nBillingTypeID, int nCDNID, string sSessionID, string sUDID)
        {
            bool res = false;

            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers_media_actions");
                //insertQuery.SetLockTimeOut(10000);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                if (nMediaID != 0)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                }
                else
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", 0);
                }

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OWNER_GROUP_ID", "=", nOwnerGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_QUALITY_ID", "=", nQualityID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_FORMAT_ID", "=", nFormatID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TYPE_ID", "=", nBillingTypeID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sSessionID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTION_ID", "=", nActionID);

                if (nMediaID != 0)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", nCDNID);
                }
                else
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", 0);
                }

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LOCATION_SEC", "=", nLoc);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BROWSER", "=", nBrowser);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", nPlatform);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);

                res = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static bool UpdateMediaViews(int nMediaID)
        {
            bool res = false;

            try
            {
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                //directQuery.SetLockTimeOut(10000);
                directQuery += "update media set views=views+1 where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
                res = directQuery.Execute();
                directQuery.Finish();
                directQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static bool UpdateMediaFilesViews(int nMediaFileID)
        {
            bool res = false;

            try
            {
                ODBCWrapper.DirectQuery directMediaFilesQuery = new ODBCWrapper.DirectQuery();
                directMediaFilesQuery += "update media_files set views=views+1 where ";
                directMediaFilesQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaFileID);
                res = directMediaFilesQuery.Execute();
                directMediaFilesQuery.Finish();
                directMediaFilesQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }


        public static bool UpdateEOHStatistics(Int32 nGroupID, 
                                                Int32 nOwnerGroupID,
                                                Int32 nMediaID, 
                                                Int32 nMediaFileID, 
                                                Int32 nBillingTypeID, 
                                                Int32 nCDNID, 
                                                Int32 nCountryID, 
                                                Int32 nPlayerID,
                                                Int32 nFileQualityID, 
                                                Int32 nFileFormatID, 
                                                DateTime dCountDate, 
                                                Int32 nWatcherID, 
                                                string sSessionID, 
                                                Int32 nDuration,
                                                Int32 nFirstPlayCounter, 
                                                Int32 nPlayCounter, 
                                                Int32 nLoadCounter, 
                                                Int32 nPauseCounter, 
                                                Int32 nStopCounter,
                                                Int32 nFullScreenCounter, 
                                                Int32 nExitFullScreenCounter, 
                                                Int32 nSendToFriendCounter, 
                                                Int32 nCurrentLocation, 
                                                Int32 nFinish, 
                                                Int32 nBrowser, 
                                                Int32 nPlatform, 
                                                string sSiteGUID, 
                                                string sUDID, 
                                                string sPlayCycleID)
        {
            bool res = false;

            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_eoh");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DURATION", "=", nDuration);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OWNER_GROUP_ID", "=", nOwnerGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TYPE_ID", "=", nBillingTypeID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", nCDNID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_QUALITY_ID", "=", nFileQualityID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_FORMAT_ID", "=", nFileFormatID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_HOUR_DATE", "=", dCountDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_COUNTER", "=", nPlayCounter);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_PLAY_COUNTER", "=", nFirstPlayCounter);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LOAD_COUNTER", "=", nLoadCounter);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAUSE_COUNTER", "=", nPauseCounter);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STOP_COUNTER", "=", nStopCounter);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FULL_SCREEN_COUNTER", "=", nFullScreenCounter);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXIT_FULL_SCREEN_COUNTER", "=", nExitFullScreenCounter);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SEND_TO_FRIEND_COUNTER", "=", nSendToFriendCounter);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_TIME_COUNTER", "=", nCurrentLocation);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sSessionID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BROWSER", "=", nBrowser);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", nPlatform);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_CYCLE_ID", "=", sPlayCycleID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);

                res = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }
    }
}
