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
        [JsonProperty("object")]
        public string Object { get; set; }
        [JsonProperty("parameter")]
        public string Parameter { get; set; }
        //[JsonProperty("action")]
        //[JsonIgnore()]
        public string Action { get; set; }

        [JsonProperty("action")]
        public string ActionEnum
        {
            get
            {
                string result = string.Empty;

                int actionNumeric = 0;
                int.TryParse(this.Action, out actionNumeric);

                if (actionNumeric > 0)
                {
                    ParameterPermissionItemAction actionEnum = (ParameterPermissionItemAction)actionNumeric;
                }

                result = Enum.GetName(typeof(ParameterPermissionItemAction), actionNumeric);

                return result;
                
            }
            set
            {
                ParameterPermissionItemAction actionEnum;
                if (Enum.TryParse<ParameterPermissionItemAction>(value, out actionEnum))
                {
                    this.Action = ((int)actionEnum).ToString();
                }
            }
        }

        public override string GetPermissionItemType()
        {
            return "object";
        }

        public override string GetFileName()
        {
            if (string.IsNullOrEmpty(this.Object))
            {
                return string.Empty;
            }
            else
            {
                return string.Format("permission_item_object_type_{0}", this.Object);
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
}
