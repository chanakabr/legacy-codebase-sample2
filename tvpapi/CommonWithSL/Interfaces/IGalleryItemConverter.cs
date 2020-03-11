using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL.Interfaces
{
    public interface IGalleryItemConverter
    {
        object ConvertItem(object inputObject, string picSize = null);
        object ConvertItem(Dictionary<string, object> inputObjectDic, string picSize = null);
    }
}
