using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using Newtonsoft.Json;
using WebAPI.ClientManagers.Client;
using Newtonsoft.Json.Linq;

namespace WebAPI.Models.General
{
    public abstract class KalturaPersistedFilter<T> : KalturaFilter<T> where T : struct, IComparable, IFormattable, IConvertible
    {
        /// <summary>
        /// Name for the presisted filter. If empty, no action will be done. If has value, the filter will be saved and persisted in user's search history.
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Name
        {
            get;
            set;
        }

        public override void AfterRequestParsed(string service, string action, string language, string userId, string deviceId, JObject json = null)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                ClientsManager.ApiClient().SaveSearchHistory(this.Name, service, action, language, userId, deviceId, json);
            }
        }
    }
}