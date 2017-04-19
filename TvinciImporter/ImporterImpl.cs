using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.DRM;
using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using KlogMonitorHelper;
using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Tvinci.Core.DAL;
using TvinciImporter.Notification_WCF;
using TVinciShared;

namespace TvinciImporter
{
    public class ImporterImpl
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected const string ROUTING_KEY_PROCESS_IMAGE_UPLOAD = "PROCESS_IMAGE_UPLOAD\\{0}";
        protected const string ROUTING_KEY_PROCESS_FREE_ITEM_UPDATE = "PROCESS_FREE_ITEM_UPDATE\\{0}";

        private const string MISSING_EXTERNAL_IDENTIFIER = "External identifier is missing ";
        private const string MISSING_ENTRY_ID = "entry_id is missing";
        private const string MISSING_ACTION = "action is missing";
        private const string ITEM_TYPE_NOT_RECOGNIZED = "Item type not recognized";
        private const string WATCH_PERMISSION_RULE_NOT_RECOGNIZED = "Watch permission rule not recognized";
        private const string GEO_BLOCK_RULE_NOT_RECOGNIZED = "Geo block rule not recognized";
        private const string DEVICE_RULE_NOT_RECOGNIZED = "Device rule not recognized";
        private const string PLAYERS_RULE_NOT_RECOGNIZED = "Players rule not recognized ";
        private const string FAILED_DOWNLOAD_PIC = "Failed download pic";
        private const string UPDATE_INDEX_FAILED = "Update index failed";
        private const string ERROR_EXPORT_CHANNEL = "ErrorExportChannel";
        private const string MEDIA_ID_NOT_EXIST = "Media Id not exist";
        private const string EPG_SCHED_ID_NOT_EXIST = "EPG schedule id not exist";

        static string m_sLocker = "";
        static protected bool IsNodeExists(ref XmlNode theItem, string sXpath)
        {
            XmlNode theNodeVal = theItem.SelectSingleNode(sXpath);
            if (theNodeVal != null)
                return true;
            return false;
        }

        static protected string GetNodeValue(ref XmlNode theItem, string sXpath)
        {
            string sNodeVal = "";

            XmlNode theNodeVal = null;
            if (sXpath != "")
                theNodeVal = theItem.SelectSingleNode(sXpath);
            else
                theNodeVal = theItem;

            if (theNodeVal != null && theNodeVal.FirstChild != null)
                sNodeVal = theNodeVal.FirstChild.Value;

            return sNodeVal;
        }

