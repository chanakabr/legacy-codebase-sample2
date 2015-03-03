using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Picture
    {
        [JsonProperty(PropertyName = "size")]
        public string Size { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        public Picture(Tvinci.Data.Loaders.TvinciPlatform.Catalog.Picture picture)
        {
            Size = picture.m_sSize;
            Url = picture.m_sURL;
        }
    }
}
