using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization.Json;
using System.IO;
using KLogMonitor;
using System.Reflection;
namespace Tvinic.GoogleAPI
{

    public static class JSONHelpers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Serialize to JSON string
        /// </summary>
        /// <param name="o">Object to serialize</param>
        /// <returns>JSON string</returns>
        /// <remarks></remarks>
        public static string dataContractToJSON(object o)
        {
            DataContractJsonSerializer _js = new DataContractJsonSerializer(o.GetType());
            string _str = string.Empty;
            using (MemoryStream _ms = new MemoryStream())
            {
                try
                {
                    _js.WriteObject(_ms, o);
                    _ms.Position = 0;
                    _str = new UTF8Encoding(true, true).GetString(_ms.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    _ms.Close();
                }
            }

            return _str;
        }

        /// <summary>
        /// Deserialize to JWTHeaderObject or InAppItemObject
        /// </summary>
        /// <param name="jwtString">Base64url encoded string representing a JWTHeaderObject or InAppItemObject</param>
        /// <param name="o">Object to deserialize to (e.g. JWTHeaderObject or InAppItemObject)</param>
        /// <returns>JWTHeaderObject or InAppItemObject</returns>
        /// <remarks>Escape processing is performed at deserialization</remarks>
        public static object dataContractJSONToObj(string jwtString, object o)
        {
            DataContractJsonSerializer _js = new DataContractJsonSerializer(o.GetType());
            jwtString = JWTHelpers.jwtDecodeB64Url(jwtString);
            using (MemoryStream _ms = new MemoryStream(new UTF8Encoding(true, true).GetBytes(jwtString)))
            {
                try
                {
                    o = _js.ReadObject(_ms);
                }
                catch (Exception ex)
                {
                    log.Error("", ex);

                    o = null;
                }
                finally
                {
                    _ms.Close();
                }

            }

            return o;
        }


    }
}
