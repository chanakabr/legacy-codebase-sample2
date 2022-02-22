using ApiObjects.Response;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    [XmlInclude(typeof(KalturaGroupPermission))]
    public partial class KalturaPermission : KalturaOTTObject
    {
        /// <summary>
        /// Permission identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Permission name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Permission friendly name
        /// </summary>
        [DataMember(Name = "friendlyName")]
        [JsonProperty("friendlyName")]
        [XmlElement(ElementName = "friendlyName")]
        public string FriendlyName { get; set; }

        /// <summary>
        /// List of permission items associated with the permission
        /// </summary>       
        [SchemeProperty(ReadOnly = true)]
        internal List<KalturaPermissionItem> PermissionItems { get; set; }

        /// <summary>
        /// Comma separated permissions names from type SPECIAL_FEATURE
        /// </summary>
        [DataMember(Name = "dependsOnPermissionNames")]
        [JsonProperty("dependsOnPermissionNames")]
        [XmlElement(ElementName = "dependsOnPermissionNames")]
        [SchemeProperty(ReadOnly = true)]
        public string DependsOnPermissionNames { get; set; }

        /// <summary>
        /// Permission type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]        
        public KalturaPermissionType Type { get; set; }

        /// <summary>
        /// Comma separated associated permission items IDs
        /// </summary>
        [DataMember(Name = "permissionItemsIds")]
        [JsonProperty("permissionItemsIds")]
        [XmlElement(ElementName = "permissionItemsIds")]
        public string PermissionItemsIds { get; set; }

        internal void ValidateForUpdate()
        {
            if (this.Type == KalturaPermissionType.GROUP)
            {

            }

            if (!string.IsNullOrEmpty(PermissionItemsIds))
            {
                var items = WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(this.PermissionItemsIds, "permissionItemsIds", true);
            }
        }

        internal void ValidateForInsert()
        {          
            if (!string.IsNullOrEmpty(PermissionItemsIds))
            {
                var items = WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(this.PermissionItemsIds, "permissionItemsIds", true);
            }
        }

        internal object GetPermissionItemsIds()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(PermissionItemsIds, "KalturaPermission.permissionItemsIds");
        }
    }

    public enum KalturaPermissionType
    {
        NORMAL = 1,
        GROUP = 2,
        SPECIAL_FEATURE = 3
    }
}