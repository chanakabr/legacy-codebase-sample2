using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;

namespace Core.Catalog
{
    [XmlRoot("feed")]
    public class IngestVODFeed
    {
        [XmlElement("export")]
        public IngestExport Export { get; set; }
    }

    public class IngestExport
    {
        [XmlElement("media")]
        public List<IngestMedia> MediaList { get; set; }
    }

    public class IngestMedia
    {
        public const string TRUE = "true";
        private const string FALSE = "false";
        public const string DELETE_ACTION = "delete";
        public const string INSERT_ACTION = "insert";
        public const string UPDATE_ACTION = "update";

        private const string MISSING_EXTERNAL_IDENTIFIER = "External identifier is missing ";
        private const string MISSING_ENTRY_ID = "entry_id is missing";
        private const string MISSING_ACTION = "action is missing";
        private const string CANNOT_BE_EMPTY = "{0} cannot be empty";

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
        public IngestBasic Basic { get; set; }

        [XmlElement("structure")]
        public IngestStructure Structure { get; set; }

        [XmlElement("files")]
        public IngestFiles Files { get; set; }

        public IngestMedia()
        {
            this.Action = INSERT_ACTION;
            this.IsActive = "true";
            this.Erase = "false";
        }

        public bool ValidateMetas(CatalogGroupCache cache, ref IngestResponse response, int index, out List<Metas> metas, out List<Tags> tags, out HashSet<long> topicIdsToRemove)
        {
            metas = new List<Metas>();
            tags = new List<Tags>();
            topicIdsToRemove = new HashSet<long>();
            if (this.Structure == null)
            {
                response.AddError("media.Structure cannot be empty");
                response.AssetsStatus[index].Status.Set((int)eResponseStatus.NameRequired, "media.Structure cannot be empty");
                return false;
            }

            Status structureValidationStatus = this.Structure.Validate(cache, ref topicIdsToRemove, ref metas, ref tags);
            if (!structureValidationStatus.IsOkStatusCode())
            {
                response.AddError(structureValidationStatus.Message);
                response.AssetsStatus[index].Status = structureValidationStatus;
                return false;
            }

            return true;
        }

        public bool ValidateOnInsertOrUpdate(int groupId, CatalogGroupCache cache, long mediaId, ref IngestResponse response, int index)
        {
            var isInsertAction = mediaId == 0;
            if (isInsertAction && !TRUE.Equals(this.IsActive) && !FALSE.Equals(this.IsActive))
            {
                response.AddError("media.IsActive cannot be empty");
                response.AssetsStatus[index].Status.Set((int)eResponseStatus.NameRequired, "media.IsActive cannot be empty");
                return false;
            }

            if (this.Basic == null)
            {
                string errMsg = string.Format(CANNOT_BE_EMPTY, "media.Basic");
                response.AddError(errMsg);
                response.AssetsStatus[index].Status.Set((int)eResponseStatus.NameRequired, errMsg);
                return false;
            }

            var warnings = new List<Status>();
            Status basicValidationStatus = this.Basic.Validate(groupId, ref warnings, isInsertAction, cache);
            if (!basicValidationStatus.IsOkStatusCode())
            {
                response.AddError(basicValidationStatus.Message);
                response.AssetsStatus[index].Status = basicValidationStatus;
                return false;
            }

            if (warnings.Count > 0)
            {
                foreach (var warning in warnings)
                {
                    response.AddError(warning.Message);
                    response.AssetsStatus[index].Warnings.Add(warning);
                }
            }

            return true;
        }

        public bool Validate(int groupId, CatalogGroupCache cache, ref IngestResponse response, int index, out long mediaId)
        {
            mediaId = 0;

            // check media.CoGuid
            if (string.IsNullOrEmpty(this.CoGuid))
            {
                response.AssetsStatus[index].Status.Set((int)eResponseStatus.MissingExternalIdentifier, MISSING_EXTERNAL_IDENTIFIER);
                response.AddError("Missing co_guid");
                return false;
            }
            
            if (string.IsNullOrEmpty(this.EntryId))
            {
                response.AssetsStatus[index].Warnings.Add(new Status((int)IngestWarnings.MissingEntryId, MISSING_ENTRY_ID));
            }

            if (string.IsNullOrEmpty(this.Action))
            {
                response.AssetsStatus[index].Warnings.Add(new Status((int)IngestWarnings.MissingAction, MISSING_ACTION));
            }
            this.Action = this.Action.Trim().ToLower();
            this.IsActive = this.IsActive.Trim().ToLower();

            mediaId = BulkAssetManager.GetMediaIdByCoGuid(groupId, this.CoGuid);

            return true;
        }
    }

