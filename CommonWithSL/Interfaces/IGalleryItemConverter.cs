using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL.Interfaces
{
    public interface IGalleryItemConverter
    {
        Media ConvertItem(object inputObject, string picSize = null);
        Media ConvertItem(Dictionary<string, object> inputObjectDic, string picSize = null);
    }
}
