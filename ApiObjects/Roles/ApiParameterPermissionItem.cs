using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    public class ApiParameterPermissionItem : PermissionItem
    {
        public string Object { get; set; }
        public string Parameter { get; set; }
        public string Action { get; set; }
    }
}
