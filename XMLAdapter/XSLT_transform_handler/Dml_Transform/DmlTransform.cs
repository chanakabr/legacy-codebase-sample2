using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using XMLAdapter;
using System.Xml;
using System.Xml.Xsl;
using System.Configuration;

namespace XSLT_transform_handlar
{
    public sealed class DmlTransform : BaseTransformHandler
    {
        // TODO: later on, read these configuration information fron outside
        string XSL_DEFAULT_FILE = TVinciShared.WS_Utils.GetTcmConfigValue("DML_XSLT_PATH");

        // transform and add the physical url files
        public override void Transform(string pathInputFile, string nameInputFile, StringWriter output)
        {
            using (output)
            {
                m_oXslt.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);
            }
        }

        // transform and add the physical url files
        public override void Transform(XmlDocument inputXml, StringWriter output)
        {
            using (output)
            {
                XmlReader reader = XmlReader.Create(new StringReader(inputXml.InnerXml));
                XmlWriter writer = XmlWriter.Create(output);
                m_oXslt.Transform(reader, m_oXslArguments, writer);
            }
        }

        public override void Init()
        {
            // init the adapter
            base.m_oAdapter = new DmlAdapter();
            m_oAdapter.Init();
            m_oXslArguments.AddExtensionObject("pda:ADIUtils", m_oAdapter as BaseXMLAdapter);

            m_oXslt.Load(XSL_DEFAULT_FILE);

            base.Init();  // Init base
        }
    }
}
