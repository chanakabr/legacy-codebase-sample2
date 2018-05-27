using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [JsonObject()]
    public class Permission
    {
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        public List<PermissionItem> PermissionItems { get; set; }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("is_excluded")]
        public bool isExcluded { get; set; }
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
