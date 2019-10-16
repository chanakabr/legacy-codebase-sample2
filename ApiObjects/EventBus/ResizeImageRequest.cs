using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.EventBus
{
    public class ResizeImageRequest : ServiceEvent
    {
        public string size;
        public string url;
    }
}
