using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPApiModule.Objects.CRM
{
    public class UserDynamicDataContainer
    {
        private string m_sDataTypeField;

        private string m_sValueField;

        /// <remarks/>
        public string data_type
        {
            get
            {
                return this.m_sDataTypeField;
            }
            set
            {
                this.m_sDataTypeField = value;
            }
        }

        /// <remarks/>
        public string value
        {
            get
            {
                return this.m_sValueField;
            }
            set
            {
                this.m_sValueField = value;
            }
        }
    }
}