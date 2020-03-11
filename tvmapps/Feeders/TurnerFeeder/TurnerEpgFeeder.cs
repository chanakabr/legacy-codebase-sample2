using ApiObjects;
using ConfigurationManager;
using EpgBL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Script.Serialization;
using TurnerEpgFeeder;

namespace TurnerFeeder
{
    public class TurnerTasker : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // m_uniqueKey unique connection key, m_url address of the get request and m_lastDate the invoke interval 
        private string m_fromDate = string.Empty;
        private string m_toDate = string.Empty;
        private string m_baseUrl = string.Empty;
        //private string  m_uniqueKey = string.Empty;
        private int m_groupID = 0;


        public TurnerTasker(int nTaskID, int nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            string[] seperator = { "|" };
            string[] splited = sParameters.Split(seperator, StringSplitOptions.None);

            if (splited.Length == 3)
            {
                m_groupID = int.Parse(splited[0]);
                m_fromDate = splited[1];
                m_toDate = splited[2];
                //m_url = splited[3];
                //m_uniqueKey = splited[4];
            }

            m_baseUrl = TVinciShared.WS_Utils.GetTcmConfigValue("UrlTurnerEpg");  // "http://schedule.saoweb.io/api/tvinci/{channel_name}/schedule?from={from_date}&to={to_date}"

            if (!string.IsNullOrEmpty(m_baseUrl))
            {
                m_baseUrl = m_baseUrl.Replace("{from_date}", m_fromDate).Replace("{to_date}", m_toDate);
            }
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new TurnerTasker(nTaskID, nIntervalInSec, sParameters);
        }


        private string GetTurnerEpgString(string sChannelName)
        {
            string ret = string.Empty;

            string sFullURL = m_baseUrl.Replace("{channel_name}", sChannelName);    //string.Format("{0}?from=\"{1}\"&to=\"{2}\"", m_url, m_fromDate, m_toDate);

            string epgJsonRes = TVinciShared.WS_Utils.SendXMLHttpReq(sFullURL, "", "", "application/json", "", "", "", "", "get");
            log.Debug("GetTurnerEpgString - " + string.Format("{0}", epgJsonRes));

            if (string.IsNullOrEmpty(epgJsonRes))
            {
                return string.Empty;
            }

            return epgJsonRes;
        }

        protected override bool DoTheTaskInner()
        {
            log.Debug("Start task - " + string.Format("Group:{0}, FromDate:{1}, ToDate:{2}", m_groupID, m_fromDate, m_toDate));

            // Get Dml xml using http post request
            DateTime d = DateTime.UtcNow;

            try
            {
                Dictionary<int, string> dEpgChannels = Utils.GetGroupEpgChannels(m_groupID);

                if (dEpgChannels == null || dEpgChannels.Count == 0)
                {
                    log.Debug("Ingest fail - No EPG channels found");
                    return false;
                }

                foreach (KeyValuePair<int, string> chIdName in dEpgChannels)
                {
                    int chID = chIdName.Key;
                    string chName = chIdName.Value;

                    string epgJsonArray = GetTurnerEpgString(chName);

                    if (string.IsNullOrEmpty(epgJsonArray))
                    {
                        log.Debug("Ingest warning - " + string.Format("No EPG data for channel {0}", chName));
                        continue;
                    }

                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    List<TurnerEpgObj> programs = ser.Deserialize<List<TurnerEpgObj>>(epgJsonArray);

                    if (programs == null || programs.Count == 0)
                    {
                        log.Debug("Ingest warning - " + string.Format("No EPG data for channel {0}", chName));
                        continue;
                    }

                    bool saved = SaveChannel(chID, programs);
                }

            }
            catch (Exception ex)
            {
                log.Error("Ingest fail - " + ex.ToString(), ex);
            }


            d = d.AddDays(1.0);
            DateTime dToDate = d.AddDays(7.0);
            string parameters = string.Format("{0}|{1}|{2}", m_groupID, d.ToString("yyyy-MM-dd"), dToDate.ToString("yyyy-MM-dd"));

            //bool parseTo = Utils.ParseEPGStrToDate(m_toDate, ref dToDate);

            //if (parseTo) // if parse OK 
            //{
            //    dToDate = dToDate.AddDays(1.0);
            //}

            //string parameters = string.Format("{0}||{1}||{2}", m_groupID, d.ToString("yyyy-MM-ddTHH:mm:ss"), dToDate.ToString("yyyy-MM-ddTHH:mm:ss"));

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("scheduled_tasks");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PARAMETERS", "=", parameters);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nTaskID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            log.Debug("Ending task, - " + d.ToString("yyyy-MM-ddTHH:mm:ss"));
            return true;
        }


