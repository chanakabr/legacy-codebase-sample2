using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// The KalturaFilterPager object enables paging management to be applied upon service list actions
    /// </summary>
    public partial class KalturaFilterPager : KalturaOTTObject
    {
        /// <summary>
        /// <![CDATA[The number of objects to retrieve. Possible range 1 ≤ value ≤ 50. If omitted or value < 1 - will be set to 25. If a value > 50 provided – will be set to 50]]>
        /// </summary>
        [DataMember(Name = "pageSize")]
        [JsonProperty(PropertyName = "pageSize")]
        [XmlElement(ElementName = "pageSize")]
        [SchemeProperty(IsNullable = true, MinInteger = 1, MaxInteger = 500, Default = 30)]
        public int? PageSize { get; set; }

        /// <summary>
        /// The page number for which {pageSize} of objects should be retrieved
        /// </summary>
        [DataMember(Name = "pageIndex")]
        [JsonProperty(PropertyName = "pageIndex")]
        [XmlElement(ElementName = "pageIndex")]
        [SchemeProperty(IsNullable = true, MinInteger = 1, Default = 1)]
        public int? PageIndex { get; set; }

        protected override void Init()
        {
            base.Init();
            PageSize = 30;
            PageIndex = 1;
        }
    }
}