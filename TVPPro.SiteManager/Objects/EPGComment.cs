using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
    public class EPGComment
    {
        public int ID { get; set; }
        public DateTime CreateDate { get; set; }
        public int EPGProgramID { get; set; }
        public int Language { get; set; }
        public string LanguageName { get; set; }
        public string ContentText { get; set; }
        public string Header { get; set; }
        public string Writer { get; set; }
        public string UserPicURL { get; set; }
    }
}
