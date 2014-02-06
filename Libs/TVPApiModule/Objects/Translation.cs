using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class Translation
    {
        public string TitleID { get; set; }
        public string OriginalText { get; set; }
        public string Culture { get; set; }
        public int LanguageID { get; set; }
    }
}
