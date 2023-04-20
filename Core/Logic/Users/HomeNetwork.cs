using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Google.Protobuf;

namespace Core.Users
{
    [DataContract]
    [Serializable]
    [JsonObject(Id = "HomeNetwork")]
    public class HomeNetwork : IEquatable<HomeNetwork>, IDeepCloneable<HomeNetwork>
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string UID;
        [DataMember]
        public string Description;
        [DataMember]
        public DateTime CreateDate;
        [DataMember]
        public bool IsActive;

        public HomeNetwork()
        {
            Name = string.Empty;
            UID = string.Empty;
            Description = string.Empty;
            CreateDate = DateTime.MinValue;
            IsActive = false;
        }

        public HomeNetwork(string name, string uID, string desc, DateTime createDate, bool isActive)
        {
            Name = name;
            UID = uID;
            Description = desc;
            CreateDate = createDate;
            IsActive = isActive;
        }

        public HomeNetwork(HomeNetwork other) {
            Name = other.Name;
            UID = other.UID;
            Description = other.Description;
            CreateDate = other.CreateDate;
            IsActive = other.IsActive;
        }
        
        public bool Equals(HomeNetwork other)
        {
            return UID.Equals(other.UID);
        }

        public HomeNetwork Clone()
        {
            return new HomeNetwork(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Concat("Name: ", Name));
            sb.Append(String.Concat(", UID: ", UID));
            sb.Append(String.Concat(", Description: ", Description));
            sb.Append(String.Concat(", CreateDate: ", CreateDate));
            sb.Append(String.Concat(", IsActive: ", IsActive.ToString().ToLower()));

            return sb.ToString();
        }
    }

    public class HomeNetworksResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<HomeNetwork> HomeNetworks { get; set; }
    }

    public class HomeNetworkResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public HomeNetwork HomeNetwork { get; set; }
    }
}
