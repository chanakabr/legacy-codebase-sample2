using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml.Serialization;


/// <summary>
/// Summary description for XmlSerializerWrapper
/// </summary>
/// 
namespace Tvinci.Helpers.Objects
{


    /// <summary>
    /// Serialize and deserialize xml and JSON
    /// </summary>
    public static class SerializerWrapper
    {

        public static TXmlObject GetObjectFromXml<TXmlObject>(string xmlStringPath)
            where TXmlObject : class,new()
        {
           
            if(!IsFileExist(xmlStringPath)){
                throw new Exception("Worng file path");
            }
            TXmlObject o = null;

            XmlSerializer x = new XmlSerializer(new TXmlObject().GetType());
            using (StreamReader reader = new StreamReader(xmlStringPath))
            {
                o = (TXmlObject)x.Deserialize(reader);
            }

            return o != null ? o : new TXmlObject();
        }

        private static bool IsFileExist(string path)
        {
            bool result = true;

            try
            {
                using (File.Open(path, FileMode.Open)) { }

            }
            catch { result = false; }

            return result;
        }
        

    }

}