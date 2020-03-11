using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPApiModule.Objects.CRM
{
    public class State
    {
        private int m_nObjecrtIDField;

        private string m_sStateNameField;

        private string m_sStateCodeField;

        private Country m_CountryField;

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
        public string state_name
        {
            get
            {
                return this.m_sStateNameField;
            }
            set
            {
                this.m_sStateNameField = value;
            }
        }

        /// <remarks/>
        public string state_code
        {
            get
            {
                return this.m_sStateCodeField;
            }
            set
            {
                this.m_sStateCodeField = value;
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
    }
}