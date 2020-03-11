using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace EPG_XDTVTransform
{
    public class GraceNoteTransform
    {
        private string XSLT_DEFAULT_FILE;           
        private XslCompiledTransform m_oXslt;
        private XsltArgumentList m_oXslArguments;
        private GraceNoteAdapter m_oAdapter;

        public GraceNoteTransform(string xsltLocation)
        {
            m_oXslt = new XslCompiledTransform();
            m_oXslArguments = new XsltArgumentList();
            XSLT_DEFAULT_FILE = xsltLocation;
        }


        public void Init(int nChannelID, string sChannelName)
        {
            m_oAdapter = new GraceNoteAdapter();
            m_oXslArguments.AddExtensionObject("pda:Utils", m_oAdapter);
            m_oXslArguments.AddParam("ID", "", nChannelID);
            m_oXslArguments.AddParam("channelName", "", sChannelName);
            string from = DateTime.Now.AddYears(-1).ToString("yyyy-MM-ddThh:mm:ssZ");
            string to = DateTime.Now.AddYears(1).ToString("yyyy-MM-ddThh:mm:ssZ");
            m_oXslArguments.AddParam("from", "", from);
            m_oXslArguments.AddParam("to", "", to);
            m_oXslt.Load(XSLT_DEFAULT_FILE);
        }

        public void Transform(XmlDocument inputXml, StringWriter output)
        {
            using (output)
            {
                XmlReader reader = XmlReader.Create(new StringReader(inputXml.InnerXml));
                XmlWriter writer = XmlWriter.Create(output);
                m_oXslt.Transform(reader, m_oXslArguments, writer);
            }
        }

     

    }
}
