using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Web;
using KLogMonitor;
using System.Reflection;
using TVinciShared;

namespace Tvinci.Configuration
{
    internal static class ConfigurationHelper
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        /// <summary>
        /// Creates configuration from xml file. If not exists or error occured default value will be returned
        /// </summary>
        /// <typeparam name="TConfiguration"></typeparam>
        /// <param name="filePath"></param>
        /// <returns>Deserialized instance of configuration from xml file. Null will be returned on error</returns>
        public static TConfiguration ExtractFromFile<TConfiguration>(string filePath, bool isEncrypted) where TConfiguration : class
        {
            // create absolute file path and store for later use
            if (!Path.IsPathRooted(filePath))
            {
                filePath = HttpContext.Current.ServerMapPath(filePath);
            }

            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(TConfiguration));

                    using (StringReader sr = new StringReader(File.ReadAllText(filePath)))
                    {
                        if (isEncrypted)
                        {
                            string decrypted = string.Empty;

                            try
                            {
                                decrypted = Tvinci.Helpers.EncryptionHelper.DecryptValue(sr.ReadToEnd());
                            }
                            catch (Exception ex)
                            {
                                logger.Error(string.Format("Failed to decrypt file content (Did you remember to encrypt the information?). File '{0}'", filePath), ex);
                                return null;
                            }


                            using (StringReader sr2 = new StringReader(decrypted))
                            {
                                return xs.Deserialize(sr2) as TConfiguration;
                            }
                        }
                        else
                        {
                            return xs.Deserialize(sr) as TConfiguration;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("Failed to deserialize configuration from file '{0}'", filePath), ex);
                    return null;
                }
            }
            else
            {
                logger.ErrorFormat("Configuration file not found '{0}'", filePath);
                return null;
            }
        }
    }
}
