using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.FlashSingleMedia;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.Context;
using System.Configuration;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;
using Phx.Lib.Appconfig;

namespace TVPPro.SiteManager.DataLoaders
{
	[Serializable]
	public class TVMFlashMediaLoader : TVMAdapter<XmlDocument>
	{
		private string m_tvmUser;
		private string m_tvmPass;
		private FlashLoadersParams m_FlashLoadersParams;

		#region Loader properties
        
        public string SiteGuid
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SiteGuid", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SiteGuid", value);
            }
        }

        public string MediaID
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "MediaID", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "MediaID", value);
			}
		}

		public string FlashParamsSTR
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "FlashParamsSTR", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "FlashParamsSTR", value);
			}
		}

		public string UseFinalEndDate
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "UseFinalEndDate", "true");
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "UseFinalEndDate", value);
			}
		}

        public string UserIP
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "UserIP", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "UserIP", value);
            }
        }
		#endregion

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override XmlDocument Execute()
        {
            
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return new FlashMediaLoader(int.Parse(MediaID), m_tvmUser, SiteHelper.GetClientIP(), m_FlashLoadersParams.Pic2Size)
                {
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    UseFinalDate = bool.Parse(UseFinalEndDate),
                    SiteGuid = SiteGuid
                }.Execute() as XmlDocument;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override bool ShouldStoreInCache(LoaderAdapterItem result)
        {
            return (result != null);
        }

		public TVMFlashMediaLoader(string tvmUn, string tvmPass, string mediaID, FlashLoadersParams FlashParams)
		{
			this.MediaID = mediaID;
			this.m_tvmPass = tvmPass;
			this.m_tvmUser = tvmUn;
			this.m_FlashLoadersParams = FlashParams;
		}

		protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
		{
			FlashSingleMedia result = new FlashSingleMedia();

			result.root.request.mediaCollection.Add(new media() { id = MediaID });

			result.root.flashvars.player_un = m_tvmUser;
			result.root.flashvars.player_pass = m_tvmPass;

			result.root.flashvars.pic_size1 = m_FlashLoadersParams.Pic1Size;
			result.root.flashvars.pic_size2 = m_FlashLoadersParams.Pic2Size;
			result.root.flashvars.pic_size3 = m_FlashLoadersParams.Pic3Size;

			if (m_FlashLoadersParams.IsPic1Poster)
			{
				result.root.flashvars.pic_size1_format = "POSTER";
				result.root.flashvars.pic_size1_quality = "HIGH";
			}

			if (m_FlashLoadersParams.IsPic2Poster)
			{
				result.root.flashvars.pic_size2_format = "POSTER";
				result.root.flashvars.pic_size2_quality = "HIGH";
			}

			
            result.root.flashvars.client_IP = (string.IsNullOrEmpty(UserIP))? SiteManager.Helper.SiteHelper.GetClientIP() : UserIP;
            
			result.root.flashvars.lang = m_FlashLoadersParams.Language;
			result.root.flashvars.file_format = m_FlashLoadersParams.MainFileFormat;
			result.root.flashvars.sub_file_format = m_FlashLoadersParams.SubFileFormat;
			result.root.flashvars.file_quality = file_quality.high;
			result.root.request.@params.with_info = true.ToString();
			result.root.request.@params.info_struct.statistics = true;
			result.root.request.@params.info_struct.type.MakeSchemaCompliant();
			result.root.request.@params.info_struct.name.MakeSchemaCompliant();
			result.root.request.@params.info_struct.description.MakeSchemaCompliant();

			string[] arrMetas = MediaConfiguration.Instance.Data.TVM.FlashMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
			string[] arrTags = MediaConfiguration.Instance.Data.TVM.FlashMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });

			foreach (string metaName in arrMetas)
			{
				result.root.request.@params.info_struct.metaCollection.Add(new meta { name = metaName });
			}

			foreach (string tagName in arrTags)
			{
				result.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
			}

			return result;
		}

		protected override XmlDocument PreCacheHandling(object retrievedData)
		{
			FlashSingleMedia data = retrievedData as FlashSingleMedia;

			if (data == null)
			{
				throw new Exception("");
			}

			XmlDocument result = new XmlDocument();

			if (data.response.mediaCollection.Count == 1)
			{
				responsemedia mediaInfo = data.response.mediaCollection[0];

				if (!string.IsNullOrEmpty(mediaInfo.id))
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
			}

			return result;
		}

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{DBBE4E6F-7B26-47c7-A67B-2D076F5000C7}"); }
		}
	}
}
