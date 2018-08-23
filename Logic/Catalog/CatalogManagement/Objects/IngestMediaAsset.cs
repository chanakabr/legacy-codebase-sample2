using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;
using ApiObjects.Response;
using System;
using System.Linq;

namespace Core.Catalog.CatalogManagement
{
    [XmlRoot("feed")]
    public class IngestFeed
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
            this.Action = "insert";
            this.IsActive = "true";
            this.Erase = "false";
        }
    }

    public class IngestBasic
    {
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
    }
    
    public class IngestMultilingual
    {
        [XmlElement("value")]
        public List<IngestLanguageValue> Values { get; set; }

        internal Status Validate(string parameterName, string groupDefaultLanguageCode, HashSet<string> groupLanguageCodes, bool shouldCheckDefaultLanguageIsSent = true, bool shouldValidateValues = true, bool shouldValidateRequestLanguage = true)
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

                if (shouldValidateValues)
                {
                    if (string.IsNullOrEmpty(ingestLanguageValue.Text))
                    {
                        status.Set((int)eResponseStatus.NameRequired, parameterName + ".value.text cannot be empty");
                        return status;
                    }

                    if (groupLanguageCodes != null && !groupLanguageCodes.Contains(ingestLanguageValue.LangCode))
                    {
                        status.Set((int)eResponseStatus.Error, string.Format("language: {0} is not part of group supported languages", ingestLanguageValue.LangCode));
                        return status;
                    }
                }

                languageCodes.Add(ingestLanguageValue.LangCode);
            }

            if (shouldCheckDefaultLanguageIsSent && !languageCodes.Contains(groupDefaultLanguageCode))
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
        [XmlElement("watch_per_rule")]
        public string WatchPerRule { get; set; }

        [XmlElement("geo_block_rule")]
        public string GeoBlockRule { get; set; }

        [XmlElement("device_rule")]
        public string DeviceRule { get; set; }
    }

    public class IngestDates
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

        internal Status ValidateStrings(Dictionary<string, Topic> topicsMapBySystemName, string groupDefaultLanguageCode, HashSet<string> groupLanguageCodes)
        {
            if (Strings != null && Strings.MetaStrings != null && Strings.MetaStrings.Count > 0)
            {
                foreach (var metaString in Strings.MetaStrings)
                {
                    Topic topic = topicsMapBySystemName[metaString.Name];
                    if (topic != null && topic.Type == ApiObjects.MetaType.MultilingualString)
                    {
                        Status status = metaString.Validate("media.structure.strings.meta", groupDefaultLanguageCode, groupLanguageCodes);
                        if (status != null && status.Code != (int)eResponseStatus.OK)
                        {
                            return status;
                        }
                    }
                }
            }

            return new Status((int)eResponseStatus.OK);
        }

        internal Status ValidateMetaTags(string mainLanguageName, HashSet<string> groupLanguageCodes)
        {
            if (Metas != null && Metas.MetaTags != null && Metas.MetaTags.Count > 0)
            {
                foreach (var metaTag in Metas.MetaTags)
                {
                    if (metaTag.Containers != null && metaTag.Containers.Count > 0)
                    {
                        foreach (var item in metaTag.Containers)
                        {
                            if (item.Values != null && item.Values.Count > 0)
                            {
                                var otherTagLangCount = item.Values.Count(x => !mainLanguageName.Equals(x.LangCode));
                                if (otherTagLangCount > 0)
                                {
                                    return new Status((int)eResponseStatus.Error, "Tag translations are not allowed using ingest controller, please use tag controller");   
                                }
                            }
                        }
                    }
                }
            }

            return new Status((int)eResponseStatus.OK);
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
    }

    public class IngestSlimMeta : IngestBaseMeta
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class IngestStrings
    {
        [XmlElement("meta")]
        public List<IngestMeta> MetaStrings { get; set; }
    }

    public class IngestMeta : IngestBaseMeta
    {
        [XmlElement("value")]
        public List<IngestLanguageValue> Values { get; set; }

        // TODO SHIR - SET DEPENDENCY INJECTION WITH MULTILINGUAL
        internal Status Validate(string parameterName, string groupDefaultLanguageCode, HashSet<string> groupLanguageCodes, bool shouldCheckDefaultLanguageIsSent = true, bool shouldValidateValues = true, bool shouldValidateRequestLanguage = true)
        {
            Status status = new Status((int)eResponseStatus.OK);

            if (Values != null && Values.Count > 0)
            {
                HashSet<string> languageCodes = new HashSet<string>();

                foreach (IngestLanguageValue ingestLanguageValue in Values)
                {
                    if (languageCodes.Contains(ingestLanguageValue.LangCode))
                    {
                        status.Set((int)eResponseStatus.Error, string.Format("languageCode: {0} has been sent more than once", ingestLanguageValue.LangCode));
                        return status;
                    }

                    if (shouldValidateValues)
                    {
                        if (string.IsNullOrEmpty(ingestLanguageValue.Text))
                        {
                            status.Set((int)eResponseStatus.NameRequired, parameterName + ".value.text cannot be empty");
                            return status;
                        }

                        if (groupLanguageCodes != null && !groupLanguageCodes.Contains(ingestLanguageValue.LangCode))
                        {
                            status.Set((int)eResponseStatus.Error, string.Format("language: {0} is not part of group supported languages", ingestLanguageValue.LangCode));
                            return status;
                        }
                    }

                    languageCodes.Add(ingestLanguageValue.LangCode);
                }

                if (shouldCheckDefaultLanguageIsSent && !languageCodes.Contains(groupDefaultLanguageCode))
                {
                    status.Set((int)eResponseStatus.Error, string.Format("Default language must be one of the values sent for {0}", parameterName));
                    return status;
                }
            }

            return status;
        }
    }

    public class IngestMetas
    {
        [XmlElement("meta")]
        public List<IngestMetaTag> MetaTags { get; set; }
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

        [XmlAttribute("PPV_MODULE")]
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

        public IngestMediaFile()
        {
            this.Quality = "HIGH";
        }
    }
}