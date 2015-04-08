using Newtonsoft.Json;
using RestfulTVPApi.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Responses
{
    public class File
    {
        [JsonProperty(PropertyName = "asset_id")]
        public int AssetId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }


        public File(FileMedia file)
        {
            if (file != null)
            {
                AssetId = file.m_nMediaID;
                Id = file.m_nFileId;
                Type = file.m_sFileFormat;
                Url = file.m_sUrl;
            }
        }
    }
}