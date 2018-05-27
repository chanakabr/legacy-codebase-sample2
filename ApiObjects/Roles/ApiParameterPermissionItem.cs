using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    [JsonObject()]
    public class ApiParameterPermissionItem : PermissionItem
    {
        public string Object { get; set; }

        public string Parameter { get; set; }

        public string Action { get; set; }

        public override ePermissionItemType GetPermissionItemType()
        {
            return ePermissionItemType.Parameter;
        }
    }
    
    public enum ParameterPermissionItemAction
    {
        READ = 1,
        INSERT = 2,
        UPDATE = 4,
        USAGE = 7
    }
}
