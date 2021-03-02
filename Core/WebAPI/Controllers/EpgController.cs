using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    // this is a workaround to generate documentation and client libs
    // should be removed, when we'll find solution to update KalturaClient.xml with non-Phoenix endpoints
    
    [Service("epg")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class EpgController : IKalturaController
    {
        /// <summary>
        /// Returns EPG assets.
        /// </summary>
        /// <param name="filter">Filters by EPG live asset identifier and date in unix timestamp, e.g. 1610928000(January 18, 2021 0:00:00), 1611014400(January 19, 2021 0:00:00)</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaEpgListResponse List(KalturaEpgFilter filter = null)
        {
            throw new NotImplementedException("call should go to EPG Cache service instead of Phoenix");
        }
    }

    public partial class KalturaEpgFilter : KalturaFilter<KalturaEpgOrderBy>
    {
        /// <summary>
        /// date in unix timestamp, e.g. 1610928000(January 18, 2021 0:00:00), 1611014400(January 19, 2021 0:00:00)
        /// </summary>
        [DataMember(Name = "dateEqual")]
        [JsonProperty(PropertyName = "dateEqual")]
        [XmlElement(ElementName = "dateEqual")]
        public long Date { get; set; }
        
        /// <summary>
        /// EPG live asset identifier
        /// </summary>
        [DataMember(Name = "liveAssetIdEqual")]
        [JsonProperty(PropertyName = "liveAssetIdEqual")]
        [XmlElement(ElementName = "liveAssetIdEqual")]
        public long LiveAssetId { get; set; }

        public override KalturaEpgOrderBy GetDefaultOrderByValue()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// EPG wrapper
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public partial class KalturaEpgListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaEpg> Objects { get; set; }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public partial class KalturaEpg : KalturaProgramAsset
    {
    }
    
    public enum KalturaEpgOrderBy
    {
    }
}