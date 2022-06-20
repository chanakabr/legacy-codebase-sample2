using System;
using System.Collections.Generic;
using System.Linq;
using Nest;

namespace ElasticSearch.NEST
{
    [ElasticsearchType(IdProperty = nameof(DocumentId))]
    public class NestBaseAsset
    {
        [Keyword(Name = "document_id")]
        public string DocumentId { get; set; }

        [PropertyName("group_id")]
        public int GroupID { get; set; }

        [PropertyName("is_active")]
        public bool IsActive { get; set; }

        [PropertyName("name")]
        public Dictionary<string, string> NamesDictionary { get; set; } //lang

        public string Name { get { return NamesDictionary?.FirstOrDefault().Value; } }

        [PropertyName("description")]
        public Dictionary<string, string> Description { get; set; } //lang

        [PropertyName("tags")]
        public Dictionary<string, Dictionary<string, HashSet<string>>> Tags { get; set; } //lang

        [PropertyName("metas")]
        public Dictionary<string, Dictionary<string, HashSet<string>>> Metas { get; set; } //lang

        [Date(Name = "create_date")]
        public DateTime CreateDate { get; set; }

        [Date(Name = "update_date")]
        public DateTime UpdateDate { get; set; }

        [Date(Name = "start_date")]
        public DateTime StartDate { get; set; }

        [Date(Name = "end_date")]
        public DateTime EndDate { get; set; }

        [PropertyName("language")]
        public string Language { get; set; }

        [PropertyName("language_id")]
        public int LanguageId { get; set; }

        [Date(Name = "cache_date")]
        public DateTime CacheDate { get; set; }
    }
}