using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;

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

        internal void Validate(string parameterName, string GroupDefaultLanguageCode,  bool shouldCheckDefaultLanguageIsSent = true, bool shouldValidateValues = true, bool shouldValidateRequestLanguage = true)
        {
        //    if (Values != null && Values.Count > 0)
        //    {
        //        if (string.IsNullOrEmpty(GroupDefaultLanguageCode))
        //        {
        //            GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
        //        }

        //        if (string.IsNullOrEmpty(RequestLanguageCode))
        //        {
        //            RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
        //        }

        //        HashSet<string> languageCodes = new HashSet<string>();
        //        HashSet<string> groupLanguageCodes = Utils.Utils.GetGroupLanguageCodes();

        //        foreach (KalturaTranslationToken token in Values)
        //        {
        //            if (languageCodes.Contains(token.Language))
        //            {
        //                throw new BadRequestException(ApiException.DUPLICATE_LANGUAGE_SENT, token.Language);
        //            }

        //            if (shouldValidateValues)
        //            {

        //                if (string.IsNullOrEmpty(token.Value))
        //                {
        //                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTranslationToken.value");
        //                }


        //                if (!groupLanguageCodes.Contains(token.Language))
        //                {
        //                    throw new BadRequestException(ApiException.GROUP_DOES_NOT_CONTAIN_LANGUAGE, token.Language);
        //                }
        //            }

        //            languageCodes.Add(token.Language);
        //        }

        //        if (shouldCheckDefaultLanguageIsSent && !languageCodes.Contains(GroupDefaultLanguageCode))
        //        {
        //            throw new BadRequestException(ApiException.DEFUALT_LANGUAGE_MUST_BE_SENT, parameterName);
        //        }

        //        if (shouldValidateRequestLanguage)
        //        {
        //            if (string.IsNullOrEmpty(RequestLanguageCode) || RequestLanguageCode != "*")
        //            {
        //                throw new BadRequestException(ApiException.GLOBAL_LANGUAGE_MUST_BE_ASTERISK_FOR_WRITE_ACTIONS);
        //            }
        //        }
        //    }
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