        static protected string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
        {
            string sVal = "";
            if (theNode != null)
            {
                XmlAttributeCollection theAttr = theNode.Attributes;
                if (theAttr != null)
                {
                    Int32 nCount = theAttr.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sName = theAttr[i].Name.ToLower();
                        if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                        {
                            sVal = theAttr[i].Value.ToString();
                            break;
                        }
                    }
                }
            }
            return sVal;
        }

        static protected string GetNodeParameterVal(ref XmlNode theNode, string sXpath, string sParameterName)
        {
            string sVal = "";
            XmlNode theRoot = theNode.SelectSingleNode(sXpath);
            if (theRoot != null)
            {
                XmlAttributeCollection theAttr = theRoot.Attributes;
                if (theAttr != null)
                {
                    Int32 nCount = theAttr.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sName = theAttr[i].Name.ToLower();
                        if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                        {
                            sVal = theAttr[i].Value.ToString();
                            break;
                        }
                    }
                }
            }

            log.DebugFormat("GetNodeParameterVal parameter:{0}, value:{1}", sParameterName, sVal);
            return sVal;
        }

        static protected Int32 GetMediaIDByEPGGuid(Int32 nGroupID, string sEPGGuid)
        {
            Int32 nMediaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from media (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", sEPGGuid);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nMediaID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMediaID;
        }

        protected static bool DoesRemotePicExists(string sURL)
        {
            //Batch Upload fix
            if (!sURL.Contains("http"))
            {
                sURL = "http://" + sURL;
            }


            Int32 nStatus = 0;
            string s = Notifier.SendGetHttpReq(sURL, ref nStatus);
            if (nStatus == 200 && s.IndexOf("404") == -1)
                return true;
            return false;
        }

        static protected Int32 GetCategoryIDByCoGuid(Int32 nGroupID, string sCoGuid)
        {
            Int32 nChannelID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from categories (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nChannelID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nChannelID;
        }

        static protected Int32 GetChannelIDByCoGuid(Int32 nGroupID, string sCoGuid)
        {
            Int32 nChannelID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from channels (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nChannelID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nChannelID;
        }

        protected static Dictionary<string, string> getMediaIDsbyCoGuids(int nGroupID, string[] sCoGuids)
        {
            Dictionary<string, string> dMediaIDs = new Dictionary<string, string>();
            if (sCoGuids.Length > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetCachedSec(0);
                selectQuery += "select id, CO_GUID from media (nolock) where status=1 and CO_GUID in ( '" + string.Join("','", sCoGuids) + "' ) and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    //  nMediaIDs = new int[nCount];
                    for (int i = 0; i < nCount; i++)
                    {
                        string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                        string sSingleCoGuid = selectQuery.Table("query").DefaultView[i].Row["CO_GUID"].ToString();
                        if (!dMediaIDs.Keys.Contains(sID))
                            dMediaIDs.Add(sID, sSingleCoGuid);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            return dMediaIDs;
        }

        static protected Int32 GetMediaIDByCoGuid(Int32 nGroupID, string sCoGuid)
        {
            Int32 nMediaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from media (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nMediaID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            log.DebugFormat("GetMediaIDByCoGuid: mediaId: {0}", nMediaID);
            return nMediaID;
        }

        static protected void DeleteMedia(Int32 nMediaID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static protected void DeleteEPGSched(Int32 nEPGSchedID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nEPGSchedID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static protected Int32 GetPlayersRuleByName(Int32 nGroupID, string sName)
        {
            Int32 nID = 0;
            string sGroups = TVinciShared.PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from players_groups_types (nolock) where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sName.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        static protected Int32 GetGeoBlockRuleByName(Int32 nGroupID, string sName)
        {
            if (sName == "")
                return 0;
            Int32 nID = 0;
            string sGroups = TVinciShared.PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from geo_block_types (nolock) where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sName.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        protected static int GetDeviceRuleByName(int nGroupID, string sDeviceRule)
        {
            int retVal = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from device_rules where IS_ACTIVE = 1 and STATUS = 1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sDeviceRule.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    if (!Int32.TryParse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString(), out retVal))
                        retVal = 0;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        static protected Int32 GetWatchPerRuleByName(Int32 nGroupID, string sName)
        {
            Int32 nID = 0;
            string sGroups = TVinciShared.PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from watch_permissions_types (nolock) where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sName.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        static protected Int32 GetItemTypeIdByName(Int32 nGroupID, string sName)
        {
            Int32 nID = 0;
            string sGroups = TVinciShared.PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from media_types (nolock) where status=1 and (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sName.Trim().ToLower());
            selectQuery += "or";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(DESCRIPTION)))", "=", sName.Trim().ToLower());
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nID == 0)
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from media_types (nolock) where status=1 and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sName.Trim().ToLower());
                selectQuery += "or";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(DESCRIPTION)))", "=", sName.Trim().ToLower());
                selectQuery += ") and GROUP_ID " + sGroups;
                //selectQuery += //ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID " + sGroups);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

            }

            return nID;
        }

        static protected void AddError(ref string sErrorMessage, string sToAdd)
        {
            if (sErrorMessage != "")
                sErrorMessage += " | ";
            sErrorMessage += sToAdd;
        }

        public static DateTime GetDateTimeFromStrUTF(string sDate, DateTime dDefault)
        {
            try
            {
                string sTime = "";
                if (sDate == "")
                {
                    return dDefault;
                }

                string[] timeHour = sDate.Split(' ');
                if (timeHour.Length == 2)
                {
                    sDate = timeHour[0];
                    sTime = timeHour[1];
                }
                else
                    return DateTime.Now;
                string[] splited = sDate.Split('/');

                Int32 nYear = 1;
                Int32 nMounth = 1;
                Int32 nDay = 1;
                Int32 nHour = 0;
                Int32 nMin = 0;
                Int32 nSec = 0;
                nYear = int.Parse(splited[2].ToString());
                nMounth = int.Parse(splited[1].ToString());
                nDay = int.Parse(splited[0].ToString());
                if (timeHour.Length == 2)
                {
                    string[] splited1 = sTime.Split(':');
                    nHour = int.Parse(splited1[0].ToString());
                    nMin = int.Parse(splited1[1].ToString());
                    nSec = int.Parse(splited1[2].ToString());
                }

                return new DateTime(nYear, nMounth, nDay, nHour, nMin, nSec);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public static DateTime? GetDateTimeFromStrUTF(string sDate)
        {
            DateTime? date = null; 
            try
            {
                string sTime = "";
                if (sDate == "")
                {
                    return date;
                }

                string[] timeHour = sDate.Split(' ');
                if (timeHour.Length == 2)
                {
                    sDate = timeHour[0];
                    sTime = timeHour[1];
                }
                else
                    return date;

                string[] splited = sDate.Split('/');

                Int32 nYear = 1;
                Int32 nMounth = 1;
                Int32 nDay = 1;
                Int32 nHour = 0;
                Int32 nMin = 0;
                Int32 nSec = 0;
                nYear = int.Parse(splited[2].ToString());
                nMounth = int.Parse(splited[1].ToString());
                nDay = int.Parse(splited[0].ToString());
                if (timeHour.Length == 2)
                {
                    string[] splited1 = sTime.Split(':');
                    nHour = int.Parse(splited1[0].ToString());
                    nMin = int.Parse(splited1[1].ToString());
                    nSec = int.Parse(splited1[2].ToString());
                }

                date = new DateTime(nYear, nMounth, nDay, nHour, nMin, nSec);
            }
            catch
            {
                
            }

            return date;
        }

        static protected void ClearMediaValues(Int32 nMediaID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", "");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("description", "=", "");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", "");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCH_PERMISSION_TYPE_ID", "=", 0);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYERS_RULES", "=", 0);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BLOCK_TEMPLATE_ID", "=", 0);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("device_rule_id", "=", 0);
            for (int i = 1; i < 21; i++)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + i.ToString() + "_STR", "=", "");
            }
            for (int i = 1; i < 11; i++)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + i.ToString() + "_DOUBLE", "=", DBNull.Value);
            }
            for (int i = 1; i < 11; i++)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + i.ToString() + "_BOOL", "=", DBNull.Value);
            }
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static protected void ClearMediaFiles(Int32 nMediaID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_files");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static protected void ClearMediaTranslateValues(Int32 nMediaID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_translate");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", "");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("description", "=", "");
            for (int i = 1; i < 21; i++)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + i.ToString() + "_STR", "=", "");
            }
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);

            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;


        }

        static protected void ClearMediaTags(Int32 nMediaID, Int32 nMediaTagType)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_tags");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 2);
            updateQuery += " where ";
            //if (nMediaTagType != 0)
            //{
            updateQuery += " tag_id in (select id from tags where TAG_TYPE_ID=" + nMediaTagType.ToString() + ") and ";
            //}
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static protected void ClearMediaDates(int mediaId)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_date_metas_values");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 2);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", mediaId);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "<>", 2);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static protected bool ProcessEPGItem(XmlNode theItem, Int32 nEPGChannelId, ref Int32 nEPGSchedID, ref string sEPGIdentifier, ref string sErrorMessage, Int32 nGroupID, ref IngestAssetStatus ingestAssetStatus)
        {
            sErrorMessage = "";
            sEPGIdentifier = GetItemParameterVal(ref theItem, "epg_identifier");
            if (sEPGIdentifier == "")
            {
                AddError(ref sErrorMessage, "Missing epg_identifier");
                ingestAssetStatus.Status.Code = (int)eResponseStatus.MissingExternalIdentifier;
                ingestAssetStatus.Status.Message = MISSING_EXTERNAL_IDENTIFIER;
                return false;
            }

            //update log topic with EPGIdentifier
            if (OperationContext.Current != null && OperationContext.Current.IncomingMessageProperties != null)
            {
                MonitorLogsHelper.SetContext(Constants.TOPIC, string.Format("ingest import epg_identifier:{0}", sEPGIdentifier));
            }

            ingestAssetStatus.Status.Code = (int)eResponseStatus.OK;
            ingestAssetStatus.Status.Message = eResponseStatus.OK.ToString();
            ingestAssetStatus.ExternalAssetId = sEPGIdentifier;

            string sAction = GetItemParameterVal(ref theItem, "action").Trim().ToLower();

            if (sAction == "delete")
            {
                nEPGSchedID = GetEPGSchedIDByEPGIdentifier(nGroupID, sEPGIdentifier);
                ingestAssetStatus.InternalAssetId = nEPGSchedID;

                if (nEPGSchedID == 0)
                {
                    log.DebugFormat("ProcessEPGItem - Action:Delete Error: EPGSchedID not exist");
                    AddError(ref sErrorMessage, "Cant delete. the item is not exist");
                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.EPGSchedIdNotExist, Message = EPG_SCHED_ID_NOT_EXIST });
                    return false;
                }

                log.DebugFormat("Delete EPGSchedID:{0}", nEPGSchedID);
                DeleteEPGSched(nEPGSchedID);
            }
            else if (sAction == "insert" || sAction == "update")
            {

                Int32 nMediaID = GetMediaIDByEPGGuid(nGroupID, sEPGIdentifier); //TODO: check with Ira mediaid?

                string sStartDate = GetNodeValue(ref theItem, "start");
                string sEndDate = GetNodeValue(ref theItem, "end");

                DateTime dStartDate = GetDateTimeFromStrUTF(sStartDate, DateTime.UtcNow);
                DateTime dEndDate = GetDateTimeFromStrUTF(sEndDate, new DateTime(2099, 1, 1));

                string sMainLang = "";
                Int32 nLangID = 0;
                GetLangData(nGroupID, ref sMainLang, ref nLangID);

                XmlNode theItemName = theItem.SelectSingleNode("name");
                XmlNode theItemDesc = theItem.SelectSingleNode("description");

                string sThumb = GetNodeParameterVal(ref theItem, "thumb", "url");

                UpdateInsertBasicEPGMainLangData(nGroupID, nEPGChannelId, ref nEPGSchedID, sEPGIdentifier, dStartDate,
                    dEndDate, sThumb, sMainLang, ref theItemName, ref theItemDesc);

                UpdateInsertBasicEPGSubLangData(nGroupID, nEPGSchedID, sMainLang, ref theItemName, ref theItemDesc);
            }

            return true;
        }

        // Used for building dictionary which contain all the orderBy values depends on group
        static private Dictionary<string, Int32> GetOrderByTypeMap(Int32 nGroupID)
        {
            Dictionary<string, Int32> ret = new Dictionary<string, Int32>();

            ret.Add("random", -6);
            ret.Add("a.b.c", -11);
            ret.Add("rating", -8);
            ret.Add("views", -7);
            ret.Add("start date", -10);
            ret.Add("likes", -9);
            ret.Add("create date", -12);

            string[] META_STR_NAME = new string[20];
            string[] META_DOUBLE_NAME = new string[10];

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select META1_STR_NAME, META2_STR_NAME, META3_STR_NAME, META4_STR_NAME, META5_STR_NAME, META6_STR_NAME, META7_STR_NAME, META8_STR_NAME, META9_STR_NAME, " +
                                   "META10_STR_NAME, META11_STR_NAME, META12_STR_NAME, META13_STR_NAME, META14_STR_NAME, META15_STR_NAME, META16_STR_NAME, META17_STR_NAME, META18_STR_NAME, " +
                                   "META19_STR_NAME, META20_STR_NAME, " +
                                   "META1_DOUBLE_NAME, META2_DOUBLE_NAME, META3_DOUBLE_NAME, META4_DOUBLE_NAME, META5_DOUBLE_NAME, META6_DOUBLE_NAME, META7_DOUBLE_NAME, " +
                                   "META8_DOUBLE_NAME, META9_DOUBLE_NAME, META10_DOUBLE_NAME" +
                                   " from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                for (int i = 0; i < 20; ++i)
                {
                    META_STR_NAME[i] = selectQuery.Table("query").DefaultView[0].Row["META" + (i + 1).ToString() + "_STR_NAME"].ToString().ToLower();
                    ret[META_STR_NAME[i]] = i + 1;
                }

                for (int i = 0; i < 10; ++i)
                {
                    META_DOUBLE_NAME[i] = selectQuery.Table("query").DefaultView[0].Row["META" + (i + 1).ToString() + "_DOUBLE_NAME"].ToString().ToLower();
                    ret[META_DOUBLE_NAME[i].ToLower()] = i + 21;
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return ret;
        }

        enum ChannelType
        {
            auto = 1,
            manual
        };

        enum OrderDirection
        {
            asc = 1,
            desc
        }
        static public bool ProcessCategoryItems(XmlDocument theItem, ref string sCoGuid, ref Int32 nChannelID, ref string sErrorMessage, Int32 nGroupID)
        {
            bool bOK = true;

            StringReader sr = new StringReader(theItem.OuterXml);
            XmlReader reader = XmlReader.Create(sr);

            XmlSerializer serializer = new XmlSerializer(typeof(CategoriesSchema.feed));
            CategoriesSchema.feed deserializedCategories;
            try
            {
                deserializedCategories = serializer.Deserialize(reader) as CategoriesSchema.feed;
            }
            catch
            {
                return false;
            }

            int currentGroupID = nGroupID;

            // loop all over the channels in deserializedCategories object
            for (int i = 0; i < deserializedCategories.export.Length; ++i)
            {
                // get parameters -> category node
                sCoGuid = deserializedCategories.export[i].co_guid;
                string sOrderNumber = deserializedCategories.export[i].order_number;
                string sUrlPic = deserializedCategories.export[i].basic.thumb;
                //nGroupID = deserializedCategories.export[i].basic.is_virtual != "" ? int.Parse(deserializedCategories.export[i].basic.is_virtual) : currentGroupID;

                CategoriesSchema.value[] theName = deserializedCategories.export[i].basic.name;
                CategoriesSchema.value[] theUniqueName = deserializedCategories.export[i].basic.unique_name;
                CategoriesSchema.value[] theDescription = deserializedCategories.export[i].basic.description;

                string sMainLang = "";
                Int32 nLangID = 0;
                GetLangData(nGroupID, ref sMainLang, ref nLangID);

                Int32 categorylID = GetCategoryIDByCoGuid(nGroupID, sCoGuid);

                CategoryUpdateInsertBasicMainLangData(nGroupID, ref categorylID, sMainLang, ref theName, ref theUniqueName, ref theDescription, sCoGuid, sUrlPic, Int32.Parse(sOrderNumber));

                CategoriesSchema.channel[] cahnnels = deserializedCategories.export[i].channels;
                UpdateCategoryChannels(nGroupID, categorylID, ref cahnnels);

                CategoriesSchema.inner_category[] theInnerCategories = deserializedCategories.export[i].inner_categories;
                ProccessCategoryChildNodes(nGroupID, categorylID, ref theInnerCategories);
            }

            //get value from tcm
            int rootCategoryID = TVinciShared.WS_Utils.GetTcmIntValue("ROOT_CATEGORY_ID");
            DAL.ImporterImpDAL.StartCategoriesTransaction(rootCategoryID);

            return bOK;
        }

        // check if child exist if so update it's parent category id if not make one and update it's patent category id
        static protected void ProccessCategoryChildNodes(Int32 nGroupID, Int32 parentCategoryID, ref CategoriesSchema.inner_category[] childs)
        {
            for (int i = 0; i < childs.Length; ++i)
            {
                // check if category exist
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select ID from categories where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", childs[i].co_guid);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);

                // check if child exist if so update it's parent category id if not make one and update it's patent category id
                if (selectQuery.Execute("query", true) != null)
                {
                    if (selectQuery.Table("query").DefaultView.Count != 0)
                    {
                        // update child node
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("categories");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_CATEGORY_ID", "=", parentCategoryID);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                        updateQuery += " and ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", childs[i].co_guid);
                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                    else
                    {
                        // create child node
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("categories");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", childs[i].co_guid);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_CATEGORY_ID", "=", parentCategoryID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                        insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
        }

        static protected void UpdateCategoryChannels(Int32 groupID, Int32 categoryID, ref CategoriesSchema.channel[] channels)
        {
            for (int i = 0; i < channels.Length; ++i)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select ID from channels where IS_ACTIVE = 1 and STATUS = 1 and";

                // find the channel if exist
                int channelID = -1;
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                if (channels[i].id != null && channels[i].id != "")
                {
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", int.Parse(channels[i].id));
                }
                if (channels[i].co_guid != null && channels[i].co_guid != "")
                {
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", channels[i].co_guid);
                }
                if (channels[i].name != null && channels[i].name != "")
                {
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", channels[i].name);
                }
                if (selectQuery.Execute("query", true) != null)
                {
                    if (selectQuery.Table("query").DefaultView.Count != 0)
                    {
                        channelID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }

                selectQuery.Finish();
                selectQuery = null;

                if (channelID != -1)
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select ID from categories_channels where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", channelID);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CATEGORY_ID", "=", categoryID);

                    // check if exist in categories_channels, if so update the data, otherwise insert the new channel
                    if (selectQuery.Execute("query", true) != null)
                    {
                        if (selectQuery.Table("query").DefaultView.Count != 0)
                        {
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("categories_channels");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", Int32.Parse(channels[i].position));
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", channelID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CATEGORY_ID", "=", categoryID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        else
                        {
                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("categories_channels");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", channelID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", Int32.Parse(channels[i].position));
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CATEGORY_ID", "=", categoryID);
                            insertQuery.Execute();
                            insertQuery.Finish();
                            insertQuery = null;
                        }
                    }
                }
            }
        }

        static protected void CategoryUpdateInsertBasicMainLangData(Int32 nGroupID, ref Int32 categoryID, string sMainLang, ref CategoriesSchema.value[] theItemNames,
                                                                    ref CategoriesSchema.value[] theItemUniqueNames, ref CategoriesSchema.value[] theItemDesc, string sCoGuid,
                                                                    string sThumb, Int32 orderNumber)
        {
            string sName = GetValMainLanguage(theItemNames, sMainLang);
            string sUniqueName = GetValMainLanguage(theItemUniqueNames, sMainLang);
            string sDescription = GetValMainLanguage(theItemDesc, sMainLang);

            if (categoryID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("categories");
                if (sName != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CATEGORY_NAME", "=", sName);
                if (sUniqueName != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ADMIN_NAME", "=", sUniqueName);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_CATEGORY_ID", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", orderNumber);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                categoryID = GetCategoryIDByCoGuid(nGroupID, sCoGuid);

                int nPicID = DownloadPic(sThumb, sName, nGroupID, categoryID, sMainLang, "THUMBNAIL", true, 0);
                log.Debug("TespIngest - EndDownloadPic");
                if (nPicID != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("categories");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", categoryID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
            }
            else
            {
                int nPicID = DownloadPic(sThumb, sName, nGroupID, categoryID, sMainLang, "THUMBNAIL", true, 0);
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("categories");
                if (sName != "")
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CATEGORY_NAME", "=", sName);
                if (sUniqueName != "")
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ADMIN_NAME", "=", sUniqueName);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
                if (nPicID != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", orderNumber);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", categoryID);

                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }

        // TODO: move public to protected AC
        static public bool ProcessChannelItems(XmlDocument theItem, ref string sCoGuid, ref Int32 nChannelID, ref string sErrorMessage, Int32 nGroupID)
        {
            try
            {
                Dictionary<string, Int32> orderByTypeMap = GetOrderByTypeMap(nGroupID);
                bool bOK = true;

                StringReader sr = new StringReader(theItem.OuterXml);
                XmlReader reader = XmlReader.Create(sr);

                XmlSerializer serializer = new XmlSerializer(typeof(ChannelsSchema.feed));
                ChannelsSchema.feed deserializedChannels;
                try
                {
                    deserializedChannels = serializer.Deserialize(reader) as ChannelsSchema.feed;
                }
                catch (Exception ex)
                {
                    log.Error("File ProcessChannelItems - Error in desiralization of the xml", ex);
                    return false;
                }

                int currentGroupID = nGroupID;

                // loop all over the channels in deserializedCategories object
                for (int i = 0; i < deserializedChannels.export.Length; ++i)
                {
                    // get parameters -> channel node
                    if (deserializedChannels.export[i] != null)
                    {
                        string sType = string.Empty;
                        string sAction = string.Empty;
                        string sOrderDirection = "asc";
                        ChannelsSchema.value[] theName = new ChannelsSchema.value[1];
                        theName[0] = new ChannelsSchema.value();
                        ChannelsSchema.value[] theUniqueName = new ChannelsSchema.value[1];
                        theUniqueName[0] = new ChannelsSchema.value();
                        ChannelsSchema.value[] theDescription = new ChannelsSchema.value[1];
                        theDescription[0] = new ChannelsSchema.value();
                        string sCutTagsType = string.Empty;
                        string sMediaType = string.Empty;
                        string sOrderBy = string.Empty;
                        if (!string.IsNullOrEmpty(deserializedChannels.export[i].co_guid))
                            sCoGuid = deserializedChannels.export[i].co_guid;
                        if (!string.IsNullOrEmpty(deserializedChannels.export[i].type))
                            sType = deserializedChannels.export[i].type;
                        Int32 nType = (Int32)(sType == "auto" ? ChannelType.auto : ChannelType.manual);
                        if (!string.IsNullOrEmpty(deserializedChannels.export[i].action))
                            sAction = deserializedChannels.export[i].action;
                        if (!string.IsNullOrEmpty(deserializedChannels.export[i].order_direction))
                            sOrderDirection = deserializedChannels.export[i].order_direction;
                        Int32 nOrderDirection = (Int32)(sOrderDirection == "asc" ? OrderDirection.asc : OrderDirection.desc);
                        if (deserializedChannels.export[i].basic != null && deserializedChannels.export[i].basic.name != null)
                            theName = deserializedChannels.export[i].basic.name;
                        if (!string.IsNullOrEmpty(deserializedChannels.export[i].order_by))
                            sOrderBy = deserializedChannels.export[i].order_by;
                        Int32 nOrderBy = 0;
                        if (orderByTypeMap.ContainsKey(sOrderBy.ToLower()))
                        {
                            nOrderBy = orderByTypeMap[sOrderBy.ToLower()];
                        }
                        else
                        {
                            nOrderBy = -12; // use defualt value
                        }

                        // get parameters -> basic node(metas)
                        bool sEnableFeed = false;
                        int nEnableFeed = 0;
                        string sPictureURL = string.Empty;
                        if (deserializedChannels.export[i].basic != null)
                        {
                            sEnableFeed = deserializedChannels.export[i].basic.enable_feed;
                            nEnableFeed = sEnableFeed == true ? 1 : 0;
                            if (!string.IsNullOrEmpty(deserializedChannels.export[i].basic.is_virtual))
                                nGroupID = deserializedChannels.export[i].basic.is_virtual != "" ? int.Parse(deserializedChannels.export[i].basic.is_virtual) : currentGroupID;
                            if (!string.IsNullOrEmpty(deserializedChannels.export[i].basic.thumb))
                                sPictureURL = deserializedChannels.export[i].basic.thumb;
                            if (deserializedChannels.export[i].basic.name != null)
                                theName = deserializedChannels.export[i].basic.name;
                            if (deserializedChannels.export[i].basic.unique_name != null)
                                theUniqueName = deserializedChannels.export[i].basic.unique_name;
                            if (deserializedChannels.export[i].basic.description != null)
                                theDescription = deserializedChannels.export[i].basic.description;
                        }
                        string sMainLang = "";
                        Int32 nLangID = 0;
                        GetLangData(nGroupID, ref sMainLang, ref nLangID);

                        Int32 channelID = GetChannelIDByCoGuid(nGroupID, sCoGuid);

                        Int32 nIsAnd = 0;
                        if (sType == "auto")
                        {
                            if (deserializedChannels.export[i].structure != null)
                            {
                                if (!string.IsNullOrEmpty(deserializedChannels.export[i].structure.cut_tags_type))
                                    sCutTagsType = deserializedChannels.export[i].structure.cut_tags_type;
                                nIsAnd = sCutTagsType == "and" ? 1 : 0;

                                if (!string.IsNullOrEmpty(deserializedChannels.export[i].structure.media_type))
                                    sMediaType = deserializedChannels.export[i].structure.media_type;
                            }
                        }

                        ChannelUpdateInsertBasicMainLangData(nGroupID, ref channelID, sMainLang, ref theName, ref theUniqueName, ref theDescription, nType, sCoGuid, sPictureURL, nEnableFeed,
                                                                    nOrderBy, nOrderDirection, nIsAnd);

                        UpdateInsertChannelBasicSubLangData(nGroupID, channelID, sMainLang, ref theName, ref theUniqueName, ref theDescription);

                        // get parameters -> structure node -- if it's an automated channel, collect all the structure data, otherwise collect all the media's
                        if (sType == "auto")
                        {
                            if (deserializedChannels.export[i].structure != null)
                            {
                                if (deserializedChannels.export[i].structure.strings != null)
                                {
                                    ChannelsSchema.meta[] sStringsMetas = deserializedChannels.export[i].structure.strings;
                                    UpdateStringChannelMainLangData(nGroupID, channelID, sMainLang, ref sStringsMetas);
                                    UpdateChannelStringSubLangData(nGroupID, channelID, sMainLang, ref sStringsMetas);
                                }
                                if (deserializedChannels.export[i].structure.doubles != null)
                                {
                                    ChannelsSchema.meta[] sDoublesMetas = deserializedChannels.export[i].structure.doubles;
                                    UpdateChannelDoublesData(nGroupID, channelID, sMainLang, ref sDoublesMetas, ref sErrorMessage);
                                }
                                if (deserializedChannels.export[i].structure.booleans != null)
                                {
                                    ChannelsSchema.meta[] sBooleansMetas = deserializedChannels.export[i].structure.booleans;
                                    UpdateChannelBoolsData(nGroupID, channelID, sMainLang, ref sBooleansMetas, ref sErrorMessage);
                                }
                                if (deserializedChannels.export[i].structure.tags_metas != null)
                                {
                                    ChannelsSchema.tags_meta[] sTagsMetas = deserializedChannels.export[i].structure.tags_metas;
                                    UpdateChannelTags(nGroupID, channelID, sMainLang, ref sTagsMetas, ref sErrorMessage);
                                }
                            }
                        }
                        else if (sType == "manual")
                        {
                            UpdateMedias(nGroupID, channelID, deserializedChannels.export[i].medias);
                        }

                        UpdateChannelIndex(LoginManager.GetLoginGroupID(), new List<int>() { channelID }, ApiObjects.eAction.Update);
                    }
                }

                UtilsDal.YesDeleteChannelsByOfferID();

                return bOK;
            }
            catch (Exception ex)
            {
                log.Error("File ProcessChannelItems - Exception during parsing the xml:" + ex.Message, ex);
                return false;
            }
        }

        static protected void UpdateMedias(Int32 nGroupID, Int32 channelID, ChannelsSchema.media[] theMedias)
        {
            // move all the channels media status 4 (not active)
            ODBCWrapper.UpdateQuery updateClearQuery = new ODBCWrapper.UpdateQuery("channels_media");
            updateClearQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 4);
            updateClearQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
            updateClearQuery += " where ";
            updateClearQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", channelID);
            updateClearQuery.Execute();
            updateClearQuery.Finish();
            updateClearQuery = null;

            //generate Dictionary with the CO_GUID and the order_number
            Dictionary<string, string> dCoGuids_OrderNum = new Dictionary<string, string>();
            foreach (ChannelsSchema.media media in theMedias)
            {
                if (!dCoGuids_OrderNum.Keys.Contains(media.ID))
                    dCoGuids_OrderNum.Add(media.ID, media.order_number);
            }
            //get the media IDs according to the co_guids. Key = the media ID, Value = the CO_GUID
            Dictionary<string, string> dMediaIDs = getMediaIDsbyCoGuids(nGroupID, dCoGuids_OrderNum.Keys.ToArray());

            if (dMediaIDs.Count > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select ID, MEDIA_ID from channels_media where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", channelID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += " and MEDIA_ID in (" + string.Join(",", (dMediaIDs.Keys.ToList()).ToArray()) + " ) ";
                selectQuery.SetCachedSec(0);
                List<string> lUpdatedMediaIDs = new List<string>();
                if (selectQuery.Execute("query", true) != null)
                {
                    // update all the relevant medias 
                    for (int j = 0; j < selectQuery.Table("query").DefaultView.Count; j++)
                    {
                        string sID = selectQuery.Table("query").DefaultView[j].Row["ID"].ToString();
                        string sMediaID = selectQuery.Table("query").DefaultView[j].Row["MEDIA_ID"].ToString();
                        if (!lUpdatedMediaIDs.Contains(sMediaID))//update the row only if there is no other row with this channel+media_ID combination
                        {
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels_media");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", int.Parse(dCoGuids_OrderNum[dMediaIDs[sMediaID]]));
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", int.Parse(sID));
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        //generate a list with all medias that were updated 
                        if (!lUpdatedMediaIDs.Contains(sMediaID))
                            lUpdatedMediaIDs.Add(sMediaID);
                    }
                }

                //remove from ditionary every media that was updated
                foreach (string sMediaIDInList in lUpdatedMediaIDs)
                {
                    if (dMediaIDs.Keys.Contains(sMediaIDInList))
                        dMediaIDs.Remove(sMediaIDInList);
                }

                // clear and null
                selectQuery.Finish();
                selectQuery = null;

                //insert all the medias that are left into channels_media
                foreach (string sMediaID in dMediaIDs.Keys.ToList())
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("channels_media");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", int.Parse(sMediaID));
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", int.Parse(dCoGuids_OrderNum[dMediaIDs[sMediaID]]));
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", channelID);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }
            }
        }

        static protected void UpdateChannelTags(Int32 nGroupID, Int32 nChannelID, string sMainLang, ref ChannelsSchema.tags_meta[] theTags, ref string sError)
        {
            Int32 nCount = theTags.Length;
            for (int i = 0; i < nCount; i++)
            {
                ChannelsSchema.tags_meta theItem = theTags[i];
                string sName = theItem.name;

                TranslatorStringHolder tagsHolder = new TranslatorStringHolder();
                ChannelsSchema.container[] theContainers = theItem.container;
                Int32 nCount1 = theContainers.Length;

                for (int j = 0; j < nCount1; j++)
                {
                    ChannelsSchema.container theContainer = theContainers[j];
                    string sVal = GetValMainLanguage(theContainer.value, sMainLang);
                    if (sVal == "")
                    {
                        AddError(ref sError, "tag :" + sName + " - no main language value");
                        continue;
                    }
                    tagsHolder.AddLanguageString(sMainLang, sVal, j.ToString(), true);  ///i->j

                    GetChannelSubLangMetaData(nGroupID, sMainLang, ref tagsHolder, ref theContainer, j.ToString());  ///i->j
                }
                Int32 nTagTypeID = GetTagTypeID(nGroupID, sName);
                if (nCount1 > 0)
                {
                    if (nTagTypeID != 0 || sName.ToLower().Trim() == "free")
                        IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", nTagTypeID.ToString(), "int", "ID", "tags", "channel_tags", "channel_id", "tag_id", "true", sMainLang, tagsHolder, nGroupID, nChannelID);

                }
            }
        }

        static protected void GetChannelSubLangMetaData(Int32 nGroupID, string sMainLang, ref TranslatorStringHolder metaHolder, ref ChannelsSchema.container theContainer, string sID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock), group_extra_languages g where g.LANGUAGE_ID=ll.id and g.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ll.code3", "<>", sMainLang);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["code3"].ToString();
                    string sVal = GetValMainLanguage(theContainer.value, sLang);
                    if (sVal != "")
                        metaHolder.AddLanguageString(sLang, sVal, sID, false);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected void UpdateInsertChannelBasicSubLangData(Int32 nGroupID, Int32 channelID,
            string sMainLang, ref ChannelsSchema.value[] theItemNames, ref ChannelsSchema.value[] theItemUniqueNames, ref ChannelsSchema.value[] theItemDesc)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock), group_extra_languages g where g.LANGUAGE_ID=ll.id and g.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(ll.code3)))", "<>", sMainLang.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.group_id", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                    Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    string sChannelName = GetValMainLanguage(theItemNames, sLang);
                    string sChannelDesc = GetValMainLanguage(theItemDesc, sLang);
                    Int32 nChannelTransID = 0;
                    bool b = false;
                    if (sChannelName.Trim() != "" || sChannelDesc.Trim() != "")
                        nChannelTransID = GetChannelTranslateID(channelID, nLangID, ref b, true);
                    else
                        nChannelTransID = GetChannelTranslateID(channelID, nLangID, ref b, false);
                    if (nChannelTransID != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channel_translate");
                        if (sChannelName != "")
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sChannelName);
                        if (sChannelDesc != "")
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sChannelDesc);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                        updateQuery += "where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nChannelTransID);

                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                }
            }

            selectQuery.Finish();
            selectQuery = null;
        }

        static protected void UpdateChannelStringSubLangData(Int32 nGroupID, Int32 channelID, string sMainLang, ref ChannelsSchema.meta[] theStrings)
        {
            if (theStrings == null)
            {
                return;
            }

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock), group_extra_languages g where g.LANGUAGE_ID=ll.id and g.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(ll.code3)))", "<>", sMainLang.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.group_id", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                    Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());

                    Int32 nChannelTransID = 0;
                    bool b = false;
                    nChannelTransID = GetChannelTranslateID(channelID, nLangID, ref b, false);
                    if (nChannelTransID != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channel_translate");
                        bool bExecute = false;
                        Int32 nCount1 = theStrings.Length;
                        for (int j = 0; j < nCount1; j++)
                        {
                            ChannelsSchema.meta theItem = theStrings[j];
                            string sName = theItem.name;
                            string sValue = GetValMainLanguage(theItem.value, sLang);
                            string sMainValue = GetValMainLanguage(theItem.value, sMainLang);

                            Int32 nMetaID = GetStringMetaIDByMetaName(nGroupID, sName);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sValue);
                            bExecute = true;
                        }
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nChannelTransID);
                        if (bExecute == true)
                        {
                            updateQuery.Execute();
                        }
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                }
            }
        }

        static protected bool UpdateChannelBoolsData(Int32 nGroupID, Int32 channelID, string sMainLang, ref ChannelsSchema.meta[] theBools, ref string sError)
        {
            if (theBools == null)
            {
                return false;
            }

            Int32 nCount = theBools.Length;
            if (nCount <= 0)
            {
                return false;
            }

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels");
            bool bExecute = false;

            for (int i = 0; i < nCount; i++)
            {
                ChannelsSchema.meta theItem = theBools[i];
                string sName = theItem.name;
                string sMainValue = GetValMainLanguage(theItem.value, sMainLang);

                Int32 nMetaID = GetBoolMetaIDByMetaName(nGroupID, sName);
                try
                {
                    if (nMetaID != 0)
                    {
                        if (sMainValue.Trim().ToLower() == "1" || sMainValue.Trim().ToLower() == "true")
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_BOOL", "=", 1);
                        else if (sMainValue.Trim().ToLower() == "0" || sMainValue.Trim().ToLower() == "false")
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_BOOL", "=", 0);
                        else
                            AddError(ref sError, "On processing boolean value: " + sName + " The values are not boolean ");
                    }
                }
                catch (Exception ex)
                {
                    AddError(ref sError, "On processing boolean value: " + sName + " exception: " + ex.Message);
                }
                bExecute = true;
            }
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelID);
            if (bExecute == true)
            {
                updateQuery.Execute();
            }
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }

        static protected bool UpdateChannelDoublesData(Int32 nGroupID, Int32 channelID, string sMainLang, ref ChannelsSchema.meta[] theDoubles, ref string sError)
        {
            if (theDoubles == null)
            {
                return false;
            }

            Int32 nCount = theDoubles.Length;
            if (nCount <= 0)
            {
                return false;
            }

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels");
            bool bExecute = false;

            for (int i = 0; i < nCount; i++)
            {
                ChannelsSchema.meta theItem = theDoubles[i];
                string sName = theItem.name;
                string sMainValue = GetValMainLanguage(theItem.value, sMainLang);

                Int32 nMetaID = GetDoubleMetaIDByMetaName(nGroupID, sName);
                if (nMetaID != 0)
                {
                    try
                    {
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_DOUBLE", "=", double.Parse(sMainValue));
                    }
                    catch (Exception ex)
                    {
                        AddError(ref sError, "On processing double value: " + sName + " exception: " + ex.Message);
                        sError = ex.Message;
                    }

                    bExecute = true;
                }
            }
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelID);
            if (bExecute == true)
            {
                updateQuery.Execute();
            }
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }

        static protected bool UpdateStringChannelMainLangData(Int32 nGroupID, Int32 channelID, string sMainLang, ref ChannelsSchema.meta[] theStrings)
        {
            if (theStrings == null)
            {
                return false;
            }

            Int32 nCount = theStrings.Length;
            if (nCount <= 0)
            {
                return false;
            }

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels");
            bool bExecute = false;
            for (int i = 0; i < nCount; i++)
            {
                ChannelsSchema.meta theItem = theStrings[i];
                string sName = theItem.name;
                string sMainValue = GetValMainLanguage(theItem.value, sMainLang);

                Int32 nMetaID = GetStringMetaIDByMetaName(nGroupID, sName);
                if (nMetaID > 0)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_STR", "=", sMainValue);
                    bExecute = true;
                }
            }
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelID);
            if (bExecute == true)
                updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }

        static private string GetValMainLanguage(CategoriesSchema.value[] values, string sMainLang)
        {
            string sRet = string.Empty;

            foreach (CategoriesSchema.value val in values)
            {
                if (val.lang == sMainLang)
                {
                    sRet = val.Value;
                    break;
                }
            }
            return sRet;
        }

        static private string GetValMainLanguage(ChannelsSchema.value[] values, string sMainLang)
        {
            string sRet = string.Empty;

            foreach (ChannelsSchema.value val in values)
            {
                if (val.lang == sMainLang)
                {
                    sRet = val.Value;
                    break;
                }
            }
            return sRet;
        }

        static protected void ChannelUpdateInsertBasicMainLangData(Int32 nGroupID, ref Int32 channelID, string sMainLang, ref ChannelsSchema.value[] theItemNames,
                                                                    ref ChannelsSchema.value[] theItemUniqueNames, ref ChannelsSchema.value[] theItemDesc, Int32 nItemType, string sCoGuid,
                                                                    string sThumb, Int32 enableFeed, Int32 orderType, Int32 orderDirection, Int32 isAnd)
        {
            string sName = GetValMainLanguage(theItemNames, sMainLang);
            string sUniqueName = GetValMainLanguage(theItemUniqueNames, sMainLang);
            string sDescription = GetValMainLanguage(theItemDesc, sMainLang);

            if (channelID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("channels");
                if (sName != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
                if (sUniqueName != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ADMIN_NAME", "=", sUniqueName);
                if (sDescription != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
                if (nItemType != 0)
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_TYPE", "=", nItemType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_BY_TYPE", "=", orderType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_BY_DIR", "=", orderDirection);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RSS", "=", enableFeed);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_AND", "=", isAnd);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by auto importer process");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                channelID = GetChannelIDByCoGuid(nGroupID, sCoGuid);

                int nPicID = DownloadPic(sThumb, sName, nGroupID, channelID, sMainLang, "THUMBNAIL", true, 0);
                log.Debug("TespIngest - EndDownloadPic");
                if (nPicID != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
            }
            else
            {
                int nPicID = DownloadPic(sThumb, sName, nGroupID, channelID, sMainLang, "THUMBNAIL", true, 0);
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels");
                if (sName != "")
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
                if (sUniqueName != "")
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ADMIN_NAME", "=", sUniqueName);
                if (sDescription != "")
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
                if (nItemType != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_TYPE", "=", nItemType);
                if (nPicID != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_BY_TYPE", "=", orderType);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_BY_DIR", "=", orderDirection);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RSS", "=", enableFeed);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_AND", "=", isAnd);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by auto importer process");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelID);

                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

        }

        static protected bool ProcessItem(XmlNode theItem, ref string sCoGuid, ref Int32 nMediaID, ref string sErrorMessage, Int32 nGroupID, ref bool isActive, ref IngestAssetStatus ingestAssetStatus)
        {
            sErrorMessage = "";

            sCoGuid = GetItemParameterVal(ref theItem, "co_guid");
            if (string.IsNullOrEmpty(sCoGuid))
            {
                AddError(ref sErrorMessage, "Missing co_guid");
                ingestAssetStatus.Status.Code = (int)eResponseStatus.MissingExternalIdentifier;
                ingestAssetStatus.Status.Message = MISSING_EXTERNAL_IDENTIFIER;
                return false;
            }

            //update log topic with media's co guid
            if (OperationContext.Current != null && OperationContext.Current.IncomingMessageProperties != null)
            {
                MonitorLogsHelper.SetContext(Constants.TOPIC, string.Format("ingest import co_guid:{0}", sCoGuid));
            }

            ingestAssetStatus.ExternalAssetId = sCoGuid;
            ingestAssetStatus.Status.Code = (int)eResponseStatus.OK;
            ingestAssetStatus.Status.Message = eResponseStatus.OK.ToString();

            string entryId = GetItemParameterVal(ref theItem, "entry_id");
            ingestAssetStatus.EntryID = entryId;
            if (string.IsNullOrEmpty(entryId))
            {
                ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MissingEntryId, Message = MISSING_ENTRY_ID });
            }

            string sAction = GetItemParameterVal(ref theItem, "action").Trim().ToLower();
            if (string.IsNullOrEmpty(sAction))
            {
                ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MissingAction, Message = MISSING_ACTION });
            }

            string sIsActive = GetItemParameterVal(ref theItem, "is_active").Trim().ToLower();
            isActive = sIsActive.Trim().ToLower() == "true";

            if (string.IsNullOrEmpty(sIsActive))
            {
                log.DebugFormat("ProcessItem media co-guid: {0}, isActive: {1}.", sCoGuid, isActive.ToString());
            }

            nMediaID = GetMediaIDByCoGuid(nGroupID, sCoGuid);

            if (sAction == "delete")
            {
                if (nMediaID == 0)
                {
                    log.Debug("ProcessItem - Action:Delete Error: media not exist");
                    AddError(ref sErrorMessage, "Cant delete. the item is not exist");
                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MediaIdNotExist, Message = MEDIA_ID_NOT_EXIST });
                    return false;
                }
                //delete media
                log.DebugFormat("Delete Media:{0}", nMediaID);
                DeleteMedia(nMediaID);
            }
            else if (sAction == "insert" || sAction == "update")
            {
                string sEraseFiles = GetNodeParameterVal(ref theItem, ".", "erase");
                if (nMediaID != 0 && sEraseFiles != "false")
                {
                    ClearMediaValues(nMediaID);
                    ClearMediaTranslateValues(nMediaID);
                    ClearMediaDates(nMediaID);
                    //ClearMediaTags(nMediaID , 0);
                    ClearMediaFiles(nMediaID);
                    log.DebugFormat("ProcessItem - Action insert/update clear media files, values.. mediaId:{0}", nMediaID);
                }

                string sItemType = GetNodeValue(ref theItem, "basic/media_type");
                log.DebugFormat("ProcessItem media co-guid: {0}, media_type: {1}.", sCoGuid, sItemType);

                Int32 nItemType = GetItemTypeIdByName(nGroupID, sItemType);

                if (nItemType == 0 && sEraseFiles != "false")
                {
                    AddError(ref sErrorMessage, "Item type not recognized");
                    log.DebugFormat("ProcessItem - Item type not recognized. mediaId:{0}", nMediaID);
                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.NotRecognizedItemType, Message = ITEM_TYPE_NOT_RECOGNIZED });
                }

                string sEpgIdentifier = GetNodeValue(ref theItem, "basic/epg_identifier");
                string sWatchPerRule = GetNodeValue(ref theItem, "basic/rules/watch_per_rule");
                Int32 nWatchPerRule = GetWatchPerRuleByName(nGroupID, sWatchPerRule);
                if (nWatchPerRule == 0 && sWatchPerRule.Trim() != "")
                {
                    AddError(ref sErrorMessage, "Watch permission rule not recognized");
                    log.DebugFormat("ProcessItem - Watch permission rule not recognized. mediaId:{0}, WatchPerRule:{1}", nMediaID, sWatchPerRule);
                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.NotRecognizedWatchPermissionRule, Message = WATCH_PERMISSION_RULE_NOT_RECOGNIZED });
                }

                string sGeoBlockRule = GetNodeValue(ref theItem, "basic/rules/geo_block_rule");
                Int32 nGeoBlockRule = GetGeoBlockRuleByName(nGroupID, sGeoBlockRule);
                if (nGeoBlockRule == 0 && sGeoBlockRule.Trim() != "")
                {
                    AddError(ref sErrorMessage, "Geo block rule not recognized");
                    log.DebugFormat("ProcessItem - Geo block rule not recognized. mediaId:{0}, GeoBlockRule:{1}", nMediaID, sGeoBlockRule);
                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.NotRecognizedGeoBlockRule, Message = GEO_BLOCK_RULE_NOT_RECOGNIZED });
                }

                string sDeviceRule = GetNodeValue(ref theItem, "basic/rules/device_rule");
                Int32 nDeviceRule = GetDeviceRuleByName(nGroupID, sDeviceRule);
                if (nDeviceRule == 0 && sDeviceRule.Trim().Length > 0)
                {
                    AddError(ref sErrorMessage, "Device rule not recognized");
                    log.DebugFormat("ProcessItem - Device rule not recognized. mediaId:{0}, DeviceRule:{1}", nMediaID, sDeviceRule);
                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.NotRecognizedDeviceRule, Message = DEVICE_RULE_NOT_RECOGNIZED });
                }

                string sPlayersRule = GetNodeValue(ref theItem, "basic/rules/players_rule");
                Int32 nPlayersRule = GetPlayersRuleByName(nGroupID, sPlayersRule);
                if (nPlayersRule == 0 && sPlayersRule.Trim() != "")
                {
                    AddError(ref sErrorMessage, "Players rule not recognized");
                    log.DebugFormat("ProcessItem - Players rule not recognized. mediaId:{0}, PlayersRule:{1}", nMediaID, sPlayersRule);
                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.NotRecognizedPlayersRule, Message = PLAYERS_RULE_NOT_RECOGNIZED });
                }

                string sCatalogStartDate = GetNodeValue(ref theItem, "basic/dates/catalog_start");
                string sStartDate = GetNodeValue(ref theItem, "basic/dates/start");
                string sCreateDate = GetNodeValue(ref theItem, "basic/dates/create");
                string sCatalogEndDate = GetNodeValue(ref theItem, "basic/dates/catalog_end");
                string sFinalEndDate = GetNodeValue(ref theItem, "basic/dates/final_end");

                DateTime dStartDate = GetDateTimeFromStrUTF(sStartDate, DateTime.UtcNow);
                DateTime dCatalogStartDate = GetDateTimeFromStrUTF(sCatalogStartDate, dStartDate);//catalog_start_date default value is start_date
                DateTime dCreate = GetDateTimeFromStrUTF(sCreateDate, DateTime.UtcNow);
                DateTime dCatalogEndDate = GetDateTimeFromStrUTF(sCatalogEndDate, new DateTime(2099, 1, 1));
                DateTime dFinalEndDate = GetDateTimeFromStrUTF(sFinalEndDate, dCatalogEndDate);

                string sThumb = GetNodeParameterVal(ref theItem, "basic/thumb", "url");

                XmlNode theItemName = theItem.SelectSingleNode("basic/name");
                XmlNode theItemDesc = theItem.SelectSingleNode("basic/description");
                XmlNodeList thePicRatios = theItem.SelectNodes("basic/pic_ratios/ratio");
                XmlNodeList theStrings = theItem.SelectNodes("structure/strings/meta");
                XmlNodeList theDoubles = theItem.SelectNodes("structure/doubles/meta");
                XmlNodeList theBools = theItem.SelectNodes("structure/booleans/meta");
                XmlNodeList theDates = theItem.SelectNodes("structure/dates/meta");
                XmlNodeList theMetas = theItem.SelectNodes("structure/metas/meta");
                XmlNodeList theFiles = theItem.SelectNodes("files/file");


                string sMainLang = "";
                Int32 nLangID = 0;
                GetLangData(nGroupID, ref sMainLang, ref nLangID);

                UpdateInsertBasicMainLangData(nGroupID, ref nMediaID, nItemType, sCoGuid, sEpgIdentifier, nWatchPerRule, nGeoBlockRule,
                    nPlayersRule, nDeviceRule, dCatalogStartDate, dStartDate, dCatalogEndDate, dFinalEndDate, sMainLang, ref theItemName,
                    ref theItemDesc, isActive, dCreate, entryId);

                //update InternalAssetId 
                ingestAssetStatus.InternalAssetId = nMediaID;

                // get all ratio and ratio's pic url from input xml
                Dictionary<string, string> ratioStrThumb = SetRatioStrThumb(thePicRatios);

                Dictionary<int, List<string>> ratioSizesList = new Dictionary<int, List<string>>();
                Dictionary<int, string> ratiosThumb = new Dictionary<int, string>();
                //get all ratio/sizes needed for DownloadPic 
                SetRatioIdsWithPicUrl(nGroupID, ratioStrThumb, out ratioSizesList, out ratiosThumb);

                //set default ratio with size
                if (!string.IsNullOrEmpty(sThumb))
                {
                    log.DebugFormat("ProcessItem - Thumb Url:{0}, mediaId:{1}", sThumb, nMediaID);
                    theItemName = DownloadThumbPic(nMediaID, nGroupID, sThumb, theItemName, sMainLang, ratiosThumb);
                }

                int picId = 0;
                foreach (int ratioKey in ratiosThumb.Keys)
                {
                    picId = DownloadPic(ratiosThumb[ratioKey], string.Empty, nGroupID, nMediaID, sMainLang, "RATIOPIC", false, ratioKey, ratioSizesList[ratioKey]);
                    if (picId == 0)
                    {
                        ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.FailedDownloadPic, Message = FAILED_DOWNLOAD_PIC });
                    }
                }
                UpdateInsertBasicSubLangData(nGroupID, nMediaID, sMainLang, ref theItemName, ref theItemDesc);
                UpdateStringMainLangData(nGroupID, nMediaID, sMainLang, ref theStrings);
                UpdateStringSubLangData(nGroupID, nMediaID, sMainLang, ref theStrings);
                UpdateDoublesData(nGroupID, nMediaID, sMainLang, ref theDoubles, ref sErrorMessage);
                UpdateBoolsData(nGroupID, nMediaID, sMainLang, ref theBools, ref sErrorMessage);
                UpdateDatesData(nGroupID, nMediaID, ref theDates, ref sErrorMessage);
                UpdateMetas(nGroupID, nMediaID, sMainLang, ref theMetas, ref sErrorMessage);
                UpdateFiles(nGroupID, sMainLang, nMediaID, ref theFiles, ref sErrorMessage);

                ProtocolsFuncs.SeperateMediaTexts(nMediaID);
            }

            return true;
        }

        private static XmlNode DownloadThumbPic(int mediaId, int groupId, string thumb, XmlNode itemName, string mainLang, Dictionary<int, string> ratiosThumb)
        {
            string sName = GetMultiLangValue(mainLang, ref itemName);

            List<string> sizes = GetMediaPicSizesExclude(groupId, ratiosThumb.Keys.ToList());
            sizes.Add("full");
            sizes.Add("tn");
            Int32 nPicID = DownloadPic(thumb, sName, groupId, mediaId, mainLang, "THUMBNAIL", true, 0, sizes);
            if (nPicID != 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_PIC_ID", "=", nPicID);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", mediaId);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            return itemName;
        }
        private static void SetRatioIdsWithPicUrl(int groupId, Dictionary<string, string> ratioStrThumb, out Dictionary<int, List<string>> ratioSizesList, out Dictionary<int, string> ratiosThumb)
        {
            ratioSizesList = new Dictionary<int, List<string>>();
            ratiosThumb = new Dictionary<int, string>();

            if (ratioStrThumb.Count > 0)
            {
                int ratioID = 0;
                string ratioStr = string.Empty;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id, ratio from lu_pics_ratios where is_active = 1 and status = 1 and ";
                selectQuery += "ratio in " + string.Format("('{0}')", string.Join("','", ratioStrThumb.Keys.Select(x => x.ToString()).ToArray()));
                if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView.Count > 0)
                {
                    for (int rowIndex = 0; rowIndex < selectQuery.Table("query").DefaultView.Count; rowIndex++)
                    {
                        ratioID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", rowIndex);
                        ratioStr = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "ratio", rowIndex);
                        if (ratioID > 0)
                        {
                            // collect ratio Ids
                            ratiosThumb.Add(ratioID, ratioStrThumb[ratioStr]);
                            //get ratio Sizes
                            ratioSizesList.Add(ratioID, GetMediaPicSizes(groupId, ratioID));
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
        }

        private static Dictionary<string, string> SetRatioStrThumb(XmlNodeList thePicRatios)
        {
            //set ratioStrThumb dictionary with ratioStr and pic url
            //  10:7  ,  https://....
            Dictionary<string, string> ratioStrThumb = new Dictionary<string, string>();
            if (thePicRatios != null)
            {
                int ratiosCount = thePicRatios.Count;
                XmlNode ratioItem = null;
                for (int i = 0; i < ratiosCount; i++)
                {
                    ratioItem = thePicRatios[i];
                    string picStr = GetItemParameterVal(ref ratioItem, "thumb");
                    string ratioStr = GetItemParameterVal(ref ratioItem, "ratio");
                    if (!string.IsNullOrEmpty(picStr) && !string.IsNullOrEmpty(ratioStr) && !ratioStrThumb.ContainsKey(ratioStr))
                    {
                        ratioStrThumb.Add(ratioStr, picStr);
                    }
                }
            }
            return ratioStrThumb;
        }

        static protected string GetPicBaseName(int picID, int groupID)
        {
            string retVal = string.Empty; ;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select base_url from pics (nolock) where STATUS=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", picID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    retVal = selectQuery.Table("query").DefaultView[0].Row["base_url"].ToString();
                }
            }
            if (!string.IsNullOrEmpty(retVal) && retVal.IndexOf('.') > 0)
            {
                int index = retVal.IndexOf('.');
                retVal = retVal.Substring(0, index);
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        static protected Int32 DoesPicExists(string sPicBaseName, Int32 nGroupID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from pics (nolock) where STATUS=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("description", "=", sPicBaseName);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 DoesEPGPicExists(string sPicBaseName, Int32 nGroupID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from EPG_pics (nolock) where STATUS=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("description", "=", sPicBaseName);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static public Int32 DownloadEPGPic(string sThumb, string sName, Int32 nGroupID, Int32 nEPGSchedID, int nChannelID, int ratioID = 0)
        {
            int picId = 0;

            if (string.IsNullOrEmpty(sThumb))
            {
                log.Debug("File download - picture name is empty. nChannelID: " + nChannelID.ToString());
                return 0;
            }

            // use old/or image queue
            if (WS_Utils.IsGroupIDContainedInConfig(nGroupID, "USE_OLD_IMAGE_SERVER", ';'))
            {
                string sUseQueue = TVinciShared.WS_Utils.GetTcmConfigValue("downloadPicWithQueue");
                if (!string.IsNullOrEmpty(sUseQueue) && sUseQueue.ToLower().Equals("true"))
                {
                    picId = DownloadEPGPicToQueue(sThumb, sName, nGroupID, nEPGSchedID, nChannelID, ratioID);
                }
                else
                {
                    picId = DownloadEPGPicToUploader(sThumb, sName, nGroupID, nEPGSchedID, nChannelID, ratioID);
                }
            }
            else
            {
                // use new image server
                picId = DownloadEPGPicToImageServer(sThumb, sName, nGroupID, nChannelID, ratioID);
            }

            if (picId == 0)
                log.ErrorFormat("Failed download pic- channelID:{0}, ratioId{1}, url:{2}", nChannelID, ratioID, sThumb);
            else
            {
                if (WS_Utils.IsGroupIDContainedInConfig(nGroupID, "USE_OLD_IMAGE_SERVER", ';'))
                    log.DebugFormat("Successfully download pic- channelID:{0}, ratioId{1}, url:{2}", nChannelID, ratioID, sThumb);
                else
                    log.DebugFormat("Successfully processed image - channelID:{0}, ratioId{1}, url:{2}", nChannelID, ratioID, sThumb);
            }

            return picId;
        }

        public static bool InsertNewEPGMultiPic(string epgIdentifier, int picID, int ratioID, int nGroupID, int nChannelID)
        {
            try
            {
                bool result = Tvinci.Core.DAL.EpgDal.InsertNewEPGMultiPic(epgIdentifier, picID, ratioID, nGroupID, nChannelID);
                return result;
            }
            catch (Exception ex)
            {
                log.Error("InsertNewEPGMultiPic - " +
                    string.Format("fail to insert picid to epg multi pictures ex={0}, epgIdentifier={1}, picID={2}, ratioID={3}, nChannelID={4},nGroupID ={5}", ex.Message, epgIdentifier, picID, ratioID, nChannelID, nGroupID),
                    ex);
                return false;
            }
        }

        static public Int32 DownloadEPGPicToUploader(string sThumb, string sName, Int32 nGroupID, Int32 nEPGSchedID, int nChannelID, int ratioID)
        {
            if (sThumb.Trim() == "")
                return 0;

            string sBasePath = GetBasePath(nGroupID);
            string sPicBaseName1 = getPictureFileName(sThumb);

            Int32 nPicID = 0;
            string picName = string.Format("{0}_{1}_{2}", nChannelID, ratioID, sPicBaseName1);
            nPicID = DoesEPGPicExists(picName, nGroupID);

            if (nPicID == 0)
            {
                string sUploadedFile = "";
                lock (m_sLocker)
                {
                    sUploadedFile = TVinciShared.ImageUtils.DownloadWebImage(sThumb, sBasePath);
                }
                if (sUploadedFile == "")
                    return 0;
                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                if (nExtractPos > 0)
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);

                string sPicBaseName = TVinciShared.ImageUtils.GetDateImageName();

                List<ImageManager.ImageObj> images = new List<ImageManager.ImageObj>();
                ImageManager.ImageObj tnImage = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.THUMB, 90, 65, sUploadedFileExt);
                ImageManager.ImageObj fullImage = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.FULL, 0, 0, sUploadedFileExt);
                images.Add(tnImage);
                images.Add(fullImage);

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from epg_pics_sizes (nolock) where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (ratioID > 0)
                {
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
                }

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;

                    for (int nI = 0; nI < nCount; nI++)
                    {

                        int nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WIDTH", nI);
                        int nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "HEIGHT", nI);
                        ImageManager.ImageObj image = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.SIZE, nWidth, nHeight, sUploadedFileExt);
                        images.Add(image);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                //chnage the second parameter to BaseURL 
                string sFullImagePath = sBasePath + "/pics/" + sUploadedFile;
                if (sThumb.Contains("http://") || sThumb.Contains("https://"))
                {
                    sFullImagePath = sThumb;
                }
                string sDestImagePath = sBasePath + "/pics/" + nGroupID.ToString();

                bool downloadRes = ImageManager.ImageHelper.DownloadAndCropImage(nGroupID, sFullImagePath, sDestImagePath, images, sPicBaseName, sUploadedFileExt);

                if (downloadRes)
                {
                    foreach (ImageManager.ImageObj image in images)
                    {
                        if (image.eResizeStatus == ImageManager.ResizeStatus.SUCCESS)
                        {
                            UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, image.ToString());
                        }
                    }

                    nPicID = InsertNewEPGPic(sName, picName, sPicBaseName + sUploadedFileExt, nGroupID);
                }
            }
            return nPicID;
        }

        static public Int32 DownloadEPGPicToQueue(string sThumb, string sName, Int32 nGroupID, Int32 nEPGSchedID, int nChannelID, int ratioID)
        {
            //string sBasePath = GetBasePath(nGroupID);
            string sBasePath = ImageUtils.getRemotePicsURL(nGroupID);
            string picName = string.Empty;
            int picId = 0;

            GetEpgPicNameAndId(sThumb, nGroupID, nChannelID, ratioID, out picName, out picId);

            if (picId == 0)
            {
                string sUploadedFileExt = ImageUtils.GetFileExt(sThumb);
                string sPicNewName = TVinciShared.ImageUtils.GetDateImageName();
                string[] sPicSizes = getEPGPicSizes(nGroupID, ratioID);

                bool bIsUpdateSucceeded = ImageUtils.SendPictureDataToQueue(sThumb, sPicNewName, sBasePath, sPicSizes, nGroupID);

                picId = InsertNewEPGPic(sName, picName, sPicNewName + sUploadedFileExt, nGroupID);  //insert with sPicName instead of full path
            }
            return picId;
        }

        public static int DownloadEPGPicToImageServer(string thumb, string name, int groupID, int channelID, int ratioID, bool isAsync = true, int? updaterId = null, string epgIdentifier = null)
        {
            int version = 0;
            string picName = string.Empty;
            int picId = 0;

            //check if thumb Url exist
            string checkImageUrl = WS_Utils.GetTcmConfigValue("CheckImageUrl");
            if (!string.IsNullOrEmpty(checkImageUrl) && checkImageUrl.ToLower().Equals("true"))
            {
                if (!ImageUtils.IsUrlExists(thumb))
                {
                    log.ErrorFormat("DownloadPicToImageServer thumb Uri not valid: {0} ", thumb);
                    return picId;
                }
            }

            // in case ratio Id = 0 get default group's ratio
            if (ratioID <= 0)
            {
                ratioID = ImageUtils.GetGroupDefaultEpgRatio(groupID);
            }

            //check for epg_image default threshold value
            int pendingThresholdInMinutes = 0;
            var epgImagePendingThresholdInMinutes = WS_Utils.GetTcmConfigValue("epgImagePendingThresholdInMinutes");
            int.TryParse(epgImagePendingThresholdInMinutes, out pendingThresholdInMinutes);

            int activeThresholdInMinutes = 0;
            var epgImageActiveThresholdInMinutes = WS_Utils.GetTcmConfigValue("epgImageActiveThresholdInMinutes");
            int.TryParse(epgImageActiveThresholdInMinutes, out activeThresholdInMinutes);

            GetEpgPicNameAndId(thumb, groupID, channelID, ratioID, out picName, out picId);

            if (picId == 0)
            {
                string sPicNewName = TVinciShared.ImageUtils.GetDateImageName();

                picId = CatalogDAL.InsertEPGPic(groupID, name, picName, sPicNewName);

                if (picId > 0)
                {

                    if (isAsync)
                    {
                        SendImageDataToImageUploadQueue(thumb, groupID, version, picId, sPicNewName, eMediaType.EPG);
                    }
                    else
                    {
                        int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupID);
                        ImageServerUploadRequest imageServerReq = new ImageServerUploadRequest() { GroupId = parentGroupId, Id = sPicNewName, SourcePath = thumb, Version = version };

                        // post image
                        string result = Utils.HttpPost(ImageUtils.GetImageServerUrl(groupID, eHttpRequestType.Post), JsonConvert.SerializeObject(imageServerReq), "application/json");

                        // check result
                        if (string.IsNullOrEmpty(result) || result.ToLower() != "true")
                        {
                            ImageUtils.UpdateImageState(groupID, picId, version, eMediaType.EPG, eTableStatus.Failed, updaterId);
                            picId = 0;
                        }
                        else if (result.ToLower() == "true")
                        {
                            ImageUtils.UpdateImageState(groupID, picId, version, eMediaType.EPG, eTableStatus.OK, updaterId);

                            // Update EpgMultiPictures
                            EpgDal.UpdateEPGMultiPic(groupID, epgIdentifier, channelID, picId, ratioID, updaterId);

                            log.DebugFormat("post image success. picId {0} ", picId);
                        }
                    }
                }
                else
                {
                    log.ErrorFormat("Error while creating new EpgPic, thumb {0}", thumb);
                }
            }
            else
            {
                log.DebugFormat("EpgPic exists, thumb {0}, picId {1}", thumb, picId);
            }

            return picId;
        }

        private static void SendImageDataToImageUploadQueue(string sourcePath, int groupId, int version, int picId, string picNewName, eMediaType mediaType)
        {
            try
            {
                // generate ImageUploadData and send to Queue 
                int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupId);

                // get image server URL
                string imageServerUrl = ImageUtils.GetImageServerUrl(groupId, eHttpRequestType.Post);
                if (string.IsNullOrEmpty(imageServerUrl))
                    throw new Exception(string.Format("IMAGE_SERVER_URL wasn't found. GID: {0}", groupId));

                if (sourcePath.ToLower().Trim().StartsWith("http://") == false &&
                sourcePath.ToLower().Trim().StartsWith("https://") == false)
                {
                    sourcePath = ImageUtils.getRemotePicsURL(groupId) + sourcePath;
                }

                ImageUploadData data = new ImageUploadData(parentGroupId, picNewName, version, sourcePath, picId, imageServerUrl, mediaType);

                var queue = new ImageUploadQueue();

                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_IMAGE_UPLOAD, parentGroupId));

                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of image upload. data: {0}", data);
                }
                else
                {
                    log.DebugFormat("successfully enqueue image upload. data: {0}", data);
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed image upload: Exception:{0} ", exc);
            }
        }

        private static void GetEpgPicNameAndId(string thumb, int groupID, int channelID, int ratioID, out string picName, out int picId, int pendingThresholdInMinutes = 0, int activeThresholdInMinutes = 0)
        {
            picName = getPictureFileName(thumb);
            picId = 0;
            picName = string.Format("{0}_{1}_{2}", channelID, ratioID, picName);
            picId = CatalogDAL.GetEpgPicsData(groupID, picName);
        }

        //this function is not used (old)
        static protected Int32 DownloadEPGPic(string sThumb, string sName, Int32 nGroupID, Int32 nEPGSchedID, string sMainLang)
        {
            if (sThumb.Trim() == "")
                return 0;

            string sBasePath = GetBasePath(nGroupID);

            char[] delim = { '/' };
            string[] splited1 = sThumb.Split(delim);
            string sPicBaseName1 = splited1[splited1.Length - 1];
            if (sPicBaseName1.IndexOf("?") != -1 && sPicBaseName1.IndexOf("uuid") != -1)
            {
                Int32 nStart = sPicBaseName1.IndexOf("uuid=", 0) + 5;
                Int32 nEnd = sPicBaseName1.IndexOf("&", nStart);
                if (nEnd != 4)
                    sPicBaseName1 = sPicBaseName1.Substring(nStart, nEnd - nStart);
                else
                    sPicBaseName1 = sPicBaseName1.Substring(nStart);
                sPicBaseName1 += ".jpg";
            }

            Int32 nPicID = 0;
            nPicID = DoesEPGPicExists(sPicBaseName1, nGroupID);

            //string sPicName = sName;
            if (nPicID == 0)
            {
                string sUploadedFile = "";
                lock (m_sLocker)
                {
                    sUploadedFile = TVinciShared.ImageUtils.DownloadWebImage(sThumb, sBasePath);
                }
                if (sUploadedFile == "")
                    return 0;
                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                if (nExtractPos > 0)
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);

                string sPicBaseName = TVinciShared.ImageUtils.GetDateImageName();

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from epg_pics_sizes (nolock) where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;

                    TVinciShared.ImageUtils.ResizeImageAndSave(sBasePath + "/pics/" + sUploadedFile, sBasePath + "/pics/" + nGroupID.ToString() + "/" + sPicBaseName + "_tn" + sUploadedFileExt, 90, 65, true);
                    UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_tn" + sUploadedFileExt);

                    TVinciShared.ImageUtils.RenameImage(sBasePath + "/pics/" + sUploadedFile, sBasePath + "/pics/" + nGroupID.ToString() + "/" + sPicBaseName + "_full" + sUploadedFileExt);
                    UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_full" + sUploadedFileExt);

                    for (int nI = 0; nI < nCount; nI++)
                    {
                        string sWidth = selectQuery.Table("query").DefaultView[nI].Row["WIDTH"].ToString();
                        string sHeight = selectQuery.Table("query").DefaultView[nI].Row["HEIGHT"].ToString();
                        string sEndName = sWidth + "X" + sHeight;

                        string sTmpImage1 = sBasePath + "/pics/" + nGroupID.ToString() + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;

                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePath + "/pics/" + sUploadedFile, sTmpImage1, int.Parse(sWidth), int.Parse(sHeight), true);
                        UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_" + sEndName + sUploadedFileExt);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;


                nPicID = InsertNewEPGPic(sName, sUploadedFile, sPicBaseName + sUploadedFileExt, nGroupID);
                //Int32 nPicTagID = InsertNewPicTag(sMediaName, sUploadedFile, sPicBaseName, nGroupID);
            }
            if (nPicID != 0)
            {
                //IngestionUtils.M2MHandling("ID", "", "", "", "ID", "tags", "pics_tags", "pic_id", "tag_id", "true", sMainLang, sName, nGroupID, nPicID, false);
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nEPGSchedID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            return nPicID;
        }

        static public Int32 DownloadPic(string sPic, string sMediaName, Int32 nGroupID, Int32 nMediaID, string sMainLang, string sPicType, bool bSetMediaThumb)
        {
            return DownloadPic(sPic, sMediaName, nGroupID, nMediaID, sMainLang, sPicType, bSetMediaThumb, 0);
        }

        static public Int32 DownloadPic_old(string sPic, string sMediaName, Int32 nGroupID, Int32 nMediaID, string sMainLang, string sPicType, bool bSetMediaThumb, int ratioID)
        {
            log.Debug("File downloaded - Start Download Pic: " + " " + sPic + " " + "MediaID: " + nMediaID.ToString() + " RatioID :" + ratioID.ToString());
            if (sPic.Trim() == "")
            {
                return 0;
            }
            string sBasePath = GetBasePath(nGroupID);

            log.Debug("File download - Base Path is " + sBasePath);

            object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
            string sPicsBasePath = string.Empty;

            if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
                sPicsBasePath = oPicsBasePath.ToString();

            char[] delim = { '/' };
            string[] splited1 = sPic.Split(delim);
            string sPicBaseName1 = splited1[splited1.Length - 1];
            if (sPicBaseName1.IndexOf("?") != -1 && sPicBaseName1.IndexOf("uuid") != -1)
            {
                Int32 nStart = sPicBaseName1.IndexOf("uuid=", 0) + 5;
                Int32 nEnd = sPicBaseName1.IndexOf("&", nStart);
                if (nEnd != 4)
                    sPicBaseName1 = sPicBaseName1.Substring(nStart, nEnd - nStart);
                else
                    sPicBaseName1 = sPicBaseName1.Substring(nStart);
                sPicBaseName1 += ".jpg";
            }

            Int32 nPicID = 0;

            //////////////////////////////////2 asstes direct to 1 pic issue
            //if (ratioID == 0)
            //{
            //    nPicID = DoesPicExists(sPicBaseName1, nGroupID);
            //}

            //else
            //{
            //    object oPic = ODBCWrapper.Utils.GetTableSingleVal("media", "media_pic_id", nMediaID);
            //    if (oPic != null && oPic != DBNull.Value && !string.IsNullOrEmpty(oPic.ToString()))
            //        nPicID = int.Parse(oPic.ToString());
            //}
            //nPicID = 0; 

            string sPicName = sMediaName;
            bool doesPicExist = true;
            log.Debug("TespIngest - Directory Creation Started " + sBasePath + "/pics/" + nGroupID.ToString());
            if (!Directory.Exists(sBasePath + "/pics/" + nGroupID.ToString() + "/"))
            {
                Directory.CreateDirectory(sBasePath + "/pics/" + nGroupID.ToString() + "/");
            }
            log.Debug("TespIngest - Directory Creation Finished " + sBasePath + "/pics/" + nGroupID.ToString());
            if (nPicID == 0)
            {
                doesPicExist = false;
                string sUploadedFile = TVinciShared.ImageUtils.DownloadWebImage(sPic, sBasePath);
                if (sUploadedFile == "")
                {
                    sUploadedFile = TVinciShared.ImageUtils.DownloadWebImage(sPicsBasePath + "/" + sPic, sBasePath);
                    if (sUploadedFile == "")
                    {
                        return 0;
                    }
                }
                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                if (nExtractPos > 0)
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);

                string sPicBaseName = string.Empty;
                if (ratioID > 0)
                {
                    sPicBaseName = TVinciShared.ImageUtils.GetDateImageName(nMediaID);
                }
                if (string.IsNullOrEmpty(sPicBaseName))
                {
                    sPicBaseName = TVinciShared.ImageUtils.GetDateImageName();
                }
                string sTmpImage = sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt;
                bool bExists = System.IO.File.Exists(sTmpImage);
                Int32 nAdd = 0;
                while (bExists)
                {
                    if (sPicBaseName.IndexOf("_") != -1)
                        sPicBaseName = sPicBaseName.Substring(0, sPicBaseName.IndexOf("_"));
                    sPicBaseName += "_" + nAdd.ToString();
                    sTmpImage = sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt;
                    bExists = System.IO.File.Exists(sTmpImage);
                    nAdd++;
                }
                //theFile.SaveAs(sTmpImage);

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from media_pics_sizes (nolock) where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (ratioID > 0)
                {
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
                }
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                    if (bSetMediaThumb)
                    {
                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePath + "/pics/" + sUploadedFile, sBasePath + "/pics/" + nGroupID.ToString() + "/" + sPicBaseName + "_tn" + sUploadedFileExt, 90, 65, true);
                        UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_tn" + sUploadedFileExt);

                        TVinciShared.ImageUtils.RenameImage(sBasePath + "/pics/" + sUploadedFile, sBasePath + "/pics/" + nGroupID.ToString() + "/" + sPicBaseName + "_full" + sUploadedFileExt);
                        UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_full" + sUploadedFileExt);
                    }

                    for (int nI = 0; nI < nCount1; nI++)
                    {
                        string sWidth = selectQuery.Table("query").DefaultView[nI].Row["WIDTH"].ToString();
                        string sHeight = selectQuery.Table("query").DefaultView[nI].Row["HEIGHT"].ToString();
                        string sEndName = sWidth + "X" + sHeight;
                        Int32 nCrop = int.Parse(selectQuery.Table("query").DefaultView[nI].Row["TO_CROP"].ToString());
                        string sTmpImage1 = sBasePath + "/pics/" + nGroupID.ToString() + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                        bool bCrop = true;
                        if (nCrop == 0)
                            bCrop = false;
                        bool bOverride = false;
                        if (ratioID > 0)
                        {
                            bOverride = true;
                        }
                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePath + "/pics/" + sUploadedFile, sTmpImage1, int.Parse(sWidth), int.Parse(sHeight), bCrop, bOverride);
                        log.Debug("File download - Resized Image " + sTmpImage1 + " from " + sBasePath + "/pics/" + sUploadedFile);

                        UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_" + sEndName + sUploadedFileExt);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                nPicID = InsertNewPic(sMediaName, sUploadedFile, sPicBaseName + sUploadedFileExt, nGroupID);
            }
            if (nPicID != 0)
            {
                if (doesPicExist)
                {
                    log.Debug("Pic exists downloading again start: " + sPic);
                    string sUploadedFile = TVinciShared.ImageUtils.DownloadWebImage(sPic, sBasePath);
                    log.Debug("Pic exists downloading again end: " + sUploadedFile);
                    if (sUploadedFile == "")
                    {
                        sUploadedFile = TVinciShared.ImageUtils.DownloadWebImage(sPicsBasePath + "/" + sPic, sBasePath);
                        if (sUploadedFile == "")
                        {
                            log.Debug("Cant download pic from FTP: " + sPic);
                            return 0;
                        }
                    }
                    string sUploadedFileExt = "";
                    int nExtractPos = sUploadedFile.LastIndexOf(".");
                    if (nExtractPos > 0)
                        sUploadedFileExt = sUploadedFile.Substring(nExtractPos);

                    string sPicBaseName = GetPicBaseName(nPicID, nGroupID);
                    log.Debug("Start re cropping: " + sPicBaseName);
                    string sTmpImage = sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt;
                    bool bExists = System.IO.File.Exists(sTmpImage);

                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select * from media_pics_sizes (nolock) where status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                    if (ratioID > 0)
                    {
                        selectQuery += " and ";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
                    }
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;

                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePath + "/pics/" + sUploadedFile, sBasePath + "/pics/" + nGroupID + "/" + sPicBaseName + "_tn" + sUploadedFileExt, 90, 65, true, true);
                        log.Debug("Re cropping: " + sBasePath + "/pics/" + sPicBaseName + "_tn" + sUploadedFileExt);
                        UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_tn" + sUploadedFileExt);

                        TVinciShared.ImageUtils.RenameImage(sBasePath + "/pics/" + sUploadedFile, sBasePath + "/pics/" + nGroupID + "/" + sPicBaseName + "_full" + sUploadedFileExt);
                        log.Debug("Re cropping: " + sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt);
                        UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_full" + sUploadedFileExt);

                        for (int nI = 0; nI < nCount1; nI++)
                        {
                            string sWidth = selectQuery.Table("query").DefaultView[nI].Row["WIDTH"].ToString();
                            string sHeight = selectQuery.Table("query").DefaultView[nI].Row["HEIGHT"].ToString();
                            string sEndName = sWidth + "X" + sHeight;
                            Int32 nCrop = int.Parse(selectQuery.Table("query").DefaultView[nI].Row["TO_CROP"].ToString());
                            string sTmpImage1 = sBasePath + "/pics/" + nGroupID + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                            bool bCrop = true;
                            if (nCrop == 0)
                                bCrop = false;

                            TVinciShared.ImageUtils.ResizeImageAndSave(sBasePath + "/pics/" + sUploadedFile, sTmpImage1, int.Parse(sWidth), int.Parse(sHeight), bCrop, true);
                            log.Debug("Re cropping: " + sTmpImage1);
                            UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, sPicBaseName + "_" + sEndName + sUploadedFileExt);
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                }


                //nPicID = InsertNewPic(sMediaName, sUploadedFile, sPicBaseName + sUploadedFileExt, nGroupID);
                IngestionUtils.M2MHandling("ID", "", "", "", "ID", "tags", "pics_tags", "pic_id", "tag_id", "true", sMainLang, sMediaName, nGroupID, nPicID, false);
                if (bSetMediaThumb == true)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_PIC_ID", "=", nPicID);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                //the media type id is invalid here
                EnterPicMediaFile(sPicType, nMediaID, nPicID, nGroupID, "HIGH");
            }
            return nPicID;
        }

        static public Int32 DownloadPic(string sPic, string sMediaName, Int32 nGroupID, Int32 nMediaID, string sMainLang, string sPicType, bool bSetMediaThumb, int ratioID, List<string> ratioSize = null)
        {
            if (string.IsNullOrEmpty(sPic))
            {
                log.Debug("File download - picture name is empty. mediaID: " + nMediaID.ToString());
                return 0;
            }

            int picId = 0;

            // use old/or image queue
            if (WS_Utils.IsGroupIDContainedInConfig(nGroupID, "USE_OLD_IMAGE_SERVER", ';'))
            {
                string sUseQueue = TVinciShared.WS_Utils.GetTcmConfigValue("downloadPicWithQueue");
                if (!string.IsNullOrEmpty(sUseQueue) && sUseQueue.ToLower().Equals("true"))
                {
                    picId = DownloadPicToQueue(sPic, sMediaName, nGroupID, nMediaID, sMainLang, sPicType, bSetMediaThumb, ratioID, ratioSize);

                }
                else
                {
                    picId = DownloadPicToUploader(sPic, sMediaName, nGroupID, nMediaID, sMainLang, sPicType, bSetMediaThumb, ratioID);
                }
            }
            else
            {
                // use new image server
                picId = DownloadPicToImageServer(sPic, sMediaName, nGroupID, nMediaID, sMainLang, bSetMediaThumb, ratioID, eAssetImageType.Media);

            }

            if (picId > 0)
            {
                IngestionUtils.M2MHandling("ID", "", "", "", "ID", "tags", "pics_tags", "pic_id", "tag_id", "true", sMainLang, sMediaName, nGroupID, picId, false);
                if (bSetMediaThumb == true)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_PIC_ID", "=", picId);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
            }

            if (picId == 0)
                log.ErrorFormat("Failed download pic- mediaId:{0}, ratioId{1}, url:{2}", nMediaID, ratioID, sPic);
            else
            {
                if (WS_Utils.IsGroupIDContainedInConfig(nGroupID, "USE_OLD_IMAGE_SERVER", ';'))
                    log.DebugFormat("Successfully download pic- mediaId:{0}, ratioId{1}, url:{2}", nMediaID, ratioID, sPic);
                else
                    log.DebugFormat("Successfully processed image - mediaId:{0}, ratioId{1}, url:{2}", nMediaID, ratioID, sPic);
            }

            return picId;
        }

        static public Int32 DownloadPicToUploader(string sPic, string sMediaName, Int32 nGroupID, Int32 nMediaID, string sMainLang, string sPicType, bool bSetMediaThumb, int ratioID)
        {
            //return DownloadPic_old(sPic, sMediaName, nGroupID, nMediaID, sMainLang, sPicType, bSetMediaThumb, ratioID);

            log.Debug("File downloaded - Start Download Pic: " + " " + sPic + " " + "MediaID: " + nMediaID.ToString() + " RatioID :" + ratioID.ToString());
            if (sPic.Trim() == "")
            {
                return 0;
            }
            string sBasePath = GetBasePath(nGroupID);
            sBasePath = string.Format("{0}\\pics\\{1}", sBasePath, nGroupID);

            log.Debug("File download - Base Path is " + sBasePath);

            string sPicsBasePath = string.Empty;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select PICS_REMOTE_BASE_URL from groups (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sPicsBasePath = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "PICS_REMOTE_BASE_URL", 0);
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            char[] delim = { '/' };
            string[] splited1 = sPic.Split(delim);
            string sPicBaseName1 = splited1[splited1.Length - 1];
            if (sPicBaseName1.IndexOf("?") != -1 && sPicBaseName1.IndexOf("uuid") != -1)
            {
                Int32 nStart = sPicBaseName1.IndexOf("uuid=", 0) + 5;
                Int32 nEnd = sPicBaseName1.IndexOf("&", nStart);
                if (nEnd != 4)
                    sPicBaseName1 = sPicBaseName1.Substring(nStart, nEnd - nStart);
                else
                    sPicBaseName1 = sPicBaseName1.Substring(nStart);
                sPicBaseName1 += ".jpg";
            }

            string sPicName = sMediaName;

            if (!Directory.Exists(sBasePath))
            {
                Directory.CreateDirectory(sBasePath);
            }

            Int32 nPicID = 0;

            string sUploadedFileExt = "";
            int nExtractPos = sPic.LastIndexOf(".");
            if (nExtractPos > 0)
                sUploadedFileExt = sPic.Substring(nExtractPos);

            string sPicBaseName = string.Empty;
            if (ratioID > 0)
            {
                sPicBaseName = TVinciShared.ImageUtils.GetDateImageName(nMediaID);
            }
            if (string.IsNullOrEmpty(sPicBaseName))
            {
                sPicBaseName = TVinciShared.ImageUtils.GetDateImageName();
            }

            List<ImageManager.ImageObj> images = new List<ImageManager.ImageObj>();
            if (bSetMediaThumb)
            {
                ImageManager.ImageObj tnImage = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.THUMB, 90, 65, sUploadedFileExt);
                ImageManager.ImageObj fullImage = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.FULL, 0, 0, sUploadedFileExt);

                images.Add(tnImage);
                images.Add(fullImage);
            }

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media_pics_sizes (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (ratioID > 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WIDTH", i);
                    int nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "HEIGHT", i);

                    ImageManager.ImageObj image = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.SIZE, nWidth, nHeight, sUploadedFileExt);
                    images.Add(image);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            bool downloadRes = ImageManager.ImageHelper.DownloadAndCropImage(nGroupID, sPic, sBasePath, images, sPicBaseName, sUploadedFileExt);
            if (!downloadRes)
            {
                ImageManager.ImageHelper.DownloadAndCropImage(nGroupID, sPicsBasePath + "/" + sPic, sBasePath, images, sPicBaseName, sUploadedFileExt);
            }
            if (downloadRes)
            {
                foreach (ImageManager.ImageObj image in images)
                {
                    if (image.eResizeStatus == ImageManager.ResizeStatus.SUCCESS)
                    {
                        UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, image.ToString());
                    }
                }
                nPicID = InsertNewPic(sMediaName, sPic, sPicBaseName + sUploadedFileExt, nGroupID);
            }

            if (nPicID != 0)
            {
                IngestionUtils.M2MHandling("ID", "", "", "", "ID", "tags", "pics_tags", "pic_id", "tag_id", "true", sMainLang, sMediaName, nGroupID, nPicID, false);
                if (bSetMediaThumb == true)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_PIC_ID", "=", nPicID);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                //the media type id is invalid here
                EnterPicMediaFile(sPicType, nMediaID, nPicID, nGroupID, "HIGH");
            }
            return nPicID;
        }

        static public Int32 DownloadPicToQueue(string sPic, string sMediaName, Int32 nGroupID, Int32 nMediaID, string sMainLang, string sPicType, bool bSetMediaThumb, int ratioID, List<string> picSizes = null)
        {
            int nPicID = 0;
            log.Debug("File downloaded - Start Download Pic: " + " " + sPic + " " + "MediaID: " + nMediaID.ToString() + " RatioID :" + ratioID.ToString());

            if (picSizes == null)
            {
                picSizes = GetMediaPicSizes(bSetMediaThumb, nGroupID, ratioID);
            }
            //generate PictureData and send to Queue 
            string sPicNewName = getNewUninqueName(ratioID, nMediaID); //the unique name            

            string sBasePath = ImageUtils.getRemotePicsURL(nGroupID);

            // generate PictureData and send to Queue
            bool bIsUpdateSucceeded = ImageUtils.SendPictureDataToQueue(sPic, sPicNewName, sBasePath, picSizes.ToArray(), nGroupID);

            //insert new Picture to DB 
            string sUploadedFileExt = ImageUtils.GetFileExt(sPic);
            nPicID = InsertNewPic(sMediaName, sPic, sPicNewName + sUploadedFileExt, nGroupID);

            if (nPicID != 0)
            {
                #region handle pic tags and update the media files
                //the media type id is invalid here
                EnterPicMediaFile(sPicType, nMediaID, nPicID, nGroupID, "HIGH");
                #endregion
            }
            return nPicID;
        }

        //according to 'bSetMediaThumb' there is "full" and "tn"
        //if there is a ratioID, then the pic size is determined by it. if there isn't ratio, then all sizes of the group will be added
        private static List<string> GetMediaPicSizes(bool bSetMediaThumb, int nGroupID, int ratioID)
        {
            List<string> mediaPicSizes = new List<string>();
            if (bSetMediaThumb)
            {
                mediaPicSizes.Add("full");
                mediaPicSizes.Add("tn");
            }

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media_pics_sizes (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (ratioID > 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WIDTH", i);
                    int nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "HEIGHT", i);

                    string size = nWidth + "X" + nHeight;
                    mediaPicSizes.Add(size);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return mediaPicSizes;
        }

        //according to 'bSetMediaThumb' there is "full" and "tn"
        //if there is a ratioID, then the pic size is determined by it. if there isn't ratio, then all sizes of the group will be added
        private static List<string> GetMediaPicSizes(int groupId, int ratioID)
        {
            List<string> mediaPicSizes = new List<string>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select WIDTH, HEIGHT from media_pics_sizes (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
            if (ratioID > 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WIDTH", i);
                    int nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "HEIGHT", i);

                    string size = nWidth + "X" + nHeight;
                    mediaPicSizes.Add(size);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return mediaPicSizes;
        }

        //according to 'bSetMediaThumb' there is "full" and "tn"
        //if there is a ratioID, then the pic size is determined by it. if there isn't ratio, then all sizes of the group will be added
        private static List<string> GetMediaPicSizesExclude(int groupId, List<int> ratiolist)
        {
            List<string> mediaPicSizes = new List<string>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select WIDTH, HEIGHT from media_pics_sizes (nolock) where status=1 and ";
            selectQuery += "group_id = " + groupId.ToString();
            if (ratiolist != null && ratiolist.Count > 0)
            {
                selectQuery += " and ";
                string ratioToExclude = BuildRatiosListString(ratiolist);
                selectQuery += "ratio_id not in " + ratioToExclude;
            }
            if (selectQuery.Execute("query", true) != null)
            {
                int rows = selectQuery.Table("query").DefaultView.Count;
                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    int nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WIDTH", rowIndex);
                    int nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "HEIGHT", rowIndex);

                    string size = nWidth + "X" + nHeight;
                    mediaPicSizes.Add(size);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return mediaPicSizes;
        }

        private static string BuildRatiosListString(List<int> ratiolist)
        {
            return string.Format("({0})", string.Join(",", ratiolist.Select(x => x.ToString()).ToArray()));
        }

        static public int DownloadPicToImageServer(string pic, string assetName, int groupId, int assetId, string mainLang, bool setMediaThumb, int ratioId, eAssetImageType assetImageType, bool isAsync = true, int? updaterId = null)
        {
            int version = 0;
            string baseUrl = string.Empty;
            int picId = 0;
            int picRatioId = 0;

            //check if pic Url exist
            string checkImageUrl = WS_Utils.GetTcmConfigValue("CheckImageUrl");
            if (!string.IsNullOrEmpty(checkImageUrl) && checkImageUrl.ToLower().Equals("true"))
            {
                if (!ImageUtils.IsUrlExists(pic))
                {
                    log.ErrorFormat("DownloadPicToImageServer pic url not valid: {0} ", pic);
                    return picId;
                }
            }

            // in case ratio Id = 0 get default group's ratio
            if (ratioId <= 0)
            {
                ratioId = ImageUtils.GetGroupDefaultRatio(groupId);
            }

            //get pic data           
            if (assetId > 0 && GetPicData(ratioId, assetId, assetImageType, out picId, out version, out baseUrl, out picRatioId))
            {
                // Get Base Url
                baseUrl = Path.GetFileNameWithoutExtension(baseUrl);

                // incase row exist --> update  version number
                if (picRatioId == ratioId)
                {
                    version++;
                }
                else
                {
                    picId = CatalogDAL.InsertPic(groupId, assetName, pic, baseUrl, ratioId, assetId, assetImageType);
                }
            }
            else // pic does not exist -- > create new pic
            {
                baseUrl = TVinciShared.ImageUtils.GetDateImageName();
                picId = CatalogDAL.InsertPic(groupId, assetName, pic, baseUrl, ratioId, assetId, assetImageType);
            }

            if (picId != 0)
            {
                if (isAsync)
                {
                    SendImageDataToImageUploadQueue(pic, groupId, version, picId, baseUrl + "_" + ratioId, eMediaType.VOD);
                }
                else
                {
                    int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupId);
                    ImageServerUploadRequest imageServerReq = new ImageServerUploadRequest() { GroupId = parentGroupId, Id = baseUrl + "_" + ratioId, SourcePath = pic, Version = version };

                    // post image
                    string result = Utils.HttpPost(ImageUtils.GetImageServerUrl(groupId, eHttpRequestType.Post), JsonConvert.SerializeObject(imageServerReq), "application/json");

                    // check result
                    if (string.IsNullOrEmpty(result) || result.ToLower() != "true")
                    {
                        ImageUtils.UpdateImageState(groupId, picId, version, eMediaType.VOD, eTableStatus.Failed, updaterId);
                        picId = 0;
                    }
                    else if (result.ToLower() == "true")
                    {
                        ImageUtils.UpdateImageState(groupId, picId, version, eMediaType.VOD, eTableStatus.OK, updaterId);
                        log.DebugFormat("post image success. picId {0} ", picId);
                    }
                }
            }

            return picId;
        }

        private static bool GetPicData(int ratioID, int assetId, eAssetImageType assetImageType, out int picId, out int version, out string baseUrl, out int picRatioId)
        {
            bool result = false;
            picId = 0;
            version = 0;
            baseUrl = string.Empty;
            picRatioId = 0;

            try
            {
                DataRowCollection rows = CatalogDAL.GetPicsTableData(assetId, assetImageType, ratioID, 0);

                if (rows != null && rows.Count > 0)
                {
                    result = true;

                    foreach (DataRow row in rows)
                    {
                        picId = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                        version = ODBCWrapper.Utils.GetIntSafeVal(row, "VERSION");
                        baseUrl = ODBCWrapper.Utils.GetSafeStr(row, "BASE_URL");
                        picRatioId = ODBCWrapper.Utils.GetIntSafeVal(row, "RATIO_ID");
                        if (picRatioId == ratioID)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
            }
            return result;
        }

        private static string getPictureFileName(string sThumb)
        {
            string sPicName = sThumb;

            if (sPicName.IndexOf("?") != -1 && sPicName.IndexOf("uuid") != -1)
            {
                Int32 nStart = sPicName.IndexOf("uuid=", 0) + 5;
                Int32 nEnd = sPicName.IndexOf("&", nStart);
                if (nEnd != 4)
                    sPicName = sPicName.Substring(nStart, nEnd - nStart);
                else
                    sPicName = sPicName.Substring(nStart);
                sPicName += ".jpg";
            }

            if (sPicName.Length >= 200) // the column in DB limit with 255 char
            {
                sPicName = sThumb.Substring(sThumb.Length - 200); // get all 200 chars from the end !!
            }

            return sPicName;
        }

        //Epg Pics will alsays have "full" and "tn". also, all sizes of the group in 'epg_pics_sizes' will be added  
        private static string[] getEPGPicSizes(int nGroupID, int ratioID)
        {
            string[] str;
            List<string> lString = new List<string>();

            lString.Add("full");
            lString.Add("tn");

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from epg_pics_sizes (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (ratioID > 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WIDTH", i);
                    int nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "HEIGHT", i);
                    string sSize = nWidth + "X" + nHeight;
                    lString.Add(sSize);
                }
            }

            str = lString.ToArray();
            return str;
        }

        //get new Unique name or existing one per media
        private static string getNewUninqueName(int ratioID, int nMediaID)
        {
            string sPicBaseName = string.Empty;
            if (ratioID > 0)
            {
                sPicBaseName = TVinciShared.ImageUtils.GetDateImageName(nMediaID);
            }
            if (string.IsNullOrEmpty(sPicBaseName))
            {
                sPicBaseName = TVinciShared.ImageUtils.GetDateImageName();
            }
            return sPicBaseName;
        }

        private static string getNewUninqueName(string baseUrl)
        {
            string sPicBaseName = string.Empty;

            if (baseUrl.IndexOf('.') > 0)
            {
                sPicBaseName = baseUrl.Substring(0, baseUrl.IndexOf('.'));
                log.DebugFormat("BaseURL {0}", sPicBaseName);
            }

            if (string.IsNullOrEmpty(sPicBaseName))
            {
                sPicBaseName = TVinciShared.ImageUtils.GetDateImageName();
            }
            return sPicBaseName;
        }

        static private string GetBasePath(int nGroupID)
        {
            string key = string.Format("pics_base_path_{0}", nGroupID);

            if (!string.IsNullOrEmpty(GetConfigVal(key)))
            {
                return GetConfigVal(key);
            }
            if (!string.IsNullOrEmpty(GetConfigVal("pics_base_path")))
            {
                return GetConfigVal("pics_base_path");
            }

            string sBasePath = string.Empty;
            try
            {
                if (System.Web.HttpContext.Current != null)
                    sBasePath = HttpContext.Current.Server.MapPath("");
                else
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(HttpRuntime.AppDomainAppPath))
                        {
                            sBasePath = HttpRuntime.AppDomainAppPath;
                        }
                        else
                            sBasePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                        if (string.IsNullOrEmpty(sBasePath))
                        {
                            sBasePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }

            return sBasePath;

        }

        static public void UploadDirectory(int nGroupID)
        {
            //string sBasePath = GetBasePath(nGroupID);
            //BaseUploader.SetRunningProcesses(0);
            //DBManipulator.UploadDirectoryToGroup(nGroupID, sBasePath + "/pics/" + nGroupID.ToString() + "/");

            log.Debug("UploadDirectory - Group : " + nGroupID.ToString());
        }

        static protected Int32 GetBillingCodeIDByName(string sName)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from lu_billing_type where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(API_VAL)))", "=", sName.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetCDNIdByName(string sName, Int32 nGroupID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from streaming_companies where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(STREAMING_COMPANY_NAME)))", "=", sName.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetAdCompID(string sAdCompName, Int32 nGroupID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from ads_companies where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ads_company_name", "=", sAdCompName);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetPlayerTypeID(string sPlayerType)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from lu_player_descriptions where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sPlayerType);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCopunt = selectQuery.Table("query").DefaultView.Count;
                if (nCopunt > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected int GetPPVModuleID(string moduleName, int groupID, ref int nCommerceGroupID)
        {
            int nRet = 0;
            nCommerceGroupID = 0;

            if (string.IsNullOrEmpty(moduleName))
            {
                return 0;
            }

            object commerceGroupIDObj = ODBCWrapper.Utils.GetTableSingleVal("groups", "commerce_group_id", groupID, 86400);
            if (commerceGroupIDObj != null && !string.IsNullOrEmpty(commerceGroupIDObj.ToString()))
            {
                nCommerceGroupID = int.Parse(commerceGroupIDObj.ToString());
            }

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from ppv_modules where IS_ACTIVE = 1 and STATUS = 1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", moduleName);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCopunt = selectQuery.Table("query").DefaultView.Count;
                if (nCopunt > 0)
                    nRet = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected bool InsertFilePPVModule(int ppvModule, int fileID, int ppvModuleGroupID, DateTime? startDate, DateTime? endDate, bool clear)
        {
            bool res = false;
            if (ppvModule == 0)
            {
                return res;
            }

            //First initialize all previous entries.
            if (clear)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_modules_media_files");
                updateQuery.SetConnectionKey("pricing_connection");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", fileID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

            int ppvFileID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from ppv_modules_media_files (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", fileID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PPV_MODULE_ID", "=", ppvModule);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    ppvFileID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            //If doesnt exist - create new entry
            if (ppvFileID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_modules_media_files");
                insertQuery.SetConnectionKey("pricing_connection");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", fileID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PPV_MODULE_ID", "=", ppvModule);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", ppvModuleGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

                if (startDate.HasValue)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", startDate);
                }

                if (endDate.HasValue)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", endDate);
                }

                res = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            else
            {
                //Update status of previous entry
                ODBCWrapper.UpdateQuery updateOldQuery = new ODBCWrapper.UpdateQuery("ppv_modules_media_files");
                updateOldQuery.SetConnectionKey("pricing_connection");
                updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("PPV_MODULE_ID", "=", ppvModule);

                if (startDate.HasValue)
                {
                    updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", startDate);
                }

                if (endDate.HasValue)
                {
                    updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", endDate);
                }

                updateOldQuery += "where";
                updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", ppvFileID);
                res = updateOldQuery.Execute();
                updateOldQuery.Finish();
                updateOldQuery = null;
            }

            return res;
        }

        static protected void EnterClipMediaFile(string sPicType,
            Int32 nMediaID, Int32 nPicID, Int32 nGroupID, string sQuality, string sCDN, string sCDNId,
            string sCDNCode, string sBillingType,
            string sPreRule, string sPostRule, string sBreakRule,
            string sOverlayRule, string sBreakPoints, string sOverlayPoints,
            bool bAdsEnabled, bool bSkipPre, bool bSkipPost, string sPlayerType, long nDuration, string ppvModuleName, string sCoGuid, string sContractFamily,
            string sLanguage, int nIsLanguageDefualt, string sOutputProtectionLevel, ref string sErrorMessage, string sProductCode,
            DateTime? fileStartDate, DateTime? fileEndDate, string sAltCoGuid, string sAltCDN, string sAltCDNID, string sAltCDNCode)
        {
            Int32 nPicType = ProtocolsFuncs.GetFileTypeID(sPicType, nGroupID);

            Int32 nOverridePlayerTypeID = GetPlayerTypeID(sPlayerType);

            Int32 nQualityID = ProtocolsFuncs.GetFileQualityID(sQuality);

            Int32 nCDNId = 0, nAltCDNId = 0;

            if (String.IsNullOrEmpty(sCDNId))
                nCDNId = GetCDNIdByName(sCDN, nGroupID);
            else
                Int32.TryParse(sCDNId, out nCDNId);

            if (string.IsNullOrEmpty(sAltCDNID))
                nAltCDNId = GetCDNIdByName(sAltCDN, nGroupID);
            else
                Int32.TryParse(sAltCDNID, out nAltCDNId);


            Int32 nBillingCodeID = GetBillingCodeIDByName(sBillingType);
            DateTime? prevStartDate = null;
            DateTime? prevEndDate = null;
            Int32 nMediaFileID = IngestionUtils.GetPicMediaFileIDWithDates(nPicType, nMediaID, nGroupID, nQualityID, true, ref prevStartDate, ref prevEndDate, sLanguage);
            Int32 nPreAdCompany = GetAdCompID(sPreRule, nGroupID);
            Int32 nPostAdCompany = GetAdCompID(sPostRule, nGroupID);
            Int32 nBreakAdCompany = GetAdCompID(sBreakRule, nGroupID);
            Int32 nOverlayAdCompany = GetAdCompID(sOverlayRule, nGroupID);

            if (nMediaFileID != 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_files");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("co_guid", "=", sCoGuid);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_SUPLIER_ID", "=", nCDNId);

                if (nBillingCodeID != 0 || sBillingType.ToLower() == "none")
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TYPE_ID", "=", nBillingCodeID);

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_CODE", "=", sCDNCode);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DURATION", "=", nDuration);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COMMERCIAL_TYPE_PRE_ID", "=", nPreAdCompany);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COMMERCIAL_TYPE_POST_ID", "=", nPostAdCompany);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COMMERCIAL_TYPE_BREAK_ID", "=", nBreakAdCompany);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COMMERCIAL_BREAK_POINTS", "=", sBreakPoints);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COMMERCIAL_TYPE_OVERLAY_ID", "=", nOverlayAdCompany);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COMMERCIAL_OVERLAY_POINTS", "=", sOverlayPoints);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_DEFAULT_LANGUAGE", "=", nIsLanguageDefualt);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE", "=", sLanguage);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Product_Code", "=", sProductCode);

                if (fileStartDate.HasValue)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", fileStartDate.Value);
                    // check if changes in the start date require future index update call, incase fileStartDate is in more than 2 years we don't update the index (per Ira's request)
                    if (RabbitHelper.IsFutureIndexUpdate(prevStartDate, fileStartDate))
                    {
                        if (!RabbitHelper.InsertFreeItemsIndexUpdate(nGroupID, ApiObjects.eObjectType.Media, new List<int>() { nMediaID }, fileStartDate.Value))
                        {
                            log.Error(string.Format("Failed inserting free items index update as part of Ingest for fileStartDate: {0}, mediaID: {1}, groupID: {2}", fileStartDate.Value, nMediaID, nGroupID));
                        }
                    }
                }

                if (fileEndDate.HasValue)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", fileEndDate.Value);
                    // check if changes in the end date require future index update call, incase fileEndDate is in more than 2 years we don't update the index (per Ira's request)
                    if (RabbitHelper.IsFutureIndexUpdate(prevEndDate, fileEndDate))
                    {
                        if (!RabbitHelper.InsertFreeItemsIndexUpdate(nGroupID, ApiObjects.eObjectType.Media, new List<int>() { nMediaID }, fileEndDate.Value))
                        {
                            log.Error(string.Format("Failed inserting free items index update as part of Ingest for fileEndDate: {0}, mediaID: {1}, groupID: {2}", fileEndDate.Value, nMediaID, nGroupID));
                        }
                    }
                }

                if (bAdsEnabled == true)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ADS_ENABLED", "=", 1);
                else
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ADS_ENABLED", "=", 0);

                if (bSkipPre == true)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("OUTER_COMMERCIAL_SKIP_PRE", "=", 1);
                else
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("OUTER_COMMERCIAL_SKIP_PRE", "=", 0);

                if (bSkipPost == true)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("OUTER_COMMERCIAL_SKIP_POST", "=", 1);
                else
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("OUTER_COMMERCIAL_SKIP_POST", "=", 0);

                if (nOverridePlayerTypeID != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("OVERRIDE_PLAYER_TYPE_ID", "=", nOverridePlayerTypeID);

                if (!string.IsNullOrEmpty(sAltCDNCode))
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ALT_STREAMING_CODE", sAltCDNCode);
                else
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ALT_STREAMING_CODE", DBNull.Value);

                if (nAltCDNId > 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ALT_STREAMING_SUPLIER_ID", nAltCDNId);
                else
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ALT_STREAMING_SUPLIER_ID", DBNull.Value);

                if (!string.IsNullOrEmpty(sAltCoGuid))
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ALT_CO_GUID", sAltCoGuid);
                else
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ALT_CO_GUID", DBNull.Value);

                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaFileID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

                SetPolicyToFile(sOutputProtectionLevel, nGroupID, sCoGuid, ref sErrorMessage);

                if (!string.IsNullOrEmpty(ppvModuleName))
                {
                    int nCommerceGroupID = 0;

                    if (ppvModuleName.Contains(";"))
                    {
                        string ParsedPPVModuleName = string.Empty;
                        DateTime? ppvStartDate = null;
                        DateTime? ppvEndDate = null;

                        //ppvModuleName = ppvModuleName.Substring(0, ppvModuleName.Length - 1);
                        string[] parameters = ppvModuleName.Split(';');

                        for (int i = 0; i < parameters.Length; i += 3)
                        {
                            int ppvID = GetPPVModuleID(parameters[i], nGroupID, ref nCommerceGroupID);

                            if (ppvID <= 0)
                            {
                                continue;
                            }

                            ppvStartDate = ExtractDate(parameters[i + 1], "dd/MM/yyyy HH:mm:ss");
                            ppvEndDate = ExtractDate(parameters[i + 2], "dd/MM/yyyy HH:mm:ss");

                            DateTime? prevPPVFileStartDate = null;
                            DateTime? prevPPVFileEndDate = null;

                            if (ppvStartDate.HasValue && ppvStartDate.HasValue)
                            {
                                DataRow updatedppvModuleMediaFileDetails = ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("ppv_modules_media_files", "media_file_id", nMediaFileID.ToString(), new List<string>() { "start_date", "end_date" }, "pricing_connection");
                                prevPPVFileStartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updatedppvModuleMediaFileDetails, "start_date");
                                prevPPVFileEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updatedppvModuleMediaFileDetails, "end_date");
                            }

                            if (InsertFilePPVModule(ppvID, nMediaFileID, nCommerceGroupID, ppvStartDate, ppvEndDate, (i == 0)))
                            {
                                // check if changes in the start date require future index update call, incase ppvStartDate is in more than 2 years we don't update the index (per Ira's request)
                                if (RabbitHelper.IsFutureIndexUpdate(prevPPVFileStartDate, ppvStartDate))
                                {
                                    if (!RabbitHelper.InsertFreeItemsIndexUpdate(nGroupID, ApiObjects.eObjectType.Media, new List<int>() { nMediaID }, ppvStartDate.Value))
                                    {
                                        log.Error(string.Format("Failed inserting free items index update for startDate: {0}, mediaID: {1}, groupID: {2}", ppvStartDate.Value, nMediaID, nGroupID));
                                    }
                                }

                                // check if changes in the end date require future index update call, incase ppvEndDate is in more than 2 years we don't update the index (per Ira's request)
                                if (RabbitHelper.IsFutureIndexUpdate(prevPPVFileEndDate, ppvStartDate))
                                {
                                    if (!RabbitHelper.InsertFreeItemsIndexUpdate(nGroupID, ApiObjects.eObjectType.Media, new List<int>() { nMediaID }, ppvEndDate.Value))
                                    {
                                        log.Error(string.Format("Failed inserting free items index update for endDate: {0}, mediaID: {1}, groupID: {2}", ppvEndDate.Value, nMediaID, nGroupID));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int ppvID = GetPPVModuleID(ppvModuleName, nGroupID, ref nCommerceGroupID);
                        InsertFilePPVModule(ppvID, nMediaFileID, nCommerceGroupID, null, null, true);
                    }
                }

                /*Insert Family Contract For File */
                if (!string.IsNullOrEmpty(sContractFamily))
                {
                    bool bInsert = InsertFileFamilyContract(sContractFamily, nMediaFileID, nGroupID);
                }
            }
        }

        static private void SetPolicyToFile(string outputProtectionLevel, int groupId, string coGuid, ref string errorMessage)
        {
            try
            {
                int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

                if (!string.IsNullOrEmpty(outputProtectionLevel))
                {
                    // check if policy to file attachment should be through new CENC or not
                    if (WS_Utils.IsGroupIDContainedInConfig(parentGroupID, "OLD_DRM_EXC_GROUPS", ';'))
                    {
                        // old policy attachment
                        string sWSURL = GetConfigVal("EncryptorService");
                        string sWSPassword = GetConfigVal("EncryptorPassword");

                        WS_Encryptor.Encryptor service = new WS_Encryptor.Encryptor();
                        if (!string.IsNullOrEmpty(sWSURL))
                            service.Url = sWSURL;

                        bool res = service.SetPolicyToFileByCoGuid(parentGroupID, sWSPassword, outputProtectionLevel, coGuid, false);

                        if (!res)
                            AddError(ref errorMessage, string.Format("Fail OPL:{0}, co_guid:{1}", outputProtectionLevel, coGuid));
                    }
                    else
                    {
                        // new policy attachment
                        CencRequest cencRequest = new CencRequest()
                        {
                            CaSystem = eCaSystem.OTT.ToString(),
                            AccountId = parentGroupID.ToString(),
                            ContentId = coGuid,
                            FileId = null,
                            Policy = new Policy() { Name = outputProtectionLevel }
                        };

                        // serialize to json
                        string jsonData = JsonConvert.SerializeObject(cencRequest);

                        // get signature key
                        string keySignature = "";
                        object keySignatureObj = ODBCWrapper.Utils.GetTableSingleVal("groups", "CENC_KEY", parentGroupID);
                        if (keySignatureObj != null && keySignatureObj != DBNull.Value)
                        {
                            keySignature = keySignatureObj.ToString();
                        }
                        else
                        {
                            AddError(ref errorMessage, string.Format("Fail OPL:{0}, co_guid:{1}, group ID: {2}. client key signature wasn't found", outputProtectionLevel, coGuid, parentGroupID));
                            return;
                        }

                        // create signature
                        string signature = HttpUtility.UrlEncode(Convert.ToBase64String(Utils.Hash(string.Format("{0}{1}", keySignature, jsonData))));

                        // build address + signature
                        string address = String.Format("{0}?signature={1}", WS_Utils.GetTcmConfigValue("CENC_ADDRESS"), signature);

                        string postResponse = string.Empty;
                        string postErrorMsg = string.Empty;
                        if (TVinciShared.WS_Utils.TrySendHttpPostRequest(address, jsonData, "application/x-www-form-urlencoded", Encoding.UTF8, ref postResponse, ref postErrorMsg))
                        {
                            if (!string.IsNullOrEmpty(postResponse) && postResponse.ToLower().Trim() == "true")
                            {
                                log.Debug("SUCCESS - " + string.Format("group:{0}, co_guid:{1}, OPL:{2}", parentGroupID, coGuid, outputProtectionLevel));
                                return;
                            }
                        }
                        AddError(ref errorMessage, string.Format("Fail OPL:{0}, co_guid:{1}, post error message: {2}", outputProtectionLevel, coGuid, errorMessage));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ERROR - " + string.Format("group:{0}, co_guid:{1}, OPL:{2}, msg:{3}", groupId, coGuid, outputProtectionLevel, ex.Message), ex);
            }
        }

        static protected void EnterPicMediaFile(string sPicType, Int32 nMediaID, Int32 nPicID, Int32 nGroupID, string sQuality)
        {
            Int32 nPicType = ProtocolsFuncs.GetFileTypeID(sPicType, nGroupID);
            Int32 nQualityID = ProtocolsFuncs.GetFileQualityID(sQuality);
            Int32 nMediaFileID = IngestionUtils.GetPicMediaFileID(nPicType, nMediaID, nGroupID, nQualityID, true);

            log.DebugFormat("Enter pic media file -  PicType:{0}, QualityID:{1}, MediaFileID:{2}", nPicType, nQualityID, nMediaFileID);

            if (nMediaFileID != 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_files");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REF_ID", "=", nPicID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaFileID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }

        static protected Int32 InsertNewPic(string sName, string sRemarks, string sBaseURL, Int32 nGroupID)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("pics");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sRemarks);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            Int32 nRet = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from pics (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sRemarks);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
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

        static protected Int32 InsertNewEPGPic(string sName, string sRemarks, string sBaseURL, Int32 nGroupID)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("EPG_pics");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sRemarks);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            Int32 nRet = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from EPG_pics (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sRemarks);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
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

        static protected void GetLangData(Int32 nGroupID, ref string sLangName, ref Int32 nLangID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock),groups g (nolock) where g.LANGUAGE_ID=ll.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sLangName = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                    nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected string GetMultiLangValue(string sMainLang, ref XmlNode theItems)
        {
            if (theItems == null)
                return "";
            XmlNode theValue = theItems.SelectSingleNode("value[@lang='" + sMainLang + "']");
            if (theValue == null)
                return "";
            else
                return GetNodeValue(ref theValue, "");
        }

        static protected bool UpdateInsertBasicSubLangData(Int32 nGroupID, Int32 nMediaID,
            string sMainLang, ref XmlNode theItemNames, ref XmlNode theItemDesc)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += "select * from lu_languages where ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(CODE3)))", "<>", sMainLang.Trim().ToLower());

            selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock), group_extra_languages g where g.LANGUAGE_ID=ll.id and g.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(ll.code3)))", "<>", sMainLang.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.group_id", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                    Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    string sMediaName = GetMultiLangValue(sLang, ref theItemNames);
                    string sMediaDesc = GetMultiLangValue(sLang, ref theItemDesc);
                    Int32 nMediaTransID = 0;
                    bool b = false;
                    if (sMediaName.Trim() != "" || sMediaDesc.Trim() != "")
                        nMediaTransID = GetMediaTranslateID(nMediaID, nLangID, ref b, true);
                    else
                        nMediaTransID = GetMediaTranslateID(nMediaID, nLangID, ref b, false);
                    if (nMediaTransID != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_translate");
                        if (sMediaName != "")
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sMediaName);
                        if (sMediaDesc != "")
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sMediaDesc);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                        updateQuery += "where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaTransID);

                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            return true;
        }

        static public Int32 GetMediaTranslateID(Int32 nMediaID, Int32 nLangID)
        {
            bool b = false;
            bool bb = true;
            return GetMediaTranslateID(nMediaID, nLangID, ref b, bb);
        }

        static protected Int32 GetEPGSchedTranslateID(Int32 nEPGSchedID, Int32 nLangID, ref bool bExists, bool bForceCreate)
        {
            bExists = true;
            Int32 nMediaTransID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from epg_channels_schedule_translate where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNELS_SCHEDULE_ID", "=", nEPGSchedID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nMediaTransID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nMediaTransID == 0)
            {
                bExists = false;
                if (bForceCreate == true)
                {
                    bool bExists1 = false;
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("epg_channels_schedule_translate");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNELS_SCHEDULE_ID", "=", nEPGSchedID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;

                    return GetEPGSchedTranslateID(nEPGSchedID, nLangID, ref bExists1, bForceCreate);
                }
            }
            return nMediaTransID;
        }

        static public Int32 GetChannelTranslateID(Int32 channelID, Int32 nLangID, ref bool bExists, bool bForceCreate)
        {
            bExists = true;
            Int32 nChannelTransID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from channel_translate where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("channel_id", "=", channelID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nChannelTransID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nChannelTransID == 0)
            {
                bExists = false;
                if (bForceCreate == true)
                {
                    bool bExists1 = false;
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("channel_translate");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("channel_ID", "=", channelID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;

                    return GetChannelTranslateID(channelID, nLangID, ref bExists1, false);
                }
            }
            return nChannelTransID;
        }

        static public Int32 GetMediaTranslateID(Int32 nMediaID, Int32 nLangID, ref bool bExists, bool bForceCreate)
        {
            bExists = true;
            Int32 nMediaTransID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from media_translate where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nMediaTransID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nMediaTransID == 0)
            {
                bExists = false;
                if (bForceCreate == true)
                {
                    bool bExists1 = false;
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_translate");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("media_ID", "=", nMediaID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;

                    return GetMediaTranslateID(nMediaID, nLangID, ref bExists1, bForceCreate);
                }
            }
            return nMediaTransID;
        }

        static protected Int32 GetStringMetaIDByMetaName(Int32 nGroupID, string sMetaName)
        {
            Int32 nMetaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 1; i < 21; i++)
                    {
                        object oCurMetaName = selectQuery.Table("query").DefaultView[0].Row["META" + i.ToString() + "_STR_NAME"];
                        if (oCurMetaName != DBNull.Value && oCurMetaName != null)
                        {
                            if (sMetaName.Trim().ToLower() == oCurMetaName.ToString().Trim().ToLower())
                                nMetaID = i;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMetaID;
        }

        static protected Int32 GetDoubleMetaIDByMetaName(Int32 nGroupID, string sMetaName)
        {
            Int32 nMetaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 1; i < 11; i++)
                    {
                        object oCurMetaName = selectQuery.Table("query").DefaultView[0].Row["META" + i.ToString() + "_DOUBLE_NAME"];
                        if (oCurMetaName != DBNull.Value && oCurMetaName != null)
                        {
                            if (sMetaName.Trim().ToLower() == oCurMetaName.ToString().Trim().ToLower())
                                nMetaID = i;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMetaID;
        }

        static protected Int32 GetBoolMetaIDByMetaName(Int32 nGroupID, string sMetaName)
        {
            Int32 nMetaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 1; i < 11; i++)
                    {
                        object oCurMetaName = selectQuery.Table("query").DefaultView[0].Row["META" + i.ToString() + "_BOOL_NAME"];
                        if (oCurMetaName != DBNull.Value && oCurMetaName != null)
                        {
                            if (sMetaName.Trim().ToLower() == oCurMetaName.ToString().Trim().ToLower())
                                nMetaID = i;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMetaID;
        }

        static protected bool UpdateStringSubLangData(Int32 nGroupID, Int32 nMediaID, string sMainLang, ref XmlNodeList theStrings)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += "select * from lu_languages where ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(CODE3)))", "<>", sMainLang.Trim().ToLower());

            selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock), group_extra_languages g where g.LANGUAGE_ID=ll.id and g.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(ll.code3)))", "<>", sMainLang.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.group_id", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                    Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());

                    Int32 nMediaTransID = 0;
                    bool b = false;
                    nMediaTransID = GetMediaTranslateID(nMediaID, nLangID, ref b, false);
                    if (nMediaTransID != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_translate");
                        bool bExecute = false;
                        Int32 nCount1 = theStrings.Count;
                        for (int j = 0; j < nCount1; j++)
                        {
                            XmlNode theItem = theStrings[j];
                            string sName = GetItemParameterVal(ref theItem, "name");
                            string sMLHandling = GetItemParameterVal(ref theItem, "ml_handling");
                            string sValue = GetMultiLangValue(sLang, ref theItem);
                            string sMainValue = GetMultiLangValue(sMainLang, ref theItem);

                            Int32 nMetaID = GetStringMetaIDByMetaName(nGroupID, sName);
                            if (nMetaID == 0)
                            {
                                log.Debug("Ingest String data - can not find the name: " + sName.ToString());
                                continue;
                            }

                            if (sMLHandling.Trim().ToLower() == "duplicate")
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_STR", "=", sMainValue);
                            else
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_STR", "=", sValue);
                            bExecute = true;
                        }
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaTransID);
                        if (bExecute == true)
                            updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            return true;
        }

        static protected bool UpdateBoolsData(Int32 nGroupID, Int32 nMediaID, string sMainLang, ref XmlNodeList theBools, ref string sError)
        {
            Int32 nCount = theBools.Count;
            if (nCount <= 0)
                return true;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            bool bExecute = false;

            for (int i = 0; i < nCount; i++)
            {
                XmlNode theItem = theBools[i];
                string sName = GetItemParameterVal(ref theItem, "name");
                string sMainValue = GetNodeValue(ref theItem, "");

                Int32 nMetaID = GetBoolMetaIDByMetaName(nGroupID, sName);
                if (nMetaID > 0)
                {
                    bExecute = true;
                    int val = 0;
                    if (sMainValue.Trim().ToLower() == "1" || sMainValue.Trim().ToLower() == "true")
                    {
                        val = 1;
                    }
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_BOOL", "=", val);
                }
                else
                {
                    AddError(ref sError, "boolean value: " + sName + " not exsits");
                }
            }
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
            if (bExecute == true)
                updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }

        static public bool UpdateDatesData(Int32 nGroupID, Int32 nMediaID, ref XmlNodeList theDates, ref string sError)
        {
            if (theDates == null || theDates.Count <= 0)
                return true;

            Dictionary<string, Tuple<int, int>> dates = new Dictionary<string, Tuple<int, int>>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select gdm.id gdmId, gdm.NAME, mdmv.ID mdmvId from groups_date_metas gdm";
            selectQuery += "left join media_date_metas_values mdmv";
            selectQuery += "on mdmv.DATE_META_ID=gdm.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mdmv.media_id", "=", nMediaID);
            selectQuery += "where gdm.status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gdm.group_id", "=", nGroupID);
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string gdmName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "NAME", i).ToLower();
                    int gdmId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "gdmId", i);
                    int mdmvId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "mdmvId", i);

                    dates.Add(gdmName, new Tuple<int, int>(gdmId, mdmvId));
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            for (int i = 0; i < theDates.Count; i++)
            {
                XmlNode theItem = theDates[i];
                string name = GetItemParameterVal(ref theItem, "name");
                string value = GetNodeValue(ref theItem, "");

                DateTime? date = GetDateTimeFromStrUTF(value);
                if (!date.HasValue)
                    continue;

                if (dates.ContainsKey(name.ToLower()))
                {
                    var res = dates[name.ToLower()];
                    if (res.Item2 == 0)
                    {
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_date_metas_values");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DATE_META_ID", "=", res.Item1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", date.Value);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                        bool bRes = insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;
                    }
                    else
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_date_metas_values");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", date.Value);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                        updateQuery += "where";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", res.Item2);
                        bool bRes = updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                }
                else
                {
                    AddError(ref sError, "date value: " + name + " not exsits");
                }
            }

            return true;
        }

        static protected bool UpdateDoublesData(Int32 nGroupID, Int32 nMediaID, string sMainLang, ref XmlNodeList theDoubles, ref string sError)
        {
            Int32 nCount = theDoubles.Count;
            if (nCount <= 0)
                return true;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            bool bExecute = false;

            for (int i = 0; i < nCount; i++)
            {
                XmlNode theItem = theDoubles[i];
                string sName = GetItemParameterVal(ref theItem, "name");
                string sMainValue = GetNodeValue(ref theItem, "");

                Int32 nMetaID = GetDoubleMetaIDByMetaName(nGroupID, sName);
                if (nMetaID != 0)
                {
                    try
                    {
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_DOUBLE", "=", double.Parse(sMainValue));
                    }
                    catch (Exception ex)
                    {
                        AddError(ref sError, "On processing double value: " + sName + " exception: " + ex.Message);
                        sError = ex.Message;
                    }

                    bExecute = true;
                }
            }
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
            if (bExecute == true)
                updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }

        static protected void GetSubLangMetaData(Int32 nGroupID, string sMainLang, ref TranslatorStringHolder metaHolder, ref XmlNode theContainer, string sID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock), group_extra_languages g where g.LANGUAGE_ID=ll.id and g.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ll.code3", "<>", sMainLang);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["code3"].ToString();
                    string sVal = GetMultiLangValue(sLang, ref theContainer);
                    if (sVal != "")
                        metaHolder.AddLanguageString(sLang, sVal, sID, false);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected void DuplicateMetaData(Int32 nGroupID, string sMainLang, ref TranslatorStringHolder metaHolder, string sVal, string sID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock), group_extra_languages g where g.LANGUAGE_ID=ll.id and g.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ll.code3", "<>", sMainLang);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                    metaHolder.AddLanguageString(sLang, sVal, sID, false);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected bool UpdateFiles(Int32 nGroupID, string sMainLang, Int32 nMediaID, ref XmlNodeList theFiles, ref string sErrorMessage)
        {
            string sMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID, 0).ToString();
            Int32 nCount = theFiles.Count;
            for (int i = 0; i < nCount; i++)
            {
                XmlNode theItem = theFiles[i];
                string sCoGuid = GetItemParameterVal(ref theItem, "co_guid");
                string sName = GetItemParameterVal(ref theItem, "handling_type");
                string sDuration = GetItemParameterVal(ref theItem, "assetDuration");
                string sQuality = GetItemParameterVal(ref theItem, "quality");
                string sFormat = GetItemParameterVal(ref theItem, "type");
                string sCDN = GetItemParameterVal(ref theItem, "cdn_name");
                string sCDNId = GetItemParameterVal(ref theItem, "cdn_id");
                string sBillingType = GetItemParameterVal(ref theItem, "billing_type");
                string sPPVModule = GetItemParameterVal(ref theItem, "PPV_Module");
                string sCDNCode = GetItemParameterVal(ref theItem, "cdn_code");
                string sPreRule = GetItemParameterVal(ref theItem, "pre_rule");
                string sPostRule = GetItemParameterVal(ref theItem, "post_rule");
                string sBreakRule = GetItemParameterVal(ref theItem, "break_rule");
                string sBreakPoints = GetItemParameterVal(ref theItem, "break_points");
                string sOverlayRule = GetItemParameterVal(ref theItem, "overlay_rule");
                string sOverlayPoints = GetItemParameterVal(ref theItem, "overlay_points");
                string sFileStartDate = GetItemParameterVal(ref theItem, "file_start_date");
                string sFileEndDate = GetItemParameterVal(ref theItem, "file_end_date");
                string sAdsEnabled = GetItemParameterVal(ref theItem, "ads_enabled");
                string sContractFamily = GetItemParameterVal(ref theItem, "contract_family");
                string sLanguage = GetItemParameterVal(ref theItem, "lang");
                string sIsDefaultLanguage = GetItemParameterVal(ref theItem, "default");
                string sOutputProtectionLevel = GetItemParameterVal(ref theItem, "output_protection_level");
                int nIsDefaultLanguage = sIsDefaultLanguage.ToLower() == "true" ? 1 : 0;
                string sProductCode = GetItemParameterVal(ref theItem, "product_code");

                // adding support for alternative cdn
                string sAltCDNCode = GetItemParameterVal(ref theItem, "alt_cdn_code");
                string sAltCoGuid = GetItemParameterVal(ref theItem, "alt_co_guid");
                string sAltCDNID = GetItemParameterVal(ref theItem, "alt_cdn_id");
                string sAltCDN = GetItemParameterVal(ref theItem, "alt_cdn_name");

                // try to parse the files date correctly
                DateTime? dStartDate = null;
                DateTime? dEndDate = null;

                dStartDate = ExtractDate(sFileStartDate, "dd/MM/yyyy HH:mm:ss");
                dEndDate = ExtractDate(sFileEndDate, "dd/MM/yyyy HH:mm:ss");


                bool bAdsEnabled = true;
                if (sAdsEnabled.Trim().ToLower() == "false")
                    bAdsEnabled = false;
                string sSkipPreEnabled = GetItemParameterVal(ref theItem, "pre_skip_enabled");
                bool bSkipPreEnabled = false;
                if (sSkipPreEnabled.Trim().ToLower() == "true")
                    bSkipPreEnabled = true;
                string sSkipPostEnabled = GetItemParameterVal(ref theItem, "post_skip_enabled");
                bool bSkipPostEnabled = false;
                if (sSkipPostEnabled.Trim().ToLower() == "true")
                    bSkipPostEnabled = true;

                long nDuration = 0;
                if (!string.IsNullOrEmpty(sDuration))
                {
                    nDuration = long.Parse(sDuration);
                }

                string sPlayerType = GetItemParameterVal(ref theItem, "player_type");

                if (sName.Trim().ToLower() == "image")
                {
                    Int32 nPicID = DownloadPic(sCDNCode, sMediaName, nGroupID, nMediaID, sMainLang, sFormat, false);
                }
                else
                {
                    EnterClipMediaFile(sFormat, nMediaID, 0, nGroupID, sQuality, sCDN, sCDNId, sCDNCode, sBillingType,
                        sPreRule, sPostRule, sBreakRule, sOverlayRule, sBreakPoints, sOverlayPoints,
                        bAdsEnabled, bSkipPreEnabled, bSkipPostEnabled, sPlayerType, nDuration, sPPVModule, sCoGuid, sContractFamily,
                        sLanguage, nIsDefaultLanguage, sOutputProtectionLevel, ref sErrorMessage, sProductCode, dStartDate, dEndDate,
                        sAltCoGuid, sAltCDN, sAltCDNID, sAltCDNCode);
                }



            }
            return true;
        }

        /*Insert media file , contract family to fr_media_files_contract_families*/
        private static bool InsertFileFamilyContract(string sContractFamily, int nMediaFileID, int nGroupID)
        {
            int nFamilylID = 0;
            bool bRes = false;
            try
            {
                //Get parent group id 
                Int32 nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "PARENT_GROUP_ID", nGroupID).ToString());
                if (nParentGroupID == 1)
                    nParentGroupID = nGroupID;

                //Get contract family id by name + group_id
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select ID ";
                selectQuery += "from fr_financial_entities ";
                selectQuery += "where status = 1 and PARENT_ENTITY_ID <> 0 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("entity_type", "=", 1);
                selectQuery += "and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nParentGroupID);
                selectQuery += "and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", sContractFamily);
                selectQuery += "order by PARENT_ENTITY_ID";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nFamilylID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                if (nFamilylID == 0)
                {
                    return false;
                }

                int nOldFamilylID = 0;
                int nOldRecordID = 0;

                //Get Old config
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select ID, CONTRACT_FAMILY_ID";
                selectQuery += "from fr_media_files_contract_families";
                selectQuery += "where status=1 and is_active=1 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nParentGroupID);
                selectQuery += "and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nOldRecordID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                        nOldFamilylID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CONTRACT_FAMILY_ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                if (nOldFamilylID == nFamilylID)
                {
                    return true;
                }

                if (nOldRecordID > 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("fr_media_files_contract_families");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 2);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", 43);
                    updateQuery += "where";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nOldRecordID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }

                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("fr_media_files_contract_families");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_FAMILY_ID", "=", nFamilylID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nParentGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", 43);
                bRes = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        static protected bool UpdateMetas(Int32 nGroupID, Int32 nMediaID, string sMainLang, ref XmlNodeList theMetas, ref string sError)
        {
            Int32 nCount = theMetas.Count;
            for (int i = 0; i < nCount; i++)
            {
                XmlNode theItem = theMetas[i];
                string sName = GetItemParameterVal(ref theItem, "name");
                string sMLHandling = GetItemParameterVal(ref theItem, "ml_handling");

                if (string.IsNullOrEmpty(sName))
                    continue;

                TranslatorStringHolder metaHolder = new TranslatorStringHolder();
                XmlNodeList theContainers = theItem.SelectNodes("container");
                Int32 nCount1 = theContainers.Count;
                if (nCount1 == 0)
                {
                    theContainers = theItem.SelectNodes("values");
                    nCount1 = theContainers.Count;
                }
                for (int j = 0; j < nCount1; j++)
                {
                    XmlNode theContainer = theContainers[j];
                    string sVal = GetMultiLangValue(sMainLang, ref theContainer);
                    if (sVal == "")
                    {
                        AddError(ref sError, "meta :" + sName + " - no main language value");
                        continue;
                    }
                    metaHolder.AddLanguageString(sMainLang, sVal, j.ToString(), true);  ///i->j
                    if (sMLHandling.Trim().ToLower() == "duplicate")
                    {
                        DuplicateMetaData(nGroupID, sMainLang, ref metaHolder, sVal, j.ToString());   ///i->j
                    }
                    else
                    {
                        GetSubLangMetaData(nGroupID, sMainLang, ref metaHolder, ref theContainer, j.ToString());  ///i->j
                    }
                }
                Int32 nTagTypeID = GetTagTypeID(nGroupID, sName);
                ClearMediaTags(nMediaID, nTagTypeID);
                if (nCount1 > 0)
                {
                    if (nTagTypeID != 0 || sName.ToLower().Trim() == "free")
                        IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", nTagTypeID.ToString(), "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true", sMainLang, metaHolder, nGroupID, nMediaID);

                }
            }
            return true;
        }
        //get tags by group and by non group 
        static protected Int32 GetTagTypeID(Int32 nGroupID, string sTagName)
        {
            if (sTagName.ToLower().Trim() == "free")
                return 0;
            Int32 nRet = 0;
            string sGroups = TVinciShared.PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mtt.id from media_tags_types mtt (nolock) where status=1 and ( (TagFamilyID IS NULL and group_id " + sGroups + " ) ";
            selectQuery += " or ( group_id = 0 and TagFamilyID = 1 ) )";
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sTagName.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nRet == 0)
            {
                bool bIs_Parent = false;
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select PARENT_GROUP_ID from groups where id = " + nGroupID;
                if (selectQuery.Execute("query", true) != null)
                {
                    bIs_Parent = selectQuery.Table("query").DefaultView[0].Row["PARENT_GROUP_ID"].ToString() == "1" ? true : false;
                }
                selectQuery.Finish();
                selectQuery = null;

                if (bIs_Parent == true)
                {
                    nRet = CheckOnChildNodes(nGroupID, sTagName);
                }
            }
            return nRet;
        }

        static protected Int32 CheckOnChildNodes(Int32 nGroupID, string sTagName)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_GROUP_ID", "=", nGroupID.ToString());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; ++i)
                    {
                        int ChildID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                        nRet = GetTagTypeID(ChildID, sTagName);

                        if (nRet != 0)
                        {
                            break;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return nRet;
        }

        static protected bool UpdateStringMainLangData(Int32 nGroupID, Int32 nMediaID, string sMainLang, ref XmlNodeList theStrings)
        {
            Int32 nCount = theStrings.Count;
            if (nCount <= 0)
                return true;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            bool bExecute = false;
            for (int i = 0; i < nCount; i++)
            {
                XmlNode theItem = theStrings[i];
                string sName = GetItemParameterVal(ref theItem, "name");
                string sMLHandling = GetItemParameterVal(ref theItem, "ml_handling");
                string sMainValue = GetMultiLangValue(sMainLang, ref theItem);

                Int32 nMetaID = GetStringMetaIDByMetaName(nGroupID, sName);
                if (nMetaID > 0)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("META" + nMetaID.ToString() + "_STR", "=", sMainValue);
                    bExecute = true;
                }
            }
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
            if (bExecute == true)
                updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }

        static protected bool UpdateInsertBasicMainLangData(Int32 nGroupID, ref Int32 nMediaID, Int32 nItemType, string sCoGuid,
            string sEpgIdentifier, Int32 nWatchPerRule, Int32 nGeoBlockRule, Int32 nPlayersRule, Int32 nDeviceRule,
            DateTime dCatalogStartDate, DateTime dStartDate, DateTime dCatalogEndDate, DateTime dFinalEndDate, string sMainLang,
            ref XmlNode theItemNames, ref XmlNode theItemDesc, bool isActive, DateTime dCreate, string entryId)
        {
            string sName = GetMultiLangValue(sMainLang, ref theItemNames);
            string sDescription = GetMultiLangValue(sMainLang, ref theItemDesc);

            if (nMediaID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media");
                if (sName != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
                if (sDescription != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ENTRY_ID", "=", entryId);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", sEpgIdentifier);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCH_PERMISSION_TYPE_ID", "=", nWatchPerRule);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYERS_RULES", "=", nPlayersRule);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BLOCK_TEMPLATE_ID", "=", nGeoBlockRule);
                if (nItemType != 0)
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nItemType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_PIC_ID", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CATALOG_START_DATE", "=", dCatalogStartDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dCatalogEndDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FINAL_END_DATE", "=", dFinalEndDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", dCreate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by auto importer process");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                if (isActive)
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_rule_id", "=", nDeviceRule);

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                nMediaID = GetMediaIDByCoGuid(nGroupID, sCoGuid);
            }
            else
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
                if (sName != "")
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
                if (sDescription != "")
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ENTRY_ID", "=", entryId);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", sEpgIdentifier);
                if (nWatchPerRule != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCH_PERMISSION_TYPE_ID", "=", nWatchPerRule);
                if (nPlayersRule != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYERS_RULES", "=", nPlayersRule);
                if (nGeoBlockRule != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BLOCK_TEMPLATE_ID", "=", nGeoBlockRule);
                if (nDeviceRule != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("device_rule_id", "=", nDeviceRule);
                if (nItemType != 0)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nItemType);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CATALOG_START_DATE", "=", dCatalogStartDate);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dCatalogEndDate);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("FINAL_END_DATE", "=", dFinalEndDate);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by auto importer process");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                if (isActive)
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                else
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0); // ask Ira
                //if (sIsActive.Trim().ToLower() == "false")
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
                bool res = updateQuery.Execute();
                log.Debug("Update - " + string.Format("Media:{0} - {1}", nMediaID, res));

                updateQuery.Finish();
                updateQuery = null;
            }

            log.DebugFormat("End UpdateInsertBasicMainLangData. GID:{0}, MediaID:{1}, ItemType:{2}, CoGuid:{3}, EpgIdentifier:{4}, WatchPerRule:{5}, GeoBlockRule:{6}, "
               + "PlayersRule:{7}, DeviceRule:{8}, CatalogStartDate:{9}, StartDate:{10}, CatalogEndDate:{11}, FinalEndDate:{12}, MainLang:{13}, isActive:{14}, Create:{15}, "
               + "entryId:{16}, Name:{17}, Description:{18}.",
               nGroupID, nMediaID, nItemType, sCoGuid, sEpgIdentifier, nWatchPerRule, nGeoBlockRule, nPlayersRule, nDeviceRule, dCatalogStartDate.ToString(),
               dStartDate.ToString(), dCatalogEndDate.ToString(), dFinalEndDate.ToString(), sMainLang, isActive, dCreate.ToString(), entryId, sName, sDescription);
            return true;
        }

        static protected Int32 GetEPGSchedIDByEPGIdentifier(Int32 nGroupID, string sEPGIdentifier)
        {
            Int32 nMediaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from epg_channels_schedule (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", sEPGIdentifier);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nMediaID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMediaID;
        }

        static protected bool UpdateInsertBasicEPGMainLangData(Int32 nGroupID, Int32 nChannelID, ref Int32 nEPGSchedID,
            string sEpgIdentifier, DateTime dStartDate, DateTime dEndDate, string sThumb, string sMainLang,
            ref XmlNode theItemNames, ref XmlNode theItemDesc)
        {
            nEPGSchedID = GetEPGSchedIDByEPGIdentifier(nGroupID, sEpgIdentifier);
            string sName = GetMultiLangValue(sMainLang, ref theItemNames);
            string sDescription = GetMultiLangValue(sMainLang, ref theItemDesc);

            log.DebugFormat("Start UpdateInsertBasicEPGMainLangData. GID:{0}, ChannelID:{1}, EPGSchedID:{2}, EpgIdentifier:{3}, StartDate:{4}, EndDate:{5}, Thumb:{6}, "
             + "MainLang:{7}, Name:{8}, Description:{9}.",
             nGroupID, nChannelID, nEPGSchedID, sEpgIdentifier, sEpgIdentifier, dStartDate.ToString(), dEndDate.ToString(), sThumb, sMainLang, sName, sDescription);

            if (nEPGSchedID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("epg_channels_schedule");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", nChannelID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", sEpgIdentifier);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                nEPGSchedID = GetEPGSchedIDByEPGIdentifier(nGroupID, sEpgIdentifier);

                Int32 nPicID = DownloadEPGPic(sThumb, sName, nGroupID, nEPGSchedID, sMainLang);
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nEPGSchedID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            else
            {
                Int32 nPicID = 0;
                nPicID = DownloadEPGPic(sThumb, sName, nGroupID, nEPGSchedID, sMainLang);
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", nChannelID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", sEpgIdentifier);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nEPGSchedID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

            log.DebugFormat("After UpdateInsertBasicEPGMainLangData. GID:{0}, ChannelID:{1}, EPGSchedID:{2}, EpgIdentifier:{3}, StartDate:{4}, EndDate:{5}, Thumb:{6}, "
           + "MainLang:{7}, Name:{8}, Description:{9}.",
           nGroupID, nChannelID, nEPGSchedID, sEpgIdentifier, sEpgIdentifier, dStartDate.ToString(), dEndDate.ToString(), sThumb, sMainLang, sName, sDescription);
            return true;
        }

        static protected bool UpdateInsertBasicEPGSubLangData(Int32 nGroupID, Int32 nEPGSchedID,
            string sMainLang, ref XmlNode theItemNames, ref XmlNode theItemDesc)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_languages where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(CODE3)))", "<>", sMainLang.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["CODE3"].ToString();
                    Int32 nLangID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    string sMediaName = GetMultiLangValue(sLang, ref theItemNames);
                    string sMediaDesc = GetMultiLangValue(sLang, ref theItemDesc);
                    Int32 nEPGSchedTransID = 0;
                    bool b = false;
                    if (sMediaName.Trim() != "" || sMediaDesc.Trim() != "")
                        nEPGSchedTransID = GetEPGSchedTranslateID(nEPGSchedID, nLangID, ref b, true);
                    else
                        nEPGSchedTransID = GetEPGSchedTranslateID(nEPGSchedID, nLangID, ref b, false);
                    if (nEPGSchedTransID != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule_translate");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sMediaName);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sMediaDesc);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                        updateQuery += "where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nEPGSchedTransID);

                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            return true;
        }

        static public bool DoesReplicationClean(Int32 nMax, ref Int32 nRet)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select commands_2_apply from replication_status";
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["commands_2_apply"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nRet > nMax)
                return false;
            return true;
        }

        static public bool TryParseXml(string feedXml, int groupId, out XmlDocument feedXmlDocument, IngestResponse response)
        {
            feedXmlDocument = new XmlDocument();

            try
            {
                feedXmlDocument.LoadXml(feedXml);
                return true;
            }
            catch (XmlException ex)
            {
                response.IngestStatus = new Status() { Code = (int)eResponseStatus.IllegalXml, Message = "XML file with wrong format" };
                log.ErrorFormat("XML file with wrong format: {0}. GID:{1}. Exception: {2}", feedXml, groupId, ex);
            }
            catch (Exception ex)
            {
                response.IngestStatus = new Status() { Code = (int)eResponseStatus.IllegalXml, Message = "Error while loading file" };
                log.ErrorFormat("Failed loading file: {0}. GID:{1}. Exception: {2}", feedXml, groupId, ex);
            }

            return false;
        }

        static public bool DoTheWorkInner(string sXML, Int32 nGroupID, string sNotifyURL, ref string sNotifyXML, bool uploadDirectory)
        {
            IngestResponse ingestResponse = null;
            return DoTheWorkInner(sXML, nGroupID, sNotifyURL, ref sNotifyXML, uploadDirectory, out ingestResponse);
        }
        static public bool DoTheWorkInner(string sXML, Int32 nGroupID, string sNotifyURL, ref string sNotifyXML, bool uploadDirectory, out IngestResponse ingestResponse)
        {
            bool isSuccess = false;
            XmlDocument theDoc = null;
            IngestAssetStatus ingestAssetStatus = null;

            ingestResponse = new IngestResponse()
            {
                IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() },
                AssetsStatus = new List<IngestAssetStatus>()
            };

            log.DebugFormat("Start Load Feed. GID:{0}", nGroupID);

            // ValidateXml
            // in case Xml not valid
            if (TryParseXml(sXML, nGroupID, out theDoc, ingestResponse))
            {
                try
                {
                    int nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nGroupID, "MAIN_CONNECTION_STRING").ToString());
                    if (nParentGroupID == 1)
                    {
                        nParentGroupID = nGroupID;
                    }

                    XmlNodeList mediaItems = theDoc.SelectNodes("/feed/export/media");
                    log.DebugFormat("Total medias count : {0}. GID:{1}", mediaItems.Count, nGroupID);

                    for (int mediaIndex = 0; mediaIndex < mediaItems.Count; mediaIndex++)
                    {
                        //create ingestAssetStatus for saving Media load data and status
                        ingestAssetStatus = new IngestAssetStatus()
                        {
                            Warnings = new List<Status>(),
                            Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() }
                        };
                        ingestResponse.AssetsStatus.Add(ingestAssetStatus);

                        Int32 nMediaID = 0;

                        try
                        {
                            string sCoGuid = "";
                            string sErrorMessage = "";
                            bool isActive = false;
                            bool bProcess = ProcessItem(mediaItems[mediaIndex], ref sCoGuid, ref nMediaID, ref sErrorMessage, nGroupID, ref isActive, ref ingestAssetStatus);

                            if (bProcess == false)
                            {
                                log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", mediaIndex, sCoGuid, nMediaID, sErrorMessage);
                                sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + sErrorMessage + "\" tvm_id=\"" + nMediaID.ToString() + "\"/>";
                                continue;
                            }
                            else
                            {
                                ingestAssetStatus.Status.Code = (int)eResponseStatus.OK;
                                ingestAssetStatus.Status.Message = eResponseStatus.OK.ToString();
                                log.DebugFormat("succeeded export media. CoGuid:{0}, MediaID:{1}, ErrorMessage:{2}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                                sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"OK\" message=\"" + ProtocolsFuncs.XMLEncode(sErrorMessage, true) + "\" tvm_id=\"" + nMediaID.ToString() + "\"/>";

                                // Update record in Catalog (see the flow inside Update Index
                                bool resultMQ = ImporterImpl.UpdateIndex(new List<int>() { nMediaID }, nParentGroupID, eAction.Update);
                                if (resultMQ)
                                    log.DebugFormat("UpdateIndex: Succeeded. CoGuid:{0}, MediaID:{1}, isActive:{2}, ErrorMessage:{3}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                                else
                                {
                                    log.ErrorFormat("UpdateIndex: Failed. CoGuid:{0}, MediaID:{1}, isActive:{2}, ErrorMessage:{3}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.UpdateIndexFailed, Message = UPDATE_INDEX_FAILED });
                                }

                                // update notification 
                                if (isActive)
                                    UpdateNotificationsRequests(nGroupID, nMediaID);
                            }
                        }
                        catch (Exception exc)
                        {
                            log.ErrorFormat("Failed process MediaID: {0}. Exception:{2}", nMediaID, exc);
                        }
                    }

                    if (uploadDirectory)
                    {
                        UploadDirectory(nGroupID);
                    }
                    else
                    {
                        UploadQueue.UploadQueueHelper.SetJobsForUpload(nGroupID);
                    }

                    isSuccess = true;
                }
                catch (Exception ex)
                {

                    log.ErrorFormat("Error while import media xml:{0}. GID:{1}", sXML, nGroupID);
                    sNotifyXML += "<exception message=\"" + ProtocolsFuncs.XMLEncode(ex.Message, true) + "\"/>";
                }

                try
                {
                    XmlNodeList channelItems = theDoc.SelectNodes("/feed/export/channel");
                    log.DebugFormat("Total channels count: {0}. GID:{1}", channelItems.Count, nGroupID);

                    for (int channelIndex = 0; channelIndex < channelItems.Count; channelIndex++)
                    {
                        Int32 nEPGChannelID = 0;
                        ingestAssetStatus = new IngestAssetStatus() { Warnings = new List<Status>(), Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() } };
                        ingestResponse.AssetsStatus.Add(ingestAssetStatus);

                        try
                        {
                            string sErrorMessage = "";
                            XmlNode theChannelItem = channelItems[channelIndex];
                            string sTVMID = GetItemParameterVal(ref theChannelItem, "tvm_id");

                            sNotifyXML += "<channel tvm_id=\"" + sTVMID + "\">";

                            if (sTVMID != "")
                            {
                                log.DebugFormat("Succeeded import EPGChannelID:{0}. GID:{1}", sTVMID, nGroupID);
                                nEPGChannelID = int.Parse(sTVMID);
                                ingestAssetStatus.InternalAssetId = nEPGChannelID;
                            }
                            else
                            {
                                log.ErrorFormat("Error import channel. GID: {0}", nGroupID);
                                sNotifyXML += "<error>No such channel</error>";
                                sNotifyXML += "</channel>";
                                ingestAssetStatus.Status.Code = (int)IngestWarnings.ErrorExportChannel;
                                ingestAssetStatus.Status.Message = ERROR_EXPORT_CHANNEL;
                                continue;
                            }

                            XmlNodeList entryItems = theChannelItem.SelectNodes("entry");
                            log.DebugFormat("Total entries count : {0}. GID: {1}", entryItems.Count, nGroupID);

                            for (int entryIndex = 0; entryIndex < entryItems.Count; entryIndex++)
                            {
                                Int32 nEPGSchedId = 0;
                                string sEPGIdentifier = "";
                                bool bProcess = ProcessEPGItem(entryItems[entryIndex], nEPGChannelID, ref nEPGSchedId, ref sEPGIdentifier, ref sErrorMessage, nGroupID, ref ingestAssetStatus);

                                if (bProcess == false)
                                {
                                    log.ErrorFormat("Error import epg. EPGChannelID:{0}, EPGSchedId:{1}, EPGIdentifier:{2}, ErrorMessage:{3}", nEPGChannelID, nEPGSchedId, sEPGIdentifier, sErrorMessage);
                                    sNotifyXML += "<entry co_guid=\"" + sEPGIdentifier + "\" status=\"FAILED\" message=\"" + sErrorMessage + "\" tvm_entry_id=\"" + nEPGSchedId.ToString() + "\"/>";
                                    continue;
                                }
                                else
                                {
                                    ingestAssetStatus.Status.Code = (int)eResponseStatus.OK;
                                    ingestAssetStatus.Status.Message = eResponseStatus.OK.ToString();
                                    log.DebugFormat("succeeded import epg.EPGChannelID:{0}, EPGSchedId:{1}, EPGIdentifier:{2}, ErrorMessage:{3}", nEPGChannelID, nEPGSchedId, sEPGIdentifier, sErrorMessage);
                                    sNotifyXML += "<entry co_guid=\"" + sEPGIdentifier + "\" status=\"OK\" message=\"" + ProtocolsFuncs.XMLEncode(sErrorMessage, true) + "\" tvm_entry_id=\"" + nEPGSchedId.ToString() + "\"/>";
                                }
                            }
                            sNotifyXML += "</channel>";
                        }
                        catch (Exception exc)
                        {
                            log.ErrorFormat("Failed process channel: {0}. GID: {1}. Exception:{2}", nEPGChannelID, nGroupID, exc);
                        }
                    }

                    isSuccess = true;
                }

                catch (Exception ex)
                {
                    sNotifyXML += "<exception message=\"" + ProtocolsFuncs.XMLEncode(ex.Message, true) + "\"/>";
                }
            }
            else
            {
                isSuccess = false;
            }

            if (isSuccess)
            {
                ingestResponse.IngestStatus.Code = (int)eResponseStatus.OK;
                ingestResponse.IngestStatus.Message = eResponseStatus.OK.ToString();
            }
            return isSuccess;
        }

        static public bool DoTheWorkInner(string sXML, Int32 nGroupID, string sNotifyURL, ref string sNotifyXML)
        {
            return DoTheWorkInner(sXML, nGroupID, sNotifyURL, ref sNotifyXML, true);
        }

        static public bool DoTheWork(Int32 nGroupID, string sXMLUrl, string sNotifyURL, Int32 nAlertID)
        {
            string sNotifyXML = "<tvm><importer>";
            Int32 nStatus = 404;
            string sXML = "";
            try
            {
                sXML = Notifier.SendGetHttpReq(sXMLUrl, ref nStatus);
            }
            catch (Exception ex)
            {
                sNotifyXML += "<exception message=\"" + ProtocolsFuncs.XMLEncode(ex.Message, true) + "\"/>";
                sNotifyXML += "</tvm></importer>";
                return false;
            }
            bool bRet = DoTheWorkInner(sXML, nGroupID, sNotifyURL, ref sNotifyXML);
            if (sNotifyURL.Trim() != "")
                Notifier.SendXMLHttpReq(sNotifyURL, sNotifyXML, ref nStatus);

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("importer_alerts");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RESPONSE_XML", "=", sNotifyXML);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_STATUS", "=", 2);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nAlertID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            return bRet;
        }

        #region Lucene

        //Update Lucene Directory for the new media that was insert
        static public bool UpdateRecordInLucene(int groupid, int nMediaID)
        {
            bool bUpdate = false;

            Lucene_WCF.Service service = new Lucene_WCF.Service();
            string sWSURL = GetLuceneUrl(groupid);
            if (!string.IsNullOrEmpty(sWSURL))
            {
                foreach (string url in sWSURL.Split(';'))
                {
                    try
                    {
                        service.Url = url;
                        bUpdate = service.UpdateRecord(groupid, nMediaID);
                        log.Debug("UpdateRecordInLucene - " + string.Format("Group:{0}, Media:{1}, Url:{2}, Res:{3}", groupid, nMediaID, url, bUpdate));
                    }
                    catch (Exception ex)
                    {
                        log.Error("Exception (UpdateRecordInLucene) - " + string.Format("Media:{0}, Url:{1}, ex:{2}", nMediaID, url, ex.Message), ex);
                    }
                }
            }

            return bUpdate;
        }

        //remove media from Lucene
        static public bool RemoveRecordInLucene(int groupid, int nMediaID)
        {
            bool bremove = false;

            Lucene_WCF.Service service = new Lucene_WCF.Service();
            string sWSURL = GetLuceneUrl(groupid);
            if (!string.IsNullOrEmpty(sWSURL))
            {
                foreach (string url in sWSURL.Split(';'))
                {
                    try
                    {
                        service.Url = url;
                        bremove = service.RemoveRecord(groupid, nMediaID);
                        log.Debug("RemoveRecordInLucene - " + string.Format("Group:{0}, Media:{1}, Url:{2}, Res:{3}", groupid, nMediaID, url, bremove));
                    }
                    catch (Exception ex)
                    {
                        log.Error("Exception (RemoveRecordInLucene) - " + string.Format("Media:{0}, Url:{1}, ex:{2}", nMediaID, url, ex.Message), ex);
                        return false;
                    }
                }
            }
            return bremove;
        }

        //Update Lucene Directory for the updated channel 
        static public bool UpdateChannelInLucene_old(int groupid, int nChannelID)
        {
            bool bUpdate = false;

            Lucene_WCF.Service service = new Lucene_WCF.Service();
            string sWSURL = GetLuceneUrl(groupid);
            if (!string.IsNullOrEmpty(sWSURL))
            {
                foreach (string url in sWSURL.Split(';'))
                {
                    try
                    {
                        service.Url = url;
                        bUpdate = service.UpdateChannel(groupid, nChannelID);
                        log.Debug("UpdateChannelInLucene - " + string.Format("Group:{0}, Channel:{1}, Url:{2}, Res:{3}", groupid, nChannelID, url, bUpdate));
                    }
                    catch (Exception ex)
                    {
                        log.Error("Exception (UpdateChannelInLucene) - " + string.Format("Channel:{0}, Url:{1}, ex:{2}", nChannelID, url, ex.Message), ex);
                        return false;
                    }
                }
            }

            return bUpdate;
        }

        static public bool UpdateChannelInLucene(int nGroupId, int nChannelID)
        {
            bool bUpdate = false;

            WSCatalog.IserviceClient wsCatalog = null;

            try
            {
                string sWSURL = GetCatalogUrlByParameters(nGroupId, eObjectType.Channel, eAction.Update);

                if (!string.IsNullOrEmpty(sWSURL))
                {
                    string[] arrAddresses = sWSURL.Split(';');

                    foreach (string sEndPointAddress in arrAddresses)
                    {
                        try
                        {
                            wsCatalog = GetCatalogClient(sEndPointAddress);

                            if (wsCatalog != null)
                            {
                                bUpdate = wsCatalog.UpdateChannel(nGroupId, nChannelID);

                                log.Debug("UpdateChannelInLucene - " + string.Format("Group:{0}, Channel:{1}, Url:{2}, Res:{3}", nGroupId, nChannelID, sEndPointAddress, bUpdate));

                                wsCatalog.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception (UpdateChannelInLucene) - " + string.Format("Channel:{0}, Url:{1}, ex:{2}", nChannelID, sEndPointAddress, ex.Message), ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception (UpdateChannelInLucene) - " + string.Format("Error:{0}", ex.Message), ex);
            }
            finally
            {
                wsCatalog.Close();
            }

            return bUpdate;
        }


        private static string GetLuceneUrl(int nGroupID)
        {
            string sLuceneURL = GetConfigVal("LUCENE_WCF_" + nGroupID);
            try
            {
                DataTable dt = DAL.ImporterImpDAL.Get_LuceneUrl(nGroupID);
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        sLuceneURL = dt.Rows[0]["lucene_url"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetLuceneUrl - GroupID : " + nGroupID + ", error : " + ex.Message, ex);
            }

            return sLuceneURL;
        }

        /// <summary>
        /// Gets all catalog urls releated to the given group id
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns>Concatenated urls from DB</returns>
        private static string GetCatalogUrl(int nGroupID)
        {
            string sCatalogURL = GetConfigVal("WS_Catalog");
            try
            {
                DataTable dt = DAL.ImporterImpDAL.Get_CatalogUrl(nGroupID);
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        sCatalogURL = dt.Rows[0]["catalog_url"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetCatalogUrl - GroupID : " + nGroupID + ", error : " + ex.Message, ex);
            }

            return sCatalogURL;
        }

        /// <summary>
        /// Gets all catalog urls releated to the given group id
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns>Concatenated urls from DB</returns>
        private static string GetCatalogUrlByParameters(int groupId, eObjectType? objectType, eAction? action)
        {
            string tcmCatalogURL = GetConfigVal("WS_Catalog");
            string catalogURL = tcmCatalogURL;

            try
            {
                catalogURL = DAL.ImporterImpDAL.Get_CatalogUrlByParameters(groupId, objectType, action);

                if (!catalogURL.Contains(tcmCatalogURL))
                {
                    catalogURL = string.Format("{0};{1}", catalogURL, tcmCatalogURL);
                }
            }
            catch (Exception ex)
            {
                log.Error("GetCatalogUrlByAction - GroupID : " + groupId + ", error : " + ex.Message, ex);
                catalogURL = tcmCatalogURL;
            }

            return catalogURL;
        }

        #endregion

        #region Notification

        static public ApiObjects.Response.Status AddMessageAnnouncement(int groupID, bool Enabled, string name, string message, int Recipients, DateTime date, string timezone, ref int id)
        {
            AddMessageAnnouncementResponse response = null;
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);
                MessageAnnouncement announcement = new MessageAnnouncement();
                announcement.Message = message;
                announcement.Name = name;
                announcement.Recipients = (eAnnouncementRecipientsType)Recipients;
                announcement.StartTime = ODBCWrapper.Utils.DateTimeToUnixTimestamp(date);
                announcement.Timezone = timezone;
                announcement.Enabled = Enabled;
                response = service.AddMessageAnnouncement(sWSUserName, sWSPass, announcement);
                if (response != null && response.Status.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    id = response.Id;
                }
                return response.Status;
            }
            catch
            {
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
        }

        static public ApiObjects.Response.Status UpdateMessageAnnouncement(int groupID, int id, bool Enabled, string name, string message, int Recipients, DateTime date, string timezone)
        {
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);
                MessageAnnouncement announcement = new MessageAnnouncement();
                announcement.Message = message;
                announcement.Name = name;
                announcement.Recipients = (eAnnouncementRecipientsType)Recipients;
                announcement.StartTime = ODBCWrapper.Utils.DateTimeToUnixTimestamp(date);
                announcement.Timezone = timezone;
                announcement.MessageAnnouncementId = id;
                announcement.Enabled = Enabled;
                MessageAnnouncementResponse response = service.UpdateMessageAnnouncement(sWSUserName, sWSPass, id, announcement);
                return response.Status;
            }
            catch (Exception)
            {
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString()); ;
            }
        }


        static public bool UpdateMessageAnnouncementStatus(int groupID, int id, bool status)
        {
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                ApiObjects.Response.Status response = service.UpdateMessageAnnouncementStatus(sWSUserName, sWSPass, id, status);
                if (response != null && response.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }


        //static public DataTable GetAllMessageAnnouncements(int groupid)
        //{
        //    DataTable dt = null;
        //    try
        //    {
        //        //Call Notifications WCF service
        //        string sWSURL = GetConfigVal("NotificationService");
        //        Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
        //        if (!string.IsNullOrEmpty(sWSURL))
        //            service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

        //        string sIP = "1.1.1.1";
        //        string sWSUserName = "";
        //        string sWSPass = "";
        //        int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupid);
        //        TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

        //        GetAllMessageAnnouncementsResponse response = service.GetAllMessageAnnouncements(sWSUserName, sWSPass, 0 ,0);

        //        if (response != null && response.totalCount > 0)
        //        {
        //            dt = null;
        //        }
        //        else
        //        {
        //            dt = new DataTable();

        //            dt.Columns.Add("ID", typeof(int));
        //            dt.Columns.Add("recipientsCode", typeof(int));
        //            dt.Columns.Add("status", typeof(int));
        //            dt.Columns.Add("is_active", typeof(int));
        //            dt.Columns.Add("name", typeof(string));
        //            dt.Columns.Add("message", typeof(string));
        //            dt.Columns.Add("start_time", typeof(DateTime));
        //            dt.Columns.Add("sent", typeof(int));
        //            dt.Columns.Add("updater_id", typeof(int));
        //            dt.Columns.Add("update_date", typeof(DateTime));
        //            dt.Columns.Add("create_date", typeof(DateTime));
        //            dt.Columns.Add("group_id", typeof(int));
        //            dt.Columns.Add("timezone", typeof(string));
        //            dt.Columns.Add("recipients", typeof(string));
        //            dt.Columns.Add("message status", typeof(string));


        //            foreach (MessageAnnouncement ma in response.messageAnnouncements)
        //            {
        //                dt.Rows.Add(  ma.MessageAnnouncementId, (int)ma.Recipients, 1, ma.Enabled, ma.Name, ma.Message, ma.StartTime, (int)ma.Status
        //                    ma.Message, ma.Name, ma.MessageAnnouncementId);

        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        dt = null;
        //    }
        //    return dt;

        //}

        static public void UpdateNotificationsRequests(int groupid, int nMediaID)
        {
            ParameterizedThreadStart start = new ParameterizedThreadStart(UpdateNotification);
            Thread t = new Thread(start);
            int[] vals = new int[2];
            vals[0] = groupid;
            vals[1] = nMediaID;
            t.Start(vals);
        }

        static private void UpdateNotification(object val)
        {
            int[] vals = (int[])val;
            int nGroupID = vals[0];
            int mediaID = vals[1];
            bool notificationResult = ImporterImpl.UpdateNotificationRequest(nGroupID, mediaID); //(LoginManager.GetLoginGroupID(), nID);
        }
        //Update Notification Request for the new media that was insert
        static private bool UpdateNotificationRequest(int groupid, int nMediaID)
        {
            bool bUpdate = false;
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupid);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);
                bUpdate = service.AddNotificationRequest(sWSUserName, sWSPass, string.Empty, NotificationTriggerType.FollowUpByTag, nMediaID);
            }
            catch (Exception ex)
            {
                log.Error("Exception (UpdateNotificationRequest) - " + string.Format("Media:{0}, groupID:{1}, ex:{2}", nMediaID, groupid, ex.Message), ex);
                return false;
            }
            return bUpdate;
        }

        static public ApiObjects.Response.Status SetMessageTemplate(int groupID, ref ApiObjects.Notification.MessageTemplate messageTemplate)
        {
            ApiObjects.Notification.MessageTemplateResponse response = null;
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                ApiObjects.Notification.MessageTemplate wcfMessageTemplate = new ApiObjects.Notification.MessageTemplate()
                {
                    AssetType = messageTemplate.AssetType,
                    Message = messageTemplate.Message,
                    Sound = messageTemplate.Sound,
                    Action = messageTemplate.Action,
                    URL = messageTemplate.URL,
                    Id = messageTemplate.Id,
                    DateFormat = messageTemplate.DateFormat
                };

                response = service.SetMessageTemplate(sWSUserName, sWSPass, wcfMessageTemplate);
                if (response != null && response.Status.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    messageTemplate = new ApiObjects.Notification.MessageTemplate()
                    {
                        Id = response.MessageTemplate.Id,
                        Message = response.MessageTemplate.Message,
                        DateFormat = response.MessageTemplate.DateFormat,
                        AssetType = response.MessageTemplate.AssetType,
                    };
                }
                return response.Status;
            }
            catch
            {
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
        }


        #endregion

        private static string GetConfigVal(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        #region create client to WCF service

        internal static class BindingFactory
        {
            internal static Binding CreateInstance()
            {
                WSHttpBinding binding = new WSHttpBinding();
                binding.Security.Mode = SecurityMode.None;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.UseDefaultWebProxy = true;
                return binding;
            }

        }


        internal static WSCatalog.IserviceClient GetWCFSvc(string sSiteUrl)
        {
            string siteUrl = GetConfigVal(sSiteUrl);
            Uri serviceUri = new Uri(siteUrl);
            EndpointAddress endpointAddress = new EndpointAddress(serviceUri);

            //Create the binding here
            Binding binding = BindingFactory.CreateInstance();
            WSCatalog.IserviceClient client = new WSCatalog.IserviceClient(binding, endpointAddress);
            return client;
        }

        /// <summary>
        /// Creates a new Catalog web service instance with the given end point address
        /// </summary>
        /// <param name="p_sEndPointAddress"></param>
        /// <returns></returns>
        internal static WSCatalog.IserviceClient GetCatalogClient(string p_sEndPointAddress)
        {
            Uri uri = new Uri(p_sEndPointAddress);
            EndpointAddress endpointAddress = new EndpointAddress(uri);

            Binding binding = BindingFactory.CreateInstance();
            WSCatalog.IserviceClient client = new WSCatalog.IserviceClient(binding, endpointAddress);

            return client;
        }

        #endregion

        public static bool UpdateIndex(List<int> lMediaIds, int nGroupId, ApiObjects.eAction eAction)
        {
            bool isUpdateIndexSucceeded = false;

            WSCatalog.IserviceClient wsCatalog = null;

            try
            {
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(nGroupId);

                if (lMediaIds != null && lMediaIds.Count > 0 && nParentGroupID > 0)
                {
                    string sWSURL = GetCatalogUrlByParameters(nParentGroupID, eObjectType.Media, eAction);

                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        string[] arrAddresses = sWSURL.Split(';');
                        int[] arrMediaIds = lMediaIds.ToArray();

                        foreach (string sEndPointAddress in arrAddresses)
                        {
                            if (string.IsNullOrWhiteSpace(sEndPointAddress))
                            {
                                log.WarnFormat("UpdateIndex - one of Catalog URLs is empty");
                                continue;
                            }

                            try
                            {
                                wsCatalog = GetCatalogClient(sEndPointAddress);

                                if (wsCatalog != null)
                                {
                                    WSCatalog.eAction actionCatalog = WSCatalog.eAction.On;

                                    switch (eAction)
                                    {
                                        case eAction.Off:
                                            actionCatalog = WSCatalog.eAction.Off;
                                            break;
                                        case eAction.On:
                                            actionCatalog = WSCatalog.eAction.On;
                                            break;
                                        case eAction.Update:
                                            actionCatalog = WSCatalog.eAction.Update;
                                            break;
                                        case eAction.Delete:
                                            actionCatalog = WSCatalog.eAction.Delete;
                                            break;
                                        case eAction.Rebuild:
                                            actionCatalog = WSCatalog.eAction.Rebuild;
                                            break;
                                        default:
                                            break;
                                    }
                                    isUpdateIndexSucceeded = wsCatalog.UpdateIndex(
                                        arrMediaIds,
                                        nParentGroupID, actionCatalog);

                                    string sInfo = isUpdateIndexSucceeded == true ? "succeeded" : "not succeeded";
                                    log.DebugFormat("Update index {0} in catalog '{1}'", sInfo, sEndPointAddress);
                                    wsCatalog.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error(string.Format("Couldn't update catalog '{0}' due to the following error: {1}", sEndPointAddress, ex.Message), ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            finally
            {
                if (wsCatalog != null)
                {
                    wsCatalog.Close();
                }
            }

            return isUpdateIndexSucceeded;
        }

        public static bool UpdateChannelIndex(int nGroupId, List<int> lChannelIds, eAction eAction)
        {
            bool isUpdateChannelIndexSucceeded = false;

            string sUseElasticSearch = GetConfigVal("indexer");
            if (!string.IsNullOrEmpty(sUseElasticSearch) && sUseElasticSearch.Equals("ES"))
            {
                WSCatalog.IserviceClient wsCatalog = null;

                try
                {
                    int nParentGroupID = DAL.UtilsDal.GetParentGroupID(nGroupId);

                    if (lChannelIds != null && lChannelIds.Count > 0 && nParentGroupID > 0)
                    {
                        string sWSURL = GetCatalogUrlByParameters(nParentGroupID, eObjectType.Channel, eAction);

                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            string[] arrAddresses = sWSURL.Split(';');
                            int[] arrChannelIds = lChannelIds.ToArray();

                            foreach (string sEndPointAddress in arrAddresses)
                            {
                                try
                                {
                                    wsCatalog = GetCatalogClient(sEndPointAddress);

                                    if (wsCatalog != null)
                                    {
                                        WSCatalog.eAction actionCatalog = WSCatalog.eAction.On;

                                        switch (eAction)
                                        {
                                            case eAction.Off:
                                                actionCatalog = WSCatalog.eAction.Off;
                                                break;
                                            case eAction.On:
                                                actionCatalog = WSCatalog.eAction.On;
                                                break;
                                            case eAction.Update:
                                                actionCatalog = WSCatalog.eAction.Update;
                                                break;
                                            case eAction.Delete:
                                                actionCatalog = WSCatalog.eAction.Delete;
                                                break;
                                            case eAction.Rebuild:
                                                actionCatalog = WSCatalog.eAction.Rebuild;
                                                break;
                                            default:
                                                break;
                                        }

                                        isUpdateChannelIndexSucceeded = wsCatalog.UpdateChannelIndex(
                                            arrChannelIds,
                                            nParentGroupID, actionCatalog);

                                        string sInfo = isUpdateChannelIndexSucceeded == true ? "succeeded" : "not succeeded";
                                        log.DebugFormat("Update channel index {0} in catalog '{1}'", sInfo, sEndPointAddress);

                                        wsCatalog.Close();
                                    }

                                }
                                catch (Exception ex)
                                {
                                    log.ErrorFormat(string.Format("Couldn't update catalog '{0}' due to the following error: {1}", sEndPointAddress, ex.Message), ex);
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    log.Error("process failed", ex);
                }
                finally
                {
                    if (wsCatalog != null)
                    {
                        wsCatalog.Close();
                    }
                }
            }
            else
            {
                if (lChannelIds != null && lChannelIds.Count > 0)
                {
                    isUpdateChannelIndexSucceeded = ImporterImpl.UpdateChannelInLucene(nGroupId, lChannelIds[0]);
                }
            }
            return isUpdateChannelIndexSucceeded;
        }

        public static bool UpdateOperator(int nGroupID, int nOperatorID, int nSubscriptionID, long lChannelID, eOperatorEvent oe)
        {
            bool res = true;
            WSCatalog.IserviceClient wsCatalog = null;
            try
            {
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(nGroupID);
                string sWSURL = GetCatalogUrlByParameters(nParentGroupID, eObjectType.Unknown, eAction.Update);

                if (!string.IsNullOrEmpty(sWSURL))
                {
                    string[] arrAddresses = sWSURL.Split(';');

                    if (arrAddresses != null && arrAddresses.Length > 0)
                    {
                        int nLength = arrAddresses.Length;

                        for (int i = 0; i < nLength; i++)
                        {
                            wsCatalog = GetCatalogClient(arrAddresses[i]);

                            if (wsCatalog != null)
                            {
                                WSCatalog.eOperatorEvent oeCatalog = WSCatalog.eOperatorEvent.ChannelAddedToSubscription;

                                switch (oe)
                                {
                                    case eOperatorEvent.ChannelAddedToSubscription:
                                        oeCatalog = WSCatalog.eOperatorEvent.ChannelAddedToSubscription;
                                        break;
                                    case eOperatorEvent.ChannelRemovedFromSubscription:
                                        oeCatalog = WSCatalog.eOperatorEvent.ChannelRemovedFromSubscription;
                                        break;
                                    case eOperatorEvent.SubscriptionAddedToOperator:
                                        oeCatalog = WSCatalog.eOperatorEvent.SubscriptionAddedToOperator;
                                        break;
                                    case eOperatorEvent.SubscriptionRemovedFromOperator:
                                        oeCatalog = WSCatalog.eOperatorEvent.SubscriptionRemovedFromOperator;
                                        break;
                                    default:
                                        break;
                                }
                                res &= wsCatalog.UpdateOperator(nParentGroupID, nOperatorID, nSubscriptionID, lChannelID, oeCatalog);

                                wsCatalog.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception. Msg: ", ex.Message));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" Operator ID: ", nOperatorID));
                sb.Append(String.Concat(" Sub ID: ", nSubscriptionID));
                sb.Append(String.Concat(" Channel ID: ", lChannelID));
                sb.Append(String.Concat(" Operator Event: ", oe.ToString().ToLower()));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                log.Error("UpdateOperator - " + sb.ToString(), ex);
                #endregion
                res = false;
            }
            finally
            {
                if (wsCatalog != null)
                    wsCatalog.Close();
            }

            return res;
        }

        public static bool UpdateEpg(List<ulong> epgIds, int groupId, ApiObjects.eAction action, bool datesUpdates = true)
        {
            bool isUpdateIndexSucceeded = false;

            if (epgIds == null || epgIds.Count == 0)
            {
                return isUpdateIndexSucceeded;
            }

            try
            {
                #region Update EPG Index (Catalog)
                isUpdateIndexSucceeded = UpdateEPGIndex(epgIds, groupId, action);

                #endregion

                #region Update Recordings (CAS)

                // Update recordings only if we know that the dates have changed
                if (datesUpdates)
                {
                    UpdateRecordingsOfEPGs(epgIds, groupId, action);
                }

                #endregion

                // Update recordings only if we know that the dates have changed
                if (datesUpdates)
                {
                    UpdateRemindersfEPGs(epgIds, groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error("ImporterImpl - Update EPG failed, ex = {0}", ex);
            }

            return isUpdateIndexSucceeded;
        }

        private static void UpdateRemindersfEPGs(List<ulong> epgIds, int groupId)
        {
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                log.DebugFormat("Start notifiction.HandleEpgEventAsync has been called for epgIds {0}", string.Join(", ", epgIds.Select(x => x.ToString()).ToArray()));
                service.HandleEpgEventAsync(groupId, epgIds.ToArray());
                log.DebugFormat("Finish notifiction.HandleEpgEventAsync has been called for epgIds {0}", string.Join(", ", epgIds.Select(x => x.ToString()).ToArray()));
            }
            catch (Exception ex)
            {
                log.Error("ImporterImpl - Update reminder failed on Notification, ex = {0}", ex);
            }
        }

        public static bool UpdateEPGIndex(List<ulong> epgIds, int groupId, ApiObjects.eAction action)
        {
            bool isUpdateIndexSucceeded = false;

            int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

            string sUseElasticSearch = GetConfigVal("indexer");  /// Indexer - ES / Lucene
            if (!string.IsNullOrEmpty(sUseElasticSearch) && sUseElasticSearch.Equals("ES")) //ES
            {
                WSCatalog.IserviceClient wsCatalog = null;

                try
                {
                    wsCatalog = GetWCFSvc("WS_Catalog");

                    string sWSURL = GetCatalogUrlByParameters(groupId, eObjectType.EPG, action);

                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        string[] arrAddresses = sWSURL.Split(';');
                        int[] arrEPGIds = new int[epgIds.Count];
                        int nArrayIndex = 0;

                        foreach (ulong item in epgIds)
                        {
                            arrEPGIds[nArrayIndex] = int.Parse(item.ToString());
                            nArrayIndex++;
                        }

                        foreach (string sEndPointAddress in arrAddresses)
                        {
                            try
                            {
                                wsCatalog = GetCatalogClient(sEndPointAddress);

                                if (wsCatalog != null)
                                {
                                    WSCatalog.eAction actionCatalog = WSCatalog.eAction.On;

                                    switch (action)
                                    {
                                        case eAction.Off:
                                            actionCatalog = WSCatalog.eAction.Off;
                                            break;
                                        case eAction.On:
                                            actionCatalog = WSCatalog.eAction.On;
                                            break;
                                        case eAction.Update:
                                            actionCatalog = WSCatalog.eAction.Update;
                                            break;
                                        case eAction.Delete:
                                            actionCatalog = WSCatalog.eAction.Delete;
                                            break;
                                        case eAction.Rebuild:
                                            actionCatalog = WSCatalog.eAction.Rebuild;
                                            break;
                                        default:
                                            break;
                                    }

                                    isUpdateIndexSucceeded = wsCatalog.UpdateEpgIndex(
                                        arrEPGIds,
                                        parentGroupID, actionCatalog);

                                    string sInfo = isUpdateIndexSucceeded == true ? "succeeded" : "not succeeded";
                                    log.DebugFormat("Update index {0} in catalog '{1}'", sInfo, sEndPointAddress);

                                    wsCatalog.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Couldn't update catalog '{0}' due to the following error: {1}", sEndPointAddress, ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("process failed", ex);
                }
                finally
                {
                    if (wsCatalog != null)
                    {
                        wsCatalog.Close();
                    }
                }
            }
            return isUpdateIndexSucceeded;
        }

        public static void UpdateRecordingsOfEPGs(List<ulong> epgIds, int groupId, ApiObjects.eAction action, string tcmKey = "conditionalaccess_ws")
        {
            try
            {
                string sWSUserName = string.Empty;
                string sWSPassword = string.Empty;
                WS_Utils.GetWSCredentials(groupId, eWSModules.CONDITIONALACCESS.ToString(), ref sWSUserName, ref sWSPassword);
                string casURL = TVinciShared.WS_Utils.GetTcmConfigValue(tcmKey);

                if (string.IsNullOrEmpty(sWSUserName) || string.IsNullOrEmpty(sWSPassword) || string.IsNullOrEmpty(casURL))
                {
                    log.ErrorFormat("Failed UpdateRecordingsOfEPGs, sWSUserName/sWSPassword/casURL is invalid for epgIds: {0}", string.Join(",", epgIds));
                    return;
                }

                TvinciImporter.WS_ConditionalAccess.module cas = new WS_ConditionalAccess.module();
                cas.Url = casURL;
                WS_ConditionalAccess.eAction casAction = WS_ConditionalAccess.eAction.Update;

                switch (action)
                {
                    case eAction.Off:
                        casAction = WS_ConditionalAccess.eAction.Off;
                        break;
                    case eAction.On:
                        casAction = WS_ConditionalAccess.eAction.On;
                        break;
                    case eAction.Update:
                        casAction = WS_ConditionalAccess.eAction.Update;
                        break;
                    case eAction.Delete:
                        casAction = WS_ConditionalAccess.eAction.Delete;
                        break;
                    case eAction.Rebuild:
                        casAction = WS_ConditionalAccess.eAction.Rebuild;
                        break;
                    default:
                        break;
                }

                cas.IngestRecordingAsync(sWSUserName, sWSPassword, epgIds.Select(i => (long)i).ToArray(), casAction);
                log.DebugFormat("cas.IngestRecordingAsync has been called for epgIds {0}", string.Join(", ", epgIds.Select(x => x.ToString()).ToArray()));
            }
            catch (Exception ex)
            {
                log.Error("ImporterImpl - Update recording failed on Conditional Access, ex = {0}", ex);
            }
        }

        public static bool UpdateEpgChannelIndex(List<ulong> epgChannels, int groupID, eAction eAction)
        {
            bool isUpdateIndexSucceeded = false;

            string sUseElasticSearch = GetConfigVal("indexer");  /// Indexer - ES / Lucene
            if (!string.IsNullOrEmpty(sUseElasticSearch) && sUseElasticSearch.Equals("ES")) //ES
            {
                WSCatalog.IserviceClient wsCatalog = null;

                try
                {
                    int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                    wsCatalog = GetWCFSvc("WS_Catalog");
                    if (epgChannels != null && epgChannels.Count > 0 && nParentGroupID > 0)
                    {
                        string sWSURL = GetCatalogUrlByParameters(nParentGroupID, eObjectType.EpgChannel, eAction);

                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            string[] arrAddresses = sWSURL.Split(';');
                            int[] arrEPGIds = new int[epgChannels.Count];
                            int nArrayIndex = 0;

                            foreach (ulong item in epgChannels)
                            {
                                arrEPGIds[nArrayIndex] = int.Parse(item.ToString());
                                nArrayIndex++;
                            }

                            foreach (string sEndPointAddress in arrAddresses)
                            {
                                try
                                {
                                    wsCatalog = GetCatalogClient(sEndPointAddress);

                                    if (wsCatalog != null)
                                    {
                                        WSCatalog.eAction actionCatalog = WSCatalog.eAction.Update;

                                        switch (eAction)
                                        {
                                            case eAction.Off:
                                                actionCatalog = WSCatalog.eAction.Off;
                                                break;
                                            case eAction.On:
                                                actionCatalog = WSCatalog.eAction.On;
                                                break;
                                            case eAction.Update:
                                                actionCatalog = WSCatalog.eAction.Update;
                                                break;
                                            case eAction.Delete:
                                                actionCatalog = WSCatalog.eAction.Delete;
                                                break;
                                            case eAction.Rebuild:
                                                actionCatalog = WSCatalog.eAction.Rebuild;
                                                break;
                                            default:
                                                break;
                                        }

                                        isUpdateIndexSucceeded = wsCatalog.UpdateEpgChannelIndex(
                                            arrEPGIds,
                                            nParentGroupID, actionCatalog);

                                        string sInfo = isUpdateIndexSucceeded == true ? "succeeded" : "not succeeded";
                                        log.DebugFormat("Update index {0} in catalog '{1}'", sInfo, sEndPointAddress);

                                        wsCatalog.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.ErrorFormat("Couldn't update catalog '{0}' due to the following error: {1}", sEndPointAddress, ex.Message);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("process failed", ex);
                }
                finally
                {
                    if (wsCatalog != null)
                    {
                        wsCatalog.Close();
                    }
                }
            }

            return isUpdateIndexSucceeded;
        }

        public static DateTime? ExtractDate(string sDate, string format)
        {
            DateTime? result = null;
            DateTime tempDt;
            if (DateTime.TryParseExact(sDate, format, null, System.Globalization.DateTimeStyles.None, out tempDt))
            {
                result = tempDt;
            }
            return result;
        }

        public static bool UpdateFreeFileTypeOfModule(int groupId, int moduleId)
        {
            bool result = false;
            if (moduleId == 0)
            {
                log.Error("Failed updating free item index because couldn't get module Id");
            }
            else
            {
                DataTable mediaIds = DAL.ImporterImpDAL.GetMediasByPPVModuleID(groupId, moduleId, 0);
                while (mediaIds != null && mediaIds.Rows != null)
                {
                    if (mediaIds.Rows.Count == 0)
                    {
                        return true;
                    }

                    List<int> mediaIDsToUpdate = new List<int>();
                    foreach (DataRow dr in mediaIds.Rows)
                    {
                        int mediaIDToAdd = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_ID");
                        if (mediaIDToAdd > 0)
                        {
                            mediaIDsToUpdate.Add(mediaIDToAdd);
                        }
                    }

                    //reset mediaIds
                    mediaIds = null;

                    if (mediaIDsToUpdate != null && mediaIDsToUpdate.Count > 0)
                    {
                        result = UpdateIndex(mediaIDsToUpdate, groupId, eAction.Update);
                        if (result)
                        {
                            int lastMediaID = mediaIDsToUpdate.Last();
                            mediaIds = DAL.ImporterImpDAL.GetMediasByPPVModuleID(groupId, moduleId, lastMediaID);
                        }
                    }
                }
            }

            return result;
        }

        static public Dictionary<string, int> GetAmountOfSubscribersPerAnnouncement(int groupID)
        {
            Dictionary<string, int> response = null;
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                response = service.GetAmountOfSubscribersPerAnnouncement(sWSUserName, sWSPass);
                if (response == null)
                {
                    log.DebugFormat("GetAmountOfSubscribersPerAnnouncement is empty");
                }
                return response;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetAmountOfSubscribersPerAnnouncement ex:{0}", ex);

                return null;
            }
        }

        public static ApiObjects.Response.Status SetTopicSettings(int groupId, int id, eTopicAutomaticIssueNotification topicAutomaticIssueNotification)
        {
            ApiObjects.Response.Status status = null;
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                status = service.UpdateAnnouncement(sWSUserName, sWSPass, id, topicAutomaticIssueNotification);
                if (status != null && status.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    log.ErrorFormat("Failed to SetTopicSettings. GID: {0}, announcementId: {1}, topicAutomaticIssueNotification: {2}", groupId, id, topicAutomaticIssueNotification.ToString());
                    return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to SetTopicSettings. GID: {0}, announcementId: {1}, topicAutomaticIssueNotification: {2}, ex: {3}", groupId, id, topicAutomaticIssueNotification.ToString(), ex);
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return status;
        }

        public static bool DeleteAnnouncement(int groupId, int announcementId)
        {
            bool result = false;

            string logData = string.Format("GID:{0}, AnnouncementId: {1}", groupId, announcementId);
            try
            {
                //Call Notifications WCF service
                string sWSURL = GetConfigVal("NotificationService");
                Notification_WCF.NotificationServiceClient service = new Notification_WCF.NotificationServiceClient();
                if (!string.IsNullOrEmpty(sWSURL))
                    service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "", "notifications", sIP, ref sWSUserName, ref sWSPass);

                if (string.IsNullOrEmpty(sWSUserName))
                {
                    log.ErrorFormat("DeleteAnnouncement failed - sWSUserName is empty. {0}", logData);
                    return false;
                }

                if (string.IsNullOrEmpty(sWSPass))
                {
                    log.ErrorFormat("DeleteAnnouncement failed - sWSPass is empty. {0}", logData);
                    return false;
                }

                if (string.IsNullOrEmpty(sWSURL))
                {
                    log.ErrorFormat("DeleteAnnouncement failed - sWSURL is empty. {0}", logData);
                    return false;
                }

                var response = service.DeleteAnnouncement(sWSUserName, sWSPass, announcementId);
                if (response == null || response.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    log.ErrorFormat("Failed to DeleteAnnouncement. {0}", logData);
                }
                else
                {
                    result = true;
                    log.DebugFormat("Succeeded DeleteAnnouncement. {0}", logData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to DeleteAnnouncement. {0}, ex:{0}", logData, ex);
            }

            return result;
        }
    }
}

