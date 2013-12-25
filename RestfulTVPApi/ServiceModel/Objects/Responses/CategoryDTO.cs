using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class CategoryDTO
    {
        public List<CategoryDTO> InnerCategories { get; set; }
        public List<ChannelDTO> Channels { get; set; }
        public string Title { get; set; }
        public string ID { get; set; }
        public string PicURL { get; set; }
    }
}