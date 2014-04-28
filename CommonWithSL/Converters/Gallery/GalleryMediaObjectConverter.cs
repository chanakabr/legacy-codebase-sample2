using CommonWithSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL.Converters.Gallery
{
    public class GalleryMediaObjectConverter : IGalleryItemConverter
    {
        public Media ConvertItem(object inputObject, string picSize = null)
        {
            Dictionary<string, object> inputObjectDic = (Dictionary<string, object>)(inputObject);
            return ConvertItem(inputObjectDic, picSize);
        }

        public Media ConvertItem(Dictionary<string, object> inputObjectDic, string picSize = null)
        {
            Media convertedItem = null;
            try
            {
                convertedItem = new CommonWithSL.Media()
                {
                    MediaTypeID = inputObjectDic["MediaTypeName"].ToString(),
                    ID = inputObjectDic["MediaID"].ToString(),
                    ImageLink = getPicLink(inputObjectDic, picSize),
                    Title = inputObjectDic["MediaName"].ToString(),
                    MediaTemplate = inputObjectDic["MediaTypeName"].ToString() + "Template",
                    Rating = getRating(inputObjectDic),
                    PictureSize = picSize
                };
            }
            catch { }
            return convertedItem;
        }

        private int getRating(Dictionary<string, object> inputObjectDic)
        {
            float rating = 0;
            if (inputObjectDic.ContainsKey("Rating"))
                float.TryParse(inputObjectDic["Rating"].ToString(), out rating);

            return (int)rating;
        }

        private string getPicLink(Dictionary<string, object> inputObjectDic, string picSize = null)
        {
            string link = string.Empty;
            if (string.IsNullOrEmpty(picSize))
            {
                link = ((Dictionary<string, object>)(((object[])(inputObjectDic["Pictures"]))[0]))["URL"].ToString();
            }
            else
            {
                var ImageObj = ((object[])(inputObjectDic["Pictures"])).Where(dicItem =>
                {
                    Dictionary<string, object> picObject = (Dictionary<string, object>)dicItem;
                    return picObject["PicSize"].ToString() == picSize;
                }).FirstOrDefault();
                if (ImageObj != null)
                    link = ((Dictionary<string, object>)ImageObj)["URL"].ToString();
            }
            return link;
        }
    }
}
