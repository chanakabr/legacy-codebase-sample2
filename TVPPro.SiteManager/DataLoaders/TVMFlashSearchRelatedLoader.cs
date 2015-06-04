using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols;
using Tvinci.Data.TVMDataLoader.Protocols.FlashSearchRelated;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.Context;

namespace TVPPro.SiteManager.DataLoaders
{
	[Serializable]
    public class TVMFlashSearchRelatedLoader : TVMAdapter<XmlDocument>
    {
        private string m_tvmUser;
        private string m_tvmPass;
		//private FlashLoadersParams m_FlashLoadersParams;

		#region Loader properties
		public long MediaID
		{
			get
			{
				return Parameters.GetParameter<long>(eParameterType.Retrieve, "MediaID", 0);
			}
			set
			{
				Parameters.SetParameter<long>(eParameterType.Retrieve, "MediaID", value);
			}
		}

		#endregion

		public TVMFlashSearchRelatedLoader(string userName, string pass, long mediaID)
        {
            MediaID = mediaID;
            m_tvmUser = userName;
            m_tvmPass = pass;
        }

        protected override IProtocol CreateProtocol()
        {
			FlashSearchRelated protocol = new FlashSearchRelated();
			
			//protocol.root.request.media.id = MediaID.ToString();

			//protocol.root.request.channel.start_index = "0";
			//protocol.root.request.channel.number_of_items = PageSize.ToString();
			//protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
			//protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();

			//if (IsPosterPic)
			//{
			//    protocol.root.flashvars.pic_size1_format = "POSTER";
			//    protocol.root.flashvars.pic_size1_quality = "HIGH";
			//}


			//protocol.root.flashvars.player_un = m_tvmUser;
			//protocol.root.flashvars.player_pass = m_tvmPass;

			//protocol.root.flashvars.file_format = m_FlashLoadersParams.MainFileFormat;
			//protocol.root.flashvars.sub_file_format = m_FlashLoadersParams.SubFileFormat;
			//protocol.root.flashvars.file_quality = file_quality.high;
			//protocol.root.request.@params.with_info = true.ToString();
			//protocol.root.request.@params.info_struct.statistics = true;
			//protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
			//protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();

			//protocol.root.flashvars.pic_size1 = m_FlashLoadersParams.Pic1Size;
			//protocol.root.flashvars.pic_size2 = m_FlashLoadersParams.Pic2Size;
			//protocol.root.flashvars.pic_size3 = m_FlashLoadersParams.Pic3Size;

			//if (m_FlashLoadersParams.IsPic1Poster)
			//{
			//    protocol.root.flashvars.pic_size1_format = "POSTER";
			//    protocol.root.flashvars.pic_size1_quality = "HIGH";
			//}

			//if (m_FlashLoadersParams.IsPic2Poster)
			//{
			//    protocol.root.flashvars.pic_size2_format = "POSTER";
			//    protocol.root.flashvars.pic_size2_quality = "HIGH";
			//}

			



			string[] arrMetas = MediaConfiguration.Instance.Data.TVM.FlashMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
			string[] arrTags = MediaConfiguration.Instance.Data.TVM.FlashMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });

			foreach (string metaName in arrMetas)
			{
				protocol.root.request.@params.info_struct.metaCollection.Add(new meta { name = metaName });
			}

			foreach (string tagName in arrTags)
			{
				protocol.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
			}

            //if (WithInfo)
            //{
            //    protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Description (Short)" });
            //}

            return protocol;
        }

		protected override XmlDocument PreCacheHandling(object retrievedData)
        {
			FlashSearchRelated data = retrievedData as FlashSearchRelated;

			if (data == null)
			{
				throw new Exception("");
			}

			XmlDocument result = new XmlDocument();

            if (data.response != null && data.response.channel.mediaCollection.Count > 0)
            {
				XmlSerializer xs = new XmlSerializer(data.GetType());

				using (StringWriter sw = new StringWriter())
				{
					XmlDocument xdoc = new XmlDocument();

					xs.Serialize(sw, data);
					xdoc.LoadXml(sw.ToString());

					XmlNode xn = xdoc.SelectSingleNode("FlashSingleMedia/response");

					if (xn != null)
						result.LoadXml(xn.OuterXml);
				}
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{7E01D5C6-2A69-4dd6-8415-A49CE3BB4FB0}"); }
        }
    }
}
