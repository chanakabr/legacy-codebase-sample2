using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
    public class RSSEntity
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public Dictionary<string,List<string>> UniqueEmelementDic { get; set; }
    }
}
