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
using Catalog.Cache;
using TvinciImporter;
using ApiObjects.Epg;

namespace EpgFeeder
{
    public class EpgGenerator
    {
        #region Member
        EpgObject m_Channels;

        protected static readonly int MaxDescriptionSize = 1024;

        #endregion

        public EpgGenerator()
        {
        }


        public string ser(EpgObject oEpg)
        {
            try
            {
                string s  = JsonConvert.SerializeObject(oEpg);
                return s;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public EpgObject Init(string sPath)
        {  
            try
            {
               // string sPath = @"C:\XmlShared\jsonEpgFeeder.txt";
                EpgObject e = null;
                using (StreamReader r = new StreamReader(sPath))
                {
                    string json = r.ReadToEnd();

                    m_Channels = DeserializeJSon<EpgObject>(json);
                }
            }
            catch (Exception ex)
            {

            }
            return m_Channels;
        }


        public static T DeserializeJSon<T>(string data)
        {
            T obj = JsonConvert.DeserializeObject<T>(data);

            return obj;
        }

        public void SaveChannelPrograms()
        {
            try
            {
               // EpgObject m_ChannelsFaild = null; // save all program that got exceptions TODO

                //Delete dates per channel id
                DeleteAllPrograms();

                // get mapping tags and metas 
                List<FieldTypeEntity> FieldEntityMapping = GetMappingFields(m_Channels.ParentGroupID); 

                BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(m_Channels.ParentGroupID);
                string update_epg_package = TVinciShared.WS_Utils.GetTcmConfigValue("update_epg_package");
                int nCountPackage = ODBCWrapper.Utils.GetIntSafeVal(update_epg_package);
                int nCount = 0;
                List<ulong> ulProgram = new List<ulong>();
                //Dictionary<string, EpgCB> epgDic = new Dictionary<string, EpgCB>();
                EpgCB newEpgItem;

                Dictionary<string, EpgCB> dEpgCbTranslate = new Dictionary<string, EpgCB>(); // Language, EpgCB
                Dictionary<string, List<KeyValuePair<string, EpgCB>>> dEpg = new Dictionary<string,List<KeyValuePair<string,EpgCB>>>();// EpgIdentifier , <Language, EpgCB>                
                

                #region each program  create CB objects
                foreach (ProgramObject prog in m_Channels.lProgramObject)
                {
                    newEpgItem = new EpgCB();
                    try
                    {
                        DateTime dProgStartDate = DateTime.MinValue;
                        DateTime dProgEndDate = DateTime.MinValue;
                       
                        int nPicID = 0;
                        string sPicUrl = string.Empty;

                        if (!Utils.ParseEPGStrToDate(prog.StartDate, ref dProgStartDate) || !Utils.ParseEPGStrToDate(prog.EndDate, ref dProgEndDate))
                        {
                            Logger.Logger.Log("Program Dates Error", string.Format("start:{0}, end:{1}", prog.StartDate, prog.EndDate), "EPG");
                            continue;
                        }
                       
                        //need this for both DB and CB!!!!!!!!!
                        //SetMappingValues(FieldEntityMapping, prog);
                                              
                        #region GenerateEpgCB 
                        newEpgItem.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(m_Channels.ChannelId);
                        newEpgItem.GroupID = ODBCWrapper.Utils.GetIntSafeVal(m_Channels.GroupID);
                        newEpgItem.ParentGroupID = m_Channels.ParentGroupID;
                        Guid EPGGuid = Guid.NewGuid();
                        newEpgItem.EpgIdentifier = EPGGuid.ToString();

                        newEpgItem.StartDate = dProgStartDate;
                        newEpgItem.EndDate = dProgEndDate;
                        newEpgItem.UpdateDate = DateTime.UtcNow;
                        newEpgItem.CreateDate = DateTime.UtcNow;
                        newEpgItem.isActive = true;
                        newEpgItem.Status = 1;
                        
                        foreach (KeyValuePair<string, string> name in prog.Name)
                        {
                            newEpgItem.Name = name.Value;
                            dEpgCbTranslate.Add(name.Key, newEpgItem);

                            if (name.Key == m_Channels.MainLangu) // create the urlPic if this is the main language
                            {
                                #region Upload Picture
                                if (prog.Pic != null)
                                {
                                    string imgurl = prog.Pic;

                                    if (!string.IsNullOrEmpty(imgurl))
                                    {
                                        nPicID = ImporterImpl.DownloadEPGPic(imgurl, name.Value, m_Channels.GroupID, 0, m_Channels.ChannelId);//verify this is OK - the epgID is not used in the function itself

                                        if (nPicID != 0)
                                        {
                                            //Update CB, the DB is updated in the end with all other data
                                            sPicUrl = TVinciShared.CouchBaseManipulator.getEpgPicUrl(nPicID);
                                        }
                                    }
                                } 
                                #endregion
                            }
                        }

                        foreach (KeyValuePair<string, string> description in prog.Description)
                        {
                            if (dEpgCbTranslate.ContainsKey(description.Key))
                            {
                                dEpgCbTranslate[description.Key].Description = description.Value;
                            }
                            else
                            {
                                newEpgItem.Description = description.Value;
                                dEpgCbTranslate.Add(description.Key, newEpgItem);
                            }
                        }

                        foreach (KeyValuePair<string, EpgCB> epg in dEpgCbTranslate)
                        {  
                            epg.Value.Metas = GetEpgProgramMetas(prog, epg.Key);
                            epg.Value.Tags = GetEpgProgramTags(prog , epg.Key);

                            //update each epgCB with the picURL +PicID
                            epg.Value.PicID = nPicID;
                            epg.Value.PicUrl = sPicUrl;
                           // epg.Value.Language = epg.Key;  - todo when support languages

                            #endregion

                            if (dEpg.ContainsKey(epg.Value.EpgIdentifier))
                            {
                                dEpg[epg.Value.EpgIdentifier].Add(epg);
                            }
                            else
                            {
                                dEpg.Add(epg.Value.EpgIdentifier, new List<KeyValuePair<string,EpgCB>>(){epg});
                            }                           
                        }
                    }
                    catch (Exception exc)
                    {
                        Logger.Logger.Log("Genarate Epgs", string.Format("Exception in generating EPG name {0} in group: {1}. exception: {2} ", newEpgItem.Name, m_Channels.ParentGroupID, exc.Message), "EpgFeeder");
                    }
                }
                #endregion 
                                
                //insert EPGs to DB in batches
                InsertEpgsDBBatches(ref dEpg, m_Channels.GroupID, nCountPackage, FieldEntityMapping, m_Channels.MainLangu);

                foreach (List<KeyValuePair<string, EpgCB>> lEpg in dEpg.Values)
                {
                    foreach (KeyValuePair<string, EpgCB> epg in lEpg)
                    {
                        nCount++;

                        #region Insert EpgProgram to CB
                        ulong epgID = 0;
                        bool bInsert = oEpgBL.InsertEpg(epg.Value, out epgID);
                        #endregion

                        #region Insert EpgProgram ES

                        if (nCount >= nCountPackage)
                        {
                            ulProgram.Add(epg.Value.EpgID);
                            bool resultEpgIndex = UpdateEpgIndex(ulProgram, m_Channels.ParentGroupID, ApiObjects.eAction.Update);
                            ulProgram = new List<ulong>();
                            nCount = 0;
                        }
                        else
                        {
                            ulProgram.Add(epg.Value.EpgID);
                        }

                        #endregion
                    }
                }

                if (nCount > 0 && ulProgram != null && ulProgram.Count > 0)
                {
                    bool resultEpgIndex = UpdateEpgIndex(ulProgram, m_Channels.ParentGroupID, ApiObjects.eAction.Update);
                }

                //start Upload proccess Queue
                UploadQueue.UploadQueueHelper.SetJobsForUpload(m_Channels.ParentGroupID);
            }


            catch (Exception ex)
            {
                Logger.Logger.Log("SaveChannelPrograms", string.Format("exception={0}", ex.Message), "EpgGenerator");
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

                DataSet ds = EpgDal.GetEpgMappingFields(lSubTree);

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
                return new List<FieldTypeEntity>();
            }
        }


        // TODO: Languages
        private Dictionary<string, List<string>> GetEpgProgramMetas(ProgramObject prog, string language)
        {
            try
            {
                Dictionary<string, List<string>> dMetas = new Dictionary<string, List<string>>();

                foreach (Meta meta in prog.lMetas)
                {
                    foreach (MetaValues value in meta.MetaValues)
                    {
                        foreach (KeyValuePair<string, string> mateTranslate in value.MetaTranslate)
                        {
                            if (mateTranslate.Key == language)
                            {
                                if (dMetas.ContainsKey(meta.MetaType))
                                {
                                    dMetas[meta.MetaType].Add(mateTranslate.Value);
                                }
                                else
                                {
                                    dMetas.Add(meta.MetaType, new List<string>() { mateTranslate.Value });
                                }
                            }
                        }
                    }
                }
                return dMetas;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetEpgProgramMetas", string.Format("faild due ex={0}", ex.Message), "EpgFeeder");
                return new Dictionary<string, List<string>>();
            }
        }


        private Dictionary<string, List<string>> GetEpgProgramTags(ProgramObject prog, string language)
        {
            try
            {
                Dictionary<string, List<string>> dTags = new Dictionary<string, List<string>>();

                foreach (Tag tag in prog.lTags)
                {
                    foreach (TagValues value in tag.TagValues)
                    {
                        foreach (KeyValuePair<string, string> tagTranslate in value.TagTranslate)
                        {
                            if (tagTranslate.Key == language)
                            {
                                if (dTags.ContainsKey(tag.TagType))
                                {
                                    dTags[tag.TagType].Add(tagTranslate.Value);
                                }
                                else
                                {
                                    dTags.Add(tag.TagType, new List<string>() { tagTranslate.Value });
                                }
                            }
                        }
                    }
                }
                return dTags;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("GetEpgProgramMetas", string.Format("faild due ex={0}", ex.Message), "EpgFeeder");
                return new Dictionary<string, List<string>>();
            }
        }

         private bool UpdateEpgIndex(List<ulong> epgIDs, int nGroupID, ApiObjects.eAction action)
        {
            bool result = false;
            try
            {
                result = ImporterImpl.UpdateEpgIndex(epgIDs, nGroupID, action);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("EpgFeeder", string.Format("failed update EpgIndex ex={0}", ex.Message), "EpgFeeder");
                return false;
            }
        }

         private void InsertEpgsDBBatches(ref Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgDic, int groupID, int nCountPackage, List<FieldTypeEntity> FieldEntityMapping, string mainLlanguage)
         {

             Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgBatch = new Dictionary<string, List<KeyValuePair<string, EpgCB>>>();
             Dictionary<int, List<string>> tagsAndValues = new Dictionary<int, List<string>>(); // <tagTypeId, List<tagValues>>
             int nEpgCount = 0;
             try
             {
                 List<KeyValuePair<string, EpgCB>> listEpg;
                 #region future use - main languages
                 //foreach (string sGuid in epgDic.Keys)
                 //{
                 //    // main languages
                 //    bool bMain = false;
                 
                 //    for (int i = 0; i < epgDic[sGuid].Count && !bMain; i++)// run until find main language
                 //    {
                 //        if (epgDic[sGuid][i].Key == mainLlanguage)
                 //        {
                 //            bMain = true;
                 //            EpgCB mainEpg = epgDic[sGuid][i].Value;
                 //            listEpg = new List<KeyValuePair<string,EpgCB>>();
                 //            listEpg.Add(new KeyValuePair<string, EpgCB>(epgDic[sGuid][i].Key, epgDic[sGuid][i].Value));
                 //            epgBatch.Add(sGuid, listEpg);
                 //            nEpgCount++;

                 //            GenerateTagsAndValues(mainEpg, FieldEntityMapping, ref tagsAndValues);
                 //        }
                 //    }
                 //for other languages
                 #endregion
                 foreach (string sGuid in epgDic.Keys)
                 {
                     foreach (KeyValuePair<string, EpgCB> epg in epgDic[sGuid])
                     {
                         listEpg = new List<KeyValuePair<string, EpgCB>>();
                         listEpg.Add(new KeyValuePair<string, EpgCB>(epg.Key, epg.Value));
                         epgBatch.Add(sGuid, listEpg);
                         nEpgCount++;

                         if (nEpgCount >= nCountPackage)
                         {
                             InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
                             nEpgCount = 0;
                             foreach (string guid in epgBatch.Keys)
                             {
                                 foreach (KeyValuePair<string, EpgCB> oEpg in epgBatch[guid])
                                 {
                                     if (oEpg.Value.EpgID > 0)
                                     {
                                         foreach (KeyValuePair<string, EpgCB> item in epgDic[guid])
                                         {
                                             item.Value.EpgID = oEpg.Value.EpgID;
                                         }
                                     }
                                 }                        
                             }
                             epgBatch.Clear();
                             tagsAndValues.Clear();
                         }
                     }
                 }
                 

                 if (nEpgCount > 0 && epgBatch.Keys.Count() > 0)
                 {
                     InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
                     foreach (string guid in epgBatch.Keys)
                     {
                         foreach (KeyValuePair<string, EpgCB> oEpg in epgBatch[guid])
                         {
                             if (oEpg.Value.EpgID > 0)
                             {
                                 foreach (KeyValuePair<string, EpgCB> item in epgDic[guid])
                                 {
                                     item.Value.EpgID = oEpg.Value.EpgID;
                                 }
                             }
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
                     Logger.Logger.Log("UpdateExistingTagValuesPerEPG", string.Format("Missing tag Definition in FieldEntityMapping of tag:{0} in EPG:{1}", tagType, epg.EpgID), "EpgFeeder");
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

         //this FUnction inserts Epgs, thier Metas and tags to DB, and updates the EPGID in the EpgCB object according to the ID of the epg_channels_schedule in the DB
         private void InsertEpgs(int nGroupID, ref Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgDic, List<FieldTypeEntity> FieldEntityMapping, Dictionary<int, List<string>> tagsAndValues)
         {
             try
             {
                 DataTable dtEpgMetas = InitEPGProgramMetaDataTable();
                 //DataTable dtEpgMetasTranslate = InitEPGProgramMetaDataTableTranslate();

                 DataTable dtEpgTags = InitEPGProgramTagsDataTable();
                 DataTable dtEpgTagsValues = InitEPG_Tags_Values();
                // DataTable dtEpgTagsValuesTranslate = InitEPG_Tags_Values_Translate();

                 int nUpdaterID = m_Channels.UpdaterID;
                if (m_Channels.UpdaterID == 0)
                     nUpdaterID = 700;

                 string sConn = "MAIN_CONNECTION_STRING";

                 List<FieldTypeEntity> FieldEntityMappingMetas = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Meta).ToList();
                 List<FieldTypeEntity> FieldEntityMappingTags = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Tag).ToList();

                 Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs = new Dictionary<KeyValuePair<string, int>, List<string>>();// new tag values and the EPGs that have them
                
                 //create dictionary  with key = tagTypeId and value = KeyValuePair<tagvalue, tagValueId> - main language
                 Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue = getTagTypeWithRelevantValues(nGroupID, FieldEntityMappingTags, tagsAndValues);//return relevant tag value ID, if they exist in the DB

                 InsertEPG_Channels_sched(ref epgDic); //for Future use -  main language
                 //TODO: insert to translate table 

                 EpgCB epg;
                 foreach (List<KeyValuePair<string, EpgCB>> lEpg in epgDic.Values)
                 {
                     foreach (KeyValuePair<string, EpgCB> kvEpg in lEpg)
                     {
                         epg = kvEpg.Value;
                         if (epg != null)
                         {
                             //update Metas
                             UpdateMetasPerEPG(ref dtEpgMetas, epg, FieldEntityMappingMetas, nUpdaterID);
                             //update Tags                    
                             UpdateExistingTagValuesPerEPG(epg, FieldEntityMappingTags, ref dtEpgTags, ref dtEpgTagsValues, TagTypeIdWithValue, ref newTagValueEpgs, nUpdaterID);
                         }
                     }
                 }

                 InsertNewTagValues(epgDic, dtEpgTagsValues, ref dtEpgTags, newTagValueEpgs, nGroupID, nUpdaterID);

                 InsertBulk(dtEpgMetas, "EPG_program_metas", sConn); //insert EPG Metas to DB
                 InsertBulk(dtEpgTags, "EPG_program_tags", sConn); //insert EPG Tags to DB
             }
             catch (Exception exc)
             {
                 Logger.Logger.Log("InsertEpgs", string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", nGroupID, exc.Message), "EpgFeeder");
                 return;
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

        private DataTable InitEPGDataTableTranslate()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EPG_ID", typeof(long));           
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("IS_ACTIVE", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(long));
            dt.Columns.Add("UPDATER_ID", typeof(long));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            dt.Columns.Add("PUBLISH_DATE", typeof(DateTime));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            //dt.Columns.Add("EPG_TAG", typeof(string));
            //dt.Columns.Add("FB_OBJECT_ID", typeof(string));
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
            return dt;
        }

        private DataTable InitEPGProgramMetaDataTableTranslate()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("meta_value_id", typeof(long));
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("language_id", typeof(long));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
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
            dt.Columns.Add("epg_tag_id", typeof(string)); // the key to EPG_tags table
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
         * (when support languages this one will be for the main language )
         */
        private void InsertEPG_Channels_sched(ref Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgDic)
        {
            List<KeyValuePair<string, EpgCB>> lEpg;
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

            DataTable dtEPG = InitEPGDataTable();
            FillEPGDataTable(epgDic, ref dtEPG);
            string sConn = "MAIN_CONNECTION_STRING";
            InsertBulk(dtEPG, "epg_channels_schedule", sConn); //insert EPGs to DB

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
                        if (epgDic.TryGetValue(sGuid, out lEpg) && lEpg != null)
                        {
                            foreach (KeyValuePair<string, EpgCB> item in lEpg)
                            {
                                item.Value.EpgID = nEPG_ID;
                            }
                        }                       
                    }
                }
            }
        }


        /*create datatable with all epgCB details - to insert it later to Database*/
        private void FillEPGDataTable(Dictionary<string, List<KeyValuePair<string, EpgCB>>> epgDic, ref DataTable dtEPG)
        {
            if (epgDic != null && epgDic.Count > 0)
            {
                EpgCB epg;
                foreach (List<KeyValuePair<string, EpgCB>> lEpg in epgDic.Values)
                {
                    foreach (KeyValuePair<string, EpgCB> kvEpg in lEpg)
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
                    #region Logging
                    //insert Logs

                    #endregion
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

        /*Build epgMeta datatable to iansert it later to db*/
        private void UpdateMetasPerEPG(ref DataTable dtEpgMetas, EpgCB epg, List<FieldTypeEntity> FieldEntityMappingMetas, int nUpdaterID)
        {
            List<FieldTypeEntity> metaField = new List<FieldTypeEntity>();
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
                        FillEpgExtraDataTable(ref dtEpgMetas, true, sValue, epg.EpgID, nID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                    }
                }
                else
                {   //missing meta definition in DB (in FieldEntityMapping)
                    Logger.Logger.Log("UpdateMetasPerEPG", string.Format("Missing Meta Definition in FieldEntityMapping of Meta:{0} in EPG:{1}", sMetaName, epg.EpgID), "EpgFeeder");
                }
                metaField = null;
            }
        }

        private void FillEpgExtraDataTable(ref DataTable dtEPGExtra, bool bIsMeta, string sValue, ulong nProgID, int nID, int nGroupID, int nStatus,
          int nUpdaterID, DateTime dCreateTime, DateTime dUpdateTime)
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
                    Logger.Logger.Log("UpdateExistingTagValuesPerEPG", string.Format("Missing tag Definition in FieldEntityMapping of tag:{0} in EPG:{1}", sTagName, epg.EpgID), "EpgFeeder");
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

        private void FillEpgTagValueTable(ref DataTable dtEPGTagValue, string sValue, ulong nProgID, int nTagTypeID, int nGroupID, int nStatus,int nUpdaterID, 
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
                            List<KeyValuePair<string,EpgCB>> lEpgToUpdate = epgDic[epgGUID];
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


        //delete all programs by dateTime 
        private void DeleteAllPrograms()
        {
            Dictionary<DateTime, bool> deletedChannelDates = new Dictionary<DateTime, bool>();
            DateTime dProgStartDate = DateTime.MinValue;
            DateTime dProgEndDate = DateTime.MinValue;

            #region Delete all existing programs in DB that have start/end dates within the new schedule
            foreach (ProgramObject progItem in m_Channels.lProgramObject)
            {
                dProgStartDate = DateTime.MinValue;
                dProgEndDate = DateTime.MinValue;

                if (!Utils.ParseEPGStrToDate(progItem.StartDate, ref dProgStartDate) || !Utils.ParseEPGStrToDate(progItem.EndDate, ref dProgEndDate))
                {
                    continue;
                }

                if (dProgStartDate.Date.Equals(dProgEndDate.Date) && !deletedChannelDates.ContainsKey(dProgStartDate.Date))
                {
                    deletedChannelDates.Add(dProgStartDate.Date, true);
                }
            }

            foreach (DateTime progStartDate in deletedChannelDates.Keys)
            {
                Logger.Logger.Log("Delete Program on Date", string.Format("Group ID = {0}; Deleting Programs on Date {1} that belong to channel {2}", m_Channels.ParentGroupID, progStartDate, m_Channels.ChannelId), "EpgFeeder");
                Tvinci.Core.DAL.EpgDal.DeleteProgramsOnDate(progStartDate, m_Channels.GroupID.ToString(), m_Channels.ChannelId);
            }
            #endregion

            #region Delete all existing programs in CB that have start/end dates within the new schedule
            int nParentGroupID = 0;
            if (m_Channels.ParentGroupID > 0)
            {
                nParentGroupID = m_Channels.ParentGroupID;
            }
            else
            {
                nParentGroupID = DAL.UtilsDal.GetParentGroupID(m_Channels.GroupID);
            }
            BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nParentGroupID);

            List<DateTime> lDates = new List<DateTime>();
            dProgStartDate = DateTime.MinValue;
            foreach (ProgramObject progItem in m_Channels.lProgramObject)
            {
                Utils.ParseEPGStrToDate(progItem.StartDate, ref dProgStartDate);
                if (!lDates.Contains(dProgStartDate.Date))
                {
                    lDates.Add(dProgStartDate.Date);
                }
            }

            Logger.Logger.Log("Delete Program on Date", string.Format("Group ID = {0}; Deleting Programs  that belong to channel {1}", m_Channels.ParentGroupID, m_Channels.ChannelId), "EpgFeeder");

            oEpgBL.RemoveGroupPrograms(lDates, m_Channels.ChannelId);
            #endregion

            #region Delete all existing programs in ES that have start/end dates within the new schedule
            bool resDelete = Utils.DeleteEPGDocFromES(m_Channels.ParentGroupID.ToString(), m_Channels.ChannelId, lDates);
            #endregion
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