    public class IngestBasic
    {
        private const string ITEM_TYPE_NOT_RECOGNIZED = "Item type \"{0}\" not recognized";

        [XmlElement("media_type")]
        public string MediaType { get; set; }

        [XmlElement("name")]
        public IngestMultilingual Name { get; set; }

        [XmlElement("description")]
        public IngestMultilingual Description { get; set; }

        [XmlElement("thumb")]
        public IngestThumb Thumb { get; set; }

        [XmlElement("pic_ratios")]
        public IngestPicsRatio PicsRatio { get; set; }

        [XmlElement("rules")]
        public IngestRules Rules { get; set; }

        [XmlElement("dates")]
        public IngestDates Dates { get; set; }

        public Status Validate(int groupId, ref List<Status> warnings, bool isInsertAction, CatalogGroupCache cache)
        {
            if (this.Rules == null)
            {
                return new Status((int)eResponseStatus.NameRequired, "media.Basic.Rules cannot be empty");
            }

            this.Rules.Validate(groupId, ref warnings);
            
            if (this.Dates == null)
            {
                return new Status((int)eResponseStatus.NameRequired, "media.Basic.Dates cannot be empty");
            }

            if (isInsertAction)
            {
                // check MediaType
                if (string.IsNullOrEmpty(this.MediaType) ||
                    !cache.AssetStructsMapBySystemName.ContainsKey(this.MediaType) ||
                    cache.AssetStructsMapBySystemName[this.MediaType].Id == 0)
                {
                    return new Status((int)eResponseStatus.InvalidMediaType, string.Format(ITEM_TYPE_NOT_RECOGNIZED, this.MediaType));
                }

                if (this.Name == null)
                {
                    return new Status((int)eResponseStatus.NameRequired, "media.Basic.Name cannot be empty");
                }

                if (this.Description == null)
                {
                    return new Status((int)eResponseStatus.NameRequired, "media.Basic.Description cannot be empty");
                }
            }

            if (isInsertAction || (this.Name != null && this.Name.Values != null && this.Name.Values.Count > 0))
            {
                Status nameValidationStatus = this.Name.Validate("media.basic.name", cache, isInsertAction);
                if (!nameValidationStatus.IsOkStatusCode())
                {
                    return nameValidationStatus;
                }
            }

            if (isInsertAction || (this.Description != null && this.Description.Values != null && this.Description.Values.Count > 0))
            {
                Status descriptionValidationStatus = this.Description.Validate("media.basic.description", cache);
                if (!descriptionValidationStatus.IsOkStatusCode())
                {
                    return descriptionValidationStatus;
                }
            }
            
            return Status.Ok;
        }
    }
    
    public class IngestMultilingual : IngestBaseMeta
    {
        [XmlElement("value")]
        public List<IngestLanguageValue> Values { get; set; }

        public Status Validate(string parameterName, CatalogGroupCache cache, bool validateText = false)
        {
            Status status = new Status((int)eResponseStatus.OK);

            if (Values == null || Values.Count == 0)
            {
                status.Set((int)eResponseStatus.NameRequired, parameterName + " cannot be empty");
                return status;
            }

            HashSet<string> languageCodes = new HashSet<string>();

            foreach (IngestLanguageValue ingestLanguageValue in Values)
            {
                if (languageCodes.Contains(ingestLanguageValue.LangCode))
                {
                    status.Set((int)eResponseStatus.Error, string.Format("languageCode: {0} has been sent more than once", ingestLanguageValue.LangCode));
                    return status;
                }
                
                if (!cache.LanguageMapByCode.ContainsKey(ingestLanguageValue.LangCode))
                {
                    status.Set((int)eResponseStatus.Error, string.Format("language: {0} is not part of group supported languages", ingestLanguageValue.LangCode));
                    return status;
                }

                if (validateText && string.IsNullOrEmpty(ingestLanguageValue.Text))
                {
                    status.Set((int)eResponseStatus.NameRequired, parameterName + " cannot be empty");
                    return status;
                }

                languageCodes.Add(ingestLanguageValue.LangCode);
            }

            // Check Default Language Is Sent
            if (!languageCodes.Contains(cache.GetDefaultLanguage().Code))
            {
                status.Set((int)eResponseStatus.Error, string.Format("Default language must be one of the values sent for {0}", parameterName));
                return status;
            }
            
            return status;
        }
    }

