using Newtonsoft.Json;
using RestfulTVPApi.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Models
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


        public static File CreateFromObject(FileMedia obj)
        {
            if (obj == null)
            {
                return null;
            }

            return new File()
            {
                AssetId = obj.m_nMediaID,
                Id = obj.m_nFileId,
                Type = obj.m_sFileFormat,
                Url = obj.m_sUrl,
            };
            
        }
    }
}