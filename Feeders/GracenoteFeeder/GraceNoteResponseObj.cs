using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GracenoteFeeder
{

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class RESPONSES
    {

        private RESPONSESRESPONSE rESPONSEField;

        /// <remarks/>
        public RESPONSESRESPONSE RESPONSE
        {
            get
            {
                return this.rESPONSEField;
            }
            set
            {
                this.rESPONSEField = value;
            }
        }
    }   

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RESPONSESRESPONSE
    {

        private RESPONSESRESPONSEUPDATE_INFO uPDATE_INFOField;

        private string sTATUSField;

        /// <remarks/>
        public RESPONSESRESPONSEUPDATE_INFO UPDATE_INFO
        {
            get
            {
                return this.uPDATE_INFOField;
            }
            set
            {
                this.uPDATE_INFOField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string STATUS
        {
            get
            {
                return this.sTATUSField;
            }
            set
            {
                this.sTATUSField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RESPONSESRESPONSEUPDATE_INFO
    {

        private string uPDATE_TYPEField;

        private string uPDATE_INSTField;

        private uint sTAMPField;

        private RESPONSESRESPONSEUPDATE_INFOURL uRLField;

        /// <remarks/>
        public string UPDATE_TYPE
        {
            get
            {
                return this.uPDATE_TYPEField;
            }
            set
            {
                this.uPDATE_TYPEField = value;
            }
        }

        /// <remarks/>
        public string UPDATE_INST
        {
            get
            {
                return this.uPDATE_INSTField;
            }
            set
            {
                this.uPDATE_INSTField = value;
            }
        }

        /// <remarks/>
        public uint STAMP
        {
            get
            {
                return this.sTAMPField;
            }
            set
            {
                this.sTAMPField = value;
            }
        }

        /// <remarks/>
        public RESPONSESRESPONSEUPDATE_INFOURL URL
        {
            get
            {
                return this.uRLField;
            }
            set
            {
                this.uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RESPONSESRESPONSEUPDATE_INFOURL
    {

        private string tYPEField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TYPE
        {
            get
            {
                return this.tYPEField;
            }
            set
            {
                this.tYPEField = value;
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
}