    public class IngestThumb
    {
        [XmlAttribute("url")]
        public string Url { get; set; }
    }

    public class IngestPicsRatio
    {
        [XmlElement("ratio")]
        public List<IngestRatio> Ratios { get; set; }
    }

    public class IngestRatio
    {
        [XmlAttribute("thumb")]
        public string Thumb { get; set; }

        [XmlAttribute("ratio")]
        public string RatioText { get; set; }
    }

    public class IngestRules
    {
        private const string GEO_BLOCK_RULE_NOT_RECOGNIZED = "Geo block rule not recognized";
        private const string DEVICE_RULE_NOT_RECOGNIZED = "Device rule not recognized";

        [XmlElement("watch_per_rule")]
        public string WatchPerRule { get; set; }

        [XmlElement("geo_block_rule")]
        public string GeoBlockRule { get; set; }

        [XmlElement("device_rule")]
        public string DeviceRule { get; set; }

        public void Validate(int groupId, ref List<Status> warnings)
        {
            if (!string.IsNullOrEmpty(this.GeoBlockRule))
            {
                var geoBlockRuleId = TvmRuleManager.GetGeoBlockRuleId(groupId, this.GeoBlockRule);
                if (!geoBlockRuleId.HasValue || geoBlockRuleId.Value == 0)
                {
                    warnings.Add(new Status((int)IngestWarnings.NotRecognizedGeoBlockRule, GEO_BLOCK_RULE_NOT_RECOGNIZED));
                }
            }

            if (!string.IsNullOrEmpty(this.DeviceRule))
            {
                var deviceRuleId = TvmRuleManager.GetDeviceRuleId(groupId, this.DeviceRule);
                if (!deviceRuleId.HasValue || deviceRuleId.Value == 0)
                {
                    warnings.Add(new Status((int)IngestWarnings.NotRecognizedDeviceRule, DEVICE_RULE_NOT_RECOGNIZED));
                }
            }
        }
    }

    public class IngestDates
    {
        [XmlElement("catalog_start")]
        public string CatalogStart { get; set; }

        [XmlElement("start")]
        public string Start { get; set; }

        [XmlElement("catalog_end")]
        public string CatalogEnd { get; set; }

        [XmlElement("final_end")]
        public string FinalEnd { get; set; }
    }

    public class IngestLanguageValue
    {
        [XmlAttribute("lang")]
        public string LangCode { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public class IngestStructure
    {
        [XmlElement("booleans")]
        public IngestSlimMetas Booleans { get; set; }

        [XmlElement("doubles")]
        public IngestSlimMetas Doubles { get; set; }

        [XmlElement("dates")]
        public IngestSlimMetas Dates { get; set; }

        [XmlElement("strings")]
        public IngestStrings Strings { get; set; }

        [XmlElement("metas")]
        public IngestMetas Metas { get; set; }

        public Status Validate(CatalogGroupCache cache, ref HashSet<long> topicIdsToRemove, ref List<Metas> metas, ref List<Tags> tags)
        {
            if (this.Doubles != null)
            {
                var doublesValidationStatus = this.Doubles.Validate(MetaType.Number, cache, ref topicIdsToRemove, ref metas);
                if (!doublesValidationStatus.IsOkStatusCode())
                {
                    return doublesValidationStatus;
                }
            }

            if (this.Booleans != null)
            {
                var boolValidationStatus = this.Booleans.Validate(MetaType.Bool, cache, ref topicIdsToRemove, ref metas);
                if (!boolValidationStatus.IsOkStatusCode())
                {
                    return boolValidationStatus;
                }
            }

            if (this.Dates != null)
            {
                var datesValidationStatus = this.Dates.Validate(MetaType.DateTime, cache, ref topicIdsToRemove, ref metas);
                if (!datesValidationStatus.IsOkStatusCode())
                {
                    return datesValidationStatus;
                }
            }
            
            if (Strings != null)
            {
                Status stringsValidationStatus = this.Strings.Validate(cache, ref topicIdsToRemove, ref metas);
                if (!stringsValidationStatus.IsOkStatusCode())
                {
                    return stringsValidationStatus;
                }
            }

            if (this.Metas != null)
            {
                var metasValidation = this.Metas.Validate(cache, ref topicIdsToRemove);
                if (!metasValidation.IsOkStatusCode())
                {
                    return metasValidation.Status;
                }

                tags = metasValidation.Objects;
            }

            return Status.Ok;
        }
    }

    public class IngestBaseMeta
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("ml_handling")]
        public string MlHandling { get; set; }

