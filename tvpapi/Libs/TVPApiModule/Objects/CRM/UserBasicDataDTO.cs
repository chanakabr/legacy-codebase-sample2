using Core.Users;
using Newtonsoft.Json;


namespace TVPApiModule.Objects.CRM
{
    public class UserBasicDataDTO
    {
        public string m_sFirstName;
        public string m_sFirstNameField { get { return m_sFirstName; } set { m_sFirstName = value; } }
        public string m_sLastName;
        public string m_sLastNameField { get { return m_sLastName; } set { m_sLastName = value; } }
        public string m_sUserName;
        public string m_sUserNameField { get { return m_sUserName; } set { m_sUserName = value; } }
        public string m_sEmail;
        public string m_sEmailField { get { return m_sEmail; } set { m_sEmail = value; } }
        public string m_sAddress;
        public string m_sAddressField { get { return m_sAddress; } set { m_sAddress = value; } }
        public string m_sCity;
        public string m_sCityField { get { return m_sCity; } set { m_sCity = value; } }
        public string m_sZip;
        public string m_sZipField { get { return m_sZip; } set { m_sZip = value; } }
        public string m_sPhone;
        public string m_sPhoneField { get { return m_sPhone; } set { m_sPhone = value; } }
        public string m_sFacebookID;
        public string m_sFacebookIDField { get { return m_sFacebookID; } set { m_sFacebookID = value; } }
        public string m_sFacebookImage;
        public string m_sFacebookImageField { get { return m_sFacebookImage; } set { m_sFacebookImage = value; } }
        public string m_sFacebookToken;
        public string m_sFacebookTokenField { get { return m_sFacebookToken; } set { m_sFacebookToken = value; } }
        public string m_sAffiliateCode;
        public string m_sAffiliateCodeField { get { return m_sAffiliateCode; } set { m_sAffiliateCode = value; } }
        public string m_CoGuid;
        public string m_CoGuidField { get { return m_CoGuid; } set { m_CoGuid = value; } }
        public UserTypeDTO m_UserType;
        public UserTypeDTO m_UserTypeField { get { return m_UserType; } set { m_UserType = value; } }
        public bool m_bIsFacebookImagePermitted;
        public bool m_bIsFacebookImagePermittedField { get { return m_bIsFacebookImagePermitted; } set { m_bIsFacebookImagePermitted = value; } }
        public string m_ExternalToken;
        public string m_ExternalTokenField { get { return m_ExternalToken; } set { m_ExternalToken = value; } }
        public string m_sTwitterToken;
        public string m_sTwitterTokenField { get { return m_sTwitterToken; } set { m_sTwitterToken = value; } }
        public string m_sTwitterTokenSecret;
        public string m_sTwitterTokenSecretField { get { return m_sTwitterTokenSecret; } set { m_sTwitterTokenSecret = value; } }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string m_sPassword;
        public string m_sPasswordField { get { return m_sPassword; } set { m_sPassword = value; } }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public StateDTO m_State;
        public StateDTO m_StateField { get { return m_State; } set { m_State = value; } }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CountryDTO m_Country;
        public CountryDTO m_CountryField { get { return m_Country; } set { m_Country = value; } }

        public static UserBasicDataDTO ConvertToDTO(UserBasicData userBasicData)
        {
            if (userBasicData == null)
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