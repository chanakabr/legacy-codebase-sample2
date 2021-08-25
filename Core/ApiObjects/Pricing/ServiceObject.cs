using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Pricing
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ServiceObject
    {
        public long ID;
        public string Name;

        public ServiceObject()
        {
        }

        public ServiceObject(long id, string name)
        {
            ID = id;
            Name = name;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class NpvrServiceObject : ServiceObject
    {
        public long Quota;

        public NpvrServiceObject() : base()
        {
        }

        public NpvrServiceObject(long id, string name, long quota) : base (id, name)
        {
            Quota = quota;
        }
    }

}
