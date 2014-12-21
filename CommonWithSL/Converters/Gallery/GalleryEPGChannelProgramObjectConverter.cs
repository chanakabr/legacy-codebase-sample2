using CommonWithSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL.Converters.Gallery
{
    public class GalleryEPGChannelProgramObjectConverter : IGalleryItemConverter
    {
        public object ConvertItem(object inputObject, string picSize = null)
        {
            Dictionary<string, object> inputObjectDic = (Dictionary<string, object>)(inputObject);
            return ConvertItem(inputObjectDic, picSize);
        }

        public object ConvertItem(Dictionary<string, object> inputObjectDic, string picSize = null)
        {
            CommonWithSL.Program convertedItem = null;
            Dictionary<string, string> tagsDic = null;
            try
            {
                tagsDic = getTags(inputObjectDic);
                convertedItem = new CommonWithSL.Program()
                {
                    Title = inputObjectDic["NAME"].ToString(),
                    ImageLink = inputObjectDic["PIC_URL"].ToString(),
                    EpgId = inputObjectDic["EPG_ID"].ToString(),
                    TemplateName = "GalleryEPGItemTemplate",
                    ChannelCode = inputObjectDic["EPG_CHANNEL_ID"].ToString(),
                    IsBlackout = isBlackOut(tagsDic),
                    StartTime = inputObjectDic["START_DATE"].ToString(),
                    EndTime = inputObjectDic["END_DATE"].ToString(),
                    Description = inputObjectDic["DESCRIPTION"].ToString()
                };
            }
            catch { }
            return convertedItem;
        }

        private string getTagValue(Dictionary<string, string> tagsDic, string key)
        {
            string value = string.Empty;

            if (tagsDic != null && tagsDic.Keys.Contains(key))
                value = tagsDic[key];

            return value;
        }

        private bool isBlackOut(Dictionary<string, string> tagsDic)
        {
            bool isBlackout = false;
            string key = "BlackOUT";

            if (tagsDic != null && tagsDic.Keys.Contains(key))
                isBlackout = bool.Parse(tagsDic[key]);

            return isBlackout;
        }

        private Dictionary<string, string> getTags(Dictionary<string, object> inputObjectDic)
        {
            Dictionary<string, string> tagsDic = null;
            try
            {
                if (inputObjectDic.Keys.Contains("EPG_TAGS"))
                {
                    tagsDic = new Dictionary<string, string>();
                    object[] metasArray = (object[])inputObjectDic["EPG_TAGS"];
                    foreach (Dictionary<string, object> item in metasArray)
                    {
                        if (!tagsDic.Keys.Contains(item["Key"]))
                        {
                            tagsDic.Add(item["Key"].ToString(), item["Value"].ToString());
                        }
                        else
                        {
                            tagsDic[item["Key"].ToString()] += string.Format(" ,{0}", item["Value"].ToString());
                        }
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
