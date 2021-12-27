using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reactive.Joins;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestByIdsFilter : KalturaFilter<KalturaIngestStatusOrderBy>
    {
        /// <summary>
        /// Comma seperated ingest profile ids
        /// </summary>
        [DataMember(Name = "ingestIdIn")]
        [JsonProperty("ingestIdIn")]
        [XmlElement(ElementName = "ingestIdIn")]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string IngestIdIn { get; set; }

        public override KalturaIngestStatusOrderBy GetDefaultOrderByValue()
        {
            return KalturaIngestStatusOrderBy.NONE;
        }
    }
}

