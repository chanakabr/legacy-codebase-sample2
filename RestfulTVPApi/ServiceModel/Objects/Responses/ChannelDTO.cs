using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class ChannelDTO
    {
        public string Title { get; set; }
        public long ChannelID { get; set; }
        public int MediaCount { get; set; }
        public string PicURL { get; set; }
    }
}