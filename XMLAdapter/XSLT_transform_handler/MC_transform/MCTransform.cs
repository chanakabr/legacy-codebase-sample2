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
    public sealed class MCTransform : BaseTransformHandler
    {
        // TODO: later on, read these configuration information fron outside
        const string XSL_DEFAULT_FILE = @"C:\ode-2013\TVM\Libs\Core\XMLAdapter\XSLTFiles\MC_transform.xsl";
        const string XSL_DEFAULT_FILE_S = @"C:\ode-2013\TVM\Libs\Core\XMLAdapter\XSLTFiles\MC_transform_series.xsl";

        protected XslCompiledTransform m_oXslt_s = new XslCompiledTransform();

        // transform and add the physical url files
        public override void Transform(string pathInputFile, string nameInputFile, StringWriter output)
        {
            using (output)
            {
                m_oXslt.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);
            }
        }

        public void Transform_S(string pathInputFile, string nameInputFile, StringWriter output)
        {
            using (output)
            {
                m_oXslt_s.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);
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

        // transform and add the physical url files
        public void Transform_S(XmlDocument inputXml, StringWriter output)
        {
            using (output)
            {
                XmlReader reader = XmlReader.Create(new StringReader(inputXml.InnerXml));
                XmlWriter writer = XmlWriter.Create(output);
                m_oXslt_s.Transform(reader, m_oXslArguments, writer);
            }
        }

        public override void Init()
        {
            // init the adapter
            base.m_oAdapter = new MCAdapter();
            m_oAdapter.Init();
            m_oXslArguments.AddExtensionObject("pda:ADIUtils", m_oAdapter as BaseXMLAdapter);

            m_oXslt.Load(XSL_DEFAULT_FILE);
            m_oXslt_s.Load(XSL_DEFAULT_FILE_S);

            base.Init();  // Init base
        }
    }
}
