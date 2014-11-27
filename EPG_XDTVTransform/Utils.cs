using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.IO.Compression;

namespace EPG_XDTVTransform
{
    public class Utils
    {
        public static string GetTcmConfigValue(string sKey)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                Logger.Logger.Log("EPG_XDTVTransform.Utils", "Key=" + sKey + "," + ex.Message, "Tcm");
            }
            return result;
        }

        public static string GetSingleNodeValue(XmlNode node, string xpath)
        {
            string res = "";
            try
            {
                if (node.SelectSingleNode(xpath) != null)
                {
                    res = node.SelectSingleNode(xpath).InnerText;
                }
            }
            catch (Exception exp)
            {
                string errorMessage = string.Format("could not get the node '{0}' innerText value, error:{1}", xpath, exp.Message);

            }
            return res;
        }

        public static string Compress(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        // Load the xml string to XmlDocument
        public static XmlDocument stringToXMLDoc(string xmlString)
        {
            XmlDocument xmlDoc = new XmlDocument();
            Encoding encoding = Encoding.UTF8;

            // Encode the XML string in a byte array
            byte[] encodedString = encoding.GetBytes(xmlString);

            // Put the byte array into a stream and rewind it to the beginning
            using (var ms = new MemoryStream(encodedString))
            {
                ms.Flush();
                ms.Position = 0;

                // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                xmlDoc.Load(ms);
            }

            return xmlDoc;
        }
    }
}
