using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;
using System.Web;

namespace Tvinci.Configuration
{


    public abstract class ConfigurationHandler<TConfiguration> : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            XmlSerializer xs = new XmlSerializer(typeof(TConfiguration));
            StringReader sr = new StringReader(section.OuterXml);
            return xs.Deserialize(sr);
        }
    }
}
