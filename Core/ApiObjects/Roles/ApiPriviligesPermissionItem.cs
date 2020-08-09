using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Roles
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ApiPriviligesPermissionItem : PermissionItem
    {
        public string Object { get; set; }

        public string Parameter { get; set; }

        public string Action { get; set; }

        public override ePermissionItemType GetPermissionItemType()
        {
            return ePermissionItemType.Priviliges;
        }
    }
}
