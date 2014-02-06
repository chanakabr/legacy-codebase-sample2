using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPApiModule.Objects.CRM
{
    public class Country
    {
        private int m_nObjecrtIDField;

        private string m_sCountryNameField;

        private string m_sCountryCodeField;

        /// <remarks/>
        public int object_id
        {
            get
            {
                return this.m_nObjecrtIDField;
            }
            set
            {
                this.m_nObjecrtIDField = value;
            }
        }

        /// <remarks/>
        public string country_name
        {
            get
            {
                return this.m_sCountryNameField;
            }
            set
            {
                this.m_sCountryNameField = value;
            }
        }

        /// <remarks/>
        public string country_code
        {
            get
            {
                return this.m_sCountryCodeField;
            }
            set
            {
                this.m_sCountryCodeField = value;
            }
        }
    }
}