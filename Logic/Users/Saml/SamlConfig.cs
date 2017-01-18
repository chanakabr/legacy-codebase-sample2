using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using KLogMonitor;

namespace Core.Users.Saml
{
    public class SamlConfig : ConfigurationSection
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly SamlConfig Instance = new SamlConfig();
        }

        private Dictionary<int, SAML> m_saml;

        private SamlConfig()
        {
            m_saml = new Dictionary<int, SAML>();
        }

        public static SamlConfig Instance
        {
            get { return Nested.Instance; }
        }

        public SAML GetSaml(int nGroupID)
        {
            if (!m_saml.ContainsKey(nGroupID))
            {
                SAML tempsaml = BuildSaml(nGroupID);
                if (tempsaml != null)
                {
                    m_saml.Add(nGroupID, tempsaml);
                }
            }
            if (m_saml != null && m_saml.ContainsKey(nGroupID))
            {
                return m_saml[nGroupID];
            }
            return null;
        }



        private SAML BuildSaml(int nGroupID)
        {
            try
            {
                XmlDocument doc = new XmlDocument();



                string samlConfigFile = TVinciShared.WS_Utils.GetTcmConfigValue("SAML_CONFIG_FILE");
                doc.Load(@samlConfigFile + nGroupID + ".config"); //TO DO savefile path ????

                // doc.Load(@"J:\\ODE_Regular\\TVM\\Web Services\\WS_Users\\\SamlConfig\\" + nGroupID + ".config"); //TO DO savefile path ????
                SAML saml = new SAML();
                saml.entityID = doc.GetElementsByTagName("EntityDescriptor")[0].Attributes["entityID"].Value; // doc.ChildNodes[1].Attributes[0].Value;
                saml.keyInfo = doc.GetElementsByTagName("ds:KeyInfo")[0].Attributes["xmlns:ds"].Value; // doc.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].NamespaceURI;

                foreach (XmlElement item in doc.GetElementsByTagName("SingleSignOnService"))
                {
                    string bindingStr = item.GetAttribute("Binding");
                    if (!string.IsNullOrEmpty(bindingStr) && bindingStr == "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect")
                    {
                        saml.singleSignOnService = item.GetAttribute("Location"); //doc.ChildNodes[1].ChildNodes[0].ChildNodes[9].Attributes[1].Value;
                        break;
                    }
                }

                saml.x509 = doc.GetElementsByTagName("ds:X509Certificate")[0].InnerText; //doc.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerText;
                if (doc.GetElementsByTagName("PrivateKey") != null && doc.GetElementsByTagName("PrivateKey")[0] != null)
                {
                    if (doc.GetElementsByTagName("PrivateKey")[0].ChildNodes != null)
                        saml.privateKey = doc.GetElementsByTagName("PrivateKey")[0].ChildNodes[0].OuterXml;
                }
                return saml;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return null;
            }
        }
    }
}
