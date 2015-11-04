using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ApiObjects;
using ApiObjects.Epg;
using EpgBL;
using Tvinci.Core.DAL;
using DAL;
using KLogMonitor;
using System.Reflection;

namespace YesEpgFeeder
{
    public class YesEpgFeederObj : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        #region CONST
        private const char spliter = '|';
        private const string LogFileName = "YesEpgFeeder";
        private const string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
        #endregion

        #region members

        public int FeederType { get; set; } // type Feeder = 1 , Notification = 2
        public int GroupID { get; set; }
        public string StartTime { get; set; } // format YYYY-MM-DDThh:mm:ssZ
        public int Duration { get; set; }
        public string Language { get; set; }
        public string ChannelID { get; set; }
        public List<string> lChannelIds { get; set; }

        private string URL { get; set; }
        private int ParentGroupID { get; set; }
        private string Region { get; set; }
        private bool DBOnly { get; set; }
        #endregion

        #region Initialize

        public YesEpgFeederObj(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            lChannelIds = new List<string>();
            InitParamter();
        }

        private void InitParamter()
        {
            try
            {
                //1|154|2014-11-22T00:00:00Z|1440|iw_IL
                string[] item = m_sParameters.Split(spliter);
                FeederType = ODBCWrapper.Utils.GetIntSafeVal(item[0]);
                GroupID = ODBCWrapper.Utils.GetIntSafeVal(item[1]);
                if (item[2].Equals("NOW"))
                {
                    StartTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                else
                {
                    StartTime = item[2];
                }
                Duration = ODBCWrapper.Utils.GetIntSafeVal(item[3]);
                Language = item[4];
                if (item.Count() >= 6)
                {
                    ChannelID = item[5];
                    //if type = notification , can be list of channels ids
                    if (item.Count() >= 7)
                    {
                        if (FeederType == 2 && !string.IsNullOrEmpty(item[6]))
                        {
                            lChannelIds = item[6].Split(';').ToList<string>();
                        }
                    }
                }
                ParentGroupID = DAL.UtilsDal.GetParentGroupID(GroupID);
                URL = TVinciShared.WS_Utils.GetTcmConfigValue("epgURL");
                Region = TVinciShared.WS_Utils.GetTcmConfigValue("regionId");
                DBOnly = TVinciShared.WS_Utils.GetTcmBoolValue("DB_Only");
            }
            catch (Exception ex)
            {
                log.Error("InitParamter - " + string.Format("fail to split the parameters ex={0}", ex.Message), ex);
                throw ex;
            }
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new YesEpgFeederObj(nTaskID, nIntervalInSec, sParameters);
        }

        #endregion


        /***************************************************************************
         *  1. Get the right XML from Yes server by Channel_ID and date + duration 
         *  2. go for each XML and insert the program to all DB (sql, CB, ES)
         **************************************************************************/
        protected override bool DoTheTaskInner()
        {
            try
            {
                if (FeederType == 1) // YesFeeder
                {
                    // get list of all channels 
                    GetChannelIds();

                    foreach (string sChannel in lChannelIds)
                    {
                        URL = TVinciShared.WS_Utils.GetTcmConfigValue("epgURL");
                        ChannelID = sChannel;
                        SaveChannelByXML();
                    }
                    // Update last time invoke parameter only on Feeder Type                    
                    string parameters = string.Format("{0}|{1}|{2}|{3}|{4}", 1, GroupID, "NOW", Duration, Language);

                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("scheduled_tasks");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PARAMETERS", "=", parameters);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nTaskID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else if (FeederType == 2) //Notification
                {
                    if (lChannelIds == null || lChannelIds.Count == 0)
                    {
                        // get list of all channels 
                        GetChannelIds();
                    }

                    foreach (string sChannel in lChannelIds)
                    {
                        URL = TVinciShared.WS_Utils.GetTcmConfigValue("epgURL");
                        ChannelID = sChannel;
                        SaveChannelByXML();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("group:{0}, ex:{1}", GroupID, ex.Message), ex);
            }
            return true;
        }

        private void GetChannelIds()
        {
            DataTable dt = ApiDAL.Get_EPGChannel(ParentGroupID);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                string channal = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    channal = ODBCWrapper.Utils.GetSafeStr(dr, "CHANNEL_ID");
                    lChannelIds.Add(channal);
                }
            }
        }

        private void SaveChannelByXML()
        {
            XmlDocument xmlDoc = getXmlTVChannel();

            // run over and insert the programs
            if (xmlDoc != null)
            {
                bool res = SaveChannel(xmlDoc);
            }
            else
            {
                log.Debug("No xml return from httpRequest - " + string.Format("group:{0}, URL:{1}, ChannelID={2}, StartTime={3}, Duration={4}, Language={5}", GroupID, URL, ChannelID, StartTime, Duration, Language));
            }
        }

        private bool SaveChannel(XmlDocument xmlDoc)
        {
            try
            {
                Dictionary<string, int> dExistChannel = new Dictionary<string, int>();
                string channel_id = "";
                int channelID = 0;

                XmlNodeList xmlnodelist = xmlDoc.GetElementsByTagName("evt");
                XmlNode node;

                List<FieldTypeEntity> FieldEntityMapping = Utils.GetMappingFields(GroupID);
                Dictionary<string, KeyValuePair<string, string>> dParentalRating = GetParentalRating();

                BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(GroupID);
                string update_epg_package = TVinciShared.WS_Utils.GetTcmConfigValue("update_epg_package");
                int nCountPackage = ODBCWrapper.Utils.GetIntSafeVal(update_epg_package);
                int nCount = 0;

                List<ulong> ulProgram = new List<ulong>();
                List<DateTime> deletedDays = new List<DateTime>();
                Dictionary<int, List<DateTime>> channelDeletedDays = new Dictionary<int, List<DateTime>>();

                Dictionary<string, EpgCB> epgDic = new Dictionary<string, EpgCB>();

                foreach (XmlNode evt in xmlnodelist)
                {
                    node = evt;
                    channel_id = TVinciShared.XmlUtils.GetNodeValue(ref node, "cns/cn");
                    // check if channel Exist
                    channelID = IsChannelExist(channel_id, ref dExistChannel);
                    if (channelID == 0)
                    {
                        continue;
                    }

                    #region Basic xml Data

                    string EPGGuid = TVinciShared.XmlUtils.GetNodeValue(ref node, "pid");
                    string scid = TVinciShared.XmlUtils.GetNodeValue(ref node, "scid");

                    #region  basic (name)
                    string name = TVinciShared.XmlUtils.GetNodeValue(ref node, "ts");
                    if (string.IsNullOrEmpty(name))
                    {
                        name = TVinciShared.XmlUtils.GetNodeValue(ref node, "ept");
                    }
                    #endregion
                    string description = TVinciShared.XmlUtils.GetNodeValue(ref node, "dss"); //basic (Description)
                    #endregion

                    // get the pic Url - if there is none , get the default picture from GraceNote API
                    string epg_url = TVinciShared.XmlUtils.GetNodeValue(ref node, "is/i"); // pic url -- should be mapped without prefix

                    #region Set field mapping valus
                    SetMappingValues(FieldEntityMapping, node, dParentalRating);
                    #endregion

                    #region Delete Programs by channel + date

                    string start_date = TVinciShared.XmlUtils.GetNodeValue(ref node, "sdt");
                    string end_date = TVinciShared.XmlUtils.GetNodeValue(ref node, "edt");

                    DateTime dDate = Utils.ParseEPGStrToDate(start_date, "000000");// get all day start from 00:00:00
                    if (!channelDeletedDays.ContainsKey(channelID) || (channelDeletedDays.ContainsKey(channelID) && !channelDeletedDays[channelID].Contains(dDate)))
                    {
                        if (!channelDeletedDays.ContainsKey(channelID))
                        {
                            channelDeletedDays.Add(channelID, new List<DateTime>() { dDate });
                        }
                        else
                        {
                            channelDeletedDays[channelID].Add(dDate);
                        }
                        Utils.DeleteProgramsByChannelAndDate(channelID, dDate, ParentGroupID);
                    }

                    #endregion

                    #region Generate EPG CB

                    DateTime dProgStartDate = DateTime.ParseExact(start_date, format, null);
                    DateTime dProgEndDate = DateTime.ParseExact(end_date, format, null);

                    EpgCB newEpgItem = Utils.generateEPGCB(epg_url, description, name, channelID, scid /*EPGGuid*/, dProgStartDate, dProgEndDate, node, GroupID, ParentGroupID, FieldEntityMapping);

                    #endregion
                    if (!epgDic.ContainsKey(scid))
                    {
                        epgDic.Add(scid, newEpgItem);
                    }
                }

                //insert EPGs to DB in batches
                InsertEpgsDBBatches(ref epgDic, GroupID, nCountPackage, FieldEntityMapping);

                if (!DBOnly)
                {
                    foreach (EpgCB epg in epgDic.Values)
                    {
                        nCount++;

                        #region Insert EpgProgram to CB
                        ulong epgID = 0;
                        bool bInsert = oEpgBL.InsertEpg(epg, out epgID);
                        #endregion

                        #region Insert EpgProgram ES

                        if (nCount >= nCountPackage)
                        {
                            ulProgram.Add(epg.EpgID);
                            bool resultEpgIndex = Utils.UpdateEpgIndex(ulProgram, ParentGroupID, ApiObjects.eAction.Update);
                            ulProgram = new List<ulong>();
                            nCount = 0;
                        }
                        else
                        {
                            ulProgram.Add(epg.EpgID);
                        }

                        #endregion
                    }

                    if (nCount > 0 && ulProgram != null && ulProgram.Count > 0)
                    {
                        bool resultEpgIndex = Utils.UpdateEpgIndex(ulProgram, ParentGroupID, ApiObjects.eAction.Update);
                    }
                }
                //start Upload proccess Queue
                UploadQueue.UploadQueueHelper.SetJobsForUpload(GroupID);

                //write to log all the non exists channels 
                foreach (KeyValuePair<string, int> item in dExistChannel)
                {
                    if (item.Value == 0)
                    {
                        log.Debug("SaveChannel - " + string.Format("ChannelID = {0} , do nothing", item.Key));
                    }
                }

                return true;
            }
            catch (Exception exp)
            {
                log.Error("SaveChannel - " + string.Format("failed group:{0},  URL:{1} ,ChannelID={2},StartTime={3}, Duration={4}, Language={5}, ex:{6}",
                                                                    GroupID, URL, ChannelID, StartTime, Duration, Language, exp.Message), exp);
                return false;
            }
            finally
            {
            }
        }

        private int IsChannelExist(string channel_id, ref Dictionary<string, int> dExistChannel)
        {
            try
            {

                if (dExistChannel.ContainsKey(channel_id))
                {
                    return dExistChannel[channel_id];
                }

                int channelID = GetExistChannel(channel_id);

                if (!dExistChannel.ContainsKey(channel_id))
                {
                    dExistChannel.Add(channel_id, channelID);
                }
                else
                {
                    dExistChannel[channel_id] = channelID;
                }

                return channelID;
            }
            catch
            {
                return 0;
            }
        }

        private Dictionary<string, KeyValuePair<string, string>> GetParentalRating()
        {
            Dictionary<string, KeyValuePair<string, string>> dict = new Dictionary<string, KeyValuePair<string, string>>();
            try
            {
                DataTable dt = EpgDal.Get_parental_rating();
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string parental_rating = ODBCWrapper.Utils.GetSafeStr(dr, "parental_rating");
                        string parental_dvb = ODBCWrapper.Utils.GetSafeStr(dr, "parental_dvb"); //parental
                        string desc = ODBCWrapper.Utils.GetSafeStr(dr, "desc"); // rating

                        KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(parental_dvb, desc);
                        if (dict.ContainsKey(parental_rating))
                        {
                            dict[parental_rating] = (kvp);
                        }
                        else
                        {
                            dict.Add(parental_rating, kvp);
                        }

                    }
                }
                return dict;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return null;
            }
        }

        private void InsertEpgsDBBatches(ref Dictionary<string, EpgCB> epgDic, int GroupID, int nCountPackage, List<FieldTypeEntity> FieldEntityMapping)
        {
            Dictionary<string, EpgCB> epgBatch = new Dictionary<string, EpgCB>();
            Dictionary<int, List<string>> tagsAndValues = new Dictionary<int, List<string>>();
            int nEpgCount = 0;
            try
            {
                foreach (string sGuid in epgDic.Keys)
                {
                    epgBatch.Add(sGuid, epgDic[sGuid]);
                    nEpgCount++;

                    //generate a Dictionary of all tag and values in the epg
                    Utils.GenerateTagsAndValues(epgDic[sGuid], FieldEntityMapping, ref tagsAndValues);

                    if (nEpgCount >= nCountPackage)
                    {
                        InsertEpgs(GroupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
                        nEpgCount = 0;
                        foreach (string guid in epgBatch.Keys)
                        {
                            if (epgBatch[guid].EpgID > 0)
                            {
                                epgDic[guid].EpgID = epgBatch[guid].EpgID;
                            }
                        }
                        epgBatch.Clear();
                        tagsAndValues.Clear();
                    }
                }

                if (nEpgCount > 0 && epgBatch.Keys.Count() > 0)
                {
                    InsertEpgs(GroupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
                    foreach (string guid in epgBatch.Keys)
                    {
                        if (epgBatch[guid].EpgID > 0)
                        {
                            epgDic[guid].EpgID = epgBatch[guid].EpgID;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                log.Error("InsertEpgsDBBatches - " + string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", GroupID, exc.Message), exc);
                return;
            }
        }

        private void InsertEpgs(int GroupID, ref Dictionary<string, EpgCB> epgDic, List<FieldTypeEntity> FieldEntityMapping, Dictionary<int, List<string>> tagsAndValues)
        {
            try
            {
                DataTable dtEpgMetas = Utils.InitEPGProgramMetaDataTable();
                DataTable dtEpgTags = Utils.InitEPGProgramTagsDataTable();
                DataTable dtEpgTagsValues = Utils.InitEPG_Tags_Values();

                int nUpdaterID = 0;
                if (nUpdaterID == 0)
                    nUpdaterID = 700;
                string sConn = "MAIN_CONNECTION_STRING";

                List<FieldTypeEntity> FieldEntityMappingMetas = FieldEntityMapping.Where(x => x.FieldType == FieldTypes.Meta).ToList();
                List<FieldTypeEntity> FieldEntityMappingTags = FieldEntityMapping.Where(x => x.FieldType == FieldTypes.Tag).ToList();

                Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs = new Dictionary<KeyValuePair<string, int>, List<string>>();// new tag values and the EPGs that have them
                //return relevant tag value ID, if they exist in the DB
                Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue = Utils.getTagTypeWithRelevantValues(GroupID, FieldEntityMappingTags, tagsAndValues);

                // insert all epg to DB (epg_channels_schedule)
                InsertEPG_Channels_sched(ref epgDic);

                // Tags and Mates
                foreach (EpgCB epg in epgDic.Values)
                {
                    if (epg != null)
                    {
                        //update Metas
                        Utils.UpdateMetasPerEPG(ref dtEpgMetas, epg, FieldEntityMappingMetas, nUpdaterID);
                        //update Tags                    
                        Utils.UpdateExistingTagValuesPerEPG(epg, FieldEntityMappingTags, ref dtEpgTags, ref dtEpgTagsValues, TagTypeIdWithValue, ref newTagValueEpgs, nUpdaterID);
                    }
                }

                Utils.InsertNewTagValues(epgDic, dtEpgTagsValues, ref dtEpgTags, newTagValueEpgs, GroupID, nUpdaterID);

                Utils.InsertBulk(dtEpgMetas, "EPG_program_metas", sConn); //insert EPG Metas to DB
                Utils.InsertBulk(dtEpgTags, "EPG_program_tags", sConn); //insert EPG Tags to DB
            }
            catch (Exception exc)
            {
                log.Error("InsertEpgs - " + string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", GroupID, exc.Message), exc);
                return;
            }
        }

        private void InsertEPG_Channels_sched(ref Dictionary<string, EpgCB> epgDic)
        {
            EpgCB epg;
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

            DataTable dtEPG = Utils.InitEPGDataTable();
            Utils.FillEPGDataTable(epgDic, ref dtEPG);
            string sConn = "MAIN_CONNECTION_STRING";
            Utils.InsertBulk(dtEPG, "epg_channels_schedule", sConn); //insert EPGs to DB

            //get back the IDs list of the EPGs          
            DataTable dtEpgIDGUID = EpgDal.Get_EpgIDbyEPGIdentifier(epgDic.Keys.ToList());
            if (dtEpgIDGUID != null && dtEpgIDGUID.Rows != null)
            {
                for (int i = 0; i < dtEpgIDGUID.Rows.Count; i++)
                {
                    DataRow row = dtEpgIDGUID.Rows[i];
                    if (row != null)
                    {
                        string sGuid = ODBCWrapper.Utils.GetSafeStr(row, "EPG_IDENTIFIER");
                        ulong nEPG_ID = (ulong)ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                        if (epgDic.TryGetValue(sGuid, out epg) && epg != null)
                            epgDic[sGuid].EpgID = nEPG_ID;  //update the EPGCB with the ID
                    }
                }
            }
        }



        private XmlDocument getXmlTVChannel()
        {
            XmlDocument xmlDoc;
            try
            {
                /*
                                 sXml = string.Empty;

                                StreamReader streamReader = new StreamReader("c:\\yesText.txt");
                                sXml = streamReader.ReadToEnd();
                                streamReader.Close();
                                */


                GetYestUrl();

                log.Debug("getXmlTVChannel - " + string.Format("GetYestUrl:{0}", URL));

                string sXml = TVinciShared.WS_Utils.SendXMLHttpReq(URL, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "GET");

                //    sXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><xml><request totalFound=\"237\" countReturned=\"237\" countRequested=\"2000\" startRequested=\"0\" /><schedules regionId=\"Israel\"><evt><ept>איך פגשתי את אמא 8 - פרק 10</ept><dss>מערכת היחסים המתפתחת בין בארני לפטריס מותירה את רובין עם שאלותלגבי הסיבה האמיתית לקיומה של אותה מערכת יחסים. בינתיים, אימו של מארשל חוזרת לצאת עם גברים, אלא שמארשל אינו מרוצה מהגבר איתו היא החליטה לצאת.</dss><scid>786534</scid><et>0</et><sdt>2014-08-17T02:10:00.000Z</sdt><edt>2014-08-17T02:35:00.000Z</edt><d>25</d><flags /><pid>program-389771-498198</pid><peid>program-389771-498198</peid><rtv>R14</rtv><epn>10</epn><gs><g>Comedy</g><g>Series</g><g>General Entertainment</g><g>Entertainment</g></gs><seid>YESP</seid><cns><cn>15</cn></cns></evt><evt><ept>המטורפים - רובין וויליאמס</ept><dss><![CDATA[רובין וויליאמס חוזר לטלוויזיה לראשונה מאז ימי \"מורקומינדי\" ומככב ביחד עם שרה מישל גלר (\"באפי ציידת הערפדים\") בקומדיה הנצפית ביותר בארה\"ב. סיימון הוא בעליו האקסצנטרי של משרד פרסום ולצידו- השותפה האחראית שלו, בתוסידני.]]></dss><scid>786540</scid><et>0</et><sdt>2014-08-17T02:35:00.000Z</sdt><edt>2014-08-17T03:00:00.000Z</edt><d>25</d><flags /><pid>program-467546-584111</pid><peid>program-467546-584111</peid><rtv>R14</rtv><epn>1</epn><gs><g>General Entertainment</g><g>Comedy</g><g>Series</g></gs><seid>YESP</seid><cns><cn>15</cn></cns></evt></schedules></xml>";

                xmlDoc = new XmlDocument();
                Encoding encoding = Encoding.UTF8;

                // Encode the XML string in a byte array
                byte[] encodedString = encoding.GetBytes(sXml);

                // Put the byte array into a stream and rewind it to the beginning
                using (var ms = new MemoryStream(encodedString))
                {
                    ms.Flush();
                    ms.Position = 0;

                    // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                    xmlDoc.Load(ms);
                }

                return xmlDoc;
            }
            catch (Exception ex)
            {
                log.Error("getXmlTVChannel - " + string.Format("Exception:{0}", ex.Message), ex);
                return null;
            }
        }


        private void GetYestUrl()
        {
            URL = string.Format("{0}/schedules?", URL);
            URL = string.Format("{0}regionId={1}", URL, Region);
            URL = string.Format("{0}&startTime={1}", URL, StartTime);
            if (!string.IsNullOrEmpty(ChannelID))
            {
                URL = string.Format("{0}&filters=cn:equals:{1}", URL, ChannelID);
            }
            URL = string.Format("{0}&locale={1}", URL, Language);
            if (Duration > 0)
            {
                URL = string.Format("{0}&duration={1}", URL, Duration.ToString());
            }
        }

        private int GetExistMedia(int EPG_IDENTIFIER)
        {
            Int32 res = 0;
            try
            {
                res = EpgDal.GetExistMedia(GroupID, EPG_IDENTIFIER);
            }
            catch (Exception exp)
            {
                log.Error(string.Empty, exp);
            }
            return res;
        }

        private int GetExistChannel(string sChannelID)
        {
            Int32 res = 0;
            if (!string.IsNullOrEmpty(sChannelID))
            {
                try
                {
                    res = EpgDal.GetChannelByChannelID(GroupID, sChannelID);
                }
                catch (Exception exp)
                {
                    log.Error("GetExistChannel - " + string.Format("could not get Get Exist Channel  by ID {0}, error message: {1}", sChannelID, exp.Message), exp);
                }
            }
            return res;
        }


        // fille the FieldEntityMapping with all values
        private void SetMappingValues(List<FieldTypeEntity> FieldEntityMapping, XmlNode node, Dictionary<string, KeyValuePair<string, string>> dParentalRating)
        {
            // fill all tags 
            GetTagsValues(node, ref FieldEntityMapping, dParentalRating);
            //fill Metas Values
            GetMetasValues(node, ref FieldEntityMapping);
        }

        // insert values to all mates
        private static void GetMetasValues(XmlNode node, ref List<FieldTypeEntity> FieldEntityMapping)
        {
            try
            {
                for (int i = 0; i < FieldEntityMapping.Count; i++)
                {
                    if (FieldEntityMapping[i].FieldType == FieldTypes.Meta)
                    {
                        FieldEntityMapping[i].Value = new List<string>();
                        foreach (string XmlRefName in FieldEntityMapping[i].XmlReffName)
                        {
                            foreach (XmlNode multinode in node.SelectNodes(XmlRefName))
                            {
                                if (!string.IsNullOrEmpty(multinode.InnerText))
                                {
                                    FieldEntityMapping[i].Value.Add(multinode.InnerXml);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetMetasValues - " + string.Format("failed with metaValues for node = {0}", ex.Message), ex);
            }
        }

        // insert values to all tags (contributed nodes or regular tags)
        private void GetTagsValues(XmlNode node, ref List<FieldTypeEntity> FieldEntityMapping, Dictionary<string, KeyValuePair<string, string>> dParentalRating)
        {
            try
            {
                for (int i = 0; i < FieldEntityMapping.Count; i++)
                {
                    if (FieldEntityMapping[i].FieldType == FieldTypes.Tag)
                    {
                        FieldEntityMapping[i].Value = new List<string>();
                        foreach (string XmlRefName in FieldEntityMapping[i].XmlReffName)
                        {

                            foreach (XmlNode multinode in node.SelectNodes(XmlRefName))
                            {
                                if (XmlRefName.ToLower() == "flags")
                                {
                                    XmlNode flags = multinode;
                                    string flag = multinode.InnerXml; //TVinciShared.XmlUtils.GetNodeValue(ref flags, "flags");
                                    if (flag.Equals("fls"))
                                    {
                                        FieldEntityMapping[i].Value.Add("True");
                                    }
                                }

                                else if (XmlRefName.ToLower() == "rtv")
                                {
                                    if (FieldEntityMapping[i].Name.ToLower() == "rating")
                                    {
                                        if (dParentalRating.ContainsKey(multinode.InnerXml))
                                        {
                                            FieldEntityMapping[i].Value.Add(dParentalRating[multinode.InnerXml].Value);
                                        }
                                    }
                                    if (FieldEntityMapping[i].Name.ToLower() == "parental")
                                    {
                                        if (dParentalRating.ContainsKey(multinode.InnerXml))
                                        {
                                            FieldEntityMapping[i].Value.Add(dParentalRating[multinode.InnerXml].Key);
                                        }
                                    }
                                }

                                else if (!string.IsNullOrEmpty(multinode.InnerText))
                                {
                                    FieldEntityMapping[i].Value.Add(multinode.InnerXml);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetTagsValues - " + string.Format("failed with tagValues for node = {0}", ex.Message), ex);
            }
        }
    }
}
