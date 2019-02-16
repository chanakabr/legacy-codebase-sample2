using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Image type 
    /// </summary>
    [Serializable]
    public partial class KalturaImageType : KalturaOTTObject
    {
        /// <summary>
        /// Image type ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [OldStandardProperty("id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1, MaxLength = 128)]
        public string Name { get; set; }

        /// <summary>
        /// System name
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty(PropertyName = "systemName")]
        [XmlElement(ElementName = "systemName")]
        [SchemeProperty(MinLength = 1, MaxLength = 512)]
        public string SystemName { get; set; }

        /// <summary>
        /// Ration ID
        /// </summary>
        [DataMember(Name = "ratioId")]
        [JsonProperty(PropertyName = "ratioId")]
        [XmlElement(ElementName = "ratioId")]
        [SchemeProperty(MinLong = 1)]
        public long? RatioId { get; set; }

        /// <summary>
        /// Help text
        /// </summary>
        [DataMember(Name = "helpText")]
        [JsonProperty(PropertyName = "helpText")]
        [XmlElement(ElementName = "helpText")]
        [SchemeProperty(MaxLength = 600)]
        public string HelpText { get; set; }

        /// <summary>
        /// Default image ID
        /// </summary>
        [DataMember(Name = "defaultImageId")]
        [JsonProperty(PropertyName = "defaultImageId")]
        [XmlElement(ElementName = "defaultImageId", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? DefaultImageId { get; set; }
    }

    public partial class KalturaImageTypeListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of partner image types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaImageType> ImageTypes { get; set; }

        internal override Dictionary<string, KalturaExcelColumn> GetExcelColumns(int groupId, Dictionary<string, object> data = null)
        {
            Dictionary<string, KalturaExcelColumn> excelColumns = new Dictionary<string, KalturaExcelColumn>();

            var baseExcelColumns = base.GetExcelColumns(groupId, data);
            excelColumns.TryAddRange(baseExcelColumns);

            if (ImageTypes != null && ImageTypes.Count > 0)
            {
                foreach (var imageType in this.ImageTypes)
                {
                    // TODO SHIR - ASK IRA IF ID IS THE RIGTH PROPERTY 
                    var image = ExcelFormatter.GetHiddenColumn(ExcelColumnType.Image, imageType.Id.ToString());
                    excelColumns.TryAdd(image, new KalturaExcelColumn(ExcelColumnType.Image, image, imageType.SystemName, imageType.HelpText));
                }
            }

            return excelColumns;
        }

        // TODO SHIR - USE BASE WHEN IS AVILABLE
        public bool HasObjects()
        {
            return (ImageTypes != null && ImageTypes.Count > 0);
        }
    }
}