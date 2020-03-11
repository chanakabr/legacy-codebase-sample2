using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Image
    {
        [JsonProperty(PropertyName = "ratio")]
        public string Ratio { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "is_default")]
        public bool IsDefault { get; set; }

        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }


        public Image(Tvinci.Data.Loaders.TvinciPlatform.Catalog.Picture picture)
        {
            if (picture != null)
            {
                Url = picture.m_sURL;
                Ratio = picture.ratio;
                Id = picture.id;
                Version = picture.version;
                IsDefault = picture.isDefault;

                // parse from "widthXheight" format
                string[] sizeArr = picture.m_sSize.ToLower().Split('x');

                if (sizeArr != null && sizeArr.Length == 2)
                {
                    int height, width;

                    if (int.TryParse(sizeArr[0].Trim(), out width))
                    {
                        Width = width;
                    }

                    if (int.TryParse(sizeArr[1].Trim(), out height))
                    {
                        Height = height;
                    }
                }
            }
        }

        public Image(Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgPicture picture)
        {
            if (picture != null)
            {
                Url = picture.Url;
                Height = picture.PicHeight;
                Width = picture.PicWidth;
                Ratio = picture.Ratio;
            }
        }
    }
}
