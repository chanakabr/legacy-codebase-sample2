using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using Newtonsoft.Json;
using Tvinci.Core.DAL;
using System.Data;
using EpgBL;
using ApiObjects;
using TvinciImporter;
using ApiObjects.Epg;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;

namespace EpgFeeder
{
    public class EpgGenerator
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        EpgChannels m_Channels;
        BaseEpgBL oEpgBL;
        List<LanguageObj> lLanguage = new List<LanguageObj>();
        List<FieldTypeEntity> FieldEntityMapping = new List<FieldTypeEntity>();
        Dictionary<int, string> ratios;
        string update_epg_package;
        int nCountPackage;

        protected static readonly int MaxDescriptionSize = 1024;
        public static readonly int MaxNameSize = 255;

        public EpgGenerator()
        {
        }
        public void Initialize(EpgChannels epgChannel)
        {
            m_Channels = epgChannel;
            oEpgBL = EpgBL.Utils.GetInstance(m_Channels.parentgroupid);
            lLanguage = GetLanguages(m_Channels.parentgroupid); // dictionary contains all language ids and its  code (string)
            // get mapping tags and metas 
            FieldEntityMapping = GetMappingFields(m_Channels.parentgroupid);
            update_epg_package = TVinciShared.WS_Utils.GetTcmConfigValue("update_epg_package");
            nCountPackage = ODBCWrapper.Utils.GetIntSafeVal(update_epg_package);
            // get mapping between ratio_id and ratio 
            Dictionary<string, string> sRatios = EpgDal.Get_PicsEpgRatios();
            ratios = sRatios.ToDictionary(x => int.Parse(x.Key), x => x.Value);
        }

