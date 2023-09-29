using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using APILogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Catalog.CatalogManagement.Validators;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.CDNAdapter;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using DAL;
using ODBCWrapper;
using Phx.Lib.Log;
using Tvinci.Core.DAL;
using TVinciShared;
using MetaType = ApiObjects.MetaType;

namespace Core.Catalog.CatalogManagement
{
    public class IngestManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly XmlSerializer xmlIngestFeedSerializer = new XmlSerializer(typeof(IngestVODFeed));

        #region Consts

        // ERRORS
        private const string WATCH_PERMISSION_RULE_NOT_RECOGNIZED = "Watch permission rule not recognized";

        private const string PLAYERS_RULE_NOT_RECOGNIZED = "Players rule not recognized ";
        private const string UPDATE_INDEX_FAILED = "Update index failed";
        private const string MEDIA_ID_NOT_EXIST = "Media Id not exist";


        private const long USER_ID = 999;
        private const string ASSET_FILE_DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";

        #endregion

        public static IngestResponse HandleMediaIngest(int groupId, string xml, string fileName)
        {
            log.DebugFormat("Start HandleMediaIngest. groupId:{0}", groupId);
            IngestResponse ingestResponse = IngestResponse.Default;

            var feedResponse = DeserializeXmlToFeed(xml, groupId, ref ingestResponse);
            if (!feedResponse.HasObject())
            {
                ingestResponse.IngestStatus = feedResponse.Status;
                VodIngestAssetResultPublisher.Instance.PublishFailedIngest(groupId, ingestResponse.IngestStatus, fileName);

                return ingestResponse;
            }

            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out var cache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling HandleMediaIngest", groupId);
                VodIngestAssetResultPublisher.Instance.PublishFailedIngest(groupId, ingestResponse.IngestStatus, fileName);

                return ingestResponse;
            }

