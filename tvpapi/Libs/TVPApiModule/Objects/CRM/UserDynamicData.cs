using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPApiModule.Objects.CRM
{
    public class UserDynamicData
    {
        private UserDynamicDataContainer[] m_sUserDataField;

        /// <remarks/>
        public UserDynamicDataContainer[] user_data
        {
            get
            {
                return this.m_sUserDataField;
            }
            set
            {
                this.m_sUserDataField = value;
            }
        }
    }
}