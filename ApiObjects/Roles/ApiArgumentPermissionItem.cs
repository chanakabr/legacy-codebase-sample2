using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [JsonObject()]
    public class ApiArgumentPermissionItem : PermissionItem
    {
        public string Service { get; set; }
        public string Action { get; set; }
        public string Parameter { get; set; }

        public override ePermissionItemType GetPermissionItemType()
        {
            return ePermissionItemType.Argument;
        }
    }
}
