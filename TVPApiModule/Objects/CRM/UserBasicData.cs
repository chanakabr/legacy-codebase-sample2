using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}