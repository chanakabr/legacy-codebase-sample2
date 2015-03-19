using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class File
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "format")]
        public string Format { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }


        public File(Tvinci.Data.Loaders.TvinciPlatform.Catalog.FileMedia file)
        {
            if (file != null)
            {
                Id = file.m_nFileId;
                Format = file.m_sFileFormat;
                Url = file.m_sUrl;
            }
        }
    }
}
