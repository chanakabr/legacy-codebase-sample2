using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiModule.Objects.CRM
{
    public class UserBasicData
    {
        private string m_sUserNameField;

        private string m_sFirstNameField;

        private string m_sLastNameField;

        private string m_sEmailField;

        private string m_sAddressField;

        private string m_sCityField;

        private State m_StateField;

        private Country m_CountryField;

        private string m_sZipField;

        private string m_sPhoneField;

        private string m_sFacebookIDField;

        private string m_sFacebookImageField;

        private bool m_bIsFacebookImagePermittedField;

        private string m_sAffiliateCodeField;

        private string m_CoGuidField;

        private string m_ExternalTokenField;

        private string m_sFacebookTokenField;

        private UserType m_UserTypeField;

        /// <remarks/>
        public string user_name
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
        public string first_name
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
        public string last_name
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
        public string email
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
        public string address
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
        public string city
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
        public State state
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
        public Country country
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
        public string zip
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
        public string phone
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
        public string facebook_id
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
        public string facebook_image
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
        public bool is_facebook_image_permitted
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
        public string affiliate_code
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
        public string co_guid
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
        public string external_token
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
        public string facebook_token
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
        public UserType user_type
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

        public UserBasicData(TVPApiModule.Objects.Responses.UserBasicData userBasicData)
        {
            this.address = userBasicData.address;
            this.affiliate_code = userBasicData.affiliate_code;
            this.city = userBasicData.city;
            this.co_guid = userBasicData.co_guid;

            if (userBasicData.country != null)
            {
                this.country = new TVPApiModule.Objects.CRM.Country();

                this.country.country_code = userBasicData.country.country_code;
                this.country.country_name = userBasicData.country.country_name;
                this.country.object_id = userBasicData.country.object_id;
            }

            this.email = userBasicData.email;
            this.external_token = userBasicData.external_token;
            this.facebook_id = userBasicData.facebook_id;
            this.facebook_image = userBasicData.facebook_image;
            this.facebook_token = userBasicData.facebook_token;
            this.first_name = userBasicData.first_name;
            this.is_facebook_image_permitted = userBasicData.is_facebook_image_permitted;
            this.last_name = userBasicData.last_name;
            this.phone = userBasicData.phone;

            if (userBasicData.state != null)
            {
                this.state = new TVPApiModule.Objects.CRM.State();

                if (userBasicData.state.country != null)
                {
                    this.state.country = new TVPApiModule.Objects.CRM.Country();

                    this.state.country.country_code = userBasicData.state.country.country_code;
                    this.state.country.country_name = userBasicData.state.country.country_name;
                    this.state.country.object_id = userBasicData.state.country.object_id;
                }

                this.state.object_id = userBasicData.state.object_id;
                this.state.state_code = userBasicData.state.state_code;
                this.state.state_name = userBasicData.state.state_name;
            }

            this.user_name = userBasicData.user_name;

            if (userBasicData.user_type != null)
            {
                this.user_type = new TVPApiModule.Objects.CRM.UserType();

                this.user_type.description = userBasicData.user_type.description;
                this.user_type.id = userBasicData.user_type.id;
                this.user_type.is_default = userBasicData.user_type.is_default;
            }

            this.zip = userBasicData.zip;
        }
    }
}