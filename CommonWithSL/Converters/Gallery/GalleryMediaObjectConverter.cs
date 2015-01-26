using CommonWithSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL.Converters.Gallery
{
    public class GalleryMediaObjectConverter : IGalleryItemConverter
    {
        public object ConvertItem(object inputObject, string picSize = null)
        {
            Dictionary<string, object> inputObjectDic = (Dictionary<string, object>)(inputObject);
            return ConvertItem(inputObjectDic, picSize);
        }

        public object ConvertItem(Dictionary<string, object> inputObjectDic, string picSize = null)
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
                    SeasonNumber = getMetaValue(inputObjectDic, "Season Number"),
                    EpisodeNumber = getMetaValue(inputObjectDic, "Episode Number"),
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
            string link = (inputObjectDic["PicURL"] == null) ? null : inputObjectDic["PicURL"].ToString();

            if (string.IsNullOrEmpty(link))
            {
                if (!string.IsNullOrEmpty(picSize))
                {
                    var ImageObj = ((object[])(inputObjectDic["Pictures"])).Where(dicItem =>
                    {
                        Dictionary<string, object> picObject = (Dictionary<string, object>)dicItem;
                        return picObject["PicSize"].ToString() == picSize;
                    }).FirstOrDefault();
                    if (ImageObj != null)
                        link = ((Dictionary<string, object>)ImageObj)["URL"].ToString();
                }

                if (string.IsNullOrEmpty(link))//default get first image
                {
                    link = ((Dictionary<string, object>)(((object[])(inputObjectDic["Pictures"]))[0]))["URL"].ToString();
                }
                inputObjectDic["PicURL"] = link;
            }

            return link;
        }

        private string getMetaValue(Dictionary<string, object> inputObjectDic, string key)
        {
            string value = string.Empty;

            Dictionary<string, string> metasDic = getMetas(inputObjectDic);
            if (metasDic != null && metasDic.Keys.Contains(key))
                value = metasDic[key];

            return value;
        }

        private Dictionary<string, string> getMetas(Dictionary<string, object> inputObjectDic)
        {
            Dictionary<string, string> metasDic = null;
            try
            {
                if (inputObjectDic.Keys.Contains("Metas"))
                {
                    metasDic = new Dictionary<string, string>();
                    object[] metasArray = (object[])inputObjectDic["Metas"];
                    foreach (Dictionary<string, object> item in metasArray)
                    {
                        metasDic.Add(item["Key"].ToString(), item["Value"].ToString());
                    }
                }
            }
            catch
            {
                metasDic = null;
            }

            return metasDic;
        }

        private Dictionary<string, string> getTags(Dictionary<string, object> inputObjectDic)
        {
            Dictionary<string, string> tagsDic = null;
            try
            {
                if (inputObjectDic.Keys.Contains("Tags"))
                {
                    tagsDic = new Dictionary<string, string>();
                    object[] metasArray = (object[])inputObjectDic["Tags"];
                    foreach (Dictionary<string, object> item in metasArray)
                    {
                        tagsDic.Add(item["Key"].ToString(), item["Value"].ToString());
                    }
                }
            }
            catch
            {
                tagsDic = null;
            }
            return tagsDic;
        }
    }
}
