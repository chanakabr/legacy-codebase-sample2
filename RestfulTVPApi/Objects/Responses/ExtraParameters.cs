using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Responses
{
    public class ExtraParameters
    {
        public List<TagMetaIntPairArray> tag_dict { get; set; }
        public int media_id { get; set; }
        public string media_pic_url { get; set; }
        public string template_email { get; set; }

        public ExtraParameters()
        {
            tag_dict = new List<TagMetaIntPairArray>();
        }
    }
}