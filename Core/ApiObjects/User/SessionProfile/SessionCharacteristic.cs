using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.User.SessionProfile
{
    [Serializable]
    public class SessionCharacteristic
    {
        [JsonProperty("RegionId")]
        public int RegionId { get; }
        
        [JsonProperty("UserSegments")]
        public List<long> UserSegments { get; }

        [JsonProperty("UserRoles")]
        public List<long> UserRoles { get; }
        
        [JsonProperty("UserSessionProfileIds")]
        public List<long> UserSessionProfileIds { get; }

        public SessionCharacteristic(int regionId, List<long> userSegments, List<long> userRoles, List<long> userSessionProfileIds)
        {
            RegionId = regionId;
            UserSegments = userSegments;
            UserRoles = userRoles;
            UserSessionProfileIds = userSessionProfileIds;
        }
    }
}