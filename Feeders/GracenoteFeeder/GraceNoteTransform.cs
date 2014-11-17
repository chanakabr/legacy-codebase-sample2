using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace GracenoteFeeder
{
    public class GraceNoteTransform
    {
        string XSLT_DEFAULT_FILE = TVinciShared.WS_Utils.GetTcmConfigValue("GraceNote_XSLT_PATH");

        private XslCompiledTransform m_oXslt ;        
        private XsltArgumentList m_oXslArguments; 
        private GraceNoteAdapter m_oAdapter;

        public GraceNoteTransform()
        {
            m_oXslt = new XslCompiledTransform();
            m_oXslArguments = new XsltArgumentList();
        }


        public void Init(int nChannelID, string sChannelName)
        {           
            m_oAdapter = new GraceNoteAdapter();         
            m_oXslArguments.AddExtensionObject("pda:Utils", m_oAdapter);
            m_oXslArguments.AddParam("ID", "", nChannelID);
            m_oXslArguments.AddParam("channelName", "", sChannelName);
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
