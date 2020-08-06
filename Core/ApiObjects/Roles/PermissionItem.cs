using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.Roles
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class PermissionItem
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsExcluded { get; set; }

        public virtual ePermissionItemType GetPermissionItemType()
        {
            return default(ePermissionItemType);
        }
    }
}
