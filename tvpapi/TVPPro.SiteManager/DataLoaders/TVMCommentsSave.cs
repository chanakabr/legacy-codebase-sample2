using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.CommentsSave;
using TVPPro.SiteManager.Services;
using System.Configuration;
using TVPPro.SiteManager.CatalogLoaders;
using Tvinci.Helpers;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects;
using ApiObjects.Response;
using TVPPro.SiteManager.Helper;
using ApiObjects.Catalog;
using Phx.Lib.Appconfig;

namespace TVPPro.SiteManager.DataLoaders
{
	[Serializable]
	public class TVMCommentsSave : TVMAdapter<bool>
	{
		private string m_tvmUser;
		private string m_tvmPass;

		#region Loader Parameters
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

		public string Writer
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "Writer", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "Writer", value);
			}
		}

		public string Header
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "Header", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "Header", value);
			}
		}

		public string Sub_Header
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "Sub_Header", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "Sub_Header", value);
			}
		}

		public string Content
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "Content", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "Content", value);
			}
		}

		public bool AutoActive
		{
			get
			{
				return Parameters.GetParameter<bool>(eParameterType.Retrieve, "AutoActive", false);
			}
			set
			{
				Parameters.SetParameter<bool>(eParameterType.Retrieve, "AutoActive", value);
			}
		}

		private int m_LanguageID
		{
			get
			{
				return Parameters.GetParameter<int>(eParameterType.Retrieve, "m_LanguageID", 0);
			}
			set
			{
				Parameters.SetParameter<int>(eParameterType.Retrieve, "m_LanguageID", value);
			}
		}

        public string DeviceUDID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "DeviceUDID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "DeviceUDID", value);
            }
        }

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
		#endregion

		public TVMCommentsSave(string TVMUser, string TVMPass, string theMediaID, string theWriter, string theHeader, string theSubHeader, string theContent,
			bool theAutoActive)
		{
			m_tvmUser = TVMUser;
			m_tvmPass = TVMPass;

			MediaID = theMediaID;
			Writer = theWriter;
			Header = theHeader;
			Sub_Header = theSubHeader;
			Content = theContent;
			AutoActive = theAutoActive;            
		}

		public override eCacheMode GetCacheMode()
		{
			return eCacheMode.Never;
		}

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override bool Execute()
        {
            
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                MediaCommentLoader commentLoader = new MediaCommentLoader(
                    m_tvmUser, 
                    SiteHelper.GetClientIP(),
                    m_LanguageID,
                    SiteGuid,
                    DeviceUDID,
                    int.Parse(MediaID),
                    Content,
                    string.Empty,
                    Header,
                    Sub_Header,
                    Writer,
                    AutoActive);
                CommentResponse commentResponse = commentLoader.Execute() as CommentResponse;
                return commentResponse.eStatusComment == StatusComment.SUCCESS;
            }
            else
            {
                return base.Execute();
            }
        }

		protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
		{
			CommentsSave protocol = new CommentsSave();

			protocol.root.request.comment.media.id = MediaID;
			protocol.root.request.comment.header = Header;
			protocol.root.request.comment.content = Content;
			protocol.root.request.comment.writer = Writer;
			protocol.root.request.comment.sub_header = Sub_Header;
			protocol.root.request.comment.auto_active = AutoActive ? "true" : "false";
            protocol.root.flashvars.site_guid = SiteGuid;
            protocol.root.flashvars.device_udid = DeviceUDID;

			protocol.root.flashvars.player_un = m_tvmUser;
			protocol.root.flashvars.player_pass = m_tvmPass;

			return protocol;
		}

		protected override bool PreCacheHandling(object retrievedData)
		{
			CommentsSave protocol = retrievedData as CommentsSave;

			if (protocol == null)
			{
				return false;
			}

			return protocol.response.type == "save_comments";
		}

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{B19A2B63-877F-4dc6-8485-10B16D824819}"); }
		}
	}
}
