using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.SearchObjects;
using MoreLinq;
using MoreLinq.Extensions;
using Nest;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using TVinciShared;
using ESUtils = ElasticSearch.Common.Utils;

namespace ApiLogic.IndexManager.NestData
{
    public class NestMedia : NestBaseAsset
    {
        [PropertyName("media_id")]
        public int MediaId { get; set; }

        [PropertyName("media_type_id")]
        public int MediaTypeId { get; set; }

        [PropertyName("wp_type_id")]
        public int WPTypeID { get; set; }

        [PropertyName("device_rule_id")]
        public int DeviceRuleId { get; set; }

        [PropertyName("epg_identifier")]
        public string EpgIdentifier { get; set; }

        [PropertyName("user_types")]
        public List<int> UserTypes { get; set; }

        [PropertyName("allowed_countries")]
        public List<int> AllowedCountries { get; set; }

        [PropertyName("like_counter")]
        public int LikeCounter { get; set; }

        [PropertyName("blocked_countries")]
        public List<int> BlockedCountries { get; set; }

        [PropertyName("inheritance_policy")]
        public int? InheritancePolicy { get; set; }

        [Date(Name = "final_date")]
        public DateTime FinalEndDate { get; set; }

        [Date(Name = "catalog_start_date")]
        public DateTime CatalogStartDate { get; set; }

        [PropertyName("views")]
        public int Views { get; set; }

        [PropertyName("votes")]
        public int Votes { get; set; }

        [PropertyName("media_file_types")]
        public long[] MediaFileTypes { get; set; }

        [PropertyName("external_id")]
        public string CoGuid { get; set; }

        [PropertyName("entry_id")]
        public string EntryId { get; set; }

        [PropertyName("regions")]
        public List<int> Regions { get; set; }

        [PropertyName("geo_block_rule")]
        public int GeoBlockRule { get; set; }

        [PropertyName("free_file_types")]
        public List<int> FreeFileTypes { get; set; }

        [PropertyName("is_free")]
        public bool IsFree { get; set; }

        [PropertyName("suppressed")]
        public string Suppressed { get; set; }

        [PropertyName("rating")]
        public double Rating { get; set; }

        public NestMedia(ApiObjects.SearchObjects.Media media, string languageCode, int languageId)
        {
            MediaId = media.m_nMediaID;
            MediaTypeId = media.m_nMediaTypeID;
            WPTypeID = media.m_nWPTypeID;
            GroupID = media.m_nGroupID;
            IsActive = media.m_nIsActive == 1;
            DeviceRuleId = media.m_nDeviceRuleId;
            LikeCounter = media.m_nLikeCounter;
            Views = media.m_nViews;
            Votes = media.m_nVotes;
            Rating = media.m_dRating;
            MediaFileTypes = media.m_sMFTypes.IsNullOrEmpty()
                ? null
                : media.m_sMFTypes.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(x => long.Parse(x))
                    .ToArray();
            CoGuid = media.CoGuid;
            EntryId = media.EntryId;
            Regions = media.regions;
            GeoBlockRule = media.geoBlockRule;
            FreeFileTypes = media.freeFileTypes;
            IsFree = media.isFree;
            AllowedCountries = media.allowedCountries;
            BlockedCountries = media.blockedCountries;
            EpgIdentifier = media.epgIdentifier;
            InheritancePolicy = media.inheritancePolicy;
            Suppressed = media.suppressed;
            LanguageId = languageId;

            UserTypes = GetUserTypesFromString(media.m_sUserTypes);

            var langCode = languageCode;

            //metas
            var metasDict = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            var metas = media.m_dMeatsValues.Keys.ToDictionary(k => k.ToLower(),
                k => new HashSet<string>() { ESUtils.ReplaceDocumentReservedCharacters(media.m_dMeatsValues[k]) });
            metasDict.Add(langCode, metas);
            Metas = metasDict; //lang

            //tags
            var tagsDict = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            var tags = new Dictionary<string, HashSet<string>>();
            foreach (var tag in media.m_dTagValues)
            {
                var tagValues = tag.Value.Select(x => ESUtils.ReplaceDocumentReservedCharacters(x, false)).Distinct();
                tags[tag.Key.ToLower()] = new HashSet<string>(tagValues);
            }
            tagsDict.Add(langCode, new Dictionary<string, HashSet<string>>(tags));
            Tags = tagsDict; //lang

            //name
            var nameDict = new Dictionary<string, string>();
            nameDict.Add(langCode, ESUtils.ReplaceDocumentReservedCharacters(media.m_sName, false));
            NamesDictionary = nameDict; //lang

            //description
            var descriptionDict = new Dictionary<string, string>();
            descriptionDict.Add(langCode, ESUtils.ReplaceDocumentReservedCharacters(media.m_sDescription, false));
            Description = descriptionDict; //lang

            StartDate = DateTime.ParseExact(media.m_sStartDate, ESUtils.ES_DATE_FORMAT, null);
            EndDate = DateTime.ParseExact(media.m_sEndDate, ESUtils.ES_DATE_FORMAT, null);
            FinalEndDate = DateTime.ParseExact(media.m_sFinalEndDate, ESUtils.ES_DATE_FORMAT, null);
            CreateDate = DateTime.ParseExact(media.m_sCreateDate, ESUtils.ES_DATE_FORMAT, null);
            UpdateDate = DateTime.ParseExact(media.m_sUpdateDate, ESUtils.ES_DATE_FORMAT, null);
            CatalogStartDate = DateTime.ParseExact(media.CatalogStartDate, ESUtils.ES_DATE_FORMAT, null);
            CacheDate = DateTime.UtcNow;

            DocumentId = $"{MediaId}_{languageCode}";
        }

        private List<int> GetUserTypesFromString(string userTypes)
        {
            List<int> result = null;

            if (string.IsNullOrEmpty(userTypes))
            {
                result = new List<int>() { 0 };
            }
            else
            {
                var split = userTypes.Split(new []{';'},StringSplitOptions.RemoveEmptyEntries);
                result = split.Where(s => int.TryParse(s, out var parsed)).Select(int.Parse).ToList();
            }

            return result;
        }
    }
}