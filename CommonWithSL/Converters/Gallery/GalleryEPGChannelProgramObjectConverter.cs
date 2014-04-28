using CommonWithSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL.Converters.Gallery
{
    public class GalleryEPGChannelProgramObjectConverter : IGalleryItemConverter
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
                    ID = inputObjectDic["media_id"].ToString(),
                    Title = inputObjectDic["NAME"].ToString(),
                    ImageLink = inputObjectDic["PIC_URL"].ToString(),
                    EpgId = inputObjectDic["EPG_ID"].ToString(),
                    MediaTemplate = "GalleryEPGItemTemplate"
                };
            }
            catch { }
            return convertedItem;
        }
    }
}
