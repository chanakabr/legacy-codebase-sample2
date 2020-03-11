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
    public sealed class RoviTransform : BaseTransformHandler
    {
        string XSL_Movie_FILE =     TVinciShared.WS_Utils.GetTcmConfigValue("ROVI_XSLT_PATH") + "Rovi_movie_transform.xsl";
        string XSL_Series_FILE =    TVinciShared.WS_Utils.GetTcmConfigValue("ROVI_XSLT_PATH") + "Rovi_series_transform.xsl";
        string XSL_Episode_FILE =   TVinciShared.WS_Utils.GetTcmConfigValue("ROVI_XSLT_PATH") + "Rovi_episode_transform.xsl";
        string XSL_CMT_FILE =       TVinciShared.WS_Utils.GetTcmConfigValue("ROVI_XSLT_PATH") + "Rovi_CMT_transform.xsl";

        XslCompiledTransform m_oXsltMovies  = new XslCompiledTransform();
        XslCompiledTransform m_oXsltEpisode = new XslCompiledTransform();
        XslCompiledTransform m_oXsltSeries  = new XslCompiledTransform();
        XslCompiledTransform m_oXsltCMT     = new XslCompiledTransform();

        public enum assetType
        {
            MOVIE = 1,
            EPISODE,
            SERIES,
            CMT
        }

        public override void Transform(string pathInputFile, string nameInputFile, StringWriter output)
        {
        }
        public override void Transform(XmlDocument inputXml, StringWriter output)
        {
        }

        // transform and add the url physical files
        public void TransformA(string pathInputFile, string nameInputFile, StringWriter output, assetType at)
        {
            using (output)
            {
                switch (at)
                {
                    case assetType.EPISODE:
                    {
                        m_oXsltEpisode.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);
                        break;
                    }
                    case assetType.MOVIE:
                    {
                        m_oXsltMovies.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);
                        break;
                    }
                    case assetType.SERIES:
                    {
                        m_oXsltSeries.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);
                        break;
                    }
                    case assetType.CMT:
                    {
                        m_oXsltCMT.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);
                        break;
                    }
                }
            }
        }

        // transform and add the url physical files
        public void TransformA(XmlDocument inputXml, StringWriter output, assetType at)
        {
            XmlReader reader = XmlReader.Create(new StringReader(inputXml.InnerXml));
            XmlWriter writer = XmlWriter.Create(output);

            using (output)
            {
                switch (at)
                {
                    case assetType.EPISODE:
                    {
                        m_oXsltEpisode.Transform(reader, writer);
                        break;
                    }
                    case assetType.MOVIE:
                    {
                        m_oXsltMovies.Transform(reader, writer);
                        break;
                    }
                    case assetType.SERIES:
                    {
                        m_oXsltSeries.Transform(reader, writer);
                        break;
                    }
                    case assetType.CMT:
                    {
                        m_oXsltCMT.Transform(reader, writer);
                        break;
                    }
                }
            }
        }

        public override void Init()
        {
            // init the adapter
            base.m_oAdapter = new RoviAdapter();
            m_oAdapter.Init();
            m_oXslArguments.AddExtensionObject("pda:ADIUtils", m_oAdapter as BaseXMLAdapter);

            m_oXsltEpisode.Load(XSL_Episode_FILE);
            m_oXsltMovies.Load(XSL_Movie_FILE);
            m_oXsltSeries.Load(XSL_Series_FILE);
            m_oXsltCMT.Load(XSL_CMT_FILE);

            base.Init();  // Init base
        }
    }
}