        private bool SaveChannel(int channelID, List<TurnerEpgObj> programs)
        {
            try
            {
                if (channelID > 0)
                {
                    log.Debug("Turner SaveChannels - " + string.Format("START EPG Channel = {0}", channelID));

                    List<FieldTypeEntity> FieldEntityMapping = Utils.GetMappingFields(m_groupID);

                    int nParentGroupID = DAL.UtilsDal.GetParentGroupID(m_groupID);

                    //#region get Group ID (parent Group if possible)

                    //if (parentGroupID == 0)
                    //{
                    //    parentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                    //}

                    //#endregion


                    BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(m_groupID);                    
                    int nCountPackage = ApplicationConfiguration.CatalogLogicConfiguration.UpdateEPGPackage.IntValue;
                    int nCount = 0;

                    List<ulong> ulProgram = new List<ulong>();
                    List<DateTime> deletedDays = new List<DateTime>();
                    Dictionary<string, EpgCB> epgDic = new Dictionary<string, EpgCB>();


                    for (int i = 0; i < programs.Count; i++)
                    {

                        #region Generate EPG CB

                        DateTime dProgStartDate = DateTime.MinValue;
                        DateTime dProgEndDate = DateTime.MaxValue;
                        bool parseStart = Utils.ParseEPGStrToDate(programs[i].begin, ref dProgStartDate);
                        bool parseEnd = Utils.ParseEPGStrToDate(programs[i].end, ref dProgEndDate);

                        if (parseStart && parseEnd) // if both dates exsits and parse OK 
                        {
                            #region Delete Programs by channel + date

                            DateTime dDate = new DateTime(dProgStartDate.Year, dProgStartDate.Month, dProgStartDate.Day);

                            if (!deletedDays.Contains(dDate))
                            {
                                deletedDays.Add(dDate);
                                Utils.DeleteProgramsByChannelAndDate(channelID, dDate, nParentGroupID);
                            }

                            #endregion

                            //need this for both DB and CB!!!!!!!!!
                            SetMappingValues(FieldEntityMapping, programs[i]);

                            EpgCBTurner newEpgItem = Utils.generateEPGCB(programs[i].image,
                                                                    programs[i].storyline,
                                                                    programs[i].title,
                                                                    programs[i].subtitle,
                                                                    channelID,
                                                                    programs[i].id.ToString(),
                                                                    dProgStartDate,
                                                                    dProgEndDate,
                                                                    null,
                                                                    m_groupID,
                                                                    nParentGroupID,
                                                                    FieldEntityMapping);

                            epgDic.Add(newEpgItem.EpgIdentifier, newEpgItem);
                        }
                        else
                        {
                            log.Error("Dates Error - " + string.Format("channelID={0}, turnerID={1}, startDate={2}, endDate={3}, TvinciChannelID={4}",
                                    channelID, programs[i].id, dProgStartDate, dProgEndDate, channelID));
                        }

                        #endregion


                    }


                    //foreach (XmlNode node in xmlnodelist)
                    //{
                    //    #region Basic xml Data

                    //    string EPGGuid = GetSingleNodeValue(node, "GN_ID");
                    //    string name = GetSingleNodeValue(node, "TITLE"); // basic (name)
                    //    string description = GetSingleNodeValue(node, "SYNOPSIS"); //basic (Description)

                    //    #endregion

                    //    // get the pic Url - if there is none , get the default picture from GraceNote API
                    //    string epg_url = GetSingleNodeValue(node, "URLGROUP/URL"); // pic url

                    //    if (string.IsNullOrEmpty(epg_url))
                    //    {
                    //        string category =
                    //            GetSingleAttributeValue(node.SelectSingleNode("IPGCATEGORY/IPGCATEGORY_L2"), "ID");

                    //        if (!dCategoryToDefaultPic.TryGetValue(category, out epg_url) ||
                    //            string.IsNullOrEmpty(epg_url))
                    //        {
                    //            epg_url = GetImageFromGN(node, EPGGuid);

                    //            dCategoryToDefaultPic[category] = epg_url;
                    //        }
                    //    }

                    //    #region Set field mapping valus

                    //    SetMappingValues(FieldEntityMapping, node);

                    //    #endregion

                    //    XmlNode tvAirNode =
                    //        xmlDoc.DocumentElement.SelectSingleNode("//TVAIRING[@GN_ID='" + EPGGuid + "']");
                    //        // get node by attribute value
                    //    string start_date = GetSingleAttributeValue(tvAirNode, "START");
                    //    string end_date = GetSingleAttributeValue(tvAirNode, "END");

                    //}

                    //insert EPGs to DB in batches
                    InsertEpgsDBBatches(ref epgDic, m_groupID, nCountPackage, FieldEntityMapping);


                    foreach (EpgCB epg in epgDic.Values)
                    {
                        nCount++;

                        #region Insert EpgProgram to CB + ES

                        ulong epgID = 0;
                        if (epg != null && epg.EpgID > 0)
                        {
                            bool bInsert = oEpgBL.InsertEpg(epg, out epgID);
                            ulProgram.Add(epg.EpgID);

                            if (nCount >= nCountPackage)
                            {
                                bool resultEpgIndex = Utils.UpdateEpgIndex(ulProgram, nParentGroupID, ApiObjects.eAction.Update);
                                ulProgram = new List<ulong>();
                                nCount = 0;
                            }

                        }

                        #endregion
                    }

                    if (nCount > 0 && ulProgram != null && ulProgram.Count > 0)
                    {
                        bool resultEpgIndex = Utils.UpdateEpgIndex(ulProgram, nParentGroupID, ApiObjects.eAction.Update);
                    }

                    //start Upload proccess Queue
                    UploadQueue.UploadQueueHelper.SetJobsForUpload(m_groupID);

                    log.Debug("Turner - " + string.Format("END EPG Channel {0}", channelID));
                }
                else
                {
                    log.Debug("Missing Channel ID - " + string.Format("GetExistChannel() ChannelID = {0}", channelID));
                    return false;
                }
            }
            catch (Exception exp)
            {
                log.Error("Turner - " + string.Format("fail to insert channel to DB ex = {0}", exp.Message), exp);
                return false;
            }
            finally
            {
            }

            return true;
        }

