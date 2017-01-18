using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Json.Converters
{
    [Serializable]
    public class BaseTimeConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public BaseTimeConverter()
        {
            base.DateTimeFormat = "yyyyMMddHHmmss";
        }
    }
}
