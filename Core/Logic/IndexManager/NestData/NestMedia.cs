using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ESUtils = ElasticSearch.Common.Utils;

namespace ApiLogic.IndexManager.NestData
{
    public class NestMedia
    {
        public int MediaId  { get; set; }
        public int MediaTypeId { get; set; }
        public int WPTypeID { get; set; }
        public int GroupID { get; set; }
        public int IsActive { get; set; }
        public int DeviceRuleId { get; set; }
        public int LikeCounter { get; set; }
        public int Views { get; set; }
        public double Rating { get; set; }
        public int Votes { get; set; }

        public string MFTypes { get; set; }
        public string Name { get; set; } //lang
        public string Description { get; set; } //lang
        public string UserTypes { get; set; }
        public string CoGuid { get; set; }
        public string EntryId { get; set; }
        public Dictionary<string, HashSet<string>> TagValues { get; set; }
        public Dictionary<string, string> MeatsValues { get; set; }
        public List<int> Regions { get; set; }
        public int GeoBlockRule { get; set; }
        public List<int> FreeFileTypes { get; set; }
        public bool IsFree { get; set; }
        public List<int> AllowedCountries { get; set; }
        public List<int> BlockedCountries { get; set; }
        public string EpgIdentifier { get; set; }
        public int? InheritancePolicy { get; set; }
        public string Suppressed { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime FinalEndDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CatalogStartDate { get; set; }
        public DateTime CacheDate { get; set; }

        public NestMedia(Media media, string languageCode)
        {
            MediaId = media.m_nMediaID;
            MediaTypeId = media.m_nWPTypeID;
            WPTypeID = media.m_nWPTypeID;
            GroupID = media.m_nGroupID;
            IsActive = media.m_nIsActive;
            DeviceRuleId = media.m_nDeviceRuleId;
            LikeCounter = media.m_nLikeCounter;
            Views = media.m_nViews;
            Votes = media.m_nVotes;
            Rating = media.m_dRating;
            MFTypes = media.m_sMFTypes;
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

            TagValues = media.m_dTagValues;
            MeatsValues = media.m_dMeatsValues;
            Name = media.m_sName;
            Description = media.m_sDescription;
            
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