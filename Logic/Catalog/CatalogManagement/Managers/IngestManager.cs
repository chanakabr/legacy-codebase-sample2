using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Core.Catalog.CatalogManagement
{
    // TODO SHIR - FINISH IngestManager
    public class IngestManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static IngestResponse HandleMediaIngest(int groupId, string xml)
        {
            IngestResponse ingestResponse = new IngestResponse()
            {
                IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() },
                AssetsStatus = new List<IngestAssetStatus>()
            };

            log.DebugFormat("Start HandleMediaIngest. groupId:{0}", groupId);

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.LoadXml(xml);
            }
            catch (XmlException ex)
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.IllegalXml, "XML file with wrong format");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return ingestResponse;
            }
            catch (Exception ex)
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.IllegalXml, "Error while loading file");
                log.ErrorFormat("Failed loading file: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return ingestResponse;
            }
            ApiObjects.
            XmlSerializer ser = new XmlSerializer(typeof(, new Type[] { typeof(StatusWrapper) });
            XmlDocument xd = null;
            using (MemoryStream memStm = new MemoryStream())
            {
                ser.Serialize(memStm, input);
                memStm.Position = 0;
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;

                using (var xtr = XmlReader.Create(memStm, settings))
                {
                    xd = new XmlDocument();
                    xd.Load(xtr);
                }
            }

            try
            {
                // TODO SHIR - ASK LIOR IF NEED THE nParentGroupID
                int parentGroupId = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", groupId, "MAIN_CONNECTION_STRING").ToString());
                if (parentGroupId == 1)
                {
                    parentGroupId = groupId;
                }

                XmlNodeList mediaXmlNodeList = xmlDoc.SelectNodes("/feed/export/media");
                log.DebugFormat("Total medias count : {0}. GID:{1}", mediaXmlNodeList.Count, groupId);
                
                for (int i = 0; i < mediaXmlNodeList.Count; i++)
                {
                    //create ingestAssetStatus for saving Media load data and status
                    IngestAssetStatus ingestAssetStatus = new IngestAssetStatus()
                    {
                        Warnings = new List<Status>(),
                        Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() }
                    };

                    Int32 nMediaID = 0;

                    try
                    {
                        StringBuilder errorMessage = new StringBuilder();
                        bool isActive = false;
                        //---------------------------

                        // get co_guid from xmlNode
                        string sCoGuid = GetItemParameterVal(mediaXmlNodeList[i], "co_guid");
                        if (string.IsNullOrEmpty(sCoGuid))
                        {
                            errorMessage.Append("Missing co_guid | ");
                            ingestAssetStatus.Status.Set((int)eResponseStatus.MissingExternalIdentifier, MISSING_EXTERNAL_IDENTIFIER);
                            log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, sCoGuid, nMediaID, errorMessage.ToString());
                            sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                            ingestResponse.AssetsStatus.Add(ingestAssetStatus);
                            continue;
                        }

                        // TODO SHIR - ASK LIOR WHAT IS THAT
                        //update log topic with media's co guid
                        if (System.ServiceModel.OperationContext.Current != null && System.ServiceModel.OperationContext.Current.IncomingMessageProperties != null)
                        {
                            KlogMonitorHelper.MonitorLogsHelper.SetContext(Constants.TOPIC, string.Format("ingest import co_guid:{0}", sCoGuid));
                        }

                        ingestAssetStatus.ExternalAssetId = sCoGuid;
                        ingestAssetStatus.Status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                        // get entry_id from xmlNode
                        string entryId = GetItemParameterVal(mediaXmlNodeList[i], "entry_id");
                        if (string.IsNullOrEmpty(entryId))
                        {
                            ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MissingEntryId, Message = MISSING_ENTRY_ID });
                        }
                        ingestAssetStatus.EntryID = entryId;

                        // get action from xmlNode
                        string sAction = GetItemParameterVal(mediaXmlNodeList[i], "action").Trim().ToLower();
                        if (string.IsNullOrEmpty(sAction))
                        {
                            ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MissingAction, Message = MISSING_ACTION });
                        }

                        // get is_active from xmlNode
                        string sIsActive = GetItemParameterVal(mediaXmlNodeList[i], "is_active").Trim().ToLower();
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
                                errorMessage.Append("Cant delete. the item is not exist | ");
                                ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MediaIdNotExist, Message = MEDIA_ID_NOT_EXIST });
                                log.Debug("ProcessItem - Action:Delete Error: media not exist");
                                log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, sCoGuid, nMediaID, errorMessage.ToString());
                                sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                                ingestResponse.AssetsStatus.Add(ingestAssetStatus);
                                continue;
                            }
                            //delete media
                            log.DebugFormat("Delete Media:{0}", nMediaID);

                            DeleteMediaPolicy groupDeleteMediaPolicy = GetGroupDeleteMediaPolicy(parentGroupId);
                            // according to groupDeleteMediaPolicy  set the index action
                            action = groupDeleteMediaPolicy == DeleteMediaPolicy.Delete ? eAction.Delete : eAction.Update;

                            DeleteMedia(nGroupID, nMediaID, groupDeleteMediaPolicy);
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

                            if (nItemType == 0 && nMediaID == 0)
                            {
                                errorMessage.Append("Item type not recognized | ");
                                log.DebugFormat("ProcessItem - Item type not recognized. mediaId:{0}", nMediaID);
                                ingestAssetStatus.Status.Set((int)eResponseStatus.InvalidMediaType, string.Format("Invalid media type \"{0}\"", sItemType));
                                log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, sCoGuid, nMediaID, errorMessage.ToString());
                                sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                                ingestResponse.AssetsStatus.Add(ingestAssetStatus);
                                continue;
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

                        isProcess = true;

                        //---------------
                        ingestAssetStatus.Status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                        log.DebugFormat("succeeded export media. CoGuid:{0}, MediaID:{1}, ErrorMessage:{2}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                        sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"OK\" message=\"" + ProtocolsFuncs.XMLEncode(sErrorMessage, true) + "\" tvm_id=\"" + nMediaID.ToString() + "\"/>";

                        // Update record in Catalog (see the flow inside Update Index
                        //change eAction.Delete
                        if (ImporterImpl.UpdateIndex(new List<int>() { nMediaID }, parentGroupId, eAction.Update))
                        {
                            log.DebugFormat("UpdateIndex: Succeeded. CoGuid:{0}, MediaID:{1}, isActive:{2}, ErrorMessage:{3}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                        }
                        else
                        {
                            log.ErrorFormat("UpdateIndex: Failed. CoGuid:{0}, MediaID:{1}, isActive:{2}, ErrorMessage:{3}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                            ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.UpdateIndexFailed, Message = UPDATE_INDEX_FAILED });
                        }

                        // update notification 
                        if (isActive)
                        {
                            UpdateNotificationsRequests(nGroupID, nMediaID);
                        }
                    }
                    catch (Exception exc)
                    {
                        log.ErrorFormat("Failed process MediaID: {0}. Exception:{1}", nMediaID, exc);
                    }

                    ingestResponse.AssetsStatus.Add(ingestAssetStatus);
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

            // TODO SHIR - CHECK IF NEED
            ProcessChannelAssetsFromXml();

            if (isSuccess)
            {
                ingestResponse.IngestStatus.Code = (int)eResponseStatus.OK;
                ingestResponse.IngestStatus.Message = eResponseStatus.OK.ToString();
            }

            ingestResponse = HandleMediaIngestResponse();

            return ingestResponse;
        }

        // TODO SHIR - FINISH ProcessMediaFromXmlNode
        private static void ProcessMediaFromXmlNode(XmlNode assetXmlNode)
        {
        }

        private static void ProcessChannelAssetsFromXml()
        {
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

        static protected string GetItemParameterVal(XmlNode theNode, string sParameterName)
        {
            if (theNode != null)
            {
                XmlAttributeCollection theAttr = theNode.Attributes;
                if (theAttr != null)
                {
                    for (int i = 0; i < theAttr.Count; i++)
                    {
                        string sName = theAttr[i].Name.ToLower();
                        if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                        {
                            return theAttr[i].Value.ToString();
                        }
                    }
                }
            }
            return string.Empty;
        }

        private static IngestResponse HandleMediaIngestResponse(string response, string data, IngestResponse ingestResponse = null)
        {
            if (string.IsNullOrEmpty(response))
            {
                log.Warn("For input " + data + " response is empty");
                return new IngestResponse() { Status = "ERROR" };
            }

            if (ingestResponse == null)
            {
                ingestResponse = new IngestResponse() { IngestStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() } };
            }

            try
            {
                if (ingestResponse.IngestStatus.Code == (int)eResponseStatus.OK)
                {
                    string sImporterResponse = "<importer>" + response + "</importer>";

                    XmlDocument theRes = new XmlDocument();
                    theRes.LoadXml(sImporterResponse);

                    XmlNodeList theItems = theRes.SelectNodes("/importer/media");

                    if (theItems != null && theItems.Count > 0)
                    {
                        XmlNode theNode = theItems[0];

                        ingestResponse.AssetID = GetItemParameterVal(ref theNode, "co_guid");
                        ingestResponse.Description = GetItemParameterVal(ref theNode, "message");
                        ingestResponse.Status = GetItemParameterVal(ref theNode, "status");
                        ingestResponse.TvmID = GetItemParameterVal(ref theNode, "tvm_id");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("For input " + data + " response is " + response, ex);
                return new IngestResponse() { Status = "ERROR" };
            }

            return ingestResponse;
        }
    }
}
