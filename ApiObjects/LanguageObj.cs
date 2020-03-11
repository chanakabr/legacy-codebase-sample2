using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class LanguageObj
    {
        [JsonProperty()]
        public int ID
        {
            get;
            set;
        }
        [JsonProperty()]
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 3 Letters code of language
        /// </summary>
        [JsonProperty()]
        public string Code
        {
            get;
            set;
        }
        [JsonProperty()]
        public string Direction
        {
            get;
            set;
        }
        [JsonProperty()]
        public bool IsDefault
        {
            get;
            set;
        }

        [JsonProperty()]
        public string DisplayName { get; set; }

        public LanguageObj()
        {
            this.ID = 0;
            this.Name = string.Empty;
            this.Code = string.Empty;
            this.Direction = string.Empty;
            this.IsDefault = false;
            this.DisplayName = string.Empty;
        }

        public LanguageObj(int id, string name, string code, string direction, bool isDefault, string displayName)
        {
            this.ID = id;
            this.Name = name;
            this.Code = code;
            this.Direction = direction;
            this.IsDefault = isDefault;
            this.DisplayName = displayName;
        }
    }
}