        public IngestBaseMeta()
        {
            this.MlHandling = "unique";
        }
    }

    public class IngestSlimMetas
    {
        [XmlElement("meta")]
        public List<IngestSlimMeta> Metas { get; set; }
        private const string One = "1";

        public Status Validate(MetaType metaType, CatalogGroupCache cache, ref HashSet<long> topicIdsToRemove, ref List<Metas> metas)
        {
            if (this.Metas == null || this.Metas.Count == 0) { return Status.Ok; }

            var metaTypeName = metaType.ToString();
            foreach (var meta in this.Metas)
            {
                meta.Name = meta.Name.Trim();

                if (!cache.TopicsMapBySystemNameAndByType.ContainsKey(meta.Name) ||
                    !cache.TopicsMapBySystemNameAndByType[meta.Name].ContainsKey(metaTypeName))
                {
                    continue;
                }

                var removeTopic = string.IsNullOrEmpty(meta.Value);
                if (topicIdsToRemove != null && removeTopic)
                {
                    topicIdsToRemove.Add(cache.TopicsMapBySystemNameAndByType[meta.Name][metaTypeName].Id);
                }
                else if (!removeTopic)
                {
                    var metaValue = metaType != MetaType.Bool || One.Equals(meta.Value) ? meta.Value : IngestMedia.TRUE.Equals(meta.Value) ? One : "0";
                    metas.Add(new Metas(new TagMeta(meta.Name, metaTypeName), metaValue));
                }
            }

            return Status.Ok;
        }
    }

    public class IngestSlimMeta : IngestBaseMeta
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class IngestStrings
    {
        [XmlElement("meta")]
        public List<IngestMultilingual> MetaStrings { get; set; }

        public Status Validate(CatalogGroupCache cache, ref HashSet<long> topicIdsToRemove, ref List<Metas> validMetaStrings)
        {
            if (this.MetaStrings != null && this.MetaStrings.Count > 0)
            {
                var multilingualStringMetaType = MetaType.MultilingualString.ToString();
                var stringMetaType = MetaType.String.ToString();

                foreach (var stringMeta in this.MetaStrings)
                {
                    stringMeta.Name = stringMeta.Name.Trim();
                    var currMetaType = multilingualStringMetaType;

                    if (stringMeta.Values == null ||
                        stringMeta.Values.Count == 0 ||
                        !cache.TopicsMapBySystemNameAndByType.ContainsKey(stringMeta.Name) ||
                        (!cache.TopicsMapBySystemNameAndByType[stringMeta.Name].ContainsKey(multilingualStringMetaType) &&
                            !cache.TopicsMapBySystemNameAndByType[stringMeta.Name].ContainsKey(stringMetaType)))
                    {
                        continue;
                    }
                    else if (cache.TopicsMapBySystemNameAndByType[stringMeta.Name].ContainsKey(stringMetaType))
                    {
                        currMetaType = stringMetaType;
                    }

                    // check if need to add or remove meta from asset
                    if (topicIdsToRemove != null && stringMeta.Values.Any(x => cache.GetDefaultLanguage().Code.Equals(x.LangCode) && string.IsNullOrEmpty(x.Text)))
                    {
                        var topic = cache.TopicsMapBySystemNameAndByType[stringMeta.Name][currMetaType];
                        if (!topicIdsToRemove.Contains(topic.Id)) { topicIdsToRemove.Add(topic.Id); }
                    }
                    else
                    {
                        Status status = stringMeta.Validate("media.structure.strings.meta", cache);
                        if (!status.IsOkStatusCode())
                        {
                            return status;
                        }

                        validMetaStrings.Add(new Metas(new TagMeta(stringMeta.Name, currMetaType),
                                             stringMeta.Values.FirstOrDefault(x => x.LangCode.Equals(cache.GetDefaultLanguage().Code)).Text,
                                             stringMeta.Values.Where(x => !x.LangCode.Equals(cache.GetDefaultLanguage().Code) && !string.IsNullOrEmpty(x.Text))
                                                              .Select(x => new LanguageContainer(x.LangCode, x.Text))));
                    }
                }
            }

            return Status.Ok;
        }
    }
    
