using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoviFeeder.ObjectList
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class RoviNowtilusVodApi
    {

        private string queryListField;

        private RoviNowtilusVodApiRequest requestField;

        private RoviNowtilusVodApiResponse responseField;

        private RoviNowtilusVodApiPresentation[] presentationListField;

        private RoviNowtilusVodApiPresentationCount presentationCountField;

        /// <remarks/>
        public string QueryList
        {
            get
            {
                return this.queryListField;
            }
            set
            {
                this.queryListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiRequest Request
        {
            get
            {
                return this.requestField;
            }
            set
            {
                this.requestField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiResponse Response
        {
            get
            {
                return this.responseField;
            }
            set
            {
                this.responseField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Presentation", IsNullable = false)]
        public RoviNowtilusVodApiPresentation[] PresentationList
        {
            get
            {
                return this.presentationListField;
            }
            set
            {
                this.presentationListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationCount PresentationCount
        {
            get
            {
                return this.presentationCountField;
            }
            set
            {
                this.presentationCountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiRequest
    {

        private string requestUriField;

        private string ip4AdressField;

        private string timeField;

        private uint timeUnixField;

        /// <remarks/>
        public string RequestUri
        {
            get
            {
                return this.requestUriField;
            }
            set
            {
                this.requestUriField = value;
            }
        }

        /// <remarks/>
        public string Ip4Adress
        {
            get
            {
                return this.ip4AdressField;
            }
            set
            {
                this.ip4AdressField = value;
            }
        }

        /// <remarks/>
        public string Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        public uint TimeUnix
        {
            get
            {
                return this.timeUnixField;
            }
            set
            {
                this.timeUnixField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiResponse
    {

        private RoviNowtilusVodApiResponseSpecification specificationField;

        private System.DateTime timeField;

        private uint timeUnixField;

        /// <remarks/>
        public RoviNowtilusVodApiResponseSpecification Specification
        {
            get
            {
                return this.specificationField;
            }
            set
            {
                this.specificationField = value;
            }
        }

        /// <remarks/>
        public System.DateTime Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        public uint TimeUnix
        {
            get
            {
                return this.timeUnixField;
            }
            set
            {
                this.timeUnixField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiResponseSpecification
    {

        private decimal versionField;

        /// <remarks/>
        public decimal Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentation
    {

        private ushort iField;

        private string typeField;

        private string hrefField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort i
        {
            get
            {
                return this.iField;
            }
            set
            {
                this.iField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/1999/xlink")]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/1999/xlink")]
        public string href
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationCount
    {

        private string resourceField;

        private ushort valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string resource
        {
            get
            {
                return this.resourceField;
            }
            set
            {
                this.resourceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public ushort Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}
