using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.Response;
using EpgBL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Tvinci.Core.DAL;
using TvinciImporter;
using TVinciShared;

namespace EpgIngest
{
    public class Ingest
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string EPGS_PROGRAM_DATES_ERROR = "Error at EPG Program Start/End Dates";
        private const string EPGS_PROGRAM_MISSING_CRID = "Warning at EPG Program crid is empty {0}";
        private const string FAILED_DOWNLOAD_PIC = "Failed download pic";



        #region Member
        EpgChannels m_Channels;
        BaseEpgBL oEpgBL;
        List<LanguageObj> lLanguage = new List<LanguageObj>();
        List<FieldTypeEntity> FieldEntityMapping = new List<FieldTypeEntity>();
        Dictionary<int, string> ratios;
        string update_epg_package;
        int nCountPackage;
        bool isTstvSettings = false;

        #endregion

        public Ingest()
        {
        }

        public bool Initialize(string data, int groupId)
        {
            IngestResponse ingestResponse = null;
            return Initialize(data, groupId, out  ingestResponse);
        }

        public bool Initialize(string Data, int groupId, out IngestResponse ingestResponse)
        {
            ingestResponse = new IngestResponse()
            {
                IngestStatus = new ApiObjects.Response.Status() { Code = (int)ApiObjects.Response.eResponseStatus.Error, Message = ApiObjects.Response.eResponseStatus.Error.ToString() },
                AssetsStatus = new List<IngestAssetStatus>()
            };

            m_Channels = SerializeEpgChannel(Data);
            if (m_Channels == null)
            {
                ingestResponse.IngestStatus.Code = (int)ApiObjects.Response.eResponseStatus.IllegalXml;
                ingestResponse.IngestStatus.Message = "Error while loading data";
                log.ErrorFormat("Failed loading data: {0}. GID:{1}.", Data, groupId);
                return false;
            }

            oEpgBL = EpgBL.Utils.GetInstance(m_Channels.parentgroupid);
            lLanguage = Utils.GetLanguages(m_Channels.parentgroupid); // dictionary contains all language ids and its  code (string)
            // get mapping tags and metas 
            FieldEntityMapping = Utils.GetMappingFields(m_Channels.parentgroupid);
            update_epg_package = TVinciShared.WS_Utils.GetTcmConfigValue("update_epg_package");
            nCountPackage = ODBCWrapper.Utils.GetIntSafeVal(update_epg_package);
            // get mapping between ratio_id and ratio 
            Dictionary<string, string> sRatios = EpgDal.Get_PicsEpgRatios();
            ratios = sRatios.ToDictionary(x => int.Parse(x.Key), x => x.Value);

            DataRow dr = DAL.ApiDAL.GetTimeShiftedTvPartnerSettings(m_Channels.parentgroupid);
             if (dr != null)
             {
                 isTstvSettings = true;
             }
            return true;
        }

