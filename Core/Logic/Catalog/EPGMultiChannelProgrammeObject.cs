using ApiObjects;
using Core.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.Catalog
{
    public class EPGMultiChannelProgrammeObject : BaseObject
    {
        public string EPG_CHANNEL_ID;
        public List<EPGChannelProgrammeObject> EPGChannelProgrammeObject;
        public void Initialize(string nEPG_CHANNEL_ID, List<EPGChannelProgrammeObject> oEPGChannelProgrammeObject)
        {
            EPG_CHANNEL_ID = nEPG_CHANNEL_ID;
            EPGChannelProgrammeObject = oEPGChannelProgrammeObject;
        }
    }

}
