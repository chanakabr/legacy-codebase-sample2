using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [JsonObject()]
    public class GroupPermission : Permission
    {
        [JsonProperty("users_group")]
        public string UsersGroup { get; set; }
    }
}