        public void SaveChannelPrograms()
        {
            try
            {
                int kalturaChannelID;
                string channelID;
                EpgChannelType epgChannelType;

                // get the kaltura+ type to each channel by its external id                
                List<string> channelExternalIds = m_Channels.channel.Select(x => x.id).ToList<string>();
                Dictionary<string, List<EpgChannelObj>> epgChannelDict = EpgDal.GetAllEpgChannelsDic(m_Channels.groupid, channelExternalIds);

                //Run for each channel - 
                foreach (KeyValuePair<string, List<EpgChannelObj>> channel in epgChannelDict)
                {
                    // get all proframs related to specific channel 
                    List<programme> programs = m_Channels.programme.Where(x => x.channel == channel.Key).ToList();

                    foreach (EpgChannelObj epgChannelObj in channel.Value)
                    {
                        kalturaChannelID = epgChannelObj.ChannelId;
                        channelID = channel.Key;
                        epgChannelType = epgChannelObj.ChannelType;

                        SaveChannelPrograms(programs, kalturaChannelID, channelID, epgChannelType);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SaveChannelPrograms - " + string.Format("exception={0}", ex.Message), ex);
            }
        }

        private void SaveChannelPrograms(List<programme> programs, int kalturaChannelID, string channelID, EpgChannelType epgChannelType)
        {
            // EpgObject m_ChannelsFaild = null; // save all program that got exceptions TODO ????????            

            int nCount = 0;
            int nPicID = 0;
            List<ulong> epgIds = new List<ulong>();
            EpgCB newEpgItem;
            DateTime dProgStartDate;
            DateTime dProgEndDate;
            EpgPicture epgPicture;
            string language = string.Empty;

            Dictionary<string, EpgCB> dEpgCbTranslate = new Dictionary<string, EpgCB>(); // Language, EpgCB
            Dictionary<string, List<KeyValuePair<string, EpgCB>>> dEpg = new Dictionary<string, List<KeyValuePair<string, EpgCB>>>();// EpgIdentifier , <Language, EpgCB>
            Dictionary<string, List<KeyValuePair<string, string>>> dMetas = new Dictionary<string, List<KeyValuePair<string, string>>>(); // metaType, List<language, metaValue>
            Dictionary<string, List<EpgTagTranslate>> dTags = new Dictionary<string, List<EpgTagTranslate>>(); // tagType, List<EpgTagTranslate>

            #region each program  create CB objects

            DateTime dPublishDate = DateTime.UtcNow; // this publish date will insert to each epg that was update / insert 
            List<DateTime> deletedDays = new List<DateTime>();
            foreach (programme prog in programs)
            {
                newEpgItem = new EpgCB();
                dEpgCbTranslate = new Dictionary<string, EpgCB>(); // Language, EpgCB
                try
                {
                    dProgStartDate = DateTime.MinValue;
                    dProgEndDate = DateTime.MinValue;
                    epgPicture = new EpgPicture();

                    nPicID = 0;
                    string sPicUrl = string.Empty;

                    if (!Utils.ParseEPGStrToDate(prog.start, ref dProgStartDate) || !Utils.ParseEPGStrToDate(prog.stop, ref dProgEndDate))
                    {
                        log.Error("Program Dates Error - " + string.Format("start:{0}, end:{1}", prog.start, prog.stop));
                        continue;
                    }

                    DateTime dDate = new DateTime(dProgStartDate.Year, dProgStartDate.Month, dProgStartDate.Day);
                    if (!deletedDays.Contains(dDate))
                    {
                        deletedDays.Add(dDate);
                    }

                    newEpgItem.ChannelID = kalturaChannelID;
                    newEpgItem.GroupID = ODBCWrapper.Utils.GetIntSafeVal(m_Channels.groupid);
                    newEpgItem.ParentGroupID = m_Channels.parentgroupid;
                    newEpgItem.EpgIdentifier = prog.external_id;
                    newEpgItem.StartDate = dProgStartDate;
                    newEpgItem.EndDate = dProgEndDate;
                    newEpgItem.UpdateDate = DateTime.UtcNow;
                    newEpgItem.CreateDate = DateTime.UtcNow;
                    newEpgItem.isActive = true;
                    newEpgItem.Status = 1;

                    string picName = string.Empty;
                    #region Name  With languages
                    foreach (title name in prog.title)
                    {
                        language = name.lang.ToLower();
                        newEpgItem.Name = name.Value;
                        dEpgCbTranslate.Add(language, newEpgItem);

                        if (language == m_Channels.mainlang.ToLower())
                        {
                            picName = name.Value;
                        }
                    }
                    #endregion

                    #region Description With languages
                    if (prog.desc != null)
                    {
                        foreach (desc description in prog.desc)
                        {
                            language = description.lang.ToLower();
                            if (dEpgCbTranslate.ContainsKey(language))
                            {
                                dEpgCbTranslate[language].Description = description.Value;
                            }
                            else
                            {
                                newEpgItem.Description = description.Value;
                                dEpgCbTranslate.Add(language, newEpgItem);
                            }
                        }
                    }
                    #endregion

                    #region Upload Picture
                    if (!string.IsNullOrEmpty(picName) && prog.icon != null) // create the urlPic if this is the main language
                    {
                        foreach (icon icon in prog.icon)
                        {
                            string imgurl = icon.src; // TO BE ABLE TO WORK WITH MORE THEN ONE PIC 
                            // get the ratioid by ratio
                            int ratio = ODBCWrapper.Utils.GetIntSafeVal(ratios.Where(x => x.Value == icon.ratio).FirstOrDefault().Key);
                            epgPicture = new EpgPicture();
                            if (!string.IsNullOrEmpty(imgurl))
                            {
                                nPicID = ImporterImpl.DownloadEPGPic(imgurl, picName, m_Channels.groupid, 0, kalturaChannelID, ratio);

                                if (nPicID != 0)
                                {
                                    //Update CB, the DB is updated in the end with all other data
                                    sPicUrl = TVinciShared.CouchBaseManipulator.getEpgPicUrl(nPicID);
                                }
                                //update each epgCB with the picURL + PicID - ONLY FIRST ONE -  all the rest will be in the list 
                                if (newEpgItem.PicID == 0)
                                {
                                    newEpgItem.PicID = nPicID;
                                }
                                if (string.IsNullOrEmpty(newEpgItem.PicUrl))
                                {
                                    newEpgItem.PicUrl = sPicUrl;
                                }

                                if (newEpgItem.pictures.Count(x => x.PicID == nPicID && x.Ratio == icon.ratio) == 0) // this ratio not exsits yet in the list
                                {
                                    epgPicture.Url = sPicUrl;
                                    epgPicture.PicID = nPicID;
                                    epgPicture.Ratio = icon.ratio;
                                    newEpgItem.pictures.Add(epgPicture);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Tags and Metas
                    foreach (KeyValuePair<string, EpgCB> epg in dEpgCbTranslate)
                    {
                        language = epg.Key.ToLower();
                        epg.Value.Metas = GetEpgProgramMetas(prog, language);
                        epg.Value.Tags = GetEpgProgramTags(prog, language);
                        epg.Value.Language = language;
                        if (dEpg.ContainsKey(epg.Value.EpgIdentifier))
                        {
                            dEpg[epg.Value.EpgIdentifier].Add(epg);
                        }
                        else
                        {
                            dEpg.Add(epg.Value.EpgIdentifier, new List<KeyValuePair<string, EpgCB>>() { epg });
                        }
                    }
                    #endregion
                }
                catch (Exception exc)
                {
                    log.Error("Generate EPGs - " + string.Format("Exception in generating EPG name {0} in group: {1}. exception: {2} ", newEpgItem.Name, m_Channels.parentgroupid, exc.Message), exc);
                }
            }
            #endregion


            //insert EPGs to DB in batches
            // find the epg that need to be updated                    
            UpdateEpgDic(ref dEpg, m_Channels.groupid, dPublishDate, kalturaChannelID);

            //insert EPGs to DB in batches
            InsertEpgsDBBatches(ref dEpg, m_Channels.groupid, nCountPackage, FieldEntityMapping, m_Channels.mainlang.ToLower(), dPublishDate, kalturaChannelID);

            // Delete all EpgIdentifiers that are not needed (per channel per day)
            List<int> lProgramsID = DeleteEpgs(dPublishDate, kalturaChannelID, m_Channels.groupid, deletedDays);
            List<string> docIds = BuildDocIdsToRemoveGroupPrograms(lProgramsID);
            oEpgBL.RemoveGroupPrograms(docIds);

            foreach (List<KeyValuePair<string, EpgCB>> lEpg in dEpg.Values)
            {
                foreach (KeyValuePair<string, EpgCB> epg in lEpg)
                {
                    nCount++;

                    #region Insert EpgProgram to CB
                    string epgID = string.Empty;
                    bool isMainLang = epg.Value.Language.ToLower() == m_Channels.mainlang.ToLower() ? true : false;
                    bool bInsert = oEpgBL.InsertEpg(epg.Value, isMainLang, out epgID);
                    #endregion

                    #region Insert EpgProgram ES

                    if (nCount >= nCountPackage)
                    {
                        if (!epgIds.Contains(epg.Value.EpgID))
                        {
                            epgIds.Add(epg.Value.EpgID);
                        }
                        bool resultEpgIndex = UpdateEpgIndex(epgIds, m_Channels.parentgroupid, ApiObjects.eAction.Update);
                        epgIds = new List<ulong>();
                        nCount = 0;
                    }
                    else
                    {
                        if (!epgIds.Contains(epg.Value.EpgID))
                        {
                            epgIds.Add(epg.Value.EpgID);
                        }
                    }

                    #endregion
                }
            }

            if (nCount > 0 && epgIds != null && epgIds.Count > 0)
            {
                bool resultEpgIndex = UpdateEpgIndex(epgIds, m_Channels.parentgroupid, ApiObjects.eAction.Update);
            }

            //start Upload proccess Queue
            UploadQueue.UploadQueueHelper.SetJobsForUpload(m_Channels.parentgroupid);
        }

        //Build docids with languages per programid 
        private List<string> BuildDocIdsToRemoveGroupPrograms(List<int> lProgramsID)
        {
            List<string> docIds = new List<string>();
            string docID = string.Empty;
            //build key for languages by languageListObj

            foreach (int id in lProgramsID)
            {
                foreach (LanguageObj language in lLanguage)
                {
                    if (language.IsDefault)// main language
                    {
                        docID = id.ToString();
                    }
                    else
                    {
                        docID = string.Format("epg_{0}_lang_{1}", id, language.Code.ToLower());
                    }
                    docIds.Add(docID);
                }
            }

            return docIds;
        }

        private List<int> DeleteEpgs(DateTime dPublishDate, int channelID, int groupID, List<DateTime> deletedDays)
        {
            try
            {
                List<int> epgIds = new List<int>();
                //Delete all program by :  channelID , publishDate , groupID, deletedDays 
                DataTable dt = EpgDal.DeleteEpgs(channelID, groupID, dPublishDate, deletedDays);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int epgID = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                        epgIds.Add(epgID);
                    }
                }
                return epgIds;
            }
            catch (Exception ex)
            {
                log.Error("KDG - " + string.Format("fail to DeleteEpgs ex = {0}", ex.Message), ex);
                return null;
            }
        }

        private bool UpdateEpgIndex(List<ulong> epgIDs, int groupID, eAction action)
        {
            bool result = false;
            try
            {
                result = ImporterImpl.UpdateEpg(epgIDs, groupID, action);
                return result;
            }
            catch (Exception ex)
            {
                log.Error("EpgFeeder - " + string.Format("failed update EpgIndex ex={0}", ex.Message), ex);
                return false;
            }
        }

        private void InsertEpgsDBBatches(ref Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgDic, int groupID, int nCountPackage, List<FieldTypeEntity> FieldEntityMapping, string mainLlanguage,
            DateTime dPublishDate, int channelID)
        {
            Dictionary<string, EpgCB> epgBatch = new Dictionary<string, EpgCB>();
            Dictionary<int, List<string>> tagsAndValues = new Dictionary<int, List<string>>(); // <tagTypeId, List<tagValues>>

            int nEpgCount = 0;
            try
            {
                foreach (string sGuid in epgDic.Keys)
                {
                    // get only the main language
                    KeyValuePair<string, EpgCB> epg = epgDic[sGuid].Where(x => x.Key == mainLlanguage).First();

                    epgBatch.Add(sGuid, epg.Value);
                    nEpgCount++;

                    //generate a Dictionary of all tag and values in the epg
                    Utils.GenerateTagsAndValues(epg.Value, FieldEntityMapping, ref tagsAndValues);

                    if (nEpgCount >= nCountPackage)
                    {
                        InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues, dPublishDate, channelID, ratios);
                        nEpgCount = 0;
                        foreach (string guid in epgBatch.Keys)
                        {
                            if (epgBatch[guid].EpgID > 0)
                            {
                                epgDic[guid].Where(w => w.Key == guid).ToList().ForEach(i => i.Value.EpgID = epgBatch[guid].EpgID); // update all languages per EpgIdentifier with epgID                                                
                            }
                        }
                        epgBatch.Clear();
                        tagsAndValues.Clear();
                    }
                }

                if (nEpgCount > 0 && epgBatch.Keys.Count() > 0)
                {
                    InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues, dPublishDate, channelID, ratios);
                    foreach (string guid in epgBatch.Keys)
                    {
                        if (epgBatch[guid].EpgID > 0)
                        {
                            epgDic[guid].Where(w => w.Key == guid).ToList().ForEach(i => i.Value.EpgID = epgBatch[guid].EpgID);// update all languages per EpgIdentifier with epgID     
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

        private void InsertEpgs(int groupID, ref Dictionary<string, EpgCB> epgDic, List<FieldTypeEntity> FieldEntityMapping, Dictionary<int, List<string>> tagsAndValues, DateTime dPublishDate, int channelID, Dictionary<int, string> ratios)
        {
            try
            {
                DataTable dtEpgMetas = InitEPGProgramMetaDataTable();
                DataTable dtEpgTags = InitEPGProgramTagsDataTable();
                DataTable dtEpgTagsValues = InitEPG_Tags_Values();
                DataTable dtEpgPictures = InitEPGProgramPicturesDataTable();

                int nUpdaterID = m_Channels.updaterid;
                if (m_Channels.updaterid == 0)
                    nUpdaterID = 700;

                string sConn = "MAIN_CONNECTION_STRING";

                List<FieldTypeEntity> FieldEntityMappingMetas = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Meta).ToList();
                List<FieldTypeEntity> FieldEntityMappingTags = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Tag).ToList();

                Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs = new Dictionary<KeyValuePair<string, int>, List<string>>();// new tag values and the EPGs that have them
                //return relevant tag value ID, if they exist in the DB
                Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue = getTagTypeWithRelevantValues(groupID, FieldEntityMappingTags, tagsAndValues);

                //update all values that already exsits in table
                UpdateEPG_Channels_sched(ref epgDic, dPublishDate, channelID);
                // insert all epg to DB (epg_channels_schedule)
                InsertEPG_Channels_sched(ref epgDic, m_Channels.mainlang, dPublishDate, channelID);

                List<int> epgIDs = new List<int>();

                // Tags and Mates
                foreach (EpgCB epg in epgDic.Values)
                {
                    if (epg != null)
                    {
                        epgIDs.Add(Convert.ToInt32(epg.EpgID));

                        //update Metas
                        UpdateMetasPerEPG(ref dtEpgMetas, epg, FieldEntityMappingMetas, nUpdaterID);
                        //update Tags                    
                        UpdateExistingTagValuesPerEPG(epg, FieldEntityMappingTags, ref dtEpgTags, ref dtEpgTagsValues, TagTypeIdWithValue, ref newTagValueEpgs, nUpdaterID);
                        // update Pictures
                        FillEpgPictureTable(ref dtEpgPictures, epg, ratios);
                    }
                }

                // batch insert 

                InsertNewTagValues(epgDic, dtEpgTagsValues, ref dtEpgTags, newTagValueEpgs, groupID, nUpdaterID);

                // delete all values per tag and meta for programIDS that exsits 
                bool bDelete = EpgDal.DeleteEpgProgramDetails(epgIDs, groupID);
                InsertBulk(dtEpgMetas, "EPG_program_metas", sConn); //insert EPG Metas to DB
                InsertBulk(dtEpgTags, "EPG_program_tags", sConn); //insert EPG Tags to DB
                InsertBulk(dtEpgPictures, "epg_multi_pictures", sConn);//insert Multi epg pictures to DB
            }
            catch (Exception exc)
            {
                log.Error("InsertEpgs - " + string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", groupID, exc.Message), exc);
                return;
            }
        }

        private void FillEpgPictureTable(ref DataTable dtEpgPictures, EpgCB epg, Dictionary<int, string> ratios)
        {
            if (epg != null)
            {
                foreach (EpgPicture epgPicture in epg.pictures)
                {
                    DataRow row = dtEpgPictures.NewRow();
                    row["channel_id"] = epg.ChannelID;
                    row["epg_identifier"] = epg.EpgIdentifier;
                    row["pic_id"] = epgPicture.PicID;

                    if (!string.IsNullOrEmpty(epgPicture.Ratio))
                    {
                        int ratioID = ratios.Where(x => x.Value == epgPicture.Ratio).First().Key;
                        row["ratio_id"] = ratioID;
                    }
                    else
                    {
                        row["ratio_id"] = 0;
                    }

                    row["STATUS"] = epg.Status;
                    row["GROUP_ID"] = epg.GroupID;
                    row["UPDATER_ID"] = 400;
                    row["UPDATE_DATE"] = epg.UpdateDate;
                    row["CREATE_DATE"] = epg.CreateDate;
                    dtEpgPictures.Rows.Add(row);
                }
            }
        }

        private DataTable InitEPGProgramPicturesDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("epg_identifier", typeof(string));
            dt.Columns.Add("channel_id", typeof(int));
            dt.Columns.Add("pic_id", typeof(int));
            dt.Columns.Add("ratio_id", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(int));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("UPDATER_ID", typeof(int));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            return dt;
        }

        private void InsertNewTagValues(Dictionary<string, EpgCB> epgDic, DataTable dtEpgTagsValues, ref DataTable dtEpgTags, Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs, int groupID, int nUpdaterID)
        {
            Dictionary<KeyValuePair<string, int>, int> tagValueWithID = new Dictionary<KeyValuePair<string, int>, int>();
            Dictionary<int, List<string>> dicTagTypeIDAndValues = new Dictionary<int, List<string>>();
            string sConn = "MAIN_CONNECTION_STRING";

            if (dtEpgTagsValues != null && dtEpgTagsValues.Rows != null && dtEpgTagsValues.Rows.Count > 0)
            {
                //insert all New tag values from dtEpgTagsValues to DB
                InsertBulk(dtEpgTagsValues, "EPG_tags", sConn);

                //retrun back all the IDs of the new Tags_Values
                for (int k = 0; k < dtEpgTagsValues.Rows.Count; k++)
                {
                    DataRow row = dtEpgTagsValues.Rows[k];
                    string sTagValue = ODBCWrapper.Utils.GetSafeStr(row, "value");
                    int nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");
                    if (!dicTagTypeIDAndValues.Keys.Contains(nTagTypeID))
                    {
                        dicTagTypeIDAndValues.Add(nTagTypeID, new List<string>() { sTagValue });
                    }
                    else
                    {
                        dicTagTypeIDAndValues[nTagTypeID].Add(sTagValue);
                    }
                }

                DataTable dtTagValueID = EpgDal.Get_EPGTagValueIDs(groupID, dicTagTypeIDAndValues);

                //update the IDs in tagValueWithID
                if (dtTagValueID != null && dtTagValueID.Rows != null)
                {
                    for (int i = 0; i < dtTagValueID.Rows.Count; i++)
                    {
                        DataRow row = dtTagValueID.Rows[i];
                        if (row != null)
                        {
                            int nTagValueID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                            string sTagValue = ODBCWrapper.Utils.GetSafeStr(row, "VALUE");
                            int nTagType = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");

                            KeyValuePair<string, int> tagValueAndType = new KeyValuePair<string, int>(sTagValue, nTagType);
                            if (!tagValueWithID.Keys.Contains(tagValueAndType))
                            {
                                tagValueWithID.Add(tagValueAndType, nTagValueID);
                            }
                        }
                    }
                }
            }

            //go over all newTagValueEpgs and update the EPG_Program_Tags
            foreach (KeyValuePair<string, int> kvpUpdated in newTagValueEpgs.Keys)
            {
                int TagValueID = 0;
                List<KeyValuePair<string, int>> tempTagValue = tagValueWithID.Keys.Where(x => x.Key == kvpUpdated.Key && x.Value == kvpUpdated.Value).ToList();
                if (tempTagValue != null && tempTagValue.Count > 0)
                {
                    TagValueID = tagValueWithID[tempTagValue[0]];
                    if (TagValueID > 0)
                    {
                        foreach (string epgGUID in newTagValueEpgs[kvpUpdated])
                        {
                            EpgCB epgToUpdate = epgDic[epgGUID];
                            FillEpgExtraDataTable(ref dtEpgTags, false, "", epgToUpdate.EpgID, TagValueID, epgToUpdate.GroupID, epgToUpdate.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                        }
                    }
                }
            }
        }

        private void UpdateEPG_Channels_sched(ref Dictionary<string, EpgCB> epgDic, DateTime dPublishDate, int channelID)
        {
            EpgCB epg;
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            Dictionary<string, EpgCB> updateEpgDic = new Dictionary<string, EpgCB>();

            //get back the IDs list of the EPGs          
            DataTable dtEpgIDGUID = EpgDal.Get_EpgIDbyEPGIdentifier(epgDic.Keys.ToList(), channelID);
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
                        {
                            epgDic[sGuid].EpgID = nEPG_ID;  //update the EPGCB with the ID
                            updateEpgDic.Add(sGuid, epgDic[sGuid]);
                        }
                    }
                }
                if (updateEpgDic != null && updateEpgDic.Count > 0)
                {
                    DataTable dtEPG = InitEPGDataTableWithID();
                    FillEPGDataTable(updateEpgDic, ref dtEPG, dPublishDate);
                    bool bUpdated = EpgDal.UpdateEpgChannelSchedule(dtEPG);
                }
            }
        }

        private DataTable InitEPGDataTableWithID()
        {
            DataTable dt = new DataTable();
            // Add three column objects to the table. 
            DataColumn ID = new DataColumn();
            ID.DataType = typeof(long);
            ID.ColumnName = "ID";
            ID.Unique = true;
            dt.Columns.Add(ID);
            dt.Columns.Add("EPG_CHANNEL_ID", typeof(long));
            dt.Columns.Add("EPG_IDENTIFIER", typeof(string));
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));
            dt.Columns.Add("START_DATE", typeof(DateTime));
            dt.Columns.Add("END_DATE", typeof(DateTime));
            dt.Columns.Add("PIC_ID", typeof(long));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("IS_ACTIVE", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(long));
            dt.Columns.Add("UPDATER_ID", typeof(long));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            dt.Columns.Add("PUBLISH_DATE", typeof(DateTime));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("EPG_TAG", typeof(string));
            dt.Columns.Add("media_id", typeof(long));
            dt.Columns.Add("FB_OBJECT_ID", typeof(string));
            dt.Columns.Add("like_counter", typeof(long));
            return dt;
        }

        private void UpdateEpgDic(ref Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgDic, int groupID, DateTime dPublishDate, int channelID)
        {
            try
            {
                List<int> epgIdsToUpdate = new List<int>();
                List<string> epgGuid = epgDic.Keys.ToList();
                List<string> epgIds = new List<string>(); // list of all exsits epg programs ids 

                string epgIDCB = string.Empty;

                DataTable dt = EpgDal.EpgGuidExsits(epgGuid, groupID, channelID);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ulong epgID = (ulong)ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                        string epgIDentifier = ODBCWrapper.Utils.GetSafeStr(dr, "EPG_IDENTIFIER");

                        if (epgDic.ContainsKey(epgIDentifier))
                        {
                            foreach (KeyValuePair<string, EpgCB> item in epgDic[epgIDentifier])
                            {
                                item.Value.EpgID = epgID;
                                if (item.Key == m_Channels.mainlang.ToLower())
                                {
                                    epgIDCB = epgID.ToString(); //main langu
                                }
                                else // otherwise add the lang to the id value 
                                {
                                    epgIDCB = string.Format("{0}_{1}", epgID.ToString(), item.Key);
                                }
                                epgIds.Add(epgIDCB);
                            }
                        }
                    }
                }

                if (epgIds.Count > 0)
                {
                    // get all epg object from CB
                    BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(groupID);
                    List<EpgCB> lResCB = oEpgBL.GetEpgs(epgIds);
                    List<KeyValuePair<string, EpgCB>> removeLang = new List<KeyValuePair<string, EpgCB>>();

                    // TO DO need to add all keys for languages 
                    if (lResCB != null && lResCB.Count > 0) // start comper the objects
                    {
                        foreach (EpgCB cbEpg in lResCB)
                        {
                            if (epgDic.ContainsKey(cbEpg.EpgIdentifier))
                            {
                                // comper object 
                                foreach (KeyValuePair<string, EpgCB> item in epgDic[cbEpg.EpgIdentifier])
                                {
                                    bool bEquals = cbEpg.Equals(item.Value);
                                    if (bEquals)
                                    {
                                        // build list with programs ids to update there Publish Date in DB later
                                        if (!epgIdsToUpdate.Contains(Convert.ToInt32(cbEpg.EpgID)))
                                        {
                                            epgIdsToUpdate.Add(Convert.ToInt32(cbEpg.EpgID));
                                        }
                                        // remove the object from the dictionary (specific for the lang)
                                        removeLang.Add(item);
                                    }
                                }
                                // remove all lang item for the EpgIdentifier
                                foreach (KeyValuePair<string, EpgCB> epgRemove in removeLang)
                                {
                                    epgDic[cbEpg.EpgIdentifier].Remove(epgRemove);
                                }
                                if (epgDic[cbEpg.EpgIdentifier].Count() == 0)
                                {
                                    epgDic.Remove(cbEpg.EpgIdentifier);
                                }
                            }
                        }

                        if (epgIdsToUpdate.Count > 0)
                        {
                            // update the epgs with publish date 
                            bool bUpdate = EpgDal.UpdateEpgChannelSchedulePublishDate(epgIdsToUpdate, dPublishDate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("KDG - " + string.Format("fail to UpdateEpgDic ex = {0}", ex.Message), ex);
            }
        }

        private List<LanguageObj> GetLanguages(int nGroupID)
        {
            List<LanguageObj> lLang = new List<LanguageObj>();
            try
            {
                lLang = CatalogDAL.GetGroupLanguages(nGroupID);
                return lLang;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new List<LanguageObj>();
            }
        }

        public List<FieldTypeEntity> GetMappingFields(int nGroupID)
        {
            try
            {
                List<FieldTypeEntity> AllFieldTypeMapping = new List<FieldTypeEntity>();
                List<FieldTypeEntity> AllFieldType = new List<FieldTypeEntity>();
                GroupManager groupManager = new GroupManager();
                List<int> lSubTree = groupManager.GetSubGroup(nGroupID);

                DataSet ds = EpgDal.GetEpgMappingFields(lSubTree, nGroupID);

                if (ds != null && ds.Tables != null && ds.Tables.Count >= 4)
                {
                    if (ds.Tables[0] != null)//basic
                    {
                        InitializeMappingFields(ds.Tables[0], ds.Tables[3], enums.FieldTypes.Basic, ref AllFieldTypeMapping);
                    }
                    if (ds.Tables[1] != null)//metas
                    {
                        InitializeMappingFields(ds.Tables[1], ds.Tables[3], enums.FieldTypes.Meta, ref AllFieldTypeMapping);
                    }
                    if (ds.Tables[2] != null)//Tags
                    {
                        InitializeMappingFields(ds.Tables[2], ds.Tables[3], enums.FieldTypes.Tag, ref AllFieldTypeMapping);
                    }

                }

                return AllFieldTypeMapping;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new List<FieldTypeEntity>();
            }
        }

        private Dictionary<string, List<string>> GetEpgProgramMetas(programme prog, string language)
        {
            try
            {
                Dictionary<string, List<string>> dEpgMetas = new Dictionary<string, List<string>>();
                if (prog.metas != null)
                {
                    foreach (metas meta in prog.metas)
                    {
                        // get all relevant language 
                        List<MetaValues> metaValues = meta.MetaValues.Where(x => x.lang.ToLower() == language).ToList();
                        foreach (MetaValues value in metaValues)
                        {
                            if (dEpgMetas.ContainsKey(meta.MetaType))
                            {
                                dEpgMetas[meta.MetaType].Add(value.Value);
                            }
                            else
                            {
                                dEpgMetas.Add(meta.MetaType, new List<string>() { value.Value });
                            }
                        }
                    }
                }
                return dEpgMetas;
            }
            catch (Exception ex)
            {
                log.Error("GetEpgProgramMetas - " + string.Format("faild due ex={0}", ex.Message), ex);
                return new Dictionary<string, List<string>>();
            }
        }

        private Dictionary<string, List<string>> GetEpgProgramTags(programme prog, string language)
        {
            try
            {
                Dictionary<string, List<string>> dEpgTags = new Dictionary<string, List<string>>();
                if (prog.tags != null)
                {
                    foreach (tags tag in prog.tags)
                    {
                        List<TagValues> tagValues = tag.TagValues.Where(x => x.lang.ToLower() == language).ToList();
                        foreach (TagValues value in tagValues)
                        {
                            if (dEpgTags.ContainsKey(tag.TagType))
                            {
                                dEpgTags[tag.TagType].Add(value.Value);
                            }
                            else
                            {
                                dEpgTags.Add(tag.TagType, new List<string>() { value.Value });
                            }
                        }
                    }
                }
                return dEpgTags;
            }
            catch (Exception ex)
            {
                log.Error("GetEpgProgramMetas - " + string.Format("failed due ex={0}", ex.Message), ex);
                return new Dictionary<string, List<string>>();
            }
        }

        //generate a Dictionary of all tag and values in the epg
        private void GenerateTagsAndValues(EpgCB epg, List<FieldTypeEntity> FieldEntityMapping, ref  Dictionary<int, List<string>> tagsAndValues)
        {
            foreach (string tagType in epg.Tags.Keys)
            {
                string tagTypel = tagType.ToLower();
                int tagTypeID = 0;
                List<FieldTypeEntity> tagField = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Tag && x.Name.ToLower() == tagTypel).ToList();
                if (tagField != null && tagField.Count > 0)
                {
                    tagTypeID = tagField[0].ID;
                }
                else
                {
                    log.Debug("UpdateExistingTagValuesPerEPG - " + string.Format("Missing tag Definition in FieldEntityMapping of tag:{0} in EPG:{1}", tagType, epg.EpgID));
                    continue;//missing tag definition in DB (in FieldEntityMapping)                        
                }

                if (!tagsAndValues.ContainsKey(tagTypeID))
                {
                    tagsAndValues.Add(tagTypeID, new List<string>());
                }
                foreach (string tagValue in epg.Tags[tagType])
                {
                    if (!tagsAndValues[tagTypeID].Contains(tagValue.ToLower()))
                        tagsAndValues[tagTypeID].Add(tagValue.ToLower());
                }
            }
        }

        private DataTable InitEPGDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EPG_CHANNEL_ID", typeof(long));
            dt.Columns.Add("EPG_IDENTIFIER", typeof(string));
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));
            dt.Columns.Add("START_DATE", typeof(DateTime));
            dt.Columns.Add("END_DATE", typeof(DateTime));
            dt.Columns.Add("PIC_ID", typeof(long));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("IS_ACTIVE", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(long));
            dt.Columns.Add("UPDATER_ID", typeof(long));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            dt.Columns.Add("PUBLISH_DATE", typeof(DateTime));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("EPG_TAG", typeof(string));
            dt.Columns.Add("media_id", typeof(long));
            dt.Columns.Add("FB_OBJECT_ID", typeof(string));
            dt.Columns.Add("like_counter", typeof(long));
            return dt;
        }

        private DataTable InitEPGProgramMetaDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_meta_id", typeof(int));
            dt.Columns.Add("program_id", typeof(int));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            dt.Columns.Add("language_id", typeof(long));
            return dt;
        }

        private DataTable InitEPGProgramTagsDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("program_id", typeof(int));
            dt.Columns.Add("epg_tag_id", typeof(int));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        private DataTable InitEPGProgramTagsTranslateDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_tag_value_id", typeof(int));
            dt.Columns.Add("language_id", typeof(int));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        private DataTable InitEPG_Tags_Values()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_tag_type_id", typeof(string));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        private DataTable InitEPG_Tags_Values_Translate()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_tag_value_id", typeof(string)); // the key to EPG_tags table
            dt.Columns.Add("language_id", typeof(long));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));

            return dt;
        }

        private Dictionary<int, List<KeyValuePair<string, int>>> getTagTypeWithRelevantValues(int nGroupID, List<FieldTypeEntity> FieldEntityMappingTags, Dictionary<int, List<string>> tagsAndValues)
        {
            Dictionary<int, List<KeyValuePair<string, int>>> dicTagTypeWithValues = new Dictionary<int, List<KeyValuePair<string, int>>>();//per tag type, thier values and IDs

            DataTable dtTagValueID = EpgDal.Get_EPGTagValueIDs(nGroupID, tagsAndValues);

            if (dtTagValueID != null && dtTagValueID.Rows != null)
            {
                for (int i = 0; i < dtTagValueID.Rows.Count; i++)
                {
                    DataRow row = dtTagValueID.Rows[i];
                    if (row != null)
                    {
                        int nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");
                        string sValue = ODBCWrapper.Utils.GetSafeStr(row, "VALUE");
                        int nID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                        KeyValuePair<string, int> kvp = new KeyValuePair<string, int>(sValue, nID);
                        if (dicTagTypeWithValues.ContainsKey(nTagTypeID))
                        {
                            //check if the value exists already in the dictionary (maybe in UpperCase\LowerCase)
                            List<KeyValuePair<string, int>> resultList = new List<KeyValuePair<string, int>>();
                            resultList = dicTagTypeWithValues[nTagTypeID].Where(x => x.Key.ToLower() == sValue.ToLower() && x.Value == nID).ToList();
                            if (resultList.Count == 0)
                            {
                                dicTagTypeWithValues[nTagTypeID].Add(kvp);
                            }
                        }
                        else
                        {
                            List<KeyValuePair<string, int>> lValues = new List<KeyValuePair<string, int>>() { kvp };
                            dicTagTypeWithValues.Add(nTagTypeID, lValues);
                        }
                    }
                }
            }
            return dicTagTypeWithValues;
        }

        /*
         ** Insert into epg_channels_schedule table  with the new epg program , 
         * and fill the dictionary with the epg_id for each 
         * EpgCB object
         * With multi languages this is ONLY  for the MAIN language 
         */
        private void InsertEPG_Channels_sched(ref Dictionary<string, EpgCB> epgDic, string mainLanguage, DateTime dPublishDate, int channelID)
        {
            EpgCB epg;
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            Dictionary<string, EpgCB> insertEpgDic = new Dictionary<string, EpgCB>();
            DataTable dtEPG = InitEPGDataTable();

            foreach (KeyValuePair<string, EpgCB> kv in epgDic)
            {
                if (kv.Value != null && kv.Value.EpgID == 0)
                {
                    insertEpgDic.Add(kv.Key, kv.Value);
                }
            }
            FillEPGDataTable(insertEpgDic, ref dtEPG, dPublishDate);
            string sConn = "MAIN_CONNECTION_STRING";
            InsertBulk(dtEPG, "epg_channels_schedule", sConn); //insert EPGs to DB

            //get back the IDs list of the EPGs          
            DataTable dtEpgIDGUID = EpgDal.Get_EpgIDbyEPGIdentifier(insertEpgDic.Keys.ToList(), channelID);
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
                        {
                            epgDic[sGuid].EpgID = nEPG_ID;  //update the EPGCB with the ID
                        }
                    }
                }
            }
        }

        private void FillEPGDataTable(Dictionary<string, EpgCB> epgDic, ref DataTable dtEPG, DateTime dPublishDate)
        {
            if (epgDic != null && epgDic.Count > 0)
            {
                foreach (EpgCB epg in epgDic.Values)
                {
                    if (epg != null)
                    {
                        DataRow row = dtEPG.NewRow();
                        row["EPG_CHANNEL_ID"] = epg.ChannelID;
                        row["EPG_IDENTIFIER"] = epg.EpgIdentifier;

                        epg.Name = epg.Name.Replace("\r", "").Replace("\n", "");
                        if (epg.Name.Length >= MaxNameSize)
                            row["NAME"] = epg.Name.Substring(0, MaxNameSize); //insert only 255 chars (limitation of the column in the DB)
                        else
                            row["NAME"] = epg.Name;
                        epg.Description = epg.Description.Replace("\r", "").Replace("\n", "");
                        if (epg.Description.Length >= MaxDescriptionSize)
                            row["DESCRIPTION"] = epg.Description.Substring(0, MaxDescriptionSize); //insert only 1024 chars (limitation of the column in the DB)
                        else
                            row["DESCRIPTION"] = epg.Description;
                        row["START_DATE"] = epg.StartDate;
                        row["END_DATE"] = epg.EndDate;
                        row["PIC_ID"] = epg.PicID;
                        row["STATUS"] = epg.Status;
                        row["IS_ACTIVE"] = epg.isActive;
                        row["GROUP_ID"] = epg.GroupID;
                        row["UPDATER_ID"] = 400;
                        row["UPDATE_DATE"] = epg.UpdateDate;
                        row["PUBLISH_DATE"] = dPublishDate;
                        row["CREATE_DATE"] = epg.CreateDate;
                        row["EPG_TAG"] = null;
                        row["media_id"] = epg.ExtraData.MediaID;
                        row["FB_OBJECT_ID"] = epg.ExtraData.FBObjectID;
                        row["like_counter"] = epg.Statistics.Likes;

                        if (row.Table.Columns.Contains("ID") && epg.EpgID > 0)
                        {
                            row["ID"] = epg.EpgID;
                        }
                        dtEPG.Rows.Add(row);
                    }
                }
            }
        }


        /*create datatable with all epgCB details - to insert it later to Database*/
        private void FillEPGDataTable(Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgDic, ref DataTable dtEPG, string mainLanguage)
        {
            if (epgDic != null && epgDic.Count > 0)
            {
                EpgCB epg;
                foreach (List<KeyValuePair<string, EpgCB>> lEpg in epgDic.Values)
                {
                    foreach (KeyValuePair<string, EpgCB> kvEpg in lEpg)
                    {
                        if (kvEpg.Key == mainLanguage)
                        {
                            epg = kvEpg.Value;
                            if (epg != null)
                            {
                                DataRow row = dtEPG.NewRow();
                                row["EPG_CHANNEL_ID"] = epg.ChannelID;
                                row["EPG_IDENTIFIER"] = epg.EpgIdentifier;
                                row["NAME"] = epg.Name;
                                if (epg.Description.Length >= MaxDescriptionSize)
                                    row["DESCRIPTION"] = epg.Description.Substring(0, MaxDescriptionSize); //insert only 1024 chars (limitation of the column in the DB)
                                else
                                    row["DESCRIPTION"] = epg.Description;
                                row["START_DATE"] = epg.StartDate;
                                row["END_DATE"] = epg.EndDate;
                                row["PIC_ID"] = epg.PicID;
                                row["STATUS"] = epg.Status;
                                row["IS_ACTIVE"] = epg.isActive;
                                row["GROUP_ID"] = epg.GroupID;
                                row["UPDATER_ID"] = 400;
                                row["UPDATE_DATE"] = epg.UpdateDate;
                                row["PUBLISH_DATE"] = DateTime.UtcNow;
                                row["CREATE_DATE"] = epg.CreateDate;
                                row["EPG_TAG"] = null;
                                row["media_id"] = epg.ExtraData.MediaID;
                                row["FB_OBJECT_ID"] = epg.ExtraData.FBObjectID;
                                row["like_counter"] = epg.Statistics.Likes;
                                dtEPG.Rows.Add(row);
                            }
                        }
                    }
                }
            }
        }

        //Insert rows of table to the db at once using bulk operation.      
        private void InsertBulk(DataTable dt, string sTableName, string sConnName)
        {
            if (dt != null)
            {
                ODBCWrapper.InsertQuery insertMessagesBulk = new ODBCWrapper.InsertQuery();
                insertMessagesBulk.SetConnectionKey(sConnName);
                try
                {
                    insertMessagesBulk.InsertBulk(sTableName, dt);
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
                finally
                {
                    if (insertMessagesBulk != null)
                    {
                        insertMessagesBulk.Finish();
                    }
                    insertMessagesBulk = null;
                }
            }
        }

        /*Build epgMeta datatable to insert it later to db*/
        private void UpdateMetasPerEPG(ref DataTable dtEpgMetas, EpgCB epg, List<FieldTypeEntity> FieldEntityMappingMetas, int nUpdaterID)
        {
            List<FieldTypeEntity> metaField = new List<FieldTypeEntity>();
            LanguageObj oLanguage = lLanguage.FirstOrDefault<LanguageObj>(x => x.Code.ToLower() == epg.Language.ToLower());
            foreach (string sMetaName in epg.Metas.Keys)
            {
                metaField = FieldEntityMappingMetas.Where(x => x.Name == sMetaName).ToList();
                int nID = 0;
                if (metaField != null && metaField.Count > 0)
                {
                    nID = metaField[0].ID;
                    if (epg.Metas[sMetaName].Count > 0)
                    {
                        string sValue = epg.Metas[sMetaName][0];
                        FillEpgExtraDataTable(ref dtEpgMetas, true, sValue, epg.EpgID, nID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow, oLanguage.ID);
                    }
                }
                else
                {   //missing meta definition in DB (in FieldEntityMapping)
                    log.Debug("UpdateMetasPerEPG - " + string.Format("Missing Meta Definition in FieldEntityMapping of Meta:{0} in EPG:{1}", sMetaName, epg.EpgID));
                }
                metaField = null;
            }
        }

        private void FillEpgExtraDataTable(ref DataTable dtEPGExtra, bool bIsMeta, string sValue, ulong nProgID, int nID, int nGroupID, int nStatus,
          int nUpdaterID, DateTime dCreateTime, DateTime dUpdateTime, int languageID = 0, bool bFillLanguageID = false)
        {
            DataRow row = dtEPGExtra.NewRow();
            if (bIsMeta)
            {
                row["value"] = sValue;
                row["epg_meta_id"] = nID;
            }
            else
            {
                row["epg_tag_id"] = nID;
            }

            row["program_id"] = nProgID;
            row["group_id"] = nGroupID;
            row["status"] = nStatus;
            row["updater_id"] = nUpdaterID;
            row["create_date"] = dCreateTime;
            row["update_date"] = dUpdateTime;
            if (bFillLanguageID)
            {
                row["language_id"] = languageID;
            }
            dtEPGExtra.Rows.Add(row);
        }

        private void FillEpgTagTranslateDataTable(ref DataTable dtEPGExtra, string sValue, int nEpgTagValueId, string sValueMain, int nGroupID, int nStatus,
        int nUpdaterID, DateTime dCreateTime, DateTime dUpdateTime, int languageID)
        {
            DataRow row = dtEPGExtra.NewRow();

            row["value"] = sValue;
            row["epg_tag_value_id"] = nEpgTagValueId;
            row["epg_tag_value_main"] = sValueMain;
            row["language_id"] = languageID;
            row["group_id"] = nGroupID;
            row["status"] = nStatus;
            row["updater_id"] = nUpdaterID;
            row["create_date"] = dCreateTime;
            row["update_date"] = dUpdateTime;

            dtEPGExtra.Rows.Add(row);
        }

        private void UpdateExistingTagValuesPerEPG(EpgCB epg, List<FieldTypeEntity> FieldEntityMappingTags, ref DataTable dtEpgTags, ref DataTable dtEpgTagsValues,
            Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue, ref Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs, int nUpdaterID)
        {
            KeyValuePair<string, int> kvp = new KeyValuePair<string, int>();

            foreach (string sTagName in epg.Tags.Keys)
            {
                List<FieldTypeEntity> tagField = FieldEntityMappingTags.Where(x => x.Name == sTagName).ToList();//get the tag_type_ID
                int nTagTypeID = 0;

                if (tagField != null && tagField.Count > 0)
                {
                    nTagTypeID = tagField[0].ID;
                }
                else
                {
                    log.Debug("UpdateExistingTagValuesPerEPG - " + string.Format("Missing tag Definition in FieldEntityMapping of tag:{0} in EPG:{1}", sTagName, epg.EpgID));
                    continue;//missing tag definition in DB (in FieldEntityMapping)                        
                }

                foreach (string sTagValue in epg.Tags[sTagName])
                {
                    if (sTagValue != "")
                    {
                        kvp = new KeyValuePair<string, int>(sTagValue, nTagTypeID);

                        if (TagTypeIdWithValue.ContainsKey(nTagTypeID))
                        {
                            List<KeyValuePair<string, int>> list = TagTypeIdWithValue[nTagTypeID].Where(x => x.Key == sTagValue.ToLower()).ToList();
                            if (list != null && list.Count > 0)
                            {
                                //Insert New EPG Tag Value in EPG_Program_Tags, we are assuming this tag value was not assigned to the program because the program is new                                                    
                                FillEpgExtraDataTable(ref dtEpgTags, false, "", epg.EpgID, list[0].Value, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                            }
                            else//tha tag value does not exist in the DB
                            {
                                //the newTagValueEpgs has this tag + value: only need to update that this specific EPG is using it
                                if (newTagValueEpgs.Where(x => x.Key.Key == kvp.Key && x.Key.Value == kvp.Value).ToList().Count > 0)
                                {
                                    newTagValueEpgs[kvp].Add(epg.EpgIdentifier);
                                }
                                else //need to insert a new tag +value to the newTagValueEpgs and update the relevant table 
                                {
                                    FillEpgTagValueTable(ref dtEpgTagsValues, sTagValue, epg.EpgID, nTagTypeID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                                    List<string> lEpgGUID = new List<string>() { epg.EpgIdentifier };
                                    newTagValueEpgs.Add(kvp, lEpgGUID);
                                }
                            }
                        }
                        else //this tag type does not have the relevant values in the DB, need to insert a new tag +value to the newTagValueEpgs and update the relevant table 
                        {
                            //check if it was not already added to the newTagValueEpgs
                            if (newTagValueEpgs.Where(x => x.Key.Key == kvp.Key && x.Key.Value == kvp.Value).ToList().Count == 0)
                            {
                                FillEpgTagValueTable(ref dtEpgTagsValues, sTagValue, epg.EpgID, nTagTypeID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                                List<string> lEpgGUID = new List<string>() { epg.EpgIdentifier };
                                newTagValueEpgs.Add(kvp, lEpgGUID);
                            }
                            else ////the newTagValueEpgs has this tag + value: only need to update that this  specific EPG is using it
                            {
                                newTagValueEpgs[kvp].Add(epg.EpgIdentifier);
                            }
                        }
                    }
                    tagField = null;
                }
            }
        }

        private void FillEpgTagValueTable(ref DataTable dtEPGTagValue, string sValue, ulong nProgID, int nTagTypeID, int nGroupID, int nStatus, int nUpdaterID,
            DateTime dCreateTime, DateTime dUpdateTime)
        {
            DataRow row = dtEPGTagValue.NewRow();
            row["value"] = sValue;
            row["epg_tag_type_id"] = nTagTypeID;
            row["group_id"] = nGroupID;
            row["status"] = nStatus;
            row["updater_id"] = nUpdaterID;
            row["create_date"] = dCreateTime;
            row["update_date"] = dUpdateTime;
            dtEPGTagValue.Rows.Add(row);
        }

        //insert new tag values and update the tag value ID in tagValueWithID
        protected void InsertNewTagValues(Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgDic, DataTable dtEpgTagsValues, ref DataTable dtEpgTags,
            Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs, int nGroupID, int nUpdaterID)
        {
            Dictionary<KeyValuePair<string, int>, int> tagValueWithID = new Dictionary<KeyValuePair<string, int>, int>();
            Dictionary<int, List<string>> dicTagTypeIDAndValues = new Dictionary<int, List<string>>();
            string sConn = "MAIN_CONNECTION_STRING";

            if (dtEpgTagsValues != null && dtEpgTagsValues.Rows != null && dtEpgTagsValues.Rows.Count > 0)
            {
                //insert all New tag values from dtEpgTagsValues to DB
                InsertBulk(dtEpgTagsValues, "EPG_tags", sConn);

                //retrun back all the IDs of the new Tags_Values
                for (int k = 0; k < dtEpgTagsValues.Rows.Count; k++)
                {
                    DataRow row = dtEpgTagsValues.Rows[k];
                    string sTagValue = ODBCWrapper.Utils.GetSafeStr(row, "value");
                    int nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");
                    if (!dicTagTypeIDAndValues.Keys.Contains(nTagTypeID))
                    {
                        dicTagTypeIDAndValues.Add(nTagTypeID, new List<string>() { sTagValue });
                    }
                    else
                    {
                        dicTagTypeIDAndValues[nTagTypeID].Add(sTagValue);
                    }
                }

                DataTable dtTagValueID = EpgDal.Get_EPGTagValueIDs(nGroupID, dicTagTypeIDAndValues);

                //update the IDs in tagValueWithID
                if (dtTagValueID != null && dtTagValueID.Rows != null)
                {
                    for (int i = 0; i < dtTagValueID.Rows.Count; i++)
                    {
                        DataRow row = dtTagValueID.Rows[i];
                        if (row != null)
                        {
                            int nTagValueID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                            string sTagValue = ODBCWrapper.Utils.GetSafeStr(row, "VALUE");
                            int nTagType = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");

                            KeyValuePair<string, int> tagValueAndType = new KeyValuePair<string, int>(sTagValue, nTagType);
                            if (!tagValueWithID.Keys.Contains(tagValueAndType))
                            {
                                tagValueWithID.Add(tagValueAndType, nTagValueID);
                            }
                        }
                    }
                }
            }

            //go over all newTagValueEpgs and update the EPG_Program_Tags
            foreach (KeyValuePair<string, int> kvpUpdated in newTagValueEpgs.Keys)
            {
                int TagValueID = 0;
                List<KeyValuePair<string, int>> tempTagValue = tagValueWithID.Keys.Where(x => x.Key == kvpUpdated.Key && x.Value == kvpUpdated.Value).ToList();
                if (tempTagValue != null && tempTagValue.Count > 0)
                {
                    TagValueID = tagValueWithID[tempTagValue[0]];
                    if (TagValueID > 0)
                    {
                        foreach (string epgGUID in newTagValueEpgs[kvpUpdated])
                        {
                            List<KeyValuePair<string, EpgCB>> lEpgToUpdate = epgDic[epgGUID];
                            foreach (KeyValuePair<string, EpgCB> KVEpgToUpdate in lEpgToUpdate)
                            {
                                EpgCB epgToUpdate = KVEpgToUpdate.Value;
                                FillEpgExtraDataTable(ref dtEpgTags, false, "", epgToUpdate.EpgID, TagValueID, epgToUpdate.GroupID, epgToUpdate.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                            }
                        }
                    }
                }
            }
        }

        // initialize each item with all external_ref  
        private void InitializeMappingFields(DataTable dataTable, DataTable dataTableRef, enums.FieldTypes fieldTypes, ref List<FieldTypeEntity> AllFieldTypeMapping)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                FieldTypeEntity item = new FieldTypeEntity();
                item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                item.Name = ODBCWrapper.Utils.GetSafeStr(dr, "Name");
                item.FieldType = fieldTypes;

                if (fieldTypes != enums.FieldTypes.Basic)
                {
                    foreach (var x in dataTableRef.Select("type = " + (int)fieldTypes + " and field_id = " + item.ID))
                    {

                        if (item.XmlReffName == null)
                        {
                            item.XmlReffName = new List<string>();
                            item.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(x, "external_ref"));
                        }
                        else
                        {
                            item.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(x, "external_ref"));
                        }
                    }
                }

                AllFieldTypeMapping.Add(item);
            }
        }
    }
}
