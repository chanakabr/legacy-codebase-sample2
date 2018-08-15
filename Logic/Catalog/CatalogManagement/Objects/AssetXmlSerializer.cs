using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using TVinciShared;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using ApiObjects;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.Catalog;

namespace Core.Catalog.CatalogManagement
{
    public class AssetXmlSerializer
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

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

        // TODO SHIR - ASK IRA FOR THE USER_ID
        private const long USER_ID = 999;
        private const string DELETE_ACTION = "delete";
        private const string INSERT_ACTION = "insert";
        private const string UPDATE_ACTION = "update";

        #endregion

        #region Classes

        [XmlRoot("feed")]
        public class Feed
        {
            [XmlElement("export")]
            public Export Export { get; set; }
        }

        public class Export
        {
            [XmlElement("media")]
            public List<Media> MediaList { get; set; }
        }

        public class Media
        {
            [XmlAttribute("co_guid")]
            public string CoGuid { get; set; }

            [XmlAttribute("entry_id")]
            public string EntryId { get; set; }

            [XmlAttribute("action")]
            public string Action { get; set; }

            [XmlAttribute("is_active")]
            public string IsActive { get; set; }

            [XmlAttribute("erase")]
            public string Erase { get; set; }
            
            [XmlElement("basic")]
            public Basic Basic { get; set; }

            [XmlElement("structure")]
            public Structure Structure { get; set; }

            [XmlElement("files")]
            public Files Files { get; set; }

            public Media()
            {
                this.Action = "insert";
                this.IsActive = "true";
                this.Erase = "false";
            }
        }

        public class Basic
        {
            [XmlElement("media_type")]
            public string MediaType { get; set; }

            [XmlElement("name")]
            public Multilingual Name { get; set; }

            [XmlElement("description")]
            public Multilingual Description { get; set; }

            [XmlElement("thumb")]
            public Thumb Thumb { get; set; }

            [XmlElement("pic_ratios")]
            public PicsRatio PicsRatio { get; set; }

            [XmlElement("rules")]
            public Rules Rules { get; set; }

            [XmlElement("dates")]
            public Dates Dates { get; set; }
        }

        public class Multilingual
        {
            [XmlElement("value")]
            public List<LanguageValue> Values { get; set; }
        }
        
        public class Thumb
        {
            [XmlAttribute("url")]
            public string Url { get; set; }
        }

        public class PicsRatio
        {
            [XmlElement("ratio")]
            public List<Ratio> Ratios { get; set; }
        }

        public class Ratio
        {
            [XmlAttribute("thumb")]
            public string Thumb { get; set; }

            [XmlAttribute("ratio")]
            public string RatioText { get; set; }
        }

        public class Rules
        {
            [XmlElement("watch_per_rule")]
            public string WatchPerRule { get; set; }

            [XmlElement("geo_block_rule")]
            public string GeoBlockRule { get; set; }

            [XmlElement("device_rule")]
            public string DeviceRule { get; set; }
        }

        public class Dates
        {
            [XmlElement("catalog_start")]
            public string CatalogStart { get; set; }

            [XmlElement("start")]
            public string Start { get; set; }
            
            [XmlElement("catalog_end")]
            public string CatalogEnd { get; set; }

            [XmlElement("end")]
            public string End { get; set; }
        }

        public class LanguageValue
        {
            [XmlAttribute("lang")]
            public string LangCode { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        public class Structure
        {
            [XmlElement("booleans")]
            public SlimMetas Booleans { get; set; }

            [XmlElement("doubles")]
            public SlimMetas Doubles { get; set; }
            
            [XmlElement("dates")]
            public SlimMetas Dates { get; set; }

            [XmlElement("strings")]
            public Strings Strings { get; set; }

            [XmlElement("metas")]
            public Metas Metas { get; set; }
        }

        public class BaseMeta
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("ml_handling")]
            public string MlHandling { get; set; }

            public BaseMeta()
            {
                this.MlHandling = "unique";
            }
        }

        public class SlimMetas
        {
            [XmlElement("meta")]
            public List<SlimMeta> Metas { get; set; }
        }
        
        public class SlimMeta : BaseMeta
        {
            [XmlText]
            public string Value { get; set; }
        }

        public class Strings
        {
            [XmlElement("meta")]
            public List<Meta> MetaStrings { get; set; }
        }
        
        public class Meta : BaseMeta
        {
            [XmlElement("value")]
            public List<LanguageValue> Values { get; set; }
        }

        public class Metas
        {
            [XmlElement("meta")]
            public List<MetaTag> MetaTags { get; set; }
        }

        public class MetaTag : BaseMeta
        {
            [XmlElement("container")]
            public List<Multilingual> Containers { get; set; }
        }
        
        public class Files
        {
            [XmlElement("file")]
            public List<MediaFile> MediaFiles { get; set; }
        }

        public class MediaFile
        {
            [XmlAttribute("type")]
            public string Type { get; set; }

