using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using XMLAdapter;
using System.Xml;
using System.Xml.Xsl;

namespace XSLT_transform_handlar
{
    public abstract class BaseTransformHandler
    {
        // the transform and the argument objects
        protected XslCompiledTransform m_oXslt       = new XslCompiledTransform();
        protected XsltArgumentList m_oXslArguments   = new XsltArgumentList();


        public BaseTransformHandler()
        {
        }

        public virtual void Init()
        {
            
        }

        public abstract void Transform(string pathInputFile, string nameInputFile, StringWriter output);
        public abstract void Transform(XmlDocument inputXml, StringWriter output);

        protected XMLAdapter.BaseXMLAdapter m_oAdapter;
    }
}
