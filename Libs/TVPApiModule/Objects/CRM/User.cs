using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPApiModule.Objects.CRM
{
    public class User
    {
        private UserBasicData m_oBasicDataField;

        private UserDynamicData m_oDynamicDataField;

        private string m_sSiteGUIDField;

        private int m_domianIDField;

        private bool m_isDomainMasterField;

        private UserState m_eUserStateField;

        private int m_nSSOOperatorIDField;

        /// <remarks/>
        public UserBasicData basic_data
        {
            get
            {
                return this.m_oBasicDataField;
            }
            set
            {
                this.m_oBasicDataField = value;
            }
        }

        /// <remarks/>
        public UserDynamicData dynamic_data
        {
            get
            {
                return this.m_oDynamicDataField;
            }
            set
            {
                this.m_oDynamicDataField = value;
            }
        }

        /// <remarks/>
        public string site_guid
        {
            get
            {
                return this.m_sSiteGUIDField;
            }
            set
            {
                this.m_sSiteGUIDField = value;
            }
        }

        /// <remarks/>
        public int domain_id
        {
            get
            {
                return this.m_domianIDField;
            }
            set
            {
                this.m_domianIDField = value;
            }
        }

        /// <remarks/>
        public bool is_domain_master
        {
            get
            {
                return this.m_isDomainMasterField;
            }
            set
            {
                this.m_isDomainMasterField = value;
            }
        }

        /// <remarks/>
        public UserState user_State
        {
            get
            {
                return this.m_eUserStateField;
            }
            set
            {
                this.m_eUserStateField = value;
            }
        }

        /// <remarks/>
        public int sso_opertaor_id
        {
            get
            {
                return this.m_nSSOOperatorIDField;
            }
            set
            {
                this.m_nSSOOperatorIDField = value;
            }
        }
    }
}