            [XmlAttribute("assetDuration")]
            public string AssetDuration { get; set; }

            [XmlAttribute("quality")]
            public string Quality { get; set; }

            [XmlAttribute("handling_type")]
            public string HandlingType { get; set; }

            [XmlAttribute("cdn_name")]
            public string CdnName { get; set; }

            [XmlAttribute("cdn_code")]
            public string CdnCode { get; set; }

            [XmlAttribute("alt_cdn_code")]
            public string AltCdnCode { get; set; }

            [XmlAttribute("co_guid")]
            public string CoGuid { get; set; }

            [XmlAttribute("billing_type")]
            public string BillingType { get; set; }

            [XmlAttribute("PPV_MODULE")]
            public string PpvModule { get; set; }

            [XmlAttribute("product_code")]
            public string ProductCode { get; set; }

            public MediaFile()
            {
                this.Quality = "HIGH";
            }
        }

        #endregion
        
        public static List<MediaAsset> ConvertToMediaAssets(string xml, int groupId, out IngestResponse ingestResponse)
        {
            List<MediaAsset> mediaAssets = new List<MediaAsset>();
            ingestResponse = new IngestResponse()
            {
                IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() },
                AssetsStatus = new List<IngestAssetStatus>()
            };

            Feed feed = DeserializeXmlToFeed(xml, groupId, ref ingestResponse);
            if (feed == null || feed.Export == null || feed.Export.MediaList == null || feed.Export.MediaList.Count == 0 || 
                ingestResponse.IngestStatus.Code == (int)eResponseStatus.IllegalXml)
            {
                return mediaAssets;
            }

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAsset", groupId);
                return mediaAssets;
            }

            string mainLanguageName = GetMainLanguageName(groupId);
            if (string.IsNullOrEmpty(mainLanguageName))
            {
                // TODO SHIR - SET SOME ERROR
            }

            for (int i = 0; i < feed.Export.MediaList.Count; i++)
            {
                Media media = feed.Export.MediaList[i];
                IngestAssetStatus ingestAssetStatus = new IngestAssetStatus()
                {
                    Warnings = new List<Status>(),
                    Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() }
                };

                // check media.CoGuid
                if (string.IsNullOrEmpty(media.CoGuid))
                {
                    //errorMessage.Append("Missing co_guid | ");
                    ingestAssetStatus.Status.Set((int)eResponseStatus.MissingExternalIdentifier, MISSING_EXTERNAL_IDENTIFIER);
                    //log.ErrorFormat("Error import mediaIndex{0}, ErrorMessage:{1}", mediaIndex, errorMessage.ToString());
                    //sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                    ingestResponse.AssetsStatus.Add(ingestAssetStatus);
                    continue;
                }

                try
                {
                    if (ValidateMedia(i, media, groupId, catalogGroupCache, ref ingestAssetStatus))
                    {
                        DateTime startDate = GetDateTimeFromString(media.Basic.Dates.Start, DateTime.UtcNow);
                        DateTime endDate = GetDateTimeFromString(media.Basic.Dates.CatalogEnd, new DateTime(2099, 1, 1));

                        // CREATE mediaAsset
                        MediaAsset mediaAsset = new MediaAsset()
                        {
                            AssetType = eAssetTypes.MEDIA,
                            CreateDate = DateTime.UtcNow,
                            MediaAssetType = MediaAssetType.Media,
                            UpdateDate = DateTime.UtcNow,
                            CoGuid = media.CoGuid,
                            EntryId = media.EntryId,
                            IsActive = string.IsNullOrEmpty(media.IsActive) ? false : media.IsActive.Trim().ToLower().Equals("true"),
                            Id = GetMediaIdByCoGuid(groupId, media.CoGuid),
                            MediaType = new MediaType(media.Basic.MediaType, (int)catalogGroupCache.AssetStructsMapBySystemName[media.Basic.MediaType].Id),
                            // TODO SHIR - ASK LIOR ABOUT NAMES
                            Name = media.Basic.Name.Values.FirstOrDefault(x => x.LangCode.Equals(mainLanguageName)).Text,
                            NamesWithLanguages = new List<LanguageContainer>(media.Basic.Name.Values
                                .Where(x => !x.Text.Equals(mainLanguageName)).Select(x => new LanguageContainer(x.LangCode, x.Text))),
                            // TODO SHIR - ASK LIOR ABOUT Thumb, PicsRatio
                            //Images
                            // TODO SHIR - ASK LIOR ABOUT Descriptions
                            Description = media.Basic.Description.Values.FirstOrDefault(x => x.LangCode.Equals(mainLanguageName)).Text,
                            DescriptionsWithLanguages = new List<LanguageContainer>(media.Basic.Description.Values
                                .Where(x => !x.Text.Equals(mainLanguageName)).Select(x => new LanguageContainer(x.LangCode, x.Text))),
                            StartDate = startDate,
                            CatalogStartDate = GetDateTimeFromString(media.Basic.Dates.CatalogStart, startDate),
                            //CreateDate = GetDateTimeFromString(media.Basic.Dates.Create, DateTime.UtcNow),
                            EndDate = endDate,
                            FinalEndDate = GetDateTimeFromString(media.Basic.Dates.End, endDate),
                            GeoBlockRuleId = CatalogLogic.GetGeoBlockRuleId(groupId, media.Basic.Rules.GeoBlockRule),
                            DeviceRuleId = CatalogLogic.GetDeviceRuleId(groupId, media.Basic.Rules.DeviceRule),
                            Metas = GetMetasList(media.Structure, mainLanguageName),
                            Tags = GetTagsList(media.Structure.Metas.MetaTags, mainLanguageName)
                            //Files = new List<AssetFile>(media.Files.MediaFiles.Select(x => new AssetFile(x.Type)
                            //{
                            //    //Id = x.id,
                            //    //AssetId = x.assetId,
                            //    //TypeId = x.TypeId,
                            //    //Url = x.CdnCode || x.AltCdnCode,
                            //    Duration = long.Parse(x.AssetDuration),
                            //    //ExternalId = x.ExternalId,
                            //    //AltExternalId = x.AltExternalId,
                            //    //ExternalStoreId = x.ExternalStoreId,
                            //    //CdnAdapaterProfileId = x.CdnAdapaterProfileId,
                            //    //AltStreamingCode = x.AltStreamingCode
                            //    //AlternativeCdnAdapaterProfileId = x.AlternativeCdnAdapaterProfileId
                            //    //AdditionalData = x.AdditionalData,
                            //    //BillingType = CatalogLogic.GetBillingTypeId(x.BillingType),
                            //    //OrderNum = x.OrderNum,
                            //    //Language = x.Language,
                            //    //IsDefaultLanguage = x.IsDefaultLanguage,
                            //    //OutputProtecationLevel = x.OutputProtecationLevel,
                            //    //StartDate = x.StartDate,
                            //    //EndDate = x.EndDate,
                            //    // FileSize = x.FileSize,
                            //    //IsActive = x.IsActive
                            //}))
                        };

                        // TODO SHIR - ASK IRA IF SOMEONE SENT IT AND THE MEDIA IS NEW, NEED EXAMPLE
                        //string sEpgIdentifier = GetNodeValue(ref theItem, "basic/epg_identifier");

                        // TODO SHIR - SET IMAGES LATER
                        //string sThumb = GetNodeParameterVal(ref theItem, "basic/thumb", "url");
                        //XmlNodeList thePicRatios = theItem.SelectNodes("basic/pic_ratios/ratio");
                        //// get all ratio and ratio's pic url from input xml
                        //Dictionary<string, string> ratioStrThumb = SetRatioStrThumb(thePicRatios);

                        //Dictionary<int, List<string>> ratioSizesList = new Dictionary<int, List<string>>();
                        //Dictionary<int, string> ratiosThumb = new Dictionary<int, string>();
                        ////get all ratio/sizes needed for DownloadPic 
                        //SetRatioIdsWithPicUrl(nGroupID, ratioStrThumb, out ratioSizesList, out ratiosThumb);

                        ////set default ratio with size
                        //if (!string.IsNullOrEmpty(sThumb))
                        //{
                        //    log.DebugFormat("ProcessItem - Thumb Url:{0}, mediaId:{1}", sThumb, nMediaID);
                        //    theItemName = DownloadThumbPic(nMediaID, nGroupID, sThumb, theItemName, mainLanguageName, ratiosThumb);
                        //}

                        //int picId = 0;
                        //foreach (int ratioKey in ratiosThumb.Keys)
                        //{
                        //    picId = DownloadPic(ratiosThumb[ratioKey], string.Empty, nGroupID, nMediaID, mainLanguageName, "RATIOPIC", false, ratioKey, ratioSizesList[ratioKey]);
                        //    if (picId == 0)
                        //    {
                        //        ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.FailedDownloadPic, Message = FAILED_DOWNLOAD_PIC });
                        //    }
                        //}

                        if (media.Action.Equals(DELETE_ACTION))
                        {
                            if (mediaAsset.Id == 0)
                            {
                                //errorMessage.Append("Cant delete. the item is not exist | ");
                                ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MediaIdNotExist, Message = MEDIA_ID_NOT_EXIST });
                                log.Debug("ProcessItem - Action:Delete Error: media not exist");
                                // log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, sCoGuid, nMediaID, errorMessage.ToString());
                                // sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                                ingestResponse.AssetsStatus.Add(ingestAssetStatus);
                                continue;
                            }

                            log.DebugFormat("Delete Media:{0}", mediaAsset.Id);

                            Status status = AssetManager.DeleteAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, USER_ID);
                            if (status.Code != (int)eResponseStatus.OK)
                            {
                                ingestAssetStatus.Status.Set(status.Code, status.Message);
                                log.Debug("ProcessItem - Action:Delete Error: media not exist");
                                // log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, sCoGuid, nMediaID, errorMessage.ToString());
                                // sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                                ingestResponse.AssetsStatus.Add(ingestAssetStatus);
                                continue;
                            }
                        }
                        else if (media.Action.Equals(INSERT_ACTION) && mediaAsset.Id == 0)
                        {
                            // 1. insert to "media" table (metas: strings, doubles, booleans)
                            // 2. insert to "media_translate" table (metas-strings)
                            // 3. insert to "media_date_metas_values" table by "groups_date_metas" and given metas - dates
                            // 4. UpdateMetas(groupId, nMediaID, mainLanguageName, ref theMetas, ref sErrorMessage);
                            // 5. UpdateFiles(groupId, mainLanguageName, nMediaID, ref theFiles, ref sErrorMessage);
                            GenericResponse<Asset> genericResponse = AssetManager.AddAsset(groupId, eAssetTypes.MEDIA, mediaAsset, USER_ID, true);
                            if (genericResponse.Status.Code != (int)eResponseStatus.OK)
                            {
                                ingestAssetStatus.Status.Set(genericResponse.Status.Code, genericResponse.Status.Message);
                                log.Debug("ProcessItem - Action:Delete Error: media not exist");
                                // log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, sCoGuid, nMediaID, errorMessage.ToString());
                                // sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                                ingestResponse.AssetsStatus.Add(ingestAssetStatus);
                                continue;
                            }
                        }
                        else if (media.Action.Equals(UPDATE_ACTION) && mediaAsset.Id != 0)
                        {
                            if (!media.Erase.Equals("false"))
                            {
                                log.DebugFormat("ProcessItem - Action insert/update clear media files, values.. mediaId:{0}", mediaAsset.Id);
                                if (!AssetManager.ClearAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, USER_ID))
                                {

                                }                                
                            }

                            GenericResponse<Asset> genericResponse = AssetManager.UpdateAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, mediaAsset, USER_ID);
                            if (genericResponse.Status.Code != (int)eResponseStatus.OK)
                            {
                                ingestAssetStatus.Status.Set(genericResponse.Status.Code, genericResponse.Status.Message);
                                log.Debug("ProcessItem - Action:Delete Error: media not exist");
                                // log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, sCoGuid, nMediaID, errorMessage.ToString());
                                // sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                                ingestResponse.AssetsStatus.Add(ingestAssetStatus);
                                continue;
                            }
                            // 1. update "media" table (metas: strings, doubles, booleans)
                            // 2. update "media_translate" table (metas-strings)
                            // 3. update "media_date_metas_values" table by "groups_date_metas" and given metas-dates
                            // 4. UpdateMetas(groupId, nMediaID, mainLanguageName, ref theMetas, ref sErrorMessage);
                            // 5. UpdateFiles(groupId, mainLanguageName, nMediaID, ref theFiles, ref sErrorMessage);
                        }

                        //update InternalAssetId 
                        ingestAssetStatus.InternalAssetId = (int)mediaAsset.Id;
                        ingestAssetStatus.Status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                        //log.DebugFormat("succeeded export media. CoGuid:{0}, MediaID:{1}, ErrorMessage:{2}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                        //sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"OK\" message=\"" + ProtocolsFuncs.XMLEncode(sErrorMessage, true) + "\" tvm_id=\"" + nMediaID.ToString() + "\"/>";

                        // TODO SHIR - ASK IRA ABOUT THIS
                        // Update record in Catalog (see the flow inside Update Index
                        //change eAction.Delete
                        //if (ImporterImpl.UpdateIndex(new List<int>() { nMediaID }, nParentGroupID, eAction.Update))
                        //{
                        //    log.DebugFormat("UpdateIndex: Succeeded. CoGuid:{0}, MediaID:{1}, isActive:{2}, ErrorMessage:{3}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                        //}
                        //else
                        //{
                        //    log.ErrorFormat("UpdateIndex: Failed. CoGuid:{0}, MediaID:{1}, isActive:{2}, ErrorMessage:{3}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                        //    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.UpdateIndexFailed, Message = UPDATE_INDEX_FAILED });
                        //}

                        //// update notification 
                        //if (mediaAsset.IsActive.HasValue && mediaAsset.IsActive.Value)
                        //{
                        //    UpdateNotificationsRequests(groupId, mediaAsset.Id);
                        //}

                        // add media
                        mediaAssets.Add(mediaAsset);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed process mediaIndex: {0}. Exception:{1}", i, ex);
                }

                ingestResponse.AssetsStatus.Add(ingestAssetStatus);
            }

            if (ingestResponse.AssetsStatus.All(x => x.Status != null && x.Status.Code == (int)eResponseStatus.OK))
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            
            return mediaAssets;

            #region comments - delete when finish 
            //// add pics_ratios
            //media.Basic.PicsRatio = new PicsRatio() { Ratios = new List<Ratio>() };
            //if (asset.Images != null)
            //{
            //    // group by ratio, take max size 
            //    List<KalturaMediaImage> images = asset.Images.GroupBy(x => x.Ratio).Select(y => y.OrderByDescending(x => ((x.Height ?? 0) * (x.Width ?? 0))).First()).ToList();

            //    bool thumbUpdated = false;
            //    foreach (var image in images)
            //    {
            //        // add thumb
            //        if (!thumbUpdated)
            //        {
            //            media.Basic.Thumb = new Thumb() { Url = image.Url != null ? ManipulateImageUrl(image.Url) : string.Empty };
            //            thumbUpdated = true;
            //        }

            //        // ratio 
            //        media.Basic.PicsRatio.Ratios.Add(new Ratio()
            //        {
            //            RatioText = image.Ratio ?? string.Empty,
            //            Thumb = image.Url != null ? ManipulateImageUrl(image.Url) : string.Empty
            //        });
            //    }
            //}

            //// add structure
            //media.Structure = new Structure()
            //{
            //    Booleans = new Booleans() { BooleanList = new List<MetaWithoutInnerElement>() },
            //    Doubles = new Doubles() { Metas = new List<MetaWithoutInnerElement>() },
            //    Strings = new Strings() { Metas = new List<Meta>() }
            //};

            //if (asset.Metas != null)
            //{
            //    foreach (var entry in asset.Metas)
            //    {
            //        // add strings
            //        if (entry.Value.GetType() == typeof(KalturaMultilingualStringValue) && entry.Value != null)
            //        {
            //            // add string key
            //            Meta newMeta = new Meta()
            //            {
            //                Name = entry.Key,
            //                Value = new List<Value>()
            //            };

            //            // add strings languages
            //            if (((KalturaMultilingualStringValue)entry.Value).value != null && ((KalturaMultilingualStringValue)entry.Value).value.Values != null)
            //            {
            //                foreach (KalturaTranslationToken item in ((KalturaMultilingualStringValue)entry.Value).value.Values)
            //                {
            //                    newMeta.Value.Add(new Value()
            //                    {
            //                        Text = item.Value,
            //                        Lang = item.Language
            //                    });
            //                }
            //            }

            //            media.Structure.Strings.Metas.Add(newMeta);
            //        }

            //        // add doubles
            //        if (entry.Value.GetType() == typeof(KalturaDoubleValue))
            //        {
            //            media.Structure.Doubles.Metas.Add(new MetaWithoutInnerElement()
            //            {
            //                Name = entry.Key ?? string.Empty,
            //                Value = ((KalturaDoubleValue)entry.Value).value.ToString()
            //            });
            //        }

            //        // add doubles (from integers)
            //        if (entry.Value.GetType() == typeof(KalturaIntegerValue))
            //        {
            //            media.Structure.Doubles.Metas.Add(new MetaWithoutInnerElement()
            //            {
            //                Name = entry.Key ?? string.Empty,
            //                Value = ((KalturaIntegerValue)entry.Value).value.ToString()
            //            });
            //        }

            //        // add booleans 
            //        if (entry.Value.GetType() == typeof(KalturaBooleanValue))
            //        {
            //            media.Structure.Booleans.BooleanList.Add(new MetaWithoutInnerElement()
            //            {
            //                Name = entry.Key ?? string.Empty,
            //                Value = ((KalturaBooleanValue)entry.Value).value ? "true" : "false"
            //            });
            //        }
            //    }
            //}

            //// add tags
            //media.Structure.Metas = new Metas() { MetasList = new List<Meta>() };
            //if (asset.Tags != null)
            //{
            //    Meta meta;
            //    foreach (var entry in asset.Tags)
            //    {
            //        // create new meta - add it's name
            //        meta = new Meta()
            //        {
            //            Name = entry.Key,
            //            Container = new List<Container>()
            //        };

            //        if (entry.Value != null && entry.Value.Objects != null)
            //        {
            //            // add meta values
            //            foreach (KalturaMultilingualStringValue containerObj in entry.Value.Objects)
            //            {
            //                if (containerObj.value != null && containerObj.value.Values != null)
            //                {
            //                    var container = new Container() { Values = new List<Value>() };

            //                    // add meta value languages
            //                    foreach (var value in containerObj.value.Values)
            //                    {
            //                        container.Values.Add(new Value()
            //                        {
            //                            Lang = value.Language,
            //                            Text = value.Value
            //                        });
            //                    }
            //                    meta.Container.Add(container);
            //                }
            //            }
            //        }

            //        // add meta (tag)
            //        media.Structure.Metas.MetasList.Add(meta);
            //    }
            //}

            //// add files
            //media.Files = new Files() { MediaFiles = new List<MediaFile>() };

            //if (asset.MediaFiles != null)
            //{
            //    foreach (var file in asset.MediaFiles)
            //    {
            //        media.Files.MediaFiles.Add(new MediaFile()
            //        {
            //            AssetDuration = file.Duration != null ? file.Duration.ToString() : string.Empty,
            //            Type = file.Type ?? string.Empty,
            //            CdnCode = file.Url ?? string.Empty,

            //            // add external ID/co_guid
            //            CoGuid = file.ExternalId ?? string.Empty,

            //            // add file values
            //            AltCdnCode = file.AltCdnCode,
            //            BillingType = file.BillingType,
            //            CdnName = file.CdnName,
            //            HandlingType = file.HandlingType,
            //            PpvModule = file.PPVModules != null && file.PPVModules.Objects != null && file.PPVModules.Objects.Count > 0 ? string.Join(";", file.PPVModules.Objects.Select(x => x.value).ToArray()) : string.Empty,
            //            ProductCode = file.ProductCode
            //        });
            //    }
            //}            
            #endregion
        }

        private static List<Catalog.Metas> GetMetasList(Structure structure, string mainLanguageName)
        {
            List<Catalog.Metas> metas = new List<Catalog.Metas>(structure.Strings.MetaStrings.Count +
                                                                structure.Booleans.Metas.Count +
                                                                structure.Doubles.Metas.Count +
                                                                structure.Dates.Metas.Count);

            // add metas-doubles
            metas.AddRange(structure.Doubles.Metas.Select
               (doubleMeta => new Catalog.Metas(new TagMeta(doubleMeta.Name, MetaType.Number.ToString()), doubleMeta.Value)));

            // add metas-bools
            metas.AddRange(structure.Booleans.Metas.Select
               (boolMeta => new Catalog.Metas(new TagMeta(boolMeta.Name, MetaType.Bool.ToString()), boolMeta.Value)));

            // add metas-dates
            metas.AddRange(structure.Dates.Metas.Select
               (dateMeta => new Catalog.Metas(new TagMeta(dateMeta.Name, MetaType.DateTime.ToString()), dateMeta.Value)));

            // add metas-strings
            metas.AddRange(structure.Strings.MetaStrings.Select
                (stringMeta => new Catalog.Metas(new TagMeta(stringMeta.Name, MetaType.String.ToString()),
                                                 stringMeta.Values.FirstOrDefault(x => x.LangCode.Equals(mainLanguageName)).Text,
                                                 stringMeta.Values.Where(x => !x.LangCode.Equals(mainLanguageName))
                                                 .Select(x => new LanguageContainer(x.LangCode, x.Text)))));

            return metas;
        }

        private static List<Tags> GetTagsList(List<MetaTag> metaTags, string mainLanguageName)
        {
            List<Tags> metas = new List<Tags>(metaTags.Count);
            
            // add metas-tags
            foreach (var metaTag in metaTags)
            {
                List<string> finalValues = new List<string>();

                foreach (var container in metaTag.Containers)
                {
                    IEnumerable<string[]> mainLanguageValues = container.Values.Where(x => x.LangCode.Equals(mainLanguageName))
                        .Select(x => x.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                    if (mainLanguageValues != null)
                    {
                        foreach (var value in mainLanguageValues)
                        {
                            finalValues.AddRange(value);
                        }
                    }
                }
                
                metas.Add(new Tags() { m_oTagMeta = new TagMeta(metaTag.Name, MetaType.Tag.ToString()), m_lValues = finalValues });
            }

            return metas;
        }
        
        private static bool ValidateMedia(int mediaIndex, Media media, int groupId, CatalogGroupCache catalogGroupCache, ref IngestAssetStatus ingestAssetStatus)
        {
            // get action from xmlNode
            if (string.IsNullOrEmpty(media.Action))
            {
                ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MissingAction, Message = MISSING_ACTION });
            }

            if (!media.Action.Equals(DELETE_ACTION))
            {
                // get entry_id from xmlNode
                if (string.IsNullOrEmpty(media.EntryId))
                {
                    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.MissingEntryId, Message = MISSING_ENTRY_ID });
                }

                if (media.Basic == null)
                {
                    // TODO SHIR - SET ERROR
                    //log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, ErrorMessage:{2}", mediaIndex, media.CoGuid, errorMessage.ToString());
                    //sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                    return false;
                }

                // get is_active from xmlNode
                if (string.IsNullOrEmpty(media.IsActive))
                {
                    log.DebugFormat("ValidateMedia - media with no activation indication. co-guid: {0}.", media.CoGuid);
                }

                // check names
                if (media.Basic.Name != null)
                {

                }

                // check Thumb
                if (media.Basic.Thumb != null)
                {

                }

                // check Descriptions
                if (media.Basic.Description != null)
                {

                }

                // check dates  
                if (media.Basic.Dates != null)
                {
                    //string sCatalogStartDate = GetNodeValue(ref theItem, "basic/dates/catalog_start");
                    //string sStartDate = GetNodeValue(ref theItem, "basic/dates/start");
                    //string sCreateDate = GetNodeValue(ref theItem, "basic/dates/create");
                    //string sCatalogEndDate = GetNodeValue(ref theItem, "basic/dates/catalog_end");
                    //string sFinalEndDate = GetNodeValue(ref theItem, "basic/dates/final_end");

                    //DateTime dStartDate = GetDateTimeFromStrUTF(sStartDate, DateTime.UtcNow);
                    //DateTime dCatalogStartDate = GetDateTimeFromStrUTF(sCatalogStartDate, dStartDate);//catalog_start_date default value is start_date
                    //DateTime dCreate = GetDateTimeFromStrUTF(sCreateDate, DateTime.UtcNow);
                    //DateTime dCatalogEndDate = GetDateTimeFromStrUTF(sCatalogEndDate, new DateTime(2099, 1, 1));
                    //DateTime dFinalEndDate = GetDateTimeFromStrUTF(sFinalEndDate, dCatalogEndDate);

                    //try
                    //{
                    //    media.Basic.Dates.CatalogEnd = asset.EndDate != null && asset.EndDate != 0 ? DateUtils.UnixTimeStampToDateTime((long)asset.EndDate).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty;
                    //    media.Basic.Dates.End = media.Basic.Dates.CatalogEnd;
                    //}
                    //catch (Exception ex)
                    //{
                    //    log.DebugFormat("Illegal end date received while formatting asset list to XML. asset ID: {0}, asset name: {1}, end date: {2}, ex: {3}", asset.Id, asset.Name, asset.EndDate, ex);
                    //}

                    //try
                    //{
                    //    media.Basic.Dates.CatalogStart = asset.StartDate != null && asset.StartDate != 0 ? DateUtils.UnixTimeStampToDateTime((long)asset.StartDate).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty;
                    //    media.Basic.Dates.Start = media.Basic.Dates.CatalogStart;
                    //}
                    //catch (Exception ex)
                    //{
                    //    log.DebugFormat("Illegal start date received while formatting asset list to XML. asset ID: {0}, asset name: {1}, start date: {2}. ex: {3}", asset.Id, asset.Name, asset.StartDate, ex);
                    //}
                }

                // check PicsRatio
                if (media.Basic.PicsRatio != null)
                {

                }

                // CHECK RULES
                if (media.Basic.Rules != null)
                {
                    if (!string.IsNullOrEmpty(media.Basic.Rules.GeoBlockRule))
                    {
                        int geoBlockRuleId = CatalogLogic.GetGeoBlockRuleId(groupId, media.Basic.Rules.GeoBlockRule);
                        if (geoBlockRuleId == 0)
                        {
                            //AddError(ref sErrorMessage, "Geo block rule not recognized");
                            log.DebugFormat("ValidateMedia - Geo block rule not recognized. mediaIndex:{0}, GeoBlockRule:{1}", mediaIndex, media.Basic.Rules.GeoBlockRule);
                            ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.NotRecognizedGeoBlockRule, Message = GEO_BLOCK_RULE_NOT_RECOGNIZED });
                        }
                    }

                    if (!string.IsNullOrEmpty(media.Basic.Rules.DeviceRule))
                    {
                        int deviceRuleId = CatalogLogic.GetDeviceRuleId(groupId, media.Basic.Rules.DeviceRule);
                        if (deviceRuleId == 0)
                        {
                            //AddError(ref sErrorMessage, "Device rule not recognized");
                            log.DebugFormat("ValidateMedia - Device rule not recognized. mediaIndex:{0}, DeviceRule:{1}", mediaIndex, media.Basic.Rules.DeviceRule);
                            ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.NotRecognizedDeviceRule, Message = DEVICE_RULE_NOT_RECOGNIZED });
                        }
                    }
                }

                if (media.Structure == null)
                {
                    // TODO SHIR - SET ERROR
                    return false;
                }

                // check meta-strings
                if (media.Structure.Strings != null)
                {

                }

                // check meta-doubles
                if (media.Structure.Doubles != null)
                {

                }

                // check meta-booleans
                if (media.Structure.Booleans != null)
                {

                }

                // CHECK META-DATES
                //if (media.Structure.Dates != null)
                //{

                //}

                // CHECK META-METAS
                if (media.Structure.Metas != null)
                {

                }

                if (media.Files == null)
                {
                    // TODO SHIR - SET ERROR
                    return false;
                }

                if (media.Action.Equals(INSERT_ACTION))
                {
                    // check MediaType
                    if (!catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(media.Basic.MediaType) ||
                        catalogGroupCache.AssetStructsMapBySystemName[media.Basic.MediaType].Id == 0)
                    {
                        //errorMessage.Append("Item type not recognized | ");
                        log.DebugFormat("ValidateMedia - Item type not recognized. co-guid:{0}", media.CoGuid);
                        ingestAssetStatus.Status.Set((int)eResponseStatus.InvalidMediaType, string.Format("Invalid media type \"{0}\"", media.Basic.MediaType));
                        //log.ErrorFormat("Error import mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, sCoGuid, nMediaID, errorMessage.ToString());
                        //sNotifyXML += "<media co_guid=\"" + sCoGuid + "\" status=\"FAILED\" message=\"" + errorMessage.ToString() + "\" tvm_id=\"" + nMediaID + "\"/>";
                        return false;
                    }
                }
                else if (media.Action.Equals(UPDATE_ACTION))
                {

                }
            }
            
            return true;
        }

        private static Feed DeserializeXmlToFeed(string xml, int groupId, ref IngestResponse ingestResponse)
        {
            Object deserializeObject = null;

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Feed));
                using (StringReader stringReader = new StringReader(xml))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                    {
                        deserializeObject = xmlSerializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (XmlException ex)
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.IllegalXml, "XML file with wrong format");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return null;
            }
            catch (Exception ex)
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.IllegalXml, "Error while loading file");
                log.ErrorFormat("Failed loading file: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return null;
            }

            if (deserializeObject == null || !(deserializeObject is Feed))
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.IllegalXml, "TOEDO SHIE SET ERROR MSG");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}.", xml, groupId);
                return null;
            }

            return deserializeObject as Feed;
        }
        
        private static int GetMediaIdByCoGuid(int groupId, string coGuid)
        {   
            DataTable existingExternalIdsDt = CatalogDAL.ValidateExternalIdsExist(groupId, new List<string>() { coGuid });
            if (existingExternalIdsDt != null && existingExternalIdsDt.Rows != null && existingExternalIdsDt.Rows.Count > 0)
            {
                DataRow dr = existingExternalIdsDt.Rows[0];
                return ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            }

            return 0;
        }

        // TODO SHIR - CHECK IF METHOD IS CURRECT
        public static DateTime GetDateTimeFromString(string date, DateTime defaultDate)
        {
            try
            {
                string sTime = "";
                if (date == "")
                {
                    return defaultDate;
                }

                string[] timeHour = date.Split(' ');
                if (timeHour.Length == 2)
                {
                    date = timeHour[0];
                    sTime = timeHour[1];
                }
                else
                    return DateTime.Now;
                string[] splited = date.Split('/');

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

        // TODO SHIR - use good method
        static protected string GetMainLanguageName(int groupId)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            try
            {
                selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock),groups g (nolock) where g.LANGUAGE_ID=ll.id and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", groupId);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        return selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                        //nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
            }
            catch (Exception)
            {
                log.ErrorFormat("GetMainLanguageName failed for groupId: {0}.", groupId);
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return null;
        }

        // TODO SHIR - use good method
        static protected bool UpdateMetas(Int32 nGroupID, Int32 nMediaID, string sMainLang, ref XmlNodeList theMetas, ref string sError)
        {
            //Int32 nCount = theMetas.Count;
            //for (int i = 0; i < nCount; i++)
            //{
            //    XmlNode theItem = theMetas[i];
            //    string sName = GetItemParameterVal(ref theItem, "name");
            //    string sMLHandling = GetItemParameterVal(ref theItem, "ml_handling");

            //    if (string.IsNullOrEmpty(sName))
            //        continue;

            //    Int32 tagTypeID = GetTagTypeID(nGroupID, sName);
            //    if (tagTypeID == 0 && sName.ToLower().Trim() != "free")
            //    {
            //        AddError(ref sError, string.Format("meta \"{0}\" does not exist) :", sName));
            //        continue;
            //    }

            //    TranslatorStringHolder metaHolder = new TranslatorStringHolder();
            //    XmlNodeList theContainers = theItem.SelectNodes("container");
            //    Int32 nCount1 = theContainers.Count;
            //    if (nCount1 == 0)
            //    {
            //        theContainers = theItem.SelectNodes("values");
            //        nCount1 = theContainers.Count;
            //    }
            //    for (int j = 0; j < nCount1; j++)
            //    {
            //        XmlNode theContainer = theContainers[j];
            //        string sVal = GetMultiLangValue(sMainLang, ref theContainer);
            //        if (sVal == "")
            //        {
            //            AddError(ref sError, "meta :" + sName + " - no main language value");
            //            continue;
            //        }
            //        metaHolder.AddLanguageString(sMainLang, sVal, j.ToString(), true);  ///i->j
            //        if (sMLHandling.Trim().ToLower() == "duplicate")
            //        {
            //            DuplicateMetaData(nGroupID, sMainLang, ref metaHolder, sVal, j.ToString());   ///i->j
            //        }
            //        else
            //        {
            //            GetSubLangMetaData(nGroupID, sMainLang, ref metaHolder, ref theContainer, j.ToString());  ///i->j
            //        }
            //    }


            //    ClearMediaTags(nMediaID, tagTypeID);
            //    if (nCount1 > 0)
            //    {
            //        if (tagTypeID != 0 || sName.ToLower().Trim() == "free")
            //            IngestionUtils.M2MHandling("ID", "TAG_TYPE_ID", tagTypeID.ToString(), "int", "ID", "tags", "media_tags", "media_id", "tag_id", "true",
            //                sMainLang, metaHolder, nGroupID, nMediaID);

            //    }
            //}
            return true;
        }
    }
}