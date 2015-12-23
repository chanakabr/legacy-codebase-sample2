using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{

    [Serializable]
    public class LanguageObj
    {
        public int ID { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 3 Letters code of language
        /// </summary>
        public string Code { get; set; }
        public string Direction { get; set; }
        public bool IsDefault { get; set; }

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
