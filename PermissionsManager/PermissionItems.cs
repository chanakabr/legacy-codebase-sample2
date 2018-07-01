using ApiObjects;
using ApiObjects.Roles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionsManager
{
    [JsonObject()]
    public class PermissionItems
    {
        [JsonIgnore()]
        public string name;

        [JsonIgnore()]
        public ePermissionItemType type;

        [JsonProperty("type")]
        public string enumType
        {
            get
            {
                return type.ToString();
            }
            set
            {
                Enum.TryParse<ePermissionItemType>(value, out type);
            }
        }

        [JsonProperty("permission_items")]
        public List<FilePermissionItem> permissionItems;

        public PermissionItems()
        {
            permissionItems = new List<FilePermissionItem>();
        }
    }

    [JsonObject()]
    public class FilePermissionItem
    {
        [JsonProperty("permissions")]
        public HashSet<string> permissionsNames;

        [JsonProperty("excluded_permissions")]
        public HashSet<string> excludedPermissionsNames;

        [JsonIgnore()]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("parameter")]
        public string Parameter { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonIgnore()]
        public string Action { get; set; }

        [JsonProperty("action")]
        public string ActionEnum
        {
            get
            {
                string result = this.Action;

                if (!string.IsNullOrEmpty(this.Object))
                {
                    int actionNumeric = 0;
                    int.TryParse(this.Action, out actionNumeric);

                    if (actionNumeric > 0)
                    {
                        ParameterPermissionItemAction actionEnum = (ParameterPermissionItemAction)actionNumeric;
                        result = Enum.GetName(typeof(ParameterPermissionItemAction), actionNumeric);
                    }
                }

                return result;

            }
            set
            {
                ParameterPermissionItemAction actionEnum;
                if (Enum.TryParse<ParameterPermissionItemAction>(value, out actionEnum))
                {
                    this.Action = ((int)actionEnum).ToString();
                }
                else
                {
                    this.Action = value;
                }
            }
        }

        [JsonIgnore()]
        public ePermissionItemType Type { get; set; }

        [JsonProperty("type")]
        public string TypeString
        {
            get
            {
                return Type.ToString();
            }
            set
            {
                ePermissionItemType type;
                if (Enum.TryParse<ePermissionItemType>(value, out type))
                {
                    this.Type = type;
                }
            }
        }

        public FilePermissionItem()
        {
            permissionsNames = new HashSet<string>();
            excludedPermissionsNames = new HashSet<string>();
        }

        public FilePermissionItem(PermissionItem original)
        {
            permissionsNames = new HashSet<string>();
            excludedPermissionsNames = new HashSet<string>();

            if (original != null)
            {
                this.Id = original.Id;
                this.Name = original.Name;

                ePermissionItemType type = original.GetPermissionItemType();

                this.Type = type;

                switch (this.Type)
                {
                    case ePermissionItemType.Action:
                        {
                            var originalCasted = original as ApiActionPermissionItem;
                            this.Action = originalCasted.Action;
                            this.Service = originalCasted.Service;
                            break;
                        }
                    case ePermissionItemType.Parameter:
                        {
                            var originalCasted = original as ApiParameterPermissionItem;
                            this.Action = originalCasted.Action;
                            this.Object = originalCasted.Object;
                            this.Parameter = originalCasted.Parameter;
                            break;
                        }
                    case ePermissionItemType.Argument:
                        {
                            var originalCasted = original as ApiArgumentPermissionItem;
                            this.Action = originalCasted.Action;
                            this.Service = originalCasted.Service;
                            this.Parameter = originalCasted.Parameter;
                            break;
                        }
                    default:
                        break;
                }
            }
        }
        
        public string GetFileName()
        {
            string result = string.Empty;

            switch (this.Type)
            {
                case ePermissionItemType.Action:
                case ePermissionItemType.Argument:
                    {
                        result = this.Service; //string.Format("controller_{0}", this.Service);
                        break;
                    }
                case ePermissionItemType.Parameter:
                    {
                        result = this.Object; //string.Format("object_type_{0}", this.Object);
                        break;
                    }
                default:
                    break;
            }

            return result;
        }
    }
    
}