        private EpgChannels SerializeEpgChannel(string Data)
        {
            EpgChannels epgchannel = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(EpgChannels));
                XmlReaderSettings settings = new XmlReaderSettings();
                // No settings need modifying here
                using (StringReader textReader = new StringReader(Data))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                    {
                        epgchannel = (EpgChannels)ser.Deserialize(xmlReader);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SerializeEpgChannel", ex);
                epgchannel = null;
            }
            return epgchannel;
        }

        public string SaveChannelPrograms()
        {
            IngestResponse ingestResponse = null;
            return SaveChannelPrograms(ref ingestResponse);
        }
        public string SaveChannelPrograms(ref IngestResponse ingestResponse)
        {
            bool success = true;
            DateTime dPublishDate = DateTime.UtcNow; // this publish date will insert to each epg that was update / insert 

            if (ingestResponse == null)
            {
                ingestResponse = new IngestResponse()
                {
                    IngestStatus = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = eResponseStatus.Error.ToString()
                    },
                    AssetsStatus = new List<IngestAssetStatus>()
                };
            }
            try
            {
                int kalturaChannelID;
                string channelID;
                EpgChannelType epgChannelType;

                // get the kaltura + type to each channel by its external id                
                List<string> channelExternalIds = m_Channels.channel.Select(x => x.id).ToList<string>();
                Dictionary<string, List<EpgChannelObj>> epgChannelDict = EpgDal.GetAllEpgChannelsDic(m_Channels.groupid, channelExternalIds);

                //Run for each channel - 
                foreach (KeyValuePair<string, List<EpgChannelObj>> channel in epgChannelDict)
                {
                    // get all programs related to specific channel 
                    List<programme> programs = m_Channels.programme.Where(x => x.channel == channel.Key).ToList();
                    log.DebugFormat("Going to save {0} programs for channel: {1}", programs.Count, channel.Key);

                    foreach (EpgChannelObj epgChannelObj in channel.Value)
                    {
                        kalturaChannelID = epgChannelObj.ChannelId;
                        channelID = channel.Key;
                        epgChannelType = epgChannelObj.ChannelType;

                        bool returnSuccess = SaveChannelPrograms(programs, kalturaChannelID, channelID, epgChannelType, ref ingestResponse, dPublishDate);
                        if (success)
                        {
                            success = returnSuccess;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SaveChannelPrograms - " + string.Format("exception={0}", ex.Message), ex);
                success = false;
            }

            if (success)
            {
                ingestResponse.IngestStatus.Code = (int)eResponseStatus.OK;
                ingestResponse.IngestStatus.Message = eResponseStatus.OK.ToString();
            }
            return success.ToString();
        }

        private bool SaveChannelPrograms(List<programme> programs, int kalturaChannelID, string channelID, EpgChannelType epgChannelType, ref IngestResponse ingestResponse, DateTime dPublishDate)
        {
            // EpgObject m_ChannelsFaild = null; // save all program that got exceptions TODO ????????            
            bool success = false;
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

            //update log topic with kaltura channelID
            OperationContext.Current.IncomingMessageProperties[Constants.TOPIC] = string.Format("save channel programs for kalturaChannelID:{0}", kalturaChannelID);

            var languages = GroupsCacheManager.GroupsCache.Instance().GetGroup(m_Channels.groupid).GetLangauges();

            #region each program  create CB objects                        
            List<DateTime> deletedDays = new List<DateTime>();
            IngestAssetStatus ingestAssetStatus = null;

            foreach (programme prog in programs)
            {
                ingestAssetStatus = new IngestAssetStatus()
                {
                    Warnings = new List<Status>(),
                    Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() },
                    InternalAssetId = kalturaChannelID,
                    ExternalAssetId = prog.external_id
                };
                ingestResponse.AssetsStatus.Add(ingestAssetStatus);

                newEpgItem = new EpgCB();
                dEpgCbTranslate = new Dictionary<string, EpgCB>(); // Language, EpgCB
                try
                {
                    dProgStartDate = DateTime.MinValue;
                    dProgEndDate = DateTime.MinValue;
                    epgPicture = new EpgPicture();

                    nPicID = 0;
                    string sPicUrl = string.Empty;
                    if (isTstvSettings && string.IsNullOrEmpty(prog.crid))
                    {
                        log.DebugFormat("crid is empty for external id {0}: ", prog.external_id);
                        ingestAssetStatus.Warnings.Add(new Status((int)IngestWarnings.EPGSProgramMissingCrid, string.Format(EPGS_PROGRAM_MISSING_CRID, prog.external_id)));
                    }

                    if (!Utils.ParseEPGStrToDate(prog.start, ref dProgStartDate) || !Utils.ParseEPGStrToDate(prog.stop, ref dProgEndDate))
                    {
                        log.Error("Program Dates Error - " + string.Format("start:{0}, end:{1}", prog.start, prog.stop));
                        ingestAssetStatus.Status.Code = (int)eResponseStatus.EPGSProgramDatesError;
                        ingestAssetStatus.Status.Message = EPGS_PROGRAM_DATES_ERROR;
                        continue;
                    }

                    ingestAssetStatus.Status.Code = (int)eResponseStatus.OK;
                    ingestAssetStatus.Status.Message = eResponseStatus.OK.ToString();

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
                    newEpgItem.EnableCatchUp = EnableLinearSetting(prog.enablecatchup);
                    newEpgItem.EnableCDVR = EnableLinearSetting(prog.enablecdvr);
                    newEpgItem.EnableStartOver = EnableLinearSetting(prog.enablestartover);
                    newEpgItem.EnableTrickPlay = EnableLinearSetting(prog.enabletrickplay);
                    newEpgItem.Crid = prog.crid;

                    string picName = string.Empty;
                    #region Name  With languages
                    foreach (title name in prog.title)
                    {
                        EpgCB newEpgItemLang = new EpgCB(newEpgItem);
                        language = name.lang.ToLower();
                        newEpgItemLang.Name = name.Value;
                        dEpgCbTranslate.Add(language, newEpgItemLang);

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
                                EpgCB newEpgItemLang = new EpgCB(newEpgItem);
                                newEpgItemLang.Description = description.Value;
                                dEpgCbTranslate.Add(language, newEpgItemLang);
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
                                    object baseURl = ODBCWrapper.Utils.GetTableSingleVal("epg_pics", "BASE_URL", nPicID);
                                    if (baseURl != null && baseURl != DBNull.Value)
                                        sPicUrl = baseURl.ToString();
                                }
                                else
                                {
                                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.FailedDownloadPic, Message = FAILED_DOWNLOAD_PIC });
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

                    EpgCB mainLanguageEpgCB = null;

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

                        if (epg.Value.Language.ToLower() == m_Channels.mainlang)
                        {
                            mainLanguageEpgCB = epg.Value;
                        }
                    }
                    #endregion

                    // Complete all languages that do not exist with copies of the main language
                    foreach (var currentLanguage in languages)
                    {
                        if (!dEpgCbTranslate.ContainsKey(currentLanguage.Code))
                        {
                            var cloneEpg = new EpgCB(mainLanguageEpgCB);
                            var clonePair = new KeyValuePair<string, EpgCB>(currentLanguage.Code, cloneEpg);
                            dEpgCbTranslate.Add(currentLanguage.Code, cloneEpg);

                            if (dEpg.ContainsKey(mainLanguageEpgCB.EpgIdentifier))
                            {
                                dEpg[mainLanguageEpgCB.EpgIdentifier].Add(clonePair);
                            }
                            else
                            {
                                dEpg.Add(mainLanguageEpgCB.EpgIdentifier, new List<KeyValuePair<string, EpgCB>>() { clonePair });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    log.Error("Genarate Epgs - " + string.Format("Exception in generating EPG name {0} in group: {1}. exception: {2} ", newEpgItem.Name, m_Channels.parentgroupid, ex.Message), ex);
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

            if (lProgramsID != null && lProgramsID.Count > 0)
            {
                List<string> docIds = Utils.BuildDocIdsToRemoveGroupPrograms(lProgramsID, lLanguage);
                oEpgBL.RemoveGroupPrograms(docIds);
                List<ulong> programIds = lProgramsID.Select(i => (ulong)i).ToList();
                bool resultEpgIndex = UpdateEpgIndex(programIds, m_Channels.parentgroupid, ApiObjects.eAction.Delete);
                if (resultEpgIndex)
                    log.DebugFormat("Succeeded. delete programIds:[{0}], kalturaChannelID:{1}", string.Join(",", programIds), kalturaChannelID);
                else
                    log.ErrorFormat("Error. delete programIds:[{0}], kalturaChannelID:{1}", string.Join(",", programIds), kalturaChannelID);
            }

            foreach (List<KeyValuePair<string, EpgCB>> lEpg in dEpg.Values)
            {
                foreach (KeyValuePair<string, EpgCB> epg in lEpg)
                {
                    nCount++;

                    #region Insert EpgProgram to CB

                    string epgID = string.Empty;
                    bool isMainLang = epg.Value.Language.ToLower() == m_Channels.mainlang.ToLower() ? true : false;
                    if (epg.Value.EpgID <= 0)
                    {
                        log.ErrorFormat("epgId is {0} for epg with identifier {1} and coguid {2}", epg.Value.EpgID, epg.Value.EpgIdentifier, epg.Value.CoGuid);
                    }
                    bool bInsert = oEpgBL.InsertEpg(epg.Value, isMainLang, out epgID);
                    if (bInsert)
                        log.DebugFormat("Succeeded. insert EpgProgram to CB. EpgID:{0}, EpgIdentifier:{1}, CoGuid {2}", epg.Value.EpgID, epg.Value.EpgIdentifier, epg.Value.CoGuid);
                    else
                        log.ErrorFormat("Error. insert EpgProgram to CB. EpgID:{0}, EpgIdentifier:{1}, CoGuid {2}", epg.Value.EpgID, epg.Value.EpgIdentifier, epg.Value.CoGuid);

                    #endregion

                    #region Insert EpgProgram ES

                    if (nCount >= nCountPackage)
                    {
                        if (!epgIds.Contains(epg.Value.EpgID))
                        {
                            epgIds.Add(epg.Value.EpgID);
                        }
                        bool resultEpgIndex = UpdateEpgIndex(epgIds, m_Channels.parentgroupid, ApiObjects.eAction.Update);
                        if (resultEpgIndex)
                            log.DebugFormat("Succeeded. insert EpgProgram to ES. EpgID:{0}, EpgIdentifier:{1}, CoGuid {2}", epg.Value.EpgID, epg.Value.EpgIdentifier, epg.Value.CoGuid);
                        else
                            log.ErrorFormat("Error. insert EpgProgram to ES. EpgID:{0}, EpgIdentifier:{1}, CoGuid {2}", epg.Value.EpgID, epg.Value.EpgIdentifier, epg.Value.CoGuid);

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
                if (resultEpgIndex)
                    log.DebugFormat("Succeeded. insert EpgProgram to ES. parent GID:{0}, epgIds Count:{1}", m_Channels.parentgroupid, epgIds.Count);
                else
                    log.ErrorFormat("Error. insert EpgProgram to ES. parent GID:{0}, epgIds Count:{1}", m_Channels.parentgroupid, epgIds.Count);
            }

            //start Upload process Queue
            UploadQueue.UploadQueueHelper.SetJobsForUpload(m_Channels.parentgroupid);

            success = true;
            return success;
        }

        private int EnableLinearSetting(string enable)
        {
            try
            {
                if (string.IsNullOrEmpty(enable))
                    return 0; // 0 == none
                if (enable == "false" || enable == "2")
                    return 2;
                if (enable == "true" || enable == "1")
                    return 1;
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private Dictionary<string, List<string>> GetEpgProgramMetas(programme prog, string language)
        {
            try
            {
                FieldTypeEntity metaMapping = null;
                bool checkReg = false;
                Regex rgx = null;
                Dictionary<string, List<string>> dEpgMetas = new Dictionary<string, List<string>>();
                if (prog.metas != null)
                {
                    foreach (metas meta in prog.metas)
                    {
                        checkReg = false;
                        rgx = null;

                        // get the regex expression to alias 
                        metaMapping = FieldEntityMapping.Where(x => x.FieldType == FieldTypes.Meta && x.Name.ToLower() == meta.MetaType).FirstOrDefault();
                        if (metaMapping != null && !string.IsNullOrEmpty(metaMapping.RegexExpression))
                        {
                            checkReg = true;
                            rgx = new Regex(metaMapping.RegexExpression);
                        }

                        // get all relevant language 
                        List<MetaValues> metaValues = meta.MetaValues.Where(x => x.lang.ToLower() == language).ToList();
                        foreach (MetaValues value in metaValues)
                        {
                            if (checkReg)
                            {
                                if (!rgx.IsMatch(value.Value))
                                {
                                    // write to log and leave this meta out 
                                    log.ErrorFormat("GetEpgProgramMetas-fail the regex expression to metatype={0}, metaValue={1}, regexExpression={2}, external_id ={3} ",
                                        meta.MetaType, value.Value, metaMapping.RegexExpression, prog.external_id);
                                    continue;
                                }
                            }
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
                FieldTypeEntity tagMapping = null;
                bool checkReg = false;
                Regex rgx = null;
                Dictionary<string, List<string>> dEpgTags = new Dictionary<string, List<string>>();
                if (prog.tags != null)
                {
                    foreach (tags tag in prog.tags)
                    {
                        checkReg = false;
                        rgx = null;
                        // get the regex expression to alias 
                        tagMapping = FieldEntityMapping.Where(x => x.FieldType == FieldTypes.Tag && x.Name.ToLower() == tag.TagType).FirstOrDefault();
                        if (tagMapping != null && !string.IsNullOrEmpty(tagMapping.RegexExpression))
                        {
                            checkReg = true;
                            rgx = new Regex(tagMapping.RegexExpression);
                        }

                        List<TagValues> tagValues = tag.TagValues.Where(x => x.lang.ToLower() == language).ToList();
                        foreach (TagValues value in tagValues)
                        {
                            if (checkReg)
                            {
                                if (!rgx.IsMatch(value.Value))
                                {
                                    // write to log and leave this meta out 
                                    log.ErrorFormat("GetEpgProgramTags-fail the regex expression to metatype={0}, metaValue={1}, regexExpression={2}, external_id ={3} ",
                                        tag.TagType, value.Value, tagMapping.RegexExpression, prog.external_id);
                                    continue;
                                }
                            }

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
                log.Error("GetEpgProgramMetas - " + string.Format("faild due ex={0}", ex.Message), ex);
                return new Dictionary<string, List<string>>();
            }
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

                                removeLang.Clear();
                            }
                        }

                        if (epgIdsToUpdate.Count > 0)
                        {
                            // update the epgs with publish date 
                            bool bUpdate = EpgDal.UpdateEpgChannelSchedulePublishDate(epgIdsToUpdate, dPublishDate);

                            //Log
                            log.DebugFormat("Programs Ids with no changes: {0}", string.Join(",", epgIdsToUpdate));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("KDG - " + string.Format("fail to UpdateEpgDic ex = {0}", ex.Message), ex);
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
                                epgDic[guid].ForEach(i => i.Value.EpgID = epgBatch[guid].EpgID); // update all languages per EpgIdentifier with epgID                                                
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
                            epgDic[guid].ForEach(i => i.Value.EpgID = epgBatch[guid].EpgID);// update all languages per EpgIdentifier with epgID     
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertEpgsDBBatches - " + string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", groupID, ex.Message), ex);
                return;
            }
        }

        private void InsertEpgs(int groupID, ref Dictionary<string, EpgCB> epgDic, List<FieldTypeEntity> FieldEntityMapping, Dictionary<int, List<string>> tagsAndValues, DateTime dPublishDate, int channelID, Dictionary<int, string> ratios)
        {
            try
            {
                DataTable dtEpgMetas = InitTables.InitEPGProgramMetaDataTable();
                DataTable dtEpgTags = InitTables.InitEPGProgramTagsDataTable();
                DataTable dtEpgTagsValues = InitTables.InitEPG_Tags_Values();
                DataTable dtEpgPictures = InitTables.InitEPGProgramPicturesDataTable();

                int nUpdaterID = m_Channels.updaterid;
                if (m_Channels.updaterid == 0)
                    nUpdaterID = 700;

                string sConn = "MAIN_CONNECTION_STRING";

                List<FieldTypeEntity> FieldEntityMappingMetas = FieldEntityMapping.Where(x => x.FieldType == FieldTypes.Meta).ToList();
                List<FieldTypeEntity> FieldEntityMappingTags = FieldEntityMapping.Where(x => x.FieldType == FieldTypes.Tag).ToList();

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
                        InitTables.FillEpgPictureTable(ref dtEpgPictures, epg, ratios);
                    }
                }

                // batch insert 

                InsertNewTagValues(epgDic, dtEpgTagsValues, ref dtEpgTags, newTagValueEpgs, groupID, nUpdaterID);

                // delete all values per tag and meta for programIDS that exsits 
                bool bDelete = EpgDal.DeleteEpgProgramDetails(epgIDs, groupID);
                bool bDeletePictures = EpgDal.DeleteEpgProgramPicturess(epgIDs, groupID, channelID);

                Utils.InsertBulk(dtEpgMetas, "EPG_program_metas", sConn); //insert EPG Metas to DB
                Utils.InsertBulk(dtEpgTags, "EPG_program_tags", sConn); //insert EPG Tags to DB
                Utils.InsertBulk(dtEpgPictures, "epg_multi_pictures", sConn);//insert Multi epg pictures to DB
            }
            catch (Exception ex)
            {
                log.Error("InsertEpgs - " + string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", groupID, ex.Message), ex);
                return;
            }
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
                    DataTable dtEPG = InitTables.InitEPGDataTableWithID();
                    InitTables.FillEPGDataTable(updateEpgDic, ref dtEPG, dPublishDate);
                    bool bUpdated = EpgDal.UpdateEpgChannelSchedule(dtEPG);
                }
            }
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
            DataTable dtEPG = InitTables.InitEPGDataTable();

            foreach (KeyValuePair<string, EpgCB> kv in epgDic)
            {
                if (kv.Value != null && kv.Value.EpgID == 0)
                {
                    insertEpgDic.Add(kv.Key, kv.Value);
                }
            }
            InitTables.FillEPGDataTable(insertEpgDic, ref dtEPG, dPublishDate);
            string sConn = "MAIN_CONNECTION_STRING";
            Utils.InsertBulk(dtEPG, "epg_channels_schedule", sConn); //insert EPGs to DB

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
                        InitTables.FillEpgExtraDataTable(ref dtEpgMetas, true, sValue, epg.EpgID, nID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow, oLanguage.ID);
                    }
                }
                else
                {   //missing meta definition in DB (in FieldEntityMapping)
                    log.Debug("UpdateMetasPerEPG - " + string.Format("Missing Meta Definition in FieldEntityMapping of Meta:{0} in EPG:{1}", sMetaName, epg.EpgID));
                }
                metaField = null;
            }
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
                            List<KeyValuePair<string, int>> list = TagTypeIdWithValue[nTagTypeID].Where(x => x.Key.ToLower() == sTagValue.ToLower()).ToList();
                            if (list != null && list.Count > 0)
                            {
                                //Insert New EPG Tag Value in EPG_Program_Tags, we are assuming this tag value was not assigned to the program because the program is new                                                    
                                InitTables.FillEpgExtraDataTable(ref dtEpgTags, false, "", epg.EpgID, list[0].Value, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
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
                                    InitTables.FillEpgTagValueTable(ref dtEpgTagsValues, sTagValue, epg.EpgID, nTagTypeID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
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
                                InitTables.FillEpgTagValueTable(ref dtEpgTagsValues, sTagValue, epg.EpgID, nTagTypeID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
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

        private void InsertNewTagValues(Dictionary<string, EpgCB> epgDic, DataTable dtEpgTagsValues, ref DataTable dtEpgTags, Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs, int groupID, int nUpdaterID)
        {
            Dictionary<KeyValuePair<string, int>, int> tagValueWithID = new Dictionary<KeyValuePair<string, int>, int>();
            Dictionary<int, List<string>> dicTagTypeIDAndValues = new Dictionary<int, List<string>>();
            string sConn = "MAIN_CONNECTION_STRING";

            if (dtEpgTagsValues != null && dtEpgTagsValues.Rows != null && dtEpgTagsValues.Rows.Count > 0)
            {
                //insert all New tag values from dtEpgTagsValues to DB
                Utils.InsertBulk(dtEpgTagsValues, "EPG_tags", sConn);

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
                            InitTables.FillEpgExtraDataTable(ref dtEpgTags, false, "", epgToUpdate.EpgID, TagValueID, epgToUpdate.GroupID, epgToUpdate.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                        }
                    }
                }
            }
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
                log.Error(string.Format("fail to DeleteEpgs from DB ex = {0}", ex.Message), ex);
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

    }
}
