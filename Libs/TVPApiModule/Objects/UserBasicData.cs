using ApiObjects;
using Core.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace TVPApiModule.Objects
{
    public class UserBasicData
    {
        public string m_sFirstNameField;

        public string m_sLastNameField;

        public string m_sUserNameField;

        public string m_sEmailField;

        public string m_sAddressField;

        public string m_sCityField;

        public Core.Users.Country m_CountryField;

        public string m_sZipField;

        public string m_sPhoneField;

        public string m_sFacebookIDField;

        public string m_sFacebookImageField;

        public string m_sFacebookTokenField;

        public string m_sAffiliateCodeField;

        public string m_CoGuidField;

        public UserType m_UserTypeField;

        public State m_StateField;

        public bool m_bIsFacebookImagePermittedField;

        public string m_ExternalTokenField;

        public string m_sTwitterTokenField;

        public string m_sTwitterTokenSecretField;

        /// <remarks/>
        public string m_sFirstName
        {
            get
            {
                return this.m_sFirstNameField;
            }
            set
            {
                this.m_sFirstNameField = value;
            }
        }

        /// <remarks/>
        public string m_sLastName
        {
            get
            {
                return this.m_sLastNameField;
            }
            set
            {
                this.m_sLastNameField = value;
            }
        }

        /// <remarks/>
        public string m_sUserName
        {
            get
            {
                return this.m_sUserNameField;
            }
            set
            {
                this.m_sUserNameField = value;
            }
        }

        /// <remarks/>
        public string m_sEmail
        {
            get
            {
                return this.m_sEmailField;
            }
            set
            {
                this.m_sEmailField = value;
            }
        }

        /// <remarks/>
        public string m_sAddress
        {
            get
            {
                return this.m_sAddressField;
            }
            set
            {
                this.m_sAddressField = value;
            }
        }

        /// <remarks/>
        public string m_sCity
        {
            get
            {
                return this.m_sCityField;
            }
            set
            {
                this.m_sCityField = value;
            }
        }

        /// <remarks/>
        public Core.Users.Country m_Country
        {
            get
            {
                return this.m_CountryField;
            }
            set
            {
                this.m_CountryField = value;
            }
        }

        /// <remarks/>
        public string m_sZip
        {
            get
            {
                return this.m_sZipField;
            }
            set
            {
                this.m_sZipField = value;
            }
        }

        /// <remarks/>
        public string m_sPhone
        {
            get
            {
                return this.m_sPhoneField;
            }
            set
            {
                this.m_sPhoneField = value;
            }
        }

        /// <remarks/>
        public string m_sFacebookID
        {
            get
            {
                return this.m_sFacebookIDField;
            }
            set
            {
                this.m_sFacebookIDField = value;
            }
        }

        /// <remarks/>
        public string m_sFacebookImage
        {
            get
            {
                return this.m_sFacebookImageField;
            }
            set
            {
                this.m_sFacebookImageField = value;
            }
        }

        /// <remarks/>
        public string m_sFacebookToken
        {
            get
            {
                return this.m_sFacebookTokenField;
            }
            set
            {
                this.m_sFacebookTokenField = value;
            }
        }

        /// <remarks/>
        public string m_sAffiliateCode
        {
            get
            {
                return this.m_sAffiliateCodeField;
            }
            set
            {
                this.m_sAffiliateCodeField = value;
            }
        }

        /// <remarks/>
        public string m_CoGuid
        {
            get
            {
                return this.m_CoGuidField;
            }
            set
            {
                this.m_CoGuidField = value;
            }
        }

        /// <remarks/>
        public UserType m_UserType
        {
            get
            {
                return this.m_UserTypeField;
            }
            set
            {
                this.m_UserTypeField = value;
            }
        }

        /// <remarks/>
        public State m_State
        {
            get
            {
                return this.m_StateField;
            }
            set
            {
                this.m_StateField = value;
            }
        }

        /// <remarks/>
        public bool m_bIsFacebookImagePermitted
        {
            get
            {
                return this.m_bIsFacebookImagePermittedField;
            }
            set
            {
                this.m_bIsFacebookImagePermittedField = value;
            }
        }

        /// <remarks/>
        public string m_ExternalToken
        {
            get
            {
                return this.m_ExternalTokenField;
            }
            set
            {
                this.m_ExternalTokenField = value;
            }
        }

        /// <remarks/>
        public string m_sTwitterToken
        {
            get
            {
                return this.m_sTwitterTokenField;
            }
            set
            {
                this.m_sTwitterTokenField = value;
            }
        }

        /// <remarks/>
        public string m_sTwitterTokenSecret
        {
            get
            {
                return this.m_sTwitterTokenSecretField;
            }
            set
            {
                this.m_sTwitterTokenSecretField = value;
            }
        }

        public Core.Users.UserBasicData ToCore()
        {
            return new Core.Users.UserBasicData()
            {
                m_bIsFacebookImagePermitted = this.m_bIsFacebookImagePermitted,
                m_CoGuid = this.m_CoGuid,
                m_Country = this.m_Country,
                m_ExternalToken = this.m_ExternalToken,
                m_sAddress = this.m_sAddress,
                m_sAffiliateCode = this.m_sAffiliateCode,
                m_sCity = this.m_sCity,
                m_sEmail = this.m_sEmail,
                m_sFacebookID = this.m_sFacebookID,
                m_sFacebookImage = this.m_sFacebookImage,
                m_sFacebookToken = this.m_sFacebookToken,
                m_sFirstName = this.m_sFirstName,
                m_UserType = this.m_UserType.ToCore(),
                m_sLastName = this.m_sLastName,
                m_sPhone = this.m_sPhone,
                m_State = this.m_State,
                m_sTwitterToken =  this.m_sTwitterToken,
                m_sTwitterTokenSecret = this.m_sTwitterTokenSecret,
                m_sUserName = this.m_sUserName,
                m_sZip = this.m_sZip
            };
        }
    }

    public class UserType
    {
        public System.Nullable<int> idField;

        public string descriptionField;

        public bool isDefaultField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public System.Nullable<int> ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        public bool IsDefault
        {
            get
            {
                return this.isDefaultField;
            }
            set
            {
                this.isDefaultField = value;
            }
        }

        public ApiObjects.UserType ToCore()
        {
            return new ApiObjects.UserType()
            {
                Description = this.Description,
                ID = this.ID,
                IsDefault = this.IsDefault
            };
        }
    }
}
