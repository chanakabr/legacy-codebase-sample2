using Core.Users;
using Newtonsoft.Json;


namespace TVPApiModule.Objects.CRM
{
    public class UserBasicDataDTO
    {
        public string m_sFirstName;
        public string m_sLastName;
        public string m_sUserName;
        public string m_sEmail;
        public string m_sAddress;
        public string m_sCity;
        public string m_sZip;
        public string m_sPhone;
        public string m_sFacebookID;
        public string m_sFacebookImage;
        public string m_sFacebookToken;
        public string m_sAffiliateCode;
        public string m_CoGuid;
        public UserTypeDTO m_UserType;
        public bool m_bIsFacebookImagePermitted;
        public string m_ExternalToken;
        public string m_sTwitterToken;
        public string m_sTwitterTokenSecret;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string m_sPassword;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public StateDTO m_State;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CountryDTO m_Country;
       
        

        public static UserBasicDataDTO ConvertToDTO(UserBasicData userBasicData)
        {
            if(userBasicData == null)
            {
                return null;
            }
            
            UserBasicDataDTO res = new UserBasicDataDTO();
            res.m_sAddress = userBasicData.m_sAddress;
            res.m_sAffiliateCode = userBasicData.m_sAffiliateCode;
            res.m_sCity = userBasicData.m_sCity;
            res.m_CoGuid = userBasicData.m_CoGuid;
            res.m_sEmail = userBasicData.m_sEmail;
            res.m_ExternalToken = userBasicData.m_ExternalToken;
            res.m_sFacebookID = userBasicData.m_sFacebookID;
            res.m_sFacebookImage = userBasicData.m_sFacebookImage;
            res.m_sFacebookToken = userBasicData.m_sFacebookToken;
            res.m_sFirstName = userBasicData.m_sFirstName;
            res.m_bIsFacebookImagePermitted = userBasicData.m_bIsFacebookImagePermitted;
            res.m_sLastName = userBasicData.m_sLastName;
            res.m_sPhone = userBasicData.m_sPhone;
            res.m_sUserName = userBasicData.m_sUserName;
            res.m_UserType = UserTypeDTO.ConvertToDTO(userBasicData.m_UserType);
            res.m_sTwitterTokenSecret = userBasicData.m_sTwitterTokenSecret;
            res.m_sZip = userBasicData.m_sZip;
            res.m_Country = CountryDTO.ConvertToDTO(userBasicData.m_Country);
            res.m_State = StateDTO.ConvertToDTO(userBasicData.m_State);
            res.m_sTwitterToken = userBasicData.m_sTwitterToken;
            return res;
        }

        public UserBasicData ToCore()
        {
            return new UserBasicData()
            {
                m_bIsFacebookImagePermitted = this.m_bIsFacebookImagePermitted,
                m_CoGuid = this.m_CoGuid,
                m_Country = CountryDTO.ConvertToCore(m_Country),
                m_ExternalToken = this.m_ExternalToken,
                m_sAddress = this.m_sAddress,
                m_sAffiliateCode = this.m_sAffiliateCode,
                m_sCity = this.m_sCity,
                m_sEmail = this.m_sEmail,
                m_sFacebookID = this.m_sFacebookID,
                m_sFacebookImage = this.m_sFacebookImage,
                m_sFacebookToken = this.m_sFacebookToken,
                m_sFirstName = this.m_sFirstName,
                m_UserType = UserTypeDTO.ConvertToCore(m_UserType),
                m_sLastName = this.m_sLastName,
                m_sPhone = this.m_sPhone,
                m_State = StateDTO.ConvertToCore(m_State),
                m_sTwitterToken = this.m_sTwitterToken,
                m_sTwitterTokenSecret = this.m_sTwitterTokenSecret,
                m_sUserName = this.m_sUserName,
                m_sZip = this.m_sZip,
            };
        }
    }
}