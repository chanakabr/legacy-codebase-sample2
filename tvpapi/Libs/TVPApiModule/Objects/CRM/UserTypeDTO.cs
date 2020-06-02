using System;
using ApiObjects;
using Newtonsoft.Json;

namespace TVPApiModule.Objects.CRM
{
    public class UserTypeDTO
    {
        public string Description;
        public string DescriptionField { get { return Description; } set { Description = value; } }

        public bool IsDefault;
        public bool IsDefaultField { get { return IsDefault; } set { IsDefault = value; } }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ID;

        public static UserTypeDTO ConvertToDTO(UserType userType)
        {
            UserTypeDTO res = new UserTypeDTO()
            {
                Description = userType.Description,
                IsDefault = userType.IsDefault,
                ID = userType.ID
            };
            return res;
        }

        public static UserType ConvertToCore(UserTypeDTO userType)
        {
            if(userType == null)
            {
                return default(ApiObjects.UserType);
            }
            UserType res = new UserType();
            res.Description = userType.Description;
            res.IsDefault = userType.IsDefault;
            res.ID = userType.ID;
            return res;
        }
    }
}