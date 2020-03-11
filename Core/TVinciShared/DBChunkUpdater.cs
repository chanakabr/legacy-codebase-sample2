using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace TVinciShared
{
    public class watchers_time_counters_row
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string m_sIdentifier;
        public watchers_time_counters_row(Int32 nWatcherID, Int32 nGroupID, DateTime dNow)
        {
            m_nWatcherID = nWatcherID;
            m_nGroupID = nGroupID;
            m_dNow = dNow;
            m_nTimeCounter = 0;
            m_nPlayTimeCounter = 0;
            m_sIdentifier = m_nWatcherID.ToString() + "|" + m_nGroupID.ToString() + "|" + m_dNow.Ticks.ToString();
        }
        public void AddCounters(Int32 nAddToTime, Int32 nAddToPlayTime)
        {
            m_nTimeCounter += nAddToTime;
            m_nPlayTimeCounter += nAddToPlayTime;
        }
        public Int32 m_nWatcherID;
        public Int32 m_nGroupID;
        public Int32 m_nTimeCounter;
        public Int32 m_nPlayTimeCounter;
        public DateTime m_dNow;
    }


    public class media_eoh_row
    {
        public string m_sIdentifier;

        public Int32 m_nWatcherID;
        public Int32 m_nDuration;
        public Int32 m_nGroupID;
        public Int32 m_nOwnerGroupID;
        public Int32 m_nMediaID;
        public Int32 m_nMediaFileID;
        public Int32 m_nCountryID;
        public Int32 m_nPlayerID;
        public Int32 m_nFileQualityID;
        public Int32 m_nFileFormatID;
        public DateTime m_dCountDate;
        public Int32 m_nPlayCounter;
        public Int32 m_nFirstPlayCounter;
        public Int32 m_nFinishCounter;
        public Int32 m_nLoadCounter;
        public Int32 m_nPauseCounter;
        public Int32 m_nStopCounter;
        public Int32 m_nFullScreenCounter;
        public Int32 m_nExitFullScreenCounter;
        public Int32 m_nSendToFriendCounter;
        public Int32 m_nPlayTimeCounter;
        public string m_sSessionID;

        public media_eoh_row(Int32 nGroupID, Int32 nOwnerGroupID,
            Int32 nMediaID, Int32 nMediaFileID, Int32 nCountryID, Int32 nPlayerID,
            Int32 nFileQualityID, Int32 nFileFormatID, DateTime dCountDate, Int32 nWatcherID, string sSessionID,
            Int32 nDuration)
        {
            m_nWatcherID = nWatcherID;
            m_nDuration = nDuration;
            m_nGroupID = nGroupID;
            m_nOwnerGroupID = nOwnerGroupID;
            m_nMediaID = nMediaID;
            m_nMediaFileID = nMediaFileID;
            m_nCountryID = nCountryID;
            m_nPlayerID = nPlayerID;
            m_nFileQualityID = nFileQualityID;
            m_nFileFormatID = nFileFormatID;
            m_dCountDate = dCountDate;
            m_nPlayCounter = 0;
            m_nFirstPlayCounter = 0;
            m_nLoadCounter = 0;
            m_nPauseCounter = 0;
            m_nStopCounter = 0;
            m_nFullScreenCounter = 0;
            m_nExitFullScreenCounter = 0;
            m_nSendToFriendCounter = 0;
            m_nPlayTimeCounter = 0;
            m_nFinishCounter = 0;
            m_sSessionID = sSessionID;

            m_sIdentifier = m_nWatcherID.ToString() + "|" + m_nDuration.ToString() + "|" + m_nGroupID.ToString() +
                m_nOwnerGroupID.ToString() + "|" + m_nMediaID.ToString() + "|" + m_nMediaFileID.ToString() +
                m_nCountryID.ToString() + "|" + m_nPlayerID.ToString() + "|" + m_nFileQualityID.ToString() +
                m_nFileFormatID.ToString() + "|" + m_dCountDate.Ticks.ToString() + "|" + m_sSessionID.ToString();
        }
        public void AddCounters(Int32 nPlayCounter, Int32 nFirstPlayCounter,
            Int32 nLoadCounter, Int32 nPauseCounter,
            Int32 nStopCounter, Int32 nFullScreenCounter,
            Int32 nExitFullScreenCounter, Int32 nSendToFriendCounter,
            Int32 nPlayTimeCounter, Int32 nFinishCounter)
        {
            m_nPlayCounter += nPlayCounter;
            m_nFirstPlayCounter += nFirstPlayCounter;
            m_nLoadCounter += nLoadCounter;
            m_nPauseCounter += nPauseCounter;
            m_nStopCounter += nStopCounter;
            m_nFullScreenCounter += nFullScreenCounter;
            m_nExitFullScreenCounter += nExitFullScreenCounter;
            m_nSendToFriendCounter += nSendToFriendCounter;
            m_nPlayTimeCounter += nPlayTimeCounter;
            m_nFinishCounter += nFinishCounter;
        }

    }

    public class WatchersTimeChunkUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        static protected bool m_bIsUpdateing;
        static protected Int32 m_nTransactions;
        static private WatchersTimeChunkUpdater m_theChunkUpdater;
        static protected object m_AdderLocker = "";
        static protected object m_DumperLocker = "";
        static protected System.Collections.Hashtable m_Rows;
        protected WatchersTimeChunkUpdater()
        {
            m_bIsUpdateing = false;
            m_nTransactions = 0;
            m_theChunkUpdater = null;
            m_Rows = new System.Collections.Hashtable();
        }

        public static WatchersTimeChunkUpdater GetInstance()
        {
            if (m_theChunkUpdater == null)
                m_theChunkUpdater = new WatchersTimeChunkUpdater();
            return m_theChunkUpdater;
        }

        protected Int32 GetCounterRecordID(Int32 nWatcherID, Int32 nGroupID, DateTime dNow)
        {
            Int32 nCounterID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from watchers_time_counters where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNT_DATE", "=", dNow);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCounterID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nCounterID;
        }

        protected void DumpRows()
        {
            lock (m_DumperLocker)
            {
                m_bIsUpdateing = true;
                System.Collections.IDictionaryEnumerator iter = m_Rows.GetEnumerator();
                while (iter.MoveNext())
                {
                    watchers_time_counters_row tmp = (watchers_time_counters_row)(iter.Value);
                    UpdateDBRow(ref (tmp));
                    System.Threading.Thread.Sleep(100);
                }
                m_Rows.Clear();
                m_nTransactions = 0;
                m_bIsUpdateing = false;
            }
        }

        protected void UpdateDBRow(ref watchers_time_counters_row theRow)
        {
            Int32 nCounterID = GetCounterRecordID(theRow.m_nWatcherID, theRow.m_nGroupID, theRow.m_dNow);
            if (nCounterID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers_time_counters");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", theRow.m_nWatcherID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", theRow.m_nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TIME_COUNTER", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNT_DATE", "=", theRow.m_dNow);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_TIME_COUNTER", "=", 0);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                nCounterID = GetCounterRecordID(theRow.m_nWatcherID, theRow.m_nGroupID, theRow.m_dNow);
            }

            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "update watchers_time_counters set TIME_COUNTER=TIME_COUNTER+" + theRow.m_nTimeCounter.ToString();
            directQuery += ",PLAY_TIME_COUNTER=PLAY_TIME_COUNTER+" + theRow.m_nPlayTimeCounter;
            directQuery += " where ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", theRow.m_nWatcherID);
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", theRow.m_nGroupID);
            directQuery += "and";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNT_DATE", "=", theRow.m_dNow);

            directQuery.Execute();
            directQuery.Finish();
            directQuery = null;
        }

        public void Update(Int32 nWatcherID, Int32 nGroupID, DateTime dNow, Int32 nAddToTime, Int32 nAddToPlayTime)
        {
            try
            {
                Int32 nUseChunk = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("site_configuration", "USE_CHUNK_READER", 1).ToString());
                if (m_bIsUpdateing == true || nUseChunk == 0)
                {
                    watchers_time_counters_row tmp = new watchers_time_counters_row(nWatcherID, nGroupID, dNow);
                    tmp.AddCounters(nAddToTime, nAddToPlayTime);
                    UpdateDBRow(ref tmp);
                }
                else
                {
                    lock (m_AdderLocker)
                    {
                        m_nTransactions++;
                        string sIdentifier = nWatcherID.ToString() + "|" + nGroupID.ToString() + "|" + dNow.Ticks.ToString();
                        if (m_Rows.Contains(sIdentifier))
                        {
                            ((watchers_time_counters_row)(m_Rows[sIdentifier])).AddCounters(nAddToTime, nAddToPlayTime);
                        }
                        else
                        {
                            watchers_time_counters_row tmp = new watchers_time_counters_row(nWatcherID, nGroupID, dNow);
                            tmp.AddCounters(nAddToTime, nAddToPlayTime);
                            m_Rows[tmp.m_sIdentifier] = tmp;
                        }
                    }
                }
                if (m_nTransactions >= 1000 || nUseChunk == 0)
                {

                    System.Threading.ThreadStart job = new System.Threading.ThreadStart(DumpRows);
                    System.Threading.Thread thread = new System.Threading.Thread(job);
                    thread.Start();
                    //DumpRows();
                    //m_bIsUpdateing = false;
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + ex.Message + " | " + ex.StackTrace, ex);
            }
        }
    }

    public class MediaEOHChunkUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        static protected bool m_bIsUpdateing;
        static protected Int32 m_nTransactions;
        static private MediaEOHChunkUpdater m_theChunkUpdater;
        static protected object m_AdderLocker = "";
        static protected object m_DumperLocker = "";
        static protected System.Collections.Hashtable m_Rows;
        protected MediaEOHChunkUpdater()
        {
            m_bIsUpdateing = false;
            m_nTransactions = 0;
            m_theChunkUpdater = null;
            m_Rows = new System.Collections.Hashtable();
        }

        public static MediaEOHChunkUpdater GetInstance()
        {
            if (m_theChunkUpdater == null)
                m_theChunkUpdater = new MediaEOHChunkUpdater();
            return m_theChunkUpdater;
        }



        protected void DumpRows()
        {
            lock (m_DumperLocker)
            {
                m_bIsUpdateing = true;
                System.Collections.IDictionaryEnumerator iter = m_Rows.GetEnumerator();
                while (iter.MoveNext())
                {
                    media_eoh_row tmp = (media_eoh_row)(iter.Value);
                    UpdateDBRow(ref (tmp));
                    System.Threading.Thread.Sleep(100);
                }
                m_Rows.Clear();
                m_nTransactions = 0;
                m_bIsUpdateing = false;
            }
        }

        static protected Int32 GetEOHStatistics(Int32 nGroupID, Int32 nOwnerGroupID,
            Int32 nMediaID, Int32 nMediaFileID, Int32 nCountryID, Int32 nPlayerID,
            Int32 nFileQualityID, Int32 nFileFormatID, DateTime dCountDate, Int32 nWatcherID, string sSessionID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from media_eoh where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("OWNER_GROUP_ID", "=", nOwnerGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_QUALITY_ID", "=", nFileQualityID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_FORMAT_ID", "=", nFileFormatID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("START_HOUR_DATE", "=", dCountDate);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sSessionID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 InsertNewEOHStatistics(Int32 nGroupID, Int32 nOwnerGroupID,
            Int32 nMediaID, Int32 nMediaFileID, Int32 nCountryID, Int32 nPlayerID,
            Int32 nFileQualityID, Int32 nFileFormatID, DateTime dCountDate, Int32 nWatcherID, string sSessionID,
            Int32 nDuration)
        {

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_eoh");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DURATION", "=", nDuration);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OWNER_GROUP_ID", "=", nOwnerGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_QUALITY_ID", "=", nFileQualityID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_FORMAT_ID", "=", nFileFormatID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_HOUR_DATE", "=", dCountDate);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_PLAY_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LOAD_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAUSE_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STOP_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FULL_SCREEN_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXIT_FULL_SCREEN_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SEND_TO_FRIEND_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_TIME_COUNTER", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sSessionID);

            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            return GetEOHStatistics(nGroupID, nOwnerGroupID, nMediaID, nMediaFileID, nCountryID,
                nPlayerID, nFileQualityID, nFileFormatID, dCountDate, nWatcherID, sSessionID);
        }

        protected void UpdateDBRow(ref media_eoh_row theRow)
        {
            Int32 nRowID = GetEOHStatistics(theRow.m_nGroupID, theRow.m_nOwnerGroupID, theRow.m_nMediaID,
                theRow.m_nMediaFileID, theRow.m_nCountryID, theRow.m_nPlayerID, theRow.m_nFileQualityID,
                theRow.m_nFileFormatID, theRow.m_dCountDate, theRow.m_nWatcherID, theRow.m_sSessionID);
            if (nRowID == 0)
                nRowID = InsertNewEOHStatistics(theRow.m_nGroupID, theRow.m_nOwnerGroupID, theRow.m_nMediaID,
                    theRow.m_nMediaFileID, theRow.m_nCountryID, theRow.m_nPlayerID, theRow.m_nFileQualityID,
                    theRow.m_nFileFormatID, theRow.m_dCountDate, theRow.m_nWatcherID, theRow.m_sSessionID,
                    theRow.m_nDuration);
            if (nRowID != 0)
            {
                bool bComma = false;
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery += "update media_eoh ";
                directQuery += " set ";
                if (theRow.m_nDuration != 0)
                {
                    bComma = true;
                    directQuery += "duration=" + theRow.m_nDuration.ToString();
                }
                if (theRow.m_nPlayCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "PLAY_COUNTER = PLAY_COUNTER + " + theRow.m_nPlayCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nFirstPlayCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "FIRST_PLAY_COUNTER = FIRST_PLAY_COUNTER + " + theRow.m_nFirstPlayCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nLoadCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "LOAD_COUNTER = LOAD_COUNTER + " + theRow.m_nLoadCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nPauseCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "PAUSE_COUNTER = PAUSE_COUNTER + " + theRow.m_nPauseCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nStopCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "STOP_COUNTER = STOP_COUNTER + " + theRow.m_nStopCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nFullScreenCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "FULL_SCREEN_COUNTER = FULL_SCREEN_COUNTER + " + theRow.m_nFullScreenCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nExitFullScreenCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "EXIT_FULL_SCREEN_COUNTER = EXIT_FULL_SCREEN_COUNTER + " + theRow.m_nExitFullScreenCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nSendToFriendCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "SEND_TO_FRIEND_COUNTER = SEND_TO_FRIEND_COUNTER + " + theRow.m_nSendToFriendCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nPlayTimeCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "PLAY_TIME_COUNTER = PLAY_TIME_COUNTER + " + theRow.m_nPlayTimeCounter.ToString();
                    bComma = true;
                }
                if (theRow.m_nFinishCounter != 0)
                {
                    if (bComma == true)
                        directQuery += ",";
                    directQuery += "FINISH_COUNTER = FINISH_COUNTER + " + theRow.m_nFinishCounter.ToString();
                    bComma = true;
                }
                directQuery += " where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nRowID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;
            }
        }

        public void Update(Int32 nGroupID, Int32 nOwnerGroupID,
            Int32 nMediaID, Int32 nMediaFileID, Int32 nCountryID, Int32 nPlayerID,
            Int32 nFileQualityID, Int32 nFileFormatID, DateTime dCountDate, Int32 nWatcherID, string sSessionID, Int32 nDuration,
            Int32 nFirstPlay, Int32 nPlay, Int32 nLoad, Int32 nPause, Int32 nStop,
            Int32 nFull, Int32 nExitFull, Int32 nSendToFriend, Int32 nPlayTime, Int32 nFinish)
        {
            try
            {
                Int32 nUseChunk = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("site_configuration", "USE_CHUNK_READER", 1).ToString());
                if (m_bIsUpdateing == true || nUseChunk == 0)
                {
                    media_eoh_row tmp = new media_eoh_row(nGroupID, nOwnerGroupID, nMediaID, nMediaFileID,
                        nCountryID, nPlayerID, nFileQualityID, nFileFormatID, dCountDate, nWatcherID,
                        sSessionID, nDuration);
                    tmp.AddCounters(nPlay, nFirstPlay, nLoad, nPause, nStop, nFull, nExitFull, nSendToFriend,
                        nPlayTime, nFinish);
                    UpdateDBRow(ref tmp);
                }
                else
                {
                    lock (m_AdderLocker)
                    {
                        m_nTransactions++;
                        string sIdentifier = nWatcherID.ToString() + "|" + nDuration.ToString() + "|" + nGroupID.ToString() +
                            nOwnerGroupID.ToString() + "|" + nMediaID.ToString() + "|" + nMediaFileID.ToString() +
                            nCountryID.ToString() + "|" + nPlayerID.ToString() + "|" + nFileQualityID.ToString() +
                            nFileFormatID.ToString() + "|" + dCountDate.Ticks.ToString() + "|" + sSessionID.ToString();
                        if (m_Rows.Contains(sIdentifier))
                        {
                            ((media_eoh_row)(m_Rows[sIdentifier])).AddCounters(nPlay, nFirstPlay, nLoad, nPause, nStop,
                                nFull, nExitFull, nSendToFriend, nPlayTime, nFinish);
                        }
                        else
                        {
                            media_eoh_row tmp = new media_eoh_row(nGroupID, nOwnerGroupID, nMediaID,
                                nMediaFileID, nCountryID, nPlayerID, nFileQualityID, nFileFormatID,
                                dCountDate, nWatcherID, sSessionID, nDuration);
                            tmp.AddCounters(nPlay, nFirstPlay, nLoad, nPause, nStop,
                                nFull, nExitFull, nSendToFriend, nPlayTime, nFinish);
                            m_Rows[tmp.m_sIdentifier] = tmp;
                        }
                    }
                }
                if (m_nTransactions >= 1000 || nUseChunk == 0)
                {
                    System.Threading.ThreadStart job = new System.Threading.ThreadStart(DumpRows);
                    System.Threading.Thread thread = new System.Threading.Thread(job);
                    thread.Start();

                    //m_bIsUpdateing = true;
                    //DumpRows();
                    //m_bIsUpdateing = false;
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + ex.Message + " | " + ex.StackTrace, ex);
            }
        }
    }
}
