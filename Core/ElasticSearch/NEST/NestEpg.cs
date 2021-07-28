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
    
    

    public class JsonSuffixNameAttribute : Attribute
    {
        public string Name { get; set; }
        public JsonSuffixNameAttribute(string name)
        {
            Name = name;
        }
    }
    
    public class CustomResolver : DefaultContractResolver
    {
        private readonly string _suffix;

        public CustomResolver(string suffix)
        {
            _suffix = suffix;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            var nameAttribute = member.GetCustomAttribute<JsonSuffixNameAttribute>();
            if (nameAttribute != null)
            {
                prop.PropertyName = $"{nameAttribute.Name}_{_suffix}";
            }
            return prop;
        }
        
    }
    
  
    public class NestEpg
    {
        #region DataMembers

        [JsonProperty("epg_id")]
        public ulong EpgID;
        
        [JsonProperty("epg_identifier")]
        public string EpgIdentifier { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("create_date")]
        [JsonConverter(typeof(EpgTimeConverter))]
        public DateTime CreateDate { get; set; }

        [JsonProperty("update_date")]
        [JsonConverter(typeof(EpgTimeConverter))]
        public DateTime UpdateDate { get; set; }

        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonProperty("parent_group_id")]
        public int ParentGroupID { get; set; }

        [JsonProperty("channel_id")]
        public int ChannelID { get; set; }
        
        [JsonSuffixName("name")]
        public string Name { get; set; }

        [JsonSuffixName("description")]
        public string Description { get; set; }

        [JsonProperty("start_date")]
        [JsonConverter(typeof(EpgTimeConverter))]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(EpgTimeConverter))]
        public DateTime EndDate { get; set; }

        [JsonProperty("co_guid")]
        public string CoGuid { get; set; }

        [JsonProperty("pic_url", NullValueHandling = NullValueHandling.Ignore)]
        public string PicUrl { get; set; }

        [JsonProperty("pic_id")]
        public int PicID { get; set; }

        // TODO: Lior - only temporary solution for linear media id until we handle new epg ingest
        [JsonProperty("linear_media_id")]
        public long LinearMediaId { get; set; }

        [JsonProperty("basic")]
        public EpgBasicData BasicData { get; protected set; }

        [JsonProperty("stats")]
        public Stats Statistics { get; protected set; }

        [JsonProperty("extra_data")]
        public EpgExtraData ExtraData { get; set; }

        [JsonProperty("metas")]
        public Dictionary<string, List<string>> Metas { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, List<string>> Tags { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
        
        [JsonProperty("date_routing",NullValueHandling = NullValueHandling.Ignore)]
        public string DateRouting { get; set; }

        // from LUNA version
        [JsonProperty("pictures", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public List<EpgPicture> pictures { get; set; }

        //from ROBIN version
        [JsonProperty("enable_cdvr", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int EnableCDVR { get; set; }

        [JsonProperty("enable_catch_up", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int EnableCatchUp { get; set; }

        [JsonProperty("enable_start_over", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int EnableStartOver { get; set; }

        [JsonProperty("enable_trick_play", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int EnableTrickPlay { get; set; }

        [JsonProperty("search_end_date", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(EpgTimeConverter))]
        public DateTime SearchEndDate { get; set; }

        //from Storm version
        [JsonProperty("crid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Crid { get; set; }

        [JsonProperty("document_id")]
        public string DocumentId { get; set; }
        
        [JsonProperty("is_auto_fill")]
        public bool IsAutoFill { get; set; }

        [JsonProperty("is_ingest_v2")]
        public bool IsIngestV2 { get; set; }

        [JsonProperty("suppressed")]
        public string Suppressed { get; set; }
        
        public string Suffix { get; set; }
        #endregion

        #region Ctor
        
        public NestEpg(EpgCB epgCb, bool groupUsesTemplates = false, bool withRouting = true,
            string esDateOOnlyFormat = "", string suffix="")
        {
            //set the lang suffix for the json serialization 
            ///for example on Name with lang code suffix will add name_heb
            Suffix = suffix;
            Initialize(epgCb, groupUsesTemplates, withRouting,esDateOOnlyFormat);
        }
        #endregion

        #region Initialize

        private void Initialize(EpgCB epgCb, bool groupUsesTemplates, bool withRouting, string esDateonlyFormat)
        {
            
            EpgID = epgCb.EpgID;
            EpgIdentifier = epgCb.EpgIdentifier;
            IsActive = epgCb.IsActive;
            Status = epgCb.Status;
            CreateDate = epgCb.CreateDate;
            UpdateDate = epgCb.UpdateDate;
            PicUrl = epgCb.PicUrl;
            PicID = epgCb.PicID;
            GroupID = groupUsesTemplates ? epgCb.ParentGroupID : epgCb.GroupID;
            ParentGroupID = epgCb.ParentGroupID;
            ChannelID = epgCb.ChannelID;

            StartDate = epgCb.StartDate;
            EndDate = epgCb.EndDate;
            SearchEndDate = epgCb.SearchEndDate;
            CoGuid = epgCb.CoGuid;
            BasicData = epgCb.BasicData;
            Statistics = epgCb.Statistics;
            ExtraData = epgCb.ExtraData;
            Metas = new Dictionary<string, List<string>>(epgCb.Metas); //lang
            Tags = new Dictionary<string, List<string>>(epgCb.Tags); //lang
            Name = epgCb.Name; //lang
            Description = epgCb.Description; //lang
            Language = epgCb.Language;
            pictures = epgCb.pictures;
            EnableCDVR = epgCb.EnableCDVR;
            EnableCatchUp = epgCb.EnableCatchUp;
            EnableStartOver = epgCb.EnableStartOver;
            EnableTrickPlay = epgCb.EnableTrickPlay;
            Crid = epgCb.Crid;
            IsIngestV2 = epgCb.IsIngestV2;
            
            if (withRouting)
            {
                DateRouting = epgCb.StartDate.ToUniversalTime().ToString(esDateonlyFormat);
            }
        }

        #endregion
    }
}