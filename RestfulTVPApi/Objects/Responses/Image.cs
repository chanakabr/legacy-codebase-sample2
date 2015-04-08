using Newtonsoft.Json;
using RestfulTVPApi.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Responses
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

        public Image(Catalog.Picture picture)
        {
            if (picture != null)
            {
                Url = picture.m_sURL;
                Ratio = picture.ratio;

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

        public Image(EpgPicture picture)
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