    public class IngestMetas
    {
        [XmlElement("meta")]
        public List<IngestMetaTag> MetaTags { get; set; }

        public GenericListResponse<Tags> Validate(CatalogGroupCache cache, ref HashSet<long> topicIdsToRemove)
        {
            var response = new GenericListResponse<Tags>();
            response.SetStatus(eResponseStatus.OK);
            string parameterName = "media.structure.metas.meta";

            if (MetaTags != null && MetaTags.Count > 0)
            {
                var metaTypeTag = MetaType.Tag.ToString();
                var metaTagsToContainers = new List<Tuple<string, List<IngestLanguageValue>, int>>();
                var metaTagsContainersToDefaultValue = new Dictionary<string, Dictionary<int, string>>();
                var metaTagsContainersToOtherValue = new Dictionary<string, Dictionary<int, Dictionary<string, string>>>();
                foreach (var metaTag in MetaTags)
                {
                    var containersCounter = 0;
                    
                    if (metaTag.Containers != null && metaTag.Containers.Count > 0)
                    {
                        metaTag.Name = metaTag.Name.Trim();

                        if (!cache.TopicsMapBySystemNameAndByType.ContainsKey(metaTag.Name) ||
                            !cache.TopicsMapBySystemNameAndByType[metaTag.Name].ContainsKey(metaTypeTag))
                        {
                            continue;
                        }

                        if (metaTag.Containers.Count == 1 &&
                            metaTag.Containers[0].Values != null &&
                            metaTag.Containers[0].Values.Any(x => cache.GetDefaultLanguage().Code.Equals(x.LangCode) && string.IsNullOrEmpty(x.Text)))
                        {
                            topicIdsToRemove.Add(cache.TopicsMapBySystemNameAndByType[metaTag.Name][metaTypeTag].Id);
                        }
                        else
                        {
                            if (metaTagsContainersToDefaultValue.ContainsKey(metaTag.Name))
                            {
                                response.SetStatus(eResponseStatus.Error, string.Format("meta: {0} has been sent more than once.", metaTag.Name));
                                return response;
                            }

                            metaTagsToContainers.AddRange(metaTag.Containers.Select(y => new Tuple<string, List<IngestLanguageValue>, int>(metaTag.Name, y.Values, containersCounter++)));
                            metaTagsContainersToDefaultValue.Add(metaTag.Name, new Dictionary<int, string>());
                            metaTagsContainersToOtherValue.Add(metaTag.Name, new Dictionary<int, Dictionary<string, string>>());
                        }
                    }
                }

                var metaTagsToLanguages = new List<Tuple<string, IngestLanguageValue, int>>();
                foreach (var metaTagContainer in metaTagsToContainers)
                {
                    var metaTagName = metaTagContainer.Item1;
                    var languagesValues = metaTagContainer.Item2;
                    var containerIndex = metaTagContainer.Item3;

                    if (languagesValues == null || languagesValues.Count == 0)
                    {
                        response.SetStatus(eResponseStatus.NameRequired, parameterName + " cannot be empty");
                        return response;
                    }

                    metaTagsToLanguages.AddRange(languagesValues.Select(y => new Tuple<string, IngestLanguageValue, int>(metaTagName, y, containerIndex)));
                }

                var defaultValuesCount = 0;
                foreach (var metaTagLanguage in metaTagsToLanguages)
                {
                    var metaTagName = metaTagLanguage.Item1;
                    var languageValue = metaTagLanguage.Item2;
                    var containerIndex = metaTagLanguage.Item3;

                    // Validate LangCode
                    if (!cache.LanguageMapByCode.ContainsKey(languageValue.LangCode))
                    {
                        response.SetStatus(eResponseStatus.Error, string.Format("language: {0} is not part of group supported languages", languageValue.LangCode));
                        return response;
                    }
                    
                    // if lang is not main 
                    if (string.IsNullOrEmpty(languageValue.Text)) { continue; }

                    // add default value
                    if (cache.GetDefaultLanguage().Code.Equals(languageValue.LangCode))
                    {
                        // check if default language allready have this value
                        if (metaTagsContainersToDefaultValue[metaTagName].ContainsKey(containerIndex))
                        {
                            response.SetStatus(eResponseStatus.Error, string.Format("For meta: {0} the value: {1} has been sent more than once default language: {2}",
                                                                                    metaTagName, languageValue.Text, languageValue.LangCode));
                            return response;
                        }

                        metaTagsContainersToDefaultValue[metaTagName].Add(containerIndex, languageValue.Text);
                        defaultValuesCount++;
                    }
                    else
                    {
                        if (!metaTagsContainersToOtherValue[metaTagName].ContainsKey(containerIndex))
                        {
                            metaTagsContainersToOtherValue[metaTagName].Add(containerIndex, new Dictionary<string, string>());
                        }

                        // Validate Value
                        if (metaTagsContainersToOtherValue[metaTagName][containerIndex].ContainsKey(languageValue.LangCode))
                        {
                            response.SetStatus(eResponseStatus.Error, string.Format("For meta: {0} the language: {1} has been sent more than once in the same container",
                                                                                    metaTagName, languageValue.LangCode));
                            return response;
                        }
                        metaTagsContainersToOtherValue[metaTagName][containerIndex].Add(languageValue.LangCode, languageValue.Text);
                    }
                }

                // Check Default Language Is Sent
                if (metaTagsToContainers.Count != defaultValuesCount)
                {
                    response.SetStatus(eResponseStatus.Error, "Every meta must have at least one value with default language");
                    return response;
                }

                response.Objects = new List<Tags>();
                foreach (var metaTagContainer in metaTagsContainersToDefaultValue)
                {
                    var metaTagName = metaTagContainer.Key;
                    var tagMeta = new TagMeta(metaTagName, metaTypeTag);
                    var defaultValues = new List<string>(metaTagContainer.Value.Select(x => x.Value));
                    var otherValues = metaTagsContainersToOtherValue[metaTagName].Select(x => x.Value.Select(y => new LanguageContainer(y.Key, y.Value)).ToArray());
                    response.Objects.Add(new Tags(tagMeta, defaultValues, otherValues));
                }
            }

            return response;
        }
    }

