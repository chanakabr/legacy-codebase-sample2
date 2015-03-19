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
        public int Ratio { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        public Image(Tvinci.Data.Loaders.TvinciPlatform.Catalog.Picture picture)
        {
            if (picture != null)
            {
                Url = picture.m_sURL;

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

        public Image(ApiObjects.PicObject picture)
        {
            if (picture != null)
            {
                Url = picture.m_sPicURL;
                Height = picture.m_nPicHeight;
                Width = picture.m_nPicWidth;
                //Ratio = 
            }
        }
    }
}
