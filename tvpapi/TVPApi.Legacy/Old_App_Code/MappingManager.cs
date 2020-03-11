//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.IO;
//using System.Xml;
//using System.Xml.Serialization;

//namespace MappingsConfiguration
//{
//    /// <summary>
//    /// Summary description for Mappings
//    /// </summary>
//    public class MappingManager
//    {
//        private string devName;
//        public MappingManager(string devName)
//        {
//            this.devName = devName;
//        }

//        public string GetValue(string key)
//        {
//            Mappings mapping = new Mappings();

//            string mappingValues = File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/DeviceMappings/" + devName + ".xml");

//            var map =  ((Mappings)FromXml(mappingValues, typeof(Mappings)));

//            for (int i = 0; i < map.KeyCollection.Capacity; i++)
//            {
//                if (map.KeyCollection[i].name == key)
//                    return map.KeyCollection[i].value;
//            }

//            return null;
//        }

//        public object FromXml(string Xml, System.Type ObjType)
//        {
//            XmlSerializer ser;
//            ser = new XmlSerializer(ObjType);
//            StringReader stringReader;
//            stringReader = new StringReader(Xml);
//            XmlTextReader xmlReader;
//            xmlReader = new XmlTextReader(stringReader);
//            object obj;
//            obj = ser.Deserialize(xmlReader);
//            xmlReader.Close();
//            stringReader.Close();
//            return obj;
//        }
//    }
//}
