using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ApiObjects;
using ApiObjects.Epg;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RestSharp;
using ESUtils = ElasticSearch.Common.Utils;

namespace ApiLogic.IndexManager.NestData
{
    [ElasticsearchType(RelationName = "epg")]
    public class NestEpg
    {
        #region DataMembers

        [PropertyName("epg_id")]
        public ulong EpgID  { get; set; }
        
        [PropertyName("epg_identifier")]
        public string EpgIdentifier { get; set; }
        
        [PropertyName("external_id")]
        public string ExternalId { get; set; }

        [PropertyName("is_active")]
        public bool IsActive { get; set; }

        [PropertyName("group_id")]
        public int GroupID { get; set; }

        [PropertyName("epg_channel_id")]
        public int ChannelID { get; set; }
        
        [PropertyName("name")]
        public Dictionary<string, string> Name { get; set; }

        [PropertyName("description")]
        public  Dictionary<string,string> Description { get; set; }

        [Date(Name = "create_date" )]
        public DateTime CreateDate { get; set; }
        
        [Date(Name = "update_date" )]
        public DateTime UpdateDate { get; set; }
        
        [Date(Name = "start_date" )]
        public DateTime StartDate { get; set; }
        
        [Date(Name = "end_date" )]
        public DateTime EndDate { get; set; }

        [Date(Name = "search_end_date" )]
        public DateTime SearchEndDate { get; set; }
        
        [Date(Name = "cache_date" )]
        public DateTime CacheDate { get; set; }
        
        [PropertyName("linear_media_id")]
        public long? LinearMediaId { get; set; }

        [PropertyName("metas")]
        public Dictionary<string,Dictionary<string, List<string>>> Metas { get; set; }

        [PropertyName("tags")]
        public Dictionary<string,Dictionary<string, List<string>>> Tags { get; set; }

        [PropertyName("language")]
        public string Language { get; set; }

        [PropertyName("language_id")]
        public int LanguageId { get; set; }

        [PropertyName("date_routing")]
        public string DateRouting { get; set; }

        [PropertyName("enable_cdvr")]
        public int? EnableCDVR { get; set; }

        [PropertyName("enable_catch_up")]
        public int? EnableCatchUp { get; set; }
        
        [PropertyName ("crid")]
        public string Crid { get; set; }

        [PropertyName ("document_id")]
        public string DocumentId { get; set; }
        
        [PropertyName ("is_auto_fill")]
        public bool IsAutoFill { get; set; }
        
        [PropertyName ("suppressed")]
        public string Suppressed { get; set; }
        
        [PropertyName ("recording_id")]
        public long? RecordingId { get; set; }

        [PropertyName("__expiration")]
        public long? Expiration { get; set; }
        
        #endregion

        #region Ctor
        
        public NestEpg(EpgCB epgCb, int languageId, bool isOpc = false, bool withRouting = true,
            string esDateOnlyFormat = "", long? recordingId = null, long? expiryUnixTimeStamp=null)
        {
            Initialize(epgCb, isOpc, withRouting,esDateOnlyFormat,languageId,recordingId,expiryUnixTimeStamp);
        }
        #endregion

        #region Initialize

        private void Initialize(EpgCB epgCb, bool isOpc, bool withRouting, string esDateOnlyFormat, int languageId,
            long? recordingId, long? expiryUnixTimeStamp)
        {
            EpgID = epgCb.EpgID;
            GroupID = isOpc ? epgCb.ParentGroupID : epgCb.GroupID;
            ChannelID = epgCb.ChannelID;
            IsActive = epgCb.IsActive;
            StartDate = epgCb.StartDate;
            EndDate = epgCb.EndDate;
            CacheDate = DateTime.UtcNow;
            CreateDate = epgCb.CreateDate;
            UpdateDate = epgCb.UpdateDate;
            SearchEndDate = epgCb.SearchEndDate;
            Crid = epgCb.Crid;
            EpgIdentifier = epgCb.EpgIdentifier;
            ExternalId = epgCb.EpgIdentifier;
            DocumentId = epgCb.DocumentId;
            IsAutoFill = epgCb.IsAutoFill;
            EnableCDVR = epgCb.EnableCDVR;
            EnableCatchUp = epgCb.EnableCatchUp;
            Suppressed = epgCb.Suppressed;

            if (epgCb.LinearMediaId > 0)
            {
                LinearMediaId = epgCb.LinearMediaId;
            }

            if (withRouting)
            {
                DateRouting = epgCb.StartDate.ToUniversalTime().ToString(esDateOnlyFormat);
            }

            var metasDict = new Dictionary<string, Dictionary<string, List<string>>>();
            var langCode = epgCb.Language;

            var metas = new Dictionary<string, List<string>>();
            foreach (var epgCbMeta in epgCb.Metas)
            {
                metas[epgCbMeta.Key] =
                    epgCbMeta.Value.Select(x => ESUtils.ReplaceDocumentReservedCharacters(x, false)).ToList();
            }

            metasDict.Add(langCode, new Dictionary<string, List<string>>(metas));
            Metas = metasDict; //lang

            var tagsDict = new Dictionary<string, Dictionary<string, List<string>>>();
            var tags = new Dictionary<string, List<string>>();
            foreach (var tag in epgCb.Tags)
            {
                tags[tag.Key] = tag.Value.Select(x => ESUtils.ReplaceDocumentReservedCharacters(x, false)).ToList();
            }

            tagsDict.Add(langCode, new Dictionary<string, List<string>>(tags));
            Tags = tagsDict; //lang

            var nameDict = new Dictionary<string, string>();
            nameDict.Add(langCode, ESUtils.ReplaceDocumentReservedCharacters(epgCb.Name, false));
            Name = nameDict; //lang

            var descriptionDict = new Dictionary<string, string>();
            descriptionDict.Add(langCode, ESUtils.ReplaceDocumentReservedCharacters(epgCb.Description, false));
            Description = descriptionDict; //lang

            Language = langCode;
            LanguageId = languageId;

            if (recordingId.HasValue)
            {
                RecordingId = recordingId.Value;
            }

            if (expiryUnixTimeStamp.HasValue)
            {
                Expiration = expiryUnixTimeStamp.Value;
            }
        }

        #endregion
    }
}