using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;

namespace ApiObjects.Pricing
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ServiceObject : IDeepCloneable<ServiceObject>
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
        
        public ServiceObject(ServiceObject other) {
            ID = other.ID;
            Name = other.Name;
        }

        public ServiceObject Clone()
        {
            return new ServiceObject(this);
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
