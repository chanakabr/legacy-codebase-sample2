using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.CouchbaseWrapperObjects;
using ApiObjects.CrowdsourceItems.Base;
using Newtonsoft.Json;

namespace ApiObjects.CrowdsourceItems.CbDocs
{
    public class CrowdsourceFeedDoc : CbDocumentBase
    {
        [JsonProperty("groupId")]
        public int GroupId { get; set; }
        [JsonProperty("language")]
        public int Language { get; set; }
        [JsonProperty("items", ItemTypeNameHandling = TypeNameHandling.All)]
        public List<BaseCrowdsourceItem> Items { get; set; }

        public override string Id
        {
            get { return string.Format("feed::{0}:LanguageId={1}", GroupId, Language); }
        }

        public CrowdsourceFeedDoc(int groupId, int language)
        {
            this.GroupId = groupId;
            this.Language = language;
            this.Items = new List<BaseCrowdsourceItem>();
        }

    }
}
