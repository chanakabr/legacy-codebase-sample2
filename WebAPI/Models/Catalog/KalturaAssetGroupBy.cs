using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public class KalturaAssetGroupBy : KalturaOTTObject
    {
        /// <summary>
        /// Value - can be a meta, tag or media_type_id
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        public string Value
        {
            get;
            set;
        }

        public List<string> getValues()
        {
            if (string.IsNullOrEmpty(Value))
                return null;

            List<string> values = new List<string>();
            string[] stringValues = Value.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string stringValue in stringValues)
            {
                values.Add(stringValue.Trim());
            }

            return values;
        }
    }

    [Serializable]
    public class KalturaAssetMetaGroupBy : KalturaAssetGroupBy
    {
    }
}