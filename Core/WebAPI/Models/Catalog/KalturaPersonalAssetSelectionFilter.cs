using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaPersonalAssetSelectionFilter : KalturaFilter<KalturaPersonalAssetSelectionOrderBy>
    {
        /// <summary>
        /// selected assets for specific slot number
        /// </summary>
        [DataMember(Name = "slotNumberEqual")]
        [JsonProperty("slotNumberEqual")]
        [XmlElement(ElementName = "slotNumberEqual")]
        public int SlotNumberEqual { get; set; }

        public override KalturaPersonalAssetSelectionOrderBy GetDefaultOrderByValue() =>
            KalturaPersonalAssetSelectionOrderBy.ASSET_SELECTION_DATE_DESC;
    }
    
    public enum KalturaPersonalAssetSelectionOrderBy
    {
        ASSET_SELECTION_DATE_DESC
    }
}
