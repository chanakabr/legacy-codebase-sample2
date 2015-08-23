using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using WebAPI.Models.General;

[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>, IXmlSerializable
{
    #region IXmlSerializable Members
    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(System.Xml.XmlReader reader)
    {
        //TODO: implement
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

        bool wasEmpty = reader.IsEmptyElement;
        reader.Read();

        if (wasEmpty)
            return;

        while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
        {
            reader.ReadStartElement("item");

            reader.ReadStartElement("key");
            TKey key = (TKey)keySerializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("value");
            TValue value = (TValue)valueSerializer.Deserialize(reader);
            reader.ReadEndElement();

            this.Add(key, value);

            reader.ReadEndElement();
            reader.MoveToContent();
        }
        reader.ReadEndElement();
    }

    public void WriteXml(System.Xml.XmlWriter writer)
    {
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

        foreach (TKey key in this.Keys)
        {            
            writer.WriteStartElement("item");

            writer.WriteStartElement("itemKey");
            writer.WriteString(key.ToString());
            writer.WriteEndElement();

            //XXX: ugly hack we need to solve sometime
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TValue));

            using (StringWriter textWriter = new StringWriter())
            {                
                xmlSerializer.Serialize(textWriter, this[key]);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(textWriter.ToString());

                writer.WriteRaw(doc.GetElementsByTagName(typeof(TValue).Name)[0].InnerXml);
            }
            
            writer.WriteEndElement();
        }
    }
    #endregion
}