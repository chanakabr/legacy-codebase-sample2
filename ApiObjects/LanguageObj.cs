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

        public LanguageObj()
        {
            ID = 0;
            Name = string.Empty;
            Code = string.Empty;
            Direction = string.Empty;
            IsDefault = false;
        }
    }
}
