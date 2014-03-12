using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace TVPPro.SiteManager.Objects
{
    public class EPGMultiChannelProgrammeObject : BaseObject 
    {
        public string EPG_CHANNEL_ID { get; set; }

        public List<EPGChannelProgrammeObject> EPGChannelProgrammeObject { get; set; }
    }

    public enum EPGUnit
    {
        Days,
        
        Hours,
        
        Current
    }
}
