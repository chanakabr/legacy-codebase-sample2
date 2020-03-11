using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace Tvinci.Helpers
{
    public enum eSource
    {
        File,
        URL
    }

    public enum eMode
    {
        Synced,
        NotSynced
    }

    public class XMLValidator
    {        
        StringBuilder m_errorMessages = new StringBuilder();
        
        public void ValidationHandler(object sender,
                                             ValidationEventArgs args)
        {
            if (m_errorMessages.Length != 0)
            {
                m_errorMessages.AppendLine();
            }

            m_errorMessages.Append(string.Format("line {0} position {1} - {2}",args.Exception.LineNumber, args.Exception.LinePosition,args.Message));                        
        }

        public bool ValidateXml(string strXMLDoc, string xsdSchema, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Declare local objects
                XmlTextReader tr = new XmlTextReader(new StringReader(xsdSchema));

                XmlSchemaCollection xsc = new XmlSchemaCollection();
                xsc.Add(null, tr);
                                                
                // XML validator object
                XmlValidatingReader vr = new XmlValidatingReader(strXMLDoc,
                             XmlNodeType.Document, null);

                vr.Schemas.Add(xsc);
                
                // Add validation event handler

                vr.ValidationType = ValidationType.Schema;
                
                vr.ValidationEventHandler += new ValidationEventHandler(ValidationHandler);

                // Validate XML data
                while (vr.Read()) ;

                vr.Close();

                // Raise exception, if XML validation fails
                if (m_errorMessages.Length != 0)
                {
                    errorMessage = m_errorMessages.ToString();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }


    public  class XmlConfiguration
    {
        public eSource Source { get; set; }
        public string Path { get; set; }
        public bool ShouldWatch { get; set; }

        public XmlConfiguration(eSource source, string path)
        {
            Path = path;
            Source = source;
        }
    }

    public sealed class XmlManager<TConfigurationObject>
    {
        public XmlConfiguration Configuration { get; private set; }
        ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();
        TConfigurationObject m_data;
        public eMode SyncMode { get; private set; }

        public XmlManager(XmlConfiguration configuration)
        {
            Configuration = configuration;
            SyncMode = eMode.NotSynced;
        }

        public TConfigurationObject Data
        {
            get
            {
                if (m_locker.TryEnterReadLock(4000))
                {
                    try
                    {
                        return m_data;
                    }
                    finally
                    {
                        m_locker.ExitReadLock();
                    }
                }

                return default(TConfigurationObject);
            }

        }

        public void Sync()
        {
            if (m_locker.TryEnterWriteLock(4000))
            {
                try
                {
                    validateConfiguration();

                    m_data = default(TConfigurationObject);
                    SyncMode = eMode.NotSynced;

                    object value = null;

                    switch (Configuration.Source)
                    {
                        case eSource.File:
                            value = handleFile();
                            break;
                        case eSource.URL:
                            break;
                        default:
                            break;
                    }

                    if (value != null && value is TConfigurationObject)
                    {
                        m_data = (TConfigurationObject)value;
                        SyncMode = eMode.Synced;
                    }
                }
                finally
                {
                    m_locker.ExitWriteLock();
                }
            }
        }

        private object handleFile()
        {
            string file = HttpContext.Current.Server.MapPath(Configuration.Path);
            
            XmlSerializer xs = new XmlSerializer(typeof(TConfigurationObject));

            StringReader sr = new StringReader(File.ReadAllText(file));

            return xs.Deserialize(sr);
            
        }

        private void validateConfiguration()
        {
            
        }
    }
}
