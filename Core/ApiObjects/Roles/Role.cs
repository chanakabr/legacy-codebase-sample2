using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.Roles
{
    [Serializable]
    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Permissions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Permission> Permissions { get; set; }
        
        public int GroupId { get; set; }
        public RoleProfileType? Profile { get; set; }
    }

    public class RolesResponse
    {
        public List<Role> Roles { get; set; }
        public Response.Status Status { get; set; }
    }

    public enum RoleProfileType
    {
        User = 0,
        Partner = 1,
        Profile = 2,
        System = 3,
        PermissionEmbedded = 4
    }
}