            // get data for group
            var mediaFileTypes = FileManager.Instance.GetMediaFileTypes(groupId);
            var groupDefaultRatio = ImageUtils.GetGroupDefaultRatioName(groupId);
            var groupRatioNamesToImageTypes = ImageManager.GetImageTypesMapBySystemName(groupId);
            var tagsTranslations = new Dictionary<string, TagsTranslations>();
            var assetsWithNoTags = new Dictionary<int, bool>();
            var cdnAdapters = GetCDNAdaptersMapping(groupId);
            for (var i = 0; i < feedResponse.Object.Export.MediaList.Count; i++)
            {
                var media = feedResponse.Object.Export.MediaList[i];
                ingestResponse.AssetsStatus.Add(IngestAssetStatus.Default);
                MediaAsset mediaAsset = null;
                AssetUserRule shopAssetUserRule = null;
                try
                {
                    if (media.Validate(groupId, cache, ref ingestResponse, i, out var mediaId))
                    {
                        switch (media.Action)
                        {
                            case IngestMedia.DELETE_ACTION:
                            {
                                var mediaAssetResponse = AssetManager.Instance.GetAsset(groupId, mediaId, eAssetTypes.MEDIA, true);
                                if (!mediaAssetResponse.IsOkStatusCode())
                                {
                                    log.DebugFormat("DeleteMediaAsset - {0}", MEDIA_ID_NOT_EXIST);
                                    ingestResponse.AssetsStatus[i].Warnings.Add(new Status((int)IngestWarnings.MediaIdNotExist, MEDIA_ID_NOT_EXIST));
                                    ingestResponse.Set(media.CoGuid, "Cant delete. the item is not exist", "OK", (int)mediaId);
                                }
                                else
                                {
                                    mediaAsset = (MediaAsset)mediaAssetResponse.Object;
                                    shopAssetUserRule = ShopAssetUserRuleResolver.Instance.ResolveByMediaAsset(groupId, mediaAsset.Metas, mediaAsset.Tags);
                                    DeleteMediaAsset((int)mediaId, media.CoGuid, groupId, ref ingestResponse, i);
                                }

                                break;
                            }
                            default:
                            {
                                var isMetasValid = media.ValidateMetas(cache, ref ingestResponse, i, out var metas, out var tags, out var topicIdsToRemove);
                                if (isMetasValid)
                                {
                                    shopAssetUserRule = ShopAssetUserRuleResolver.Instance.ResolveByMediaAsset(groupId, media.Basic?.MediaType, metas, tags);
                                }

                                if (isMetasValid && media.ValidateOnInsertOrUpdate(groupId, cache, mediaId, ref ingestResponse, i))
                                {
                                    mediaAsset = CreateMediaAsset(groupId, mediaId, media, cache, tags, metas);
                                    var images = GetImages(media.Basic, groupId, groupDefaultRatio, groupRatioNamesToImageTypes);
                                    var assetFiles = GetAssetFiles(media.Files, mediaFileTypes, cdnAdapters);
                                    bool isMediaExists = mediaId > 0 || (images != null && images.Count > 0) || (assetFiles != null && assetFiles.Count > 0);
                                    media.Erase = media.Erase.ToLower().Trim();
                                    topicIdsToRemove = mediaId > 0 ? topicIdsToRemove : null;
                                    var upsertStatus = BulkAssetManager.UpsertMediaAsset(groupId, ref mediaAsset, USER_ID, images, assetFiles, ASSET_FILE_DATE_FORMAT, IngestMedia.TRUE.Equals(media.Erase), true, topicIdsToRemove);
                                    if (!upsertStatus.IsOkStatusCode())
                                    {
                                        ingestResponse.AssetsStatus[i].Status = upsertStatus.Status;
                                        ingestResponse.Set(mediaAsset.CoGuid, "UpsertMediaAsset faild", "FAILED", (int)mediaAsset.Id);
                                        continue;
                                    }

                                    ingestResponse.AssetsStatus[i].Warnings.AddRange(upsertStatus.Objects);
                                    ingestResponse.Set(mediaAsset.CoGuid, "succeeded Upsert media", "OK", (int)mediaAsset.Id);
                                    ingestResponse.AssetsStatus[i].InternalAssetId = (int)mediaAsset.Id;
                                    ingestResponse.AssetsStatus[i].ExternalAssetId = mediaAsset.CoGuid;
                                    if (tags == null || tags.Count == 0)
                                    {
                                        assetsWithNoTags.Add((int)mediaAsset.Id, isMediaExists);
                                    }
                                    else
                                    {
                                        bool doesMediaHaveTagTranslations = false;
                                        AddTagsToTranslations(tags, (int)mediaAsset.Id, isMediaExists, ref tagsTranslations, ref doesMediaHaveTagTranslations);
                                        if (!doesMediaHaveTagTranslations)
                                        {
                                            assetsWithNoTags.Add((int)mediaAsset.Id, isMediaExists);
                                        }
                                    }

                                    // update notification
                                    if (mediaAsset.IsActive.Value)
                                    {
                                        Notification.Module.AddFollowNotificationRequestForOpc(groupId, mediaAsset, USER_ID, cache);
                                    }

                                    // succeeded import media
                                    ingestResponse.AssetsStatus[i].Status.Set(eResponseStatus.OK);
                                    log.DebugFormat("succeeded import media. CoGuid:{0}, MediaID:{1}, ErrorMessage:{2}", media.CoGuid, mediaId, media.IsActive, ingestResponse.Description);
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        ingestResponse.Set(media.CoGuid, "Media data is not valid", "FAILED", (int)mediaId);
                        log.ErrorFormat("Media data is not valid. mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, media.CoGuid, mediaId, ingestResponse.Description);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = string.Format("Exception while HandleMediaIngest for mediaIndex: {0}, Exception:{1}", i, ex);
                    log.Error(errorMsg);
                    ingestResponse.AssetsStatus[i].Status.Set((int)eResponseStatus.Error, errorMsg);
                }

                var publishContext = new VodIngestPublishContext
                {
                    AssetStatus = ingestResponse.AssetsStatus[i],
                    GroupId = groupId,
                    FileName = fileName,
                    Media = media,
                    MediaAsset = mediaAsset,
                    ShopAssetUserRuleId = shopAssetUserRule?.Id
                };

                VodIngestAssetResultPublisher.Instance.Publish(publishContext);
            }

            if (ingestResponse.AssetsStatus.All(x => x.Status != null && x.Status.Code == (int)eResponseStatus.OK))
            {
                ingestResponse.IngestStatus.Set(eResponseStatus.OK);
            }

            if (tagsTranslations.Count > 0)
            {
                HandleTagsTranslations(tagsTranslations, groupId, cache, ref ingestResponse);
            }

            if (assetsWithNoTags.Count > 0)
            {
                IndexAndInvalidateAssets(groupId, assetsWithNoTags);
            }

            log.DebugFormat("End HandleMediaIngest. groupId:{0}", groupId);

            return ingestResponse;
        }

        private static GenericResponse<IngestVODFeed> DeserializeXmlToFeed(string xml, int groupId, ref IngestResponse ingestResponse)
        {
            var response = new GenericResponse<IngestVODFeed>();
            Object deserializeObject = null;

            try
            {
                using (StringReader stringReader = new StringReader(xml))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                    {
                        deserializeObject = xmlIngestFeedSerializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (XmlException ex)
            {
                response.SetStatus(eResponseStatus.IllegalXml, "XML file with wrong format");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return response;
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.IllegalXml, "Error while loading file");
                log.ErrorFormat("Failed loading file: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return response;
            }

            if (deserializeObject == null || !(deserializeObject is IngestVODFeed))
            {
                response.SetStatus(eResponseStatus.IllegalXml, "TODO - SET ERROR MSG");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}.", xml, groupId);
                return response;
            }

            var feed = deserializeObject as IngestVODFeed;
            if (feed == null || feed.Export == null || feed.Export.MediaList == null || feed.Export.MediaList.Count == 0)
            {
                response.SetStatus(eResponseStatus.IllegalXml);
                return response;
            }

            response.Object = feed;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private static MediaAsset CreateMediaAsset(int groupId, long mediaId, IngestMedia media, CatalogGroupCache cache, List<Tags> tags, List<Metas> metas)
        {
            DateTime startDate = GetDateTimeFromString(media.Basic.Dates.Start, DateTime.UtcNow);
            DateTime endDate = GetDateTimeFromString(media.Basic.Dates.CatalogEnd, new DateTime(2099, 1, 1));

            string mediaType = media.Basic.MediaType;
            if (mediaId != 0)
            {
                var assetResponse = AssetManager.Instance.GetAsset(groupId, mediaId, eAssetTypes.MEDIA, true);
                if (assetResponse.HasObject() && assetResponse.Object is MediaAsset)
                {
                    mediaType = (assetResponse.Object as MediaAsset).MediaType.m_sTypeName;
                }
            }

            MediaAsset mediaAsset = new MediaAsset()
            {
                Id = mediaId,
                AssetType = eAssetTypes.MEDIA,
                MediaAssetType = MediaAssetType.Media,
                CoGuid = media.CoGuid,
                EntryId = media.EntryId,
                IsActive = StringUtils.TryConvertTo<bool>(media.IsActive),
                MediaType = new MediaType(mediaType, (int)cache.AssetStructsMapBySystemName[mediaType].Id),
                Name = GetMainLanguageValue(cache.GetDefaultLanguage().Code, media.Basic.Name),
                NamesWithLanguages = GetOtherLanguages(cache.GetDefaultLanguage().Code, media.Basic.Name),
                Description = GetMainLanguageValue(cache.GetDefaultLanguage().Code, media.Basic.Description),
                DescriptionsWithLanguages = GetOtherLanguages(cache.GetDefaultLanguage().Code, media.Basic.Description),
                StartDate = startDate,
                CatalogStartDate = GetDateTimeFromString(media.Basic.Dates.CatalogStart, startDate),
                EndDate = endDate,
                FinalEndDate = GetDateTimeFromString(media.Basic.Dates.FinalEnd, endDate),
                GeoBlockRuleId = (int?)TvmRuleManager.GetGeoBlockRuleId(groupId, media.Basic.Rules.GeoBlockRule),
                DeviceRuleId = (int?)TvmRuleManager.GetDeviceRuleId(groupId, media.Basic.Rules.DeviceRule),
                Metas = metas,
                Tags = tags
                // TODO - ASK IRA IF SOMEONE SENT IT AND THE MEDIA IS NEW, NEED EXAMPLE
                //string sEpgIdentifier = GetNodeValue(ref theItem, "basic/epg_identifier");
            };

            return mediaAsset;
        }

        private static void AddTagsToTranslations(List<Tags> tags, int mediaId, bool isMediaExists, ref Dictionary<string, TagsTranslations> tagsTranslations, ref bool doesMediaHaveTagTranslations)
        {
            var slimTags = new List<Tuple<string, string, LanguageContainer[]>>();
            foreach (var tag in tags)
            {
                for (int i = 0; i < tag.m_lValues.Count; i++)
                {
                    var translations = tag.Values != null && tag.Values.Count > i ? tag.Values[i] : null;
                    slimTags.Add(new Tuple<string, string, LanguageContainer[]>(tag.m_oTagMeta.m_sName, tag.m_lValues[i], translations));
                }
            }

            doesMediaHaveTagTranslations = slimTags.Count > 0;
            var existingTagsTranslations = new List<Tuple<string, LanguageContainer>>();
            foreach (var slimTag in slimTags)
            {
                var topicSystemName = slimTag.Item1;
                var defaultTagValue = slimTag.Item2;
                var translations = slimTag.Item3;

                var tagKey = TagsTranslations.GetKey(topicSystemName, defaultTagValue);
                if (tagsTranslations.ContainsKey(tagKey))
                {
                    tagsTranslations[tagKey].AssetsToInvalidate.Add(new KeyValuePair<int, bool>(mediaId, isMediaExists));
                    if (translations != null && translations.Length > 0)
                    {
                        existingTagsTranslations.AddRange(translations.Select(x => new Tuple<string, LanguageContainer>(tagKey, x)));
                    }
                }
                else
                {
                    tagsTranslations.Add(tagKey, new TagsTranslations(topicSystemName, defaultTagValue, translations, mediaId, isMediaExists));
                }
            }

            if (existingTagsTranslations.Count > 0)
            {
                foreach (var translation in existingTagsTranslations)
                {
                    var tagKey = translation.Item1;
                    var language = translation.Item2;
                    if (tagsTranslations[tagKey].Translations.ContainsKey(language.m_sLanguageCode3))
                    {
                        tagsTranslations[tagKey].Translations[language.m_sLanguageCode3].m_sValue = language.m_sValue;
                    }
                    else
                    {
                        tagsTranslations[tagKey].Translations.Add(language.m_sLanguageCode3, language);
                    }
                }
            }
        }

        private static void HandleTagsTranslations(Dictionary<string, TagsTranslations> tagsTranslations, int groupId, CatalogGroupCache cache, ref IngestResponse response)
        {
            var tagMetaType = MetaType.Tag.ToString();
            List<TagToInvalidate> tagsToInvalidate = new List<TagToInvalidate>();
            var defaultLanguageId = cache.GetDefaultLanguage().ID;
            Dictionary<string, TagValue> tagsMap = new Dictionary<string, TagValue>();

            foreach (var tag in tagsTranslations)
            {
                if (cache.TopicsMapBySystemNameAndByType.ContainsKey(tag.Value.TopicSystemName) &&
                    cache.TopicsMapBySystemNameAndByType[tag.Value.TopicSystemName].ContainsKey(tagMetaType))
                {
                    Topic topicTag = cache.TopicsMapBySystemNameAndByType[tag.Value.TopicSystemName][tagMetaType];
                    tag.Value.TopicId = topicTag.Id;

                    string key = $"{topicTag.Id}_{tag.Value.DefaultTagValue.ToLower()}";

                    TagValue tagValue = null;

                    if (!tagsMap.ContainsKey(key))
                    {
                        log.DebugFormat($"BEO-8957 tagsMap Miss:{key}");
                        var tagResponse = CatalogManager.SearchTags(groupId, true, tag.Value.DefaultTagValue, (int)topicTag.Id, defaultLanguageId, 0, 1);
                        if (tagResponse.HasObjects())
                        {
                            log.DebugFormat($"BEO-8957 tag ES Hit:{key}");
                            tagsMap.Add(key, tagResponse.Objects[0]);
                            tagValue = tagResponse.Objects[0];
                        }
                        else
                        {
                            log.DebugFormat($"BEO-8957 tag ES Miss:{key}");
                            var tagByValue = CatalogManager.GetTagByValue(groupId, tag.Value.DefaultTagValue, topicTag.Id);
                            if (tagByValue.HasObject())
                            {
                                log.DebugFormat($"BEO-8957 tag DB Hit:{key}");
                                tagsMap.Add(key, tagByValue.Object);
                                tagValue = tagByValue.Object;
                            }
                        }
                    }
                    else
                    {
                        log.DebugFormat($"BEO-8957 tag tagsMap Hit:{key}");
                        tagValue = tagsMap[key];
                    }

                    if (tagValue == null)
                    {
                        tagsToInvalidate.Add(AddTagTranslations(groupId, tag.Value, defaultLanguageId, ref response));
                    }
                    else
                    {
                        tagsToInvalidate.Add(UpdateTagTranslations(groupId, tag.Value, defaultLanguageId, tagValue, ref response));
                    }
                }
            }

            // invalidate assets and tags
            IndexAndInvalidateTags(groupId, tagsToInvalidate, cache);
        }

        private static TagToInvalidate AddTagTranslations(int groupId, TagsTranslations tag, int defaultLanguageId, ref IngestResponse response)
        {
            var tagToInvalidate = tag.GetTagToInvalidate(false, defaultLanguageId);

            var addTagResponse = CatalogManager.Instance.AddTag(groupId, tagToInvalidate.TagValue, USER_ID, true);
            if (!addTagResponse.HasObject())
            {
                string errorMsg = string.Format("AddTagTranslations faild. topicName: {0}, topicId: {1}, tagValue: {2}, addTagStatus: {3}.",
                                                tag.TopicSystemName, tag.TopicId, tag.DefaultTagValue, addTagResponse.ToStringStatus());
                response.AddError(errorMsg);
                log.Debug(errorMsg);
                tagToInvalidate.TagValue.tagId = -1;
                return tagToInvalidate;
            }

            tagToInvalidate.TagValue.tagId = addTagResponse.Object.tagId;
            return tagToInvalidate;
        }

        private static TagToInvalidate UpdateTagTranslations(int groupId, TagsTranslations tag, int defaultLanguageId, TagValue oldTagValue, ref IngestResponse response)
        {
            oldTagValue.TagsInOtherLanguages.ForEach(x =>
            {
                if (!tag.Translations.ContainsKey(x.m_sLanguageCode3))
                {
                    tag.Translations.Add(x.m_sLanguageCode3, x);
                }
            });

            var tagToInvalidate = tag.GetTagToInvalidate(true, defaultLanguageId);
            tagToInvalidate.TagValue.tagId = oldTagValue.tagId;

            if (oldTagValue.IsNeedToUpdate(tagToInvalidate.TagValue))
            {
                var updateTagResponse = CatalogManager.UpdateTag(groupId, tagToInvalidate.TagValue, USER_ID, true);
                if (!updateTagResponse.HasObject() && updateTagResponse.Status.Code != (int)eResponseStatus.NoValuesToUpdate)
                {
                    string errorMsg = string.Format("UpdateTagTranslations faild. topicName: {0}, topicId: {1}, tagValue: {2}, tagId: {3}, updateTagStatus: {4}.",
                                                    tag.TopicSystemName, tag.TopicId, tag.DefaultTagValue, oldTagValue.tagId, updateTagResponse.ToStringStatus());
                    response.AddError(errorMsg);
                    log.Debug(errorMsg);
                    tagToInvalidate.TagValue.tagId = -1;
                }
            }
            else
            {
                tagToInvalidate.TagValue.tagId = -1;
            }

            return tagToInvalidate;
        }

        private static void IndexAndInvalidateTags(int groupId, List<TagToInvalidate> tagsToInvalidate, CatalogGroupCache catalogGroupCache)
        {
            var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
            var assetsToInvalidate = new Dictionary<int, bool>();

            foreach (var tag in tagsToInvalidate)
            {
                tag.AssetsToInvalidate.ForEach(x =>
                {
                    if (!assetsToInvalidate.ContainsKey(x.Key))
                    {
                        assetsToInvalidate.Add(x.Key, x.Value);
                    }
                });

                // Index And Invalidate Tag only if tag Is Need To be Update (id > 0)
                if (tag.TagValue.tagId > 0)
                {
                    if (tag.IsTagExists)
                    {
                        // get all assets whom contains this tag for Index and Invalidate them
                        var ds = CatalogDAL.GetTagAssets(groupId, tag.TagValue.tagId);
                        List<int> mediaIds, epgIds;
                        CatalogManager.CreateAssetsListForUpdateIndexFromDataSet(ds, out mediaIds, out epgIds);
                        mediaIds.ForEach(x =>
                        {
                            if (!assetsToInvalidate.ContainsKey(x))
                            {
                                assetsToInvalidate.Add(x, true);
                            }
                        });
                    }

                    var result = indexManager.UpdateTag(tag.TagValue);
                    if (!result.IsOkStatusCode())
                    {
                        log.ErrorFormat("Failed UpdateTag index for tag: {0}, groupId: {1}, error: {2} after IndexAndInvalidateTags", tag.TagValue.ToString(), groupId, result.ToString());
                    }
                }
            }

            IndexAndInvalidateAssets(groupId, assetsToInvalidate);
        }

        private static void IndexAndInvalidateAssets(int groupId, Dictionary<int, bool> assetsToInvalidate)
        {
            foreach (var asset in assetsToInvalidate)
            {
                if (!IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertMedia(asset.Key))
                {
                    log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after IndexAndInvalidateAssets", asset.Key, groupId);
                }
                //extracted it from upsertMedia it was called also for OPC accounts,searchDefinitions
                //not sure it's required but better be safe
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, asset.Key));

                // if asset is exists
                if (asset.Value)
                {
                    AssetManager.Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, asset.Key);
                }
            }
        }

        private static bool DeleteMediaAsset(int mediaId, string coGuid, int groupId, ref IngestResponse ingestResponse, int mediaIndex)
        {
            Status deleteStatus = new Status(eResponseStatus.OK);

            var config = Api.Module.GetGeneralPartnerConfiguration(groupId);
            if (config.HasObjects() && config.Objects[0].DeleteMediaPolicy.HasValue && config.Objects[0].DeleteMediaPolicy.Value == DeleteMediaPolicy.Delete)
            {
                deleteStatus = AssetManager.Instance.DeleteAsset(groupId, mediaId, eAssetTypes.MEDIA, USER_ID);
            }
            else
            {
                if (!CatalogDAL.SetMediaIsActiveToOff(mediaId, USER_ID))
                {
                    deleteStatus.Set(eResponseStatus.Error);
                }
                else
                {
                    var assets = new Dictionary<int, bool>();
                    assets.Add(mediaId, true);
                    IndexAndInvalidateAssets(groupId, assets);
                }
            }

            if (deleteStatus.Code != (int)eResponseStatus.OK)
            {
                ingestResponse.AssetsStatus[mediaIndex].Status = deleteStatus;
                log.Debug("DeleteMediaAsset - faild");
                ingestResponse.Set(coGuid, "DeleteAsset faild", "FAILED", mediaId);
                return false;
            }

            ingestResponse.Set(coGuid, "succeeded delete media", "OK", mediaId);
            ingestResponse.AssetsStatus[mediaIndex].InternalAssetId = mediaId;
            ingestResponse.AssetsStatus[mediaIndex].ExternalAssetId = coGuid;
            ingestResponse.AssetsStatus[mediaIndex].Status.Set(eResponseStatus.OK);

            return true;
        }

        private static string GetMainLanguageValue(string mainLanguageName, IngestMultilingual multilingual)
        {
            if (multilingual != null && multilingual.Values != null && multilingual.Values.Count > 0)
            {
                var mainLanguage = multilingual.Values.FirstOrDefault(x => x.LangCode.Equals(mainLanguageName));
                if (mainLanguage != null)
                {
                    return mainLanguage.Text;
                }
            }

            return null;
        }

        private static List<LanguageContainer> GetOtherLanguages(string mainLanguageCode, IngestMultilingual multilingual)
        {
            if (multilingual != null && multilingual.Values != null && multilingual.Values.Count > 0)
            {
                return new List<LanguageContainer>(multilingual.Values.Where(x => !x.LangCode.Equals(mainLanguageCode)).Select(x => new LanguageContainer(x.LangCode, x.Text)));
            }

            return null;
        }

        private static DateTime GetDateTimeFromString(string date, DateTime defaultDate)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return defaultDate;
                }

                string sTime = "";
                string[] timeHour = date.Split(' ');
                if (timeHour.Length == 2)
                {
                    date = timeHour[0];
                    sTime = timeHour[1];
                }
                else
                    return DateTime.UtcNow;
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
                return DateTime.UtcNow;
            }
        }