        private void SetMappingValues(List<FieldTypeEntity> FieldEntityMapping, TurnerEpgObj prog)
        {
            if (prog == null)
            {
                return;
            }

            for (int i = 0; i < FieldEntityMapping.Count; i++)
            {
                FieldEntityMapping[i].Value = new List<string>();

                foreach (string fieldName in FieldEntityMapping[i].XmlReffName)
                {
                    switch (fieldName.ToLower())
                    {
                        case "subtitle":
                            {
                                if (prog.subtitle != null)
                                {
                                    FieldEntityMapping[i].Value.Add(prog.subtitle);
                                }
                                break;
                            }

                        case "country":
                            {
                                if (prog.country != null)
                                {
                                    FieldEntityMapping[i].Value.Add(prog.country);
                                }
                                break;
                            }

                        case "year":
                            {
                                if (prog.year != null)
                                {
                                    FieldEntityMapping[i].Value.Add(prog.year);
                                }
                                break;
                            }

                        case "rate":
                            {
                                if (prog.rate != null)
                                {
                                    FieldEntityMapping[i].Value.Add(prog.rate);
                                }
                                break;
                            }

                        case "genre":
                            {
                                if (prog.genre != null && prog.genre.Length > 0)
                                {
                                    for (int j = 0; j < prog.genre.Length; j++)
                                    {
                                        FieldEntityMapping[i].Value.Add(prog.genre[j]);
                                    }
                                }
                                break;
                            }

                        default:
                            break;
                    }
                }
            }

        }

        private void InsertEpgsDBBatches(ref Dictionary<string, EpgCB> epgDic, int groupID, int nCountPackage, List<FieldTypeEntity> FieldEntityMapping)
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
                        Utils.InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
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

                if (nEpgCount > 0 && epgBatch.Keys.Count > 0)
                {
                    Utils.InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
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
                log.Error("InsertEpgsDBBatches - " + string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", groupID, exc.Message), exc);
                return;
            }
        }

    }

}
