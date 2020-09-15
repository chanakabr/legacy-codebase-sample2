using ApiLogic.Api.Managers;
using ApiLogic.Base;
using ApiObjects;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    public partial class KalturaDynamicList : KalturaCrudObject<DynamicList, long>
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Create date of the DynamicList
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Update date of the DynamicList
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        internal override ICrudHandler<DynamicList, long> Handler { get { return DynamicListManager.Instance; } }

        internal override void SetId(long id)
        {
            this.Id = id;
        }

        public KalturaDynamicList() { }
    }

    public partial class KalturaDynamicListListResponse : KalturaListResponse<KalturaDynamicList>
    {
        public KalturaDynamicListListResponse() : base() { }
    }

    public partial class KalturaUdidDynamicList : KalturaDynamicList
    {
       
    }
}
