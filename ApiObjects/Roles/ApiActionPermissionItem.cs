using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [JsonObject()]
    public class ApiActionPermissionItem : PermissionItem
    {
        public string Service { get; set; }
        public string Action { get; set; }

        public override ePermissionItemType GetPermissionItemType()
        {
            return ePermissionItemType.Action;
        }
    }
}
