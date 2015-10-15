using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.IO.Compression;
using KLogMonitor;
using System.Reflection;

namespace EPG_XDTVTransform
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                log.Error("EPG_XDTVTransform.Utils - Key=" + sKey + "," + ex.Message, ex);
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

        public static string Decompress(string s)
        {
            var bytes = Convert.FromBase64String(s);

            using (var msi = new MemoryStream(bytes))
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                using (var mso = new MemoryStream())
                {
                    gs.CopyTo(mso);

                    bytes = mso.ToArray();
                }

                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }

        }

        public static string convertUTF16toUTF8(string input)
        {
            string result = string.Empty;
            try
            {
                // Convert the string into a byte array. 
                var bytes = Encoding.Unicode.GetBytes(input);

                // Perform the conversion from one encoding to the other. 
                byte[] ut8fBytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, bytes);

                // Convert the new byte[] into a char[] and then into a string. 
                char[] utf8Chars = new char[Encoding.UTF8.GetCharCount(ut8fBytes, 0, ut8fBytes.Length)];
                Encoding.UTF8.GetChars(ut8fBytes, 0, ut8fBytes.Length, utf8Chars, 0);
                result = new string(utf8Chars);

                //the string is already converted to UTF-*, now just removing the xml-declaration
                if (result.Contains("encoding=\"utf-16\""))
                {
                    result = result.Replace("encoding=\"utf-16\"", "");
                }
            }
            catch (Exception exc)
            {

            }
            return result;
        }
    }
}
