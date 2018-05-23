using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [JsonObject()]
    public class Role
    {
        [JsonIgnore()]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        //[JsonProperty("permissions")]
        [JsonIgnore()]
        public List<Permission> Permissions { get; set; }

        [JsonProperty("group_id")]
        [JsonIgnore()]
        public int GroupId { get; set;
        }
    }

    public class RolesResponse
    {
        public List<Role> Roles { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
    }
}
