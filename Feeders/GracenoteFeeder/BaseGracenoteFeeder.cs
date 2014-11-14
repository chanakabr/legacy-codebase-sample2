using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using ApiObjects;
using EnumProject;
using EpgBL;
using Tvinci.Core.DAL;

namespace GracenoteFeeder
{
    public class BaseGracenoteFeeder 
    {
        #region members
        
        public int groupID { get; set; }
        public int parentGroupID { get; set; }
        public int UpdaterID { get; set; }
        public string client { get; set; }
        public string user { get; set; }
        public string URL { get; set; }
        public string ChannelXml { get; set; }
        public string CategoryXml { get; set; }

        public static Dictionary<string, string> dCategoryToDefaultPic = new Dictionary<string, string>();

        #endregion


        public BaseGracenoteFeeder(string sClient, string sUser, int nGroupID, string sURL, string sChannelXml, string sCategoryXml, int nParentGroupID = 0, int nUpdaterID = 0)
        {
            groupID = nGroupID;
            parentGroupID = nParentGroupID;
            UpdaterID = nUpdaterID;
            client = sClient;
            user = sUser;
            URL = sURL;
            ChannelXml = sChannelXml;
            CategoryXml = sCategoryXml;
        }

        private string GetSingleAttributeValue(XmlNode node, string xpath)
        {
            string res = "";
            try
            {
                res = node.Attributes[xpath].Value;
            }
            catch (Exception exp)
            {
                string errorMessage = string.Format("could not get the node '{0}' innerText value, error:{1}", xpath, exp.Message);

            }
            return res;
        }

        private string GetSingleNodeValue(XmlNode node, string xpath)
        {
            string res = "";
            try
            {
                if (node.SelectSingleNode(xpath) != null)
                {
                    res = node.SelectSingleNode(xpath).InnerText;
                }
            }
            catch (Exception exp)
            {
                string errorMessage = string.Format("could not get the node '{0}' innerText value, error:{1}", xpath, exp.Message);

            }
            return res;
        }

        private XmlNodeList GetNodesValue(XmlNode node, string xpath)
        {
            XmlNodeList res = null;
            try
            {
                res = node.SelectNodes(xpath);
            }
            catch (Exception exp)
            {
                string errorMessage = string.Format("could not get the node '{0}' innerText value, error:{1}", xpath, exp.Message);

            }
            return res;
        }

        private Dictionary<string, List<string>> GetTagsValue(XmlNode node)
        {
            XmlNodeList nodeList = GetNodesValue(node, "CONTRIBUTOR"); // get all tags in the node CONTRIBUTOR = GetNodesValue(node, "CONTRIBUTOR"); // get all tags in the node CONTRIBUTOR

            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            try
            {
                foreach (XmlNode item in nodeList)
                {
                    string tagType = GetSingleNodeValue(item, "CONTRIBUTION/CONTRIBUTION_TYPE");
                    if (!string.IsNullOrEmpty(tagType))
                    {
                        string value = GetSingleNodeValue(item, "NAME");
                        if (res.ContainsKey(tagType.ToUpper()))
                            res[tagType.ToUpper()].Add(value);
                        else
                            res.Add(tagType.ToUpper(), new List<string>() { value });

                    }
                }
            }
            catch (Exception exp)
            {
            }
            return res;
        }

