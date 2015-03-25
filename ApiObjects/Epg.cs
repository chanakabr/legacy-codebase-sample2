using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ApiObjects
{
    [Serializable]
    [JsonObject(Id = "epg")]
    public class EpgCB 
    {
        [JsonProperty("epg_id")]
        public ulong EpgID;
        [JsonProperty("epg_identifier")]
        public string EpgIdentifier { get; set; }
        [JsonProperty("is_active")]
        public bool isActive { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("type")]
        public string Type { get; protected set; }
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
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
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

        [JsonProperty("basic")]
        public EpgBasicData BasicData { get; protected set; }
        [JsonProperty("stats")]
        public Stats Statistics { get; protected set; }
        [JsonProperty("extra_data")]
        public EpgExtraData ExtraData { get; set; }
        [JsonProperty("metas")]
        public Dictionary<string, List<string>> Metas { get;  set; }
       
        [JsonProperty("tags")]
        public Dictionary<string, List<string>> Tags { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
        
        public EpgCB()
        {
            EpgID = 0;
            EpgIdentifier = string.Empty;
            isActive = false;
            Status = 2;
            Type = "epg";
            CreateDate = DateTime.MinValue;
            UpdateDate = DateTime.MinValue;
            PicUrl = string.Empty;
            PicID = 0;
            GroupID = 0;
            ParentGroupID = 0;
            ChannelID = 0;
            Name = string.Empty;
            Description = string.Empty;
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            CoGuid = string.Empty;

            BasicData = new EpgBasicData();
            Statistics = new Stats();
            ExtraData = new EpgExtraData();
            Metas = new Dictionary<string, List<string>>();
            Tags = new Dictionary<string, List<string>>();

            Language = string.Empty;
        }

              
        public bool Equals(EpgCB obj)
        {
            //Check for null and compare run-time types. 
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                if (this.EpgID != obj.EpgID || this.EpgIdentifier != obj.EpgIdentifier)
                    return false;
                if (this.StartDate != obj.StartDate || this.EndDate != obj.EndDate)
                    return false;
                if (this.ChannelID != obj.ChannelID)
                    return false;
                if (this.CoGuid != obj.CoGuid)
                    return false;
                if (this.Description != obj.Description || this.Name != obj.Name)
                    return false;
                if (this.PicID != obj.PicID)
                    return false;
                if (this.Language != obj.Language)
                    return false;
                if (obj.ExtraData != null && obj.ExtraData.MediaID > 0)
                {
                    if (obj.ExtraData.MediaID != this.ExtraData.MediaID)
                        return false;
                }

                #region Tags
                if (this.Tags != null && obj.Tags != null && this.Tags.Count == obj.Tags.Count)
                {
                    foreach (string objTagKey in obj.Tags.Keys)
                    {
                        if (!this.Tags.ContainsKey(objTagKey.ToLower()))
                        {
                            return false;
                        }

                        int countObjTagValues = obj.Tags[objTagKey] == null ? 0 : obj.Tags[objTagKey].Count;
                        int countThisTagValues = this.Tags[objTagKey.ToLower()] == null ? 0 : this.Tags[objTagKey.ToLower()].Count;

                        if (countObjTagValues != countThisTagValues)
                        {
                            return false;
                        }
                        else
                        {
                            // compare the values between the lists
                            foreach (string sTagValue in obj.Tags[objTagKey])
                            {
                                if (!this.Tags[objTagKey.ToLower()].Contains(sTagValue))                                
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Metas
                if (this.Metas != null && obj.Metas != null && this.Metas.Count == obj.Metas.Count)
                {
                    foreach (string objMetaKey in obj.Metas.Keys)
                    {
                        if (!this.Metas.ContainsKey(objMetaKey.ToLower()))
                        {
                            return false;
                        }

                        int countObjMetaValues = obj.Metas[objMetaKey] == null ? 0 : obj.Metas[objMetaKey].Count;
                        int countThisMetaValues = this.Metas[objMetaKey.ToLower()] == null ? 0 : this.Metas[objMetaKey.ToLower()].Count;

                        if (countObjMetaValues != countThisMetaValues)
                        {
                            return false;
                        }
                        else
                        {
                            // compare the values between the lists
                            foreach (string sMetaValue in obj.Tags[objMetaKey])
                            {
                                if (!this.Tags[objMetaKey.ToLower()].Contains(sMetaValue))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            return true;
        }
       
    }

    [Serializable]
    public class EpgBasicData
    {
       

        public EpgBasicData()
        {

        }
    }

    [Serializable]
    public class EpgExtraData
    {
        [JsonProperty("media_id")]
        public int MediaID { get; set; }
        [JsonProperty("fb_object_id", NullValueHandling = NullValueHandling.Ignore)]
        public string FBObjectID { get; set; }

        public EpgExtraData()
        {
            MediaID = 0;
            FBObjectID = string.Empty;
        }
    }

    public class Stats
    {
        [JsonProperty("views")]
        public long Views { get; set; }
        [JsonProperty("likes")]
        public long Likes { get; set; }

        public Stats()
        {
            Views = 0;
            Likes = 0;
        }
    }

    [Serializable]
    public class EpgTimeConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public EpgTimeConverter()
        {
            base.DateTimeFormat = "yyyyMMddHHmmss";
        }
    }

    //public class Epg
    //{
    //    #region members
    //    public int m_nGroupID;
    //    public int EPG_ID;
    //    public int m_nChannelID;
    //    public string m_sName;
    //    public string m_sDescription;
    //    public int m_nIsActive;
    //    public string m_sStartDate;
    //    public string m_sEndDate;
    //    public Dictionary<string, string> m_dMetas;
    //    public Dictionary<string, string> m_dTags;
    //    #endregion

    //    public Epg()
    //    {
    //        m_sName = string.Empty;
    //        m_sDescription = string.Empty;
    //        EPG_ID = 0;
    //        m_nGroupID = 0;
    //        m_nIsActive = 0;

    //        string sNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    //        string sMax = DateTime.MaxValue.ToString("yyyyMMddHHmmss");
    //        m_sStartDate = sNow;
    //        m_sEndDate = sMax;
    //        m_dMetas = new Dictionary<string, string>();
    //        m_dTags = new Dictionary<string, string>();
    //    }
    //}

    [Serializable]
    [JsonObject(Id = "epggroupsettings")]
    public class EpgGroupSettings
    {
        #region members

        public List<string> m_lTagsName;
        public List<string> m_lMetasName;

        #endregion

        public EpgGroupSettings()
        {
            m_lTagsName = new List<string>();
            m_lMetasName = new List<string>();
        }
    }
}
