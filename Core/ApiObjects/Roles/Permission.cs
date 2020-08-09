using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class Permission
    {
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty(PropertyName = "PermissionItems",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<PermissionItem> PermissionItems { get; set; }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("is_excluded")]
        public bool isExcluded { get; set; }

        [JsonProperty("depends_on_permission_names")]
        public string DependsOnPermissionNames { get; set; }

        [JsonProperty("type")]
        public ePermissionType Type { get; set; }

        [JsonProperty("permission_items_ids")]
        public List<long> PermissionItemsIds { get; set; }
    }


    public class PermissionsResponse
    {
        public List<Permission> Permissions { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
    }

    public class PermissionResponse
    {
        public Permission Permission { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
    }
}