        /* Go over this xmldocumnet that include all programs for specific channel id 
         * add new programs to DB , CB &  ES(via Rabbit)
         add new tags and metas if needed to DB
         * insert picture to queue (via Rabbit)
         */
        public virtual void SaveChannel(XmlDocument xmlDoc)
        {
            try
            {
                string channel_id = "";
                int channelID = 0;

                XmlNodeList xmlChannel = xmlDoc.GetElementsByTagName("TVGRIDBATCH");
                XmlNodeList xmlnodelist = xmlDoc.GetElementsByTagName("TVPROGRAM");

                if (xmlnodelist.Count > 0)
                {
                    channel_id = GetSingleNodeValue(xmlChannel[0], "GN_ID");
                    channelID = GetExistChannel(channel_id);
                }

                if (channelID > 0)
                {
                    Logger.Logger.Log("KDG SaveChannels", string.Format("START EPG Channel = {0}", channelID), "GracenoteFeeder");

                    List<FieldTypeEntity> FieldEntityMapping = Utils.GetMappingFields(groupID);

                    #region get Group ID (parent Group if possible)

                    if (parentGroupID == 0)
                    {
                        parentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);
                    }
                    #endregion

                    BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(groupID);
                    string update_epg_package = TVinciShared.WS_Utils.GetTcmConfigValue("update_epg_package");
                    int nCountPackage = ODBCWrapper.Utils.GetIntSafeVal(update_epg_package);
                    int nCount = 0;
                    List<ulong> ulProgram = new List<ulong>();
                    List<DateTime> deletedDays = new List<DateTime>();
                    Dictionary<string, EpgCB> epgDic = new Dictionary<string, EpgCB>();

                    foreach (XmlNode node in xmlnodelist)
                    {
                        #region Basic xml Data

                        string EPGGuid = GetSingleNodeValue(node, "GN_ID");
                        string name = GetSingleNodeValue(node, "TITLE"); // basic (name)
                        string description = GetSingleNodeValue(node, "SYNOPSIS"); //basic (Description)
                        #endregion

                        // get the pic Url - if there is none , get the default picture from GraceNote API
                        string epg_url = GetSingleNodeValue(node, "URLGROUP/URL"); // pic url

                        if (string.IsNullOrEmpty(epg_url))
                        {
                            string category = GetSingleAttributeValue(node.SelectSingleNode("IPGCATEGORY/IPGCATEGORY_L2"), "ID");

                            if (!dCategoryToDefaultPic.TryGetValue(category, out epg_url) || string.IsNullOrEmpty(epg_url))
                            {
                                epg_url = GetImageFromGN(node, EPGGuid);

                                dCategoryToDefaultPic[category] = epg_url;
                            }
                        }

                        #region Set field mapping valus
                        SetMappingValues(FieldEntityMapping, node);
                        #endregion

                        XmlNode tvAirNode = xmlDoc.DocumentElement.SelectSingleNode("//TVAIRING[@GN_ID='" + EPGGuid + "']"); // get node by attribute value
                        string start_date = GetSingleAttributeValue(tvAirNode, "START");
                        string end_date = GetSingleAttributeValue(tvAirNode, "END");

                        #region Generate EPG CB

                        DateTime dProgStartDate = DateTime.MinValue;
                        DateTime dProgEndDate = DateTime.MaxValue;
                        bool parseStart = Utils.ParseEPGStrToDate(start_date, ref dProgStartDate);
                        bool parseEnd = Utils.ParseEPGStrToDate(end_date, ref dProgEndDate);
                        if (parseStart && parseEnd) // if both dates exsits and parse OK 
                        {
                            #region Delete Programs by channel + date
                            DateTime dDate = new DateTime(dProgStartDate.Year, dProgStartDate.Month, dProgStartDate.Day);
                            if (!deletedDays.Contains(dDate))
                            {
                                deletedDays.Add(dDate);
                                Utils.DeleteProgramsByChannelAndDate(channelID, dDate, parentGroupID);
                            }
                            #endregion

                            EpgCB newEpgItem = Utils.generateEPGCB(epg_url, description, name, channelID, EPGGuid, dProgStartDate, dProgEndDate, node, groupID, parentGroupID, FieldEntityMapping);
                            epgDic.Add(newEpgItem.EpgIdentifier, newEpgItem);
                        }
                        else
                        {
                            Logger.Logger.Log("Dates Error", string.Format("channel_id={0}, GN_ID={1}, startDate={2}, endDate={3}, TvinciChannelID={4}",
                                channel_id, EPGGuid, start_date, end_date, channelID), "GraceNoteFeeder");
                        }
                        #endregion
                    }

                    //insert EPGs to DB in batches
                    InsertEpgsDBBatches(ref epgDic, groupID, nCountPackage, FieldEntityMapping);

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
                                bool resultEpgIndex = Utils.UpdateEpgIndex(ulProgram, parentGroupID, ApiObjects.eAction.Update);
                                ulProgram = new List<ulong>();
                                nCount = 0;
                            }
                           
                        }
                        #endregion
                    }

                    if (nCount > 0 && ulProgram != null && ulProgram.Count > 0)
                    {
                        bool resultEpgIndex = Utils.UpdateEpgIndex(ulProgram, parentGroupID, ApiObjects.eAction.Update);
                    }

                    //start Upload proccess Queue
                    UploadQueue.UploadQueueHelper.SetJobsForUpload(groupID);

                    Logger.Logger.Log("KDG", string.Format("END EPG Channel {0}", channelID), "GraceNoteFeeder");
                }
                else
                {
                    Logger.Logger.Log("Missing Channel ID", string.Format("GetExistChannel() ChannelID = {0}", channel_id), "GraceNoteFeeder");
                }
            }
            catch (Exception exp)
            {
                Logger.Logger.Log("KDG", string.Format("fail to insert channel to DB ex = {0}", exp.Message), "GraceNoteFeeder");
            }
            finally
            {
            }


        }

        /*Get the default picture url from GraceNote API*/
        private string GetImageFromGN(XmlNode node, string EPGGuid)
        {
            string epg_url = string.Empty;
            try
            {
                string uri = URL;
                string method = "POST";
                string postData = string.Format(CategoryXml, client, user, EPGGuid);
                string sXml = Utils.getXmlFromGracenote(postData, method, uri);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(sXml);
                XmlNode myNode = doc.DocumentElement;
                epg_url = GetSingleNodeValue(myNode, "RESPONSE/TVPROGRAM/URL");
                return epg_url;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetImageFromGN", string.Format("fail to get pic URL from GraceNote API EPGGuid ={0}, ex = {1}", EPGGuid, ex.Message), "GraceNoteFeeder");
                return string.Empty;
            }
        }

        // fille the FieldEntityMapping with all values
        private void SetMappingValues(List<FieldTypeEntity> FieldEntityMapping, XmlNode node)
        {
            // fill all tags 
            GetTagsValues(node, ref FieldEntityMapping);
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
                    if (FieldEntityMapping[i].FieldType == enums.FieldTypes.Meta)
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
                Logger.Logger.Log("GetMetasValues", string.Format("failed with metaValues for node = {0}", ex.Message), "GraceNoteFeeder");
            }
        }

        // insert values to all tags (contributed nodes or regular tags)
        private void GetTagsValues(XmlNode node, ref List<FieldTypeEntity> FieldEntityMapping)
        {
            try
            {
                Dictionary<string, List<string>> contibutorDict = GetTagsValue(node);

                for (int i = 0; i < FieldEntityMapping.Count; i++)
                {
                    if (FieldEntityMapping[i].FieldType == enums.FieldTypes.Tag)
                    {
                        FieldEntityMapping[i].Value = new List<string>();
                        foreach (string XmlRefName in FieldEntityMapping[i].XmlReffName)
                        {
                            if (contibutorDict != null && contibutorDict.Count > 0 && contibutorDict.ContainsKey(XmlRefName.ToUpper())) // if tag exsits in contibutorDict - than add all its values
                            {
                                FieldEntityMapping[i].Value.AddRange(contibutorDict[XmlRefName.ToUpper()]);
                            }
                            else // regular tag
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
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetTagsValues", string.Format("failed with tagValues for node = {0}", ex.Message), "GraceNoteFeeder");
            }
        }

        private Int32 GetExistChannel(string sChannelID)
        {
            Int32 res = 0;
            if (!string.IsNullOrEmpty(sChannelID))
            {
                try
                {
                    res = EpgDal.GetChannelByChannelID(groupID, sChannelID);
                }
                catch (Exception exp)
                {
                    Logger.Logger.Log("GetExistChannel", string.Format("could not get Get Exist Channel  by ID {0}, error message: {1}", sChannelID, exp.Message), "GraceNoteFeeder");
                }
            }
            return res;
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
                        InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
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
                    InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
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
                Logger.Logger.Log("InsertEpgsDBBatches", string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", groupID, exc.Message), "EpgFeeder");
                return;
            }
        }

        private void InsertEpgs(int nGroupID, ref Dictionary<string, EpgCB> epgDic, List<FieldTypeEntity> FieldEntityMapping, Dictionary<int, List<string>> tagsAndValues)
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

                List<FieldTypeEntity> FieldEntityMappingMetas = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Meta).ToList();
                List<FieldTypeEntity> FieldEntityMappingTags = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Tag).ToList();

                Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs = new Dictionary<KeyValuePair<string, int>, List<string>>();// new tag values and the EPGs that have them
                //return relevant tag value ID, if they exist in the DB
                Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue = Utils.getTagTypeWithRelevantValues(nGroupID, FieldEntityMappingTags, tagsAndValues);

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

                Utils.InsertNewTagValues(epgDic, dtEpgTagsValues, ref dtEpgTags, newTagValueEpgs, nGroupID, nUpdaterID);

                Utils.InsertBulk(dtEpgMetas, "EPG_program_metas", sConn); //insert EPG Metas to DB
                Utils.InsertBulk(dtEpgTags, "EPG_program_tags", sConn); //insert EPG Tags to DB
            }
            catch (Exception exc)
            {
                Logger.Logger.Log("InsertEpgs", string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", nGroupID, exc.Message), "EpgFeeder");
                return;
            }
        }
    }
}    
