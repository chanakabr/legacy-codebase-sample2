using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Roles
{
    public class ApiActionPermissionItem : PermissionItem
    {
        public string Service { get; set; }
        public string Action { get; set; }
    }
}
