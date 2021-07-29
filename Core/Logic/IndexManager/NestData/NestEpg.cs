using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ApiObjects.Epg;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ApiObjects.Nest
{
    public class NestEpg
    {
        #region DataMembers

        [PropertyName("epg_id")]
        public ulong EpgID  { get; set; }
        
        [PropertyName("epg_identifier")]
        public string EpgIdentifier { get; set; }

        [PropertyName("is_active")]
        public bool IsActive { get; set; }

        [PropertyName("status")]
        public int Status { get; set; }

        [PropertyName("create_date")]
        public DateTime CreateDate { get; set; }

        [PropertyName("update_date")]
        public DateTime UpdateDate { get; set; }

        [PropertyName("group_id")]
        public int GroupID { get; set; }

        [PropertyName("parent_group_id")]
        public int ParentGroupID { get; set; }

        [PropertyName("channel_id")]
        public int ChannelID { get; set; }
        
        [PropertyName("name")]
        public Dictionary<string, string> Name { get; set; }

        [PropertyName("description")]
        public  Dictionary<string,string> Description { get; set; }

        [PropertyName("start_date")]
        [JsonConverter(typeof(EpgTimeConverter))]
        public DateTime StartDate { get; set; }

        [PropertyName("end_date")]
        [JsonConverter(typeof(EpgTimeConverter))]
        public DateTime EndDate { get; set; }

        [PropertyName("co_guid")]
        public string CoGuid { get; set; }

        [PropertyName("pic_url")]
        public string PicUrl { get; set; }

        [PropertyName("pic_id")]
        public int PicID { get; set; }

        // TODO: Lior - only temporary solution for linear media id until we handle new epg ingest
        [PropertyName("linear_media_id")]
        public long LinearMediaId { get; set; }

        [PropertyName("basic")]
        public EpgBasicData BasicData { get; protected set; }

        [PropertyName("stats")]
        public Stats Statistics { get; protected set; }

        [PropertyName("extra_data")]
        public EpgExtraData ExtraData { get; set; }

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

        // from LUNA version
        [PropertyName("pictures")]
        public List<EpgPicture> pictures { get; set; }

        //from ROBIN version
        [PropertyName("enable_cdvr")]
        public int EnableCDVR { get; set; }

        [PropertyName("enable_catch_up")]
        public int EnableCatchUp { get; set; }

        [PropertyName("enable_start_over")]
        public int EnableStartOver { get; set; }

        [PropertyName("enable_trick_play")]
        public int EnableTrickPlay { get; set; }

        [PropertyName("search_end_date")]
        public DateTime SearchEndDate { get; set; }

        //from Storm version
        [PropertyName ("crid")]
        public string Crid { get; set; }

        [PropertyName ("document_id")]
        public string DocumentId { get; set; }
        
        [PropertyName ("is_auto_fill")]
        public bool IsAutoFill { get; set; }

        [PropertyName ("is_ingest_v2")]
        public bool IsIngestV2 { get; set; }

        [PropertyName ("suppressed")]
        public string Suppressed { get; set; }
        
        #endregion

        #region Ctor
        
        public NestEpg(EpgCB epgCb, int languageId, bool isOpc = false, bool withRouting = true,
            string esDateOnlyFormat = "")
        {
            Initialize(epgCb, isOpc, withRouting,esDateOnlyFormat,languageId);
        }
        #endregion

        #region Initialize

        private void Initialize(EpgCB epgCb, bool isOpc, bool withRouting, string esDateOnlyFormat, int languageId)
        {
            EpgID = epgCb.EpgID;
            EpgIdentifier = epgCb.EpgIdentifier;
            IsActive = epgCb.IsActive;
            Status = epgCb.Status;
            CreateDate = epgCb.CreateDate;
            UpdateDate = epgCb.UpdateDate;
            PicUrl = epgCb.PicUrl;
            PicID = epgCb.PicID;
            GroupID = isOpc ? epgCb.ParentGroupID : epgCb.GroupID;
            ParentGroupID = epgCb.ParentGroupID;
            ChannelID = epgCb.ChannelID;

            StartDate = epgCb.StartDate;
            EndDate = epgCb.EndDate;
            SearchEndDate = epgCb.SearchEndDate;
            CoGuid = epgCb.CoGuid;
            BasicData = epgCb.BasicData;
            Statistics = epgCb.Statistics;
            ExtraData = epgCb.ExtraData;


            var metasDict = new Dictionary<string, Dictionary<string, List<string>>>();
            metasDict.Add(epgCb.Language,new Dictionary<string, List<string>>(epgCb.Metas));
            Metas = metasDict; //lang
            
            var tagsDict = new Dictionary<string, Dictionary<string, List<string>>>();
            tagsDict.Add(epgCb.Language,new Dictionary<string, List<string>>(epgCb.Tags));
            Tags = tagsDict; //lang

            var nameDict = new Dictionary<string, string>();
            nameDict.Add(epgCb.Language, epgCb.Name);
            Name = nameDict; //lang
            
            var descriptionDict = new Dictionary<string, string>();
            descriptionDict.Add(epgCb.Language, epgCb.Description);
            Description = descriptionDict; //lang
            
            Language = epgCb.Language;
            LanguageId = languageId;
            pictures = epgCb.pictures;
            EnableCDVR = epgCb.EnableCDVR;
            EnableCatchUp = epgCb.EnableCatchUp;
            EnableStartOver = epgCb.EnableStartOver;
            EnableTrickPlay = epgCb.EnableTrickPlay;
            Crid = epgCb.Crid;
            IsIngestV2 = epgCb.IsIngestV2;
            
            if (withRouting)
            {
                DateRouting = epgCb.StartDate.ToUniversalTime().ToString(esDateOnlyFormat);
            }
        }

        #endregion
    }
}