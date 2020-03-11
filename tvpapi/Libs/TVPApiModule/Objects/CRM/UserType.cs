using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPApiModule.Objects.CRM
{
    public class UserType
    {
        private System.Nullable<int> idField;

        private string descriptionField;

        private bool isDefaultField;

        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public System.Nullable<int> id
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

        public string description
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

        public bool is_default
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
    }
}