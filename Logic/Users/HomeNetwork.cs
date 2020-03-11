using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Users
{
    [DataContract]
    [Serializable]
    [JsonObject(Id = "HomeNetwork")]
    public class HomeNetwork : IEquatable<HomeNetwork>
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
            this.Name = name;
            this.UID = uID;
            this.Description = desc;
            this.CreateDate = createDate;
            this.IsActive = isActive;
        }


        public bool Equals(HomeNetwork other)
        {
            return UID.Equals(other.UID);
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