        // TODO - use good method
        private static int GetBillingTypeIdByName(string billingTypeName)
        {
            DataSetSelectQuery selectQuery = new DataSetSelectQuery();

            try
            {
                selectQuery += "select id from lu_billing_type where ";
                selectQuery += Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(API_VAL)))", "=", billingTypeName.Trim().ToLower());
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        return int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
            }
            catch (Exception)
            {
                log.ErrorFormat("GetBillingIdByName failed for billingName: {0}.", billingTypeName);
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return 0;
        }

        private static Dictionary<long, Image> GetImages(IngestBasic basic, int groupId, string groupDefaultRatio, Dictionary<string, ImageType> groupRatioNamesToImageTypes)
        {
            Dictionary<long, Image> images = null;

            if (groupRatioNamesToImageTypes != null && groupRatioNamesToImageTypes.Count > 0)
            {
                if (basic.Thumb != null && !string.IsNullOrEmpty(basic.Thumb.Url) && !string.IsNullOrEmpty(groupDefaultRatio) && groupRatioNamesToImageTypes.ContainsKey(groupDefaultRatio))
                {
                    Image image = new Image()
                    {
                        Url = basic.Thumb.Url,
                        ImageTypeId = groupRatioNamesToImageTypes[groupDefaultRatio].Id
                    };

                    images = new Dictionary<long, Image>() { { image.ImageTypeId, image } };
                }

                if (basic.PicsRatio != null && basic.PicsRatio.Ratios != null && basic.PicsRatio.Ratios.Count > 0)
                {
                    foreach (var ratio in basic.PicsRatio.Ratios)
                    {
                        if (groupRatioNamesToImageTypes.ContainsKey(ratio.RatioText))
                        {
                            if (images == null)
                            {
                                images = new Dictionary<long, Image>();
                            }

                            long imageTypeId = groupRatioNamesToImageTypes[ratio.RatioText].Id;

                            if (images.ContainsKey(imageTypeId))
                            {
                                images[imageTypeId].Url = ratio.Thumb;
                            }
                            else
                            {
                                images.Add(imageTypeId, new Image() { ImageTypeId = imageTypeId, Url = ratio.Thumb });
                            }
                        }
                    }
                }
            }

            return images;
        }

        private static Dictionary<int, Tuple<AssetFile, string>> GetAssetFiles(IngestFiles files, GenericListResponse<MediaFileType> mediaFileTypes, Dictionary<string, CDNAdapter> cdnAdapters)
        {
            Dictionary<int, Tuple<AssetFile, string>> assetFiles = null;

            if (files != null && files.MediaFiles != null && files.MediaFiles.Count > 0 && mediaFileTypes != null && mediaFileTypes.HasObjects())
            {
                foreach (var mediaFile in files.MediaFiles)
                {
                    var mediaFileType = mediaFileTypes.Objects.FirstOrDefault(x => x.Name.Equals(mediaFile.Type));
                    if (mediaFileType != null)
                    {
                        if (assetFiles == null)
                        {
                            assetFiles = new Dictionary<int, Tuple<AssetFile, string>>();
                        }

                        int mediaFileTypeId = (int)mediaFileType.Id;
                        if (!assetFiles.ContainsKey(mediaFileTypeId))
                        {
                            var allDynamicData = mediaFile.DynamicData.Items.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
                            var validateDynamicData = MediaFileValidator.Instance.GetValidatedDynamicData(mediaFileType, allDynamicData);

                            assetFiles.Add(mediaFileTypeId, new Tuple<AssetFile, string>(new AssetFile(mediaFile.Type)
                            {
                                //Id
                                //AssetId
                                TypeId = mediaFileTypeId,
                                Url = mediaFile.CdnCode,
                                Duration = StringUtils.TryConvertTo<long>(mediaFile.AssetDuration),
                                ExternalId = mediaFile.CoGuid,
                                AltExternalId = mediaFile.AltCoGuid,
                                ExternalStoreId = mediaFile.ProductCode,
                                CdnAdapaterProfileId = !string.IsNullOrEmpty(mediaFile.CdnName) && cdnAdapters.ContainsKey(mediaFile.CdnName) ? cdnAdapters[mediaFile.CdnName].ID : (long?)null,
                                AltStreamingCode = mediaFile.AltCdnCode,
                                AlternativeCdnAdapaterProfileId = !string.IsNullOrEmpty(mediaFile.AltCdnName) && cdnAdapters.ContainsKey(mediaFile.AltCdnName) ? cdnAdapters[mediaFile.AltCdnName].ID : (long?)null,
                                //AdditionalData
                                BillingType = GetBillingTypeIdByName(mediaFile.BillingType),
                                //OrderNum
                                Language = mediaFile.Language,
                                IsDefaultLanguage = StringUtils.TryConvertTo<bool>(mediaFile.IsDefaultLanguage),
                                OutputProtecationLevel = mediaFile.OutputProtecationLevel,
                                StartDate = DateUtils.TryExtractDate(mediaFile.FileStartDate, ASSET_FILE_DATE_FORMAT),
                                EndDate = DateUtils.TryExtractDate(mediaFile.FileEndDate, ASSET_FILE_DATE_FORMAT),
                                FileSize = StringUtils.TryConvertTo<long>(mediaFile.FileSize),
                                IsActive = true,
                                CatalogEndDate = DateUtils.TryExtractDate(mediaFile.FileCatalogEndDate, ASSET_FILE_DATE_FORMAT),
                                Labels = mediaFile.Labels,
                                DynamicData = validateDynamicData
                            }, mediaFile.PpvModule));
                        }
                    }
                }
            }

            return assetFiles;
        }

        private static Dictionary<string, CDNAdapter> GetCDNAdaptersMapping(int groupId)
        {
            Dictionary<string, CDNAdapter> cdnAdapterMapping = new Dictionary<string, CDNAdapter>();
            var cdnAdapterList = ApiDAL.GetCDNAdapters(groupId);

            foreach (var cdnAdapter in cdnAdapterList)
            {
                if (!cdnAdapterMapping.ContainsKey(cdnAdapter.Name))
                {
                    cdnAdapterMapping.Add(cdnAdapter.Name, cdnAdapter);
                }
            }

            return cdnAdapterMapping;
        }
    }
}