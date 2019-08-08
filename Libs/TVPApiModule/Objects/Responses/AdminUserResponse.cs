using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class AdminUserResponse
    {
        [JsonProperty(PropertyName = "admin_user")]
        public AdminUser AdminUser { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
    }
  
    public class AdminUser
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; } 

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "group_id")]
        public int GroupId { get; set; }

        public AdminUser(AdminAccountUserResponse userResponse)
        {
            Id = userResponse.m_adminUser.m_accountUserID;
            Username = userResponse.m_adminUser.m_accountUserName;
            Email = userResponse.m_adminUser.m_accountEmail;
            GroupId = userResponse.m_adminUser.m_groupID;
        }
    }
}
