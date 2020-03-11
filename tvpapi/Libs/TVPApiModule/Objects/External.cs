using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApi.External
{
    public struct DeviceInfo
    {
        public string Name;
        public string UDID;
        public string Type;
        public bool Active;
    }

    public struct ResponseStatus
    {
        public string Code;
        public string Description;
    }
}
