using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Reflection;

namespace WebAPI.Models.General
{
    public interface IKalturaJsonable
    {
        string ToJson(Version currentVersion, bool omitObsolete);
    }

    public class KalturaJsonable : IKalturaJsonable
    {
        public virtual string ToJson(Version currentVersion, bool omitObsolete)
        {
            return "{" + PropertiesToJson(currentVersion, omitObsolete) + "}";
        }

        protected virtual string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            return "";
        }
    }

    public interface IKalturaOTTObject
    {
    }

    /// <summary>
    /// Base class
    /// </summary>
    public partial class KalturaOTTObject : KalturaJsonable, IKalturaOTTObject
    {
        [DataMember(Name = "objectType")]
        [JsonProperty(PropertyName = "objectType")]
        [XmlElement(ElementName = "objectType")]
        public string objectType { get { return this.GetType().Name; } set { } }

        [DataMember(Name = "relatedObjects")]
        [JsonProperty(PropertyName = "relatedObjects")]
        [XmlElement(ElementName = "relatedObjects")]
        public SerializableDictionary<string, KalturaListResponse> relatedObjects
        {
            get;
            set;
        }

        public virtual void AfterRequestParsed(string service, string action, string language, int groupId, string userId, string deviceId, JObject json = null)
        {

        }
    }
}