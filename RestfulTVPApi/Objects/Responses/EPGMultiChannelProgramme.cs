using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class EPGMultiChannelProgrammeObject
    {
        public string epg_channel_id { get; set; }

        public EPGChannelProgrammeObject[] epg_channel_program_object { get; set; }
    }

}
