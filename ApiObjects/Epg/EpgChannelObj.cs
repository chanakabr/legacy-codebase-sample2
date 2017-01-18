using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Epg
{
    public class EpgChannelObj
    {
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public EpgChannelType ChannelType { get; set; }

        public EpgChannelObj(int nChannelId, string sChannelName, int nChannelType)
        {
            this.ChannelId = nChannelId;
            this.ChannelName = sChannelName;
            this.ChannelType = (EpgChannelType)nChannelType;
        }
    }
}