    public class IngestMetaTag : IngestBaseMeta
    {
        [XmlElement("container")]
        public List<IngestMultilingual> Containers { get; set; }
    }

    public class IngestFiles
    {
        [XmlElement("file")]
        public List<IngestMediaFile> MediaFiles { get; set; }
    }

    public class IngestMediaFile
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

        [XmlAttribute("alt_cdn_name")]
        public string AltCdnName { get; set; }

        [XmlAttribute("cdn_code")]
        public string CdnCode { get; set; }

        [XmlAttribute("alt_cdn_code")]
        public string AltCdnCode { get; set; }

        [XmlAttribute("co_guid")]
        public string CoGuid { get; set; }

        [XmlAttribute("alt_co_guid")]
        public string AltCoGuid { get; set; }

        [XmlAttribute("billing_type")]
        public string BillingType { get; set; }
        
        [XmlAttribute("ppv_module")]
        public string PpvModule { get; set; }

        [XmlAttribute("product_code")]
        public string ProductCode { get; set; }

        [XmlAttribute("lang")]
        public string Language { get; set; }

        [XmlAttribute("default")]
        public string IsDefaultLanguage { get; set; }

        [XmlAttribute("output_protection_level")]
        public string OutputProtecationLevel { get; set; }

        [XmlAttribute("file_start_date")]
        public string FileStartDate { get; set; }

        [XmlAttribute("file_end_date")]
        public string FileEndDate { get; set; }

        [XmlAttribute("file_size")]
        public string FileSize { get; set; }

        [XmlAttribute("file_catalog_end_date")]
        public string FileCatalogEndDate { get; set; }

        [XmlAttribute("labels")]
        public string Labels { get; set; }
        
        [XmlElement("dynamicData")]
        public IngestDynamicData<string, string[]> DynamicData { get; set; }

        public IngestMediaFile()
        {
            Quality = "HIGH";
            DynamicData = new IngestDynamicData<string, string[]>
            {
                Items = Array.Empty<IngestKeyValuePair<string, string[]>>()
            };
        }
    }

    public class IngestDynamicData<TKey, TValue>
    {
        [XmlElement("keyValues")]
        public IngestKeyValuePair<TKey, TValue>[] Items { get; set; }
    }

    public class IngestKeyValuePair<TKey, TValue>
    {
        [XmlElement("key")]
        public TKey Key { get; set; }

        [XmlElement("value")]
        public TValue Value { get; set; }
    }
}