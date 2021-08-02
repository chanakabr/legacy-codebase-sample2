using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.SearchObjects;
using MoreLinq;
using MoreLinq.Extensions;
using Nest;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using ESUtils = ElasticSearch.Common.Utils;

namespace ApiLogic.IndexManager.NestData
{
    public class NestMedia
    {
        [PropertyName("media_id")]
        public int MediaId  { get; set; }
        
        [PropertyName("media_type_id")]
        public int MediaTypeId { get; set; }
        
        [PropertyName("wp_type_id")]
        public int WPTypeID { get; set; }
        
        [PropertyName("group_id")]
        public int GroupID { get; set; }
        
        [PropertyName("is_active")]
        public bool IsActive { get; set; }

        [PropertyName("device_rule_id")]
        public int DeviceRuleId { get; set; }

        [PropertyName("epg_channel_id")]
        public string EpgIdentifier { get; set; }

        [PropertyName("user_types")]
        public string UserTypes { get; set; }

        [PropertyName("language_id")]
        public int LanguageId { get; set; }

        [PropertyName("allowed_countries")]
        public List<int> AllowedCountries { get; set; }

        [PropertyName("like_counter")]
        public int LikeCounter { get; set; }
        
        [PropertyName("blocked_countries")]
        public List<int> BlockedCountries { get; set; }
        
        [PropertyName("inheritance_policy")]
        public int? InheritancePolicy { get; set; }
        
        [Date(Name= "start_date")] 
        public DateTime StartDate { get; set; }
        
        [Date(Name= "end_date")] 
        public DateTime EndDate { get; set; }
        
        [Date(Name= "final_date")] 
        public DateTime FinalEndDate { get; set; }
        
        [Date(Name= "create_date")] 
        public DateTime CreateDate { get; set; }
        
        [Date(Name= "update_date")] 
        public DateTime UpdateDate { get; set; }

        [Date(Name= "catalog_start_date")] 
        public DateTime CatalogStartDate { get; set; }

        [Date(Name= "cache_date")] 
        public DateTime CacheDate { get; set; }
        
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

        [PropertyName("name")]
        public Dictionary<string,string> Name { get; set; } //lang

        [PropertyName("description")]
        public Dictionary<string,string> Description { get; set; } //lang

        [PropertyName("tags")]
        public Dictionary<string,Dictionary<string, HashSet<string>>> TagValues { get; set; } //lang

        [PropertyName("metas")]
        public Dictionary<string,Dictionary<string, string>> MeatsValues { get; set; } //lang

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

        public NestMedia(Media media, string languageCode, int languageId)
        {
            MediaId = media.m_nMediaID;
            MediaTypeId = media.m_nWPTypeID;
            WPTypeID = media.m_nWPTypeID;
            GroupID = media.m_nGroupID;
            IsActive = media.m_nIsActive==1;
            DeviceRuleId = media.m_nDeviceRuleId;
            LikeCounter = media.m_nLikeCounter;
            Views = media.m_nViews;
            Votes = media.m_nVotes;
            Rating = media.m_dRating;
            MediaFileTypes = media.m_sMFTypes.Split(';').Select(x=> long.Parse(x)).ToArray();
            UserTypes = media.m_sUserTypes;
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

            var langCode = languageCode;
            
            //metas
            var metasDict = new Dictionary<string,Dictionary<string, string>> ();
            var metas = media.m_dMeatsValues.Keys.ToDictionary(k => k,
                k => ESUtils.ReplaceDocumentReservedCharacters(media.m_dMeatsValues[k]));
            metasDict.Add(langCode,metas);
            MeatsValues = metasDict; //lang
            
            //tags
            var tagsDict = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            var tags= new Dictionary<string, HashSet<string>>();
            foreach (var tag in media.m_dTagValues)
            {
                var tagValues = tag.Value.Select(x => ESUtils.ReplaceDocumentReservedCharacters(x, false)).Distinct();
                tags[tag.Key] = new HashSet<string>(tagValues);
            }
            tagsDict.Add(langCode,new Dictionary<string, HashSet<string>> (tags));
            TagValues = tagsDict; //lang
            
            //name
            var nameDict = new Dictionary<string, string>();
            nameDict.Add(langCode, ESUtils.ReplaceDocumentReservedCharacters(media.m_sName, false));
            Name = nameDict; //lang
            
            //description
            var descriptionDict = new Dictionary<string, string>();
            descriptionDict.Add(langCode, ESUtils.ReplaceDocumentReservedCharacters(media.m_sDescription,false));
            Description = descriptionDict; //lang
            
            StartDate = DateTime.ParseExact(media.m_sStartDate, ESUtils.ES_DATE_FORMAT, null);
            EndDate = DateTime.ParseExact(media.m_sEndDate, ESUtils.ES_DATE_FORMAT, null); 
            FinalEndDate = DateTime.ParseExact(media.m_sFinalEndDate, ESUtils.ES_DATE_FORMAT, null);
            CreateDate = DateTime.ParseExact(media.m_sCreateDate, ESUtils.ES_DATE_FORMAT, null);
            UpdateDate = DateTime.ParseExact(media.m_sUpdateDate, ESUtils.ES_DATE_FORMAT, null);
            CatalogStartDate =  DateTime.ParseExact(media.CatalogStartDate, ESUtils.ES_DATE_FORMAT, null);
            CacheDate = DateTime.UtcNow;
        }
    }
}