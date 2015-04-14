using Newtonsoft.Json;
using RestfulTVPApi.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Models
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

        public static Image CreateFromObject(Picture obj)
        {
            if (obj == null)
            {
                return null;
            }

            Image image = new Image()
            {
                Url = obj.m_sURL,
                Ratio = obj.ratio
            };

            // parse from "widthXheight" format
            string[] sizeArr = obj.m_sSize.ToLower().Split('x');

            if (sizeArr != null && sizeArr.Length == 2)
            {
                int height, width;

                if (int.TryParse(sizeArr[0].Trim(), out width))
                {
                    image.Width = width;
                }

                if (int.TryParse(sizeArr[1].Trim(), out height))
                {
                    image.Height = height;
                }
            }

            return image;
        }

        public static Image CreateFromObject(EpgPicture obj)
        {
            if (obj == null)
            {
                return null;
            }

            return new Image()
            {
                Url = obj.Url,
                Height = obj.PicHeight,
                Width = obj.PicWidth,
                Ratio = obj.Ratio,
            };
        }
    }
}