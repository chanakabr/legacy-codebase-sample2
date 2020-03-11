using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using System.Xml.Serialization;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.Pics;
using TVPPro.Configuration.Technical;
using System.Runtime.Serialization;
using System.Configuration;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;
using ConfigurationManager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary()
        {

        }

        protected SerializableDictionary(SerializationInfo information, StreamingContext context)
            : base(information, context)
        {
        } 


        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
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



                writer.WriteStartElement("key");

                keySerializer.Serialize(writer, key);

                writer.WriteEndElement();



                writer.WriteStartElement("value");

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();



                writer.WriteEndElement();

            }

        }

        #endregion
    }

    [Serializable]
    public class PicLoader : TVMAdapter<SerializableDictionary<string, string>>
    {
        private PicturesLoader m_oPicturesLoader;
        

        private string m_tvmUser;
        private string m_tvmPass;

		#region Properties
		public string PicsIDStr
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "PicsIDStr", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "PicsIDStr", value);
			}
		}

		public string[] PicsIDArr
		{
			get
			{
				return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "PicsIDList", null);
			}
			set
			{
				Parameters.SetParameter<string[]>(eParameterType.Retrieve, "PicsIDList", value);
			}
		}

		public string PictureSize
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "PictureSize", null);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "PictureSize", value);
			}
		}

		public bool ContainBranding
		{
			get
			{
				return Parameters.GetParameter<bool>(eParameterType.Retrieve, "ContainBranding", false);
			}
			set
			{
				Parameters.SetParameter<bool>(eParameterType.Retrieve, "ContainBranding", value);
			}
		} 
		#endregion

        public PicLoader(string[] picIDs, string picSize)
        {
            this.PicsIDArr = picIDs;
            PictureSize = picSize;
        }

        public PicLoader(string[] picIDs, string picSize, string tvmUser, string tvmPass)
        {
            this.PicsIDArr = picIDs;
            PictureSize = picSize;
            m_tvmUser = tvmUser;
            m_tvmPass = tvmPass;
        }


        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override SerializableDictionary<string, string> Execute()
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                List<int> picIDs = PicsIDArr.Select(id => int.Parse(id)).ToList();
                m_oPicturesLoader = new PicturesLoader(picIDs, m_tvmUser, SiteHelper.GetClientIP(), PictureSize)
                {
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                };
                return m_oPicturesLoader.Execute() as SerializableDictionary<string, string>;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            Pics protocol = new Pics();

            foreach (string idStr in PicsIDArr)
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    protocol.root.request.picCollection.Add(new pic() { id = idStr });
                }
            }
            protocol.root.flashvars.no_cache = "0";

            if (string.IsNullOrEmpty(m_tvmPass))
            {
                protocol.root.flashvars.player_pass = TechnicalConfiguration.Instance.Data.TVM.Configuration.Password;
            }
            else
            {
                protocol.root.flashvars.player_pass = m_tvmPass;
            }
            if (string.IsNullOrEmpty(m_tvmUser))
            {
                protocol.root.flashvars.player_un = TechnicalConfiguration.Instance.Data.TVM.Configuration.User;
            }
            else
            {
                protocol.root.flashvars.player_un = m_tvmUser;
            }
            protocol.root.flashvars.pic_size1 = PictureSize;

            if (ContainBranding)//for picture on brandig page in order to recive pic_size2 also
            {
                protocol.root.flashvars.pic_size2 = "full";
            }

            return protocol;
        }

        protected override SerializableDictionary<string, string> PreCacheHandling(object retrievedData)
        {
            Pics protocol = retrievedData as Pics;
            string PicUrl = string.Empty;

            if (protocol == null)
            {
                throw new Exception("Returned object is not a Pics protocol");
            }

            SerializableDictionary<string, string> picsList = new SerializableDictionary<string, string>();

            for (int i = 0; i < protocol.response.picCollection.Count; i++)
            {
                responsepic retPic = protocol.response.picCollection[i];
                 
                if (!picsList.ContainsKey(retPic.id))
                {
                    if (!string.IsNullOrEmpty(retPic.pic_size2)) // means the response contain branding !
                    {
                        PicUrl = string.Format("{0};{1}", retPic.pic_size1, retPic.pic_size2); // return the two pics with separator ';'
                    }
                    else
                    {
                        PicUrl = retPic.pic_size1;
                    }

                    picsList.Add(retPic.id, PicUrl);
                }
            }
           
            return picsList;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{68904B13-1050-4a18-B86E-A099D21157A1}"); }
        }
    }
}
