using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.CommentsList;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.CatalogLoaders;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;
using ConfigurationManager;

namespace TVPPro.SiteManager.DataLoaders
{
	#region MediaComments
	public class MediaComments
	{
		public List<CommentContext> commentsList = new List<CommentContext>();
		public dsComments commentsDS = new dsComments();
	}

	public class CommentContext
	{
		public string CommentType { get; set; }
		public string Date { get; set; }
		public string Writer { get; set; }
		public string Header { get; set; }
		public string Sub_Header { get; set; }
		public string Content { get; set; }

		public string MediaID { get; set; }
		public bool AutoActive { get; set; }

        public string UserPicURL { get; set; }

        public CommentContext()
        {
        }

        public CommentContext(string theType, string theDate, string theWriter, string theHeader, string theSubHeader, string theContent, string theMediaId, string userPicURL)
		{
			CommentType = theType;
			Date = theDate;
			Writer = theWriter;
			Header = theHeader;
			Sub_Header = theSubHeader;
			Content = theContent;
            MediaID = theMediaId;
            UserPicURL = userPicURL;
		}

		public CommentContext(bool theAutoActive, string theMediaID, string theWriter, string theHeader, string theSubHeader, string theContent)
		{
			AutoActive = theAutoActive;
			MediaID = theMediaID;
			Writer = theWriter;
			Header = theHeader;
			Sub_Header = theSubHeader;
			Content = theContent;
		}
	}
	#endregion

	[Serializable]
	public class TVMCommentsLoader : TVMAdapter<MediaComments>
	{
        private MediaCommentsListLoader m_oCommentsListLoader;
        

		//Comments type : All, All except users, type name
		const string AllTypes = "All";
		const string AllExceptUsers = "All except users";
        private string m_tvmUser;
        private string m_tvmPass;

		public enum eCommentsType
		{
			All,
			AllExceptUsers,
			ByType
		}

        public enum OrderComments
        {
            None,
            CommentNumber
        }

		public eCommentsType CommentsType
		{
			get
			{
				return Parameters.GetParameter<eCommentsType>(eParameterType.Retrieve, "CommentsType", eCommentsType.All);
			}
			set
			{
				Parameters.SetParameter<eCommentsType>(eParameterType.Retrieve, "CommentsType", value);
			}
		}

		public string CommentsTypeName
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "CommentsTypeName", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "CommentsTypeName", value);
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

        public OrderComments OrderBy
        {
            get
            {
                return Parameters.GetParameter<OrderComments>(eParameterType.Retrieve, "OrderBy", OrderComments.None);
            }
            set
            {
                Parameters.SetParameter<OrderComments>(eParameterType.Retrieve, "OrderBy", value);
            }
        }

        public TVMCommentsLoader(string theMediaID)
        {            
            MediaID = theMediaID;
        }

		public TVMCommentsLoader(string m_tvmUser, string m_tvmPass, string theMediaID)
		{
            this.m_tvmUser = m_tvmUser;
            this.m_tvmPass = m_tvmPass;
			MediaID = theMediaID;
		}

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override MediaComments Execute()
        {
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oCommentsListLoader = new MediaCommentsListLoader(int.Parse(MediaID), m_tvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex)
                {
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    OrderBy = (TVPPro.SiteManager.CatalogLoaders.MediaCommentsListLoader.eOrderComments)OrderBy
                };
                return m_oCommentsListLoader.Execute() as MediaComments;
            }
            else
            {
                return base.Execute();
            }
        }

        public override bool TryGetItemsCount(out long count)
        {
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return m_oCommentsListLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }


		public override eCacheMode GetCacheMode()
		{
			return eCacheMode.Never;
		}

		protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
		{
			CommentsList protocol = new CommentsList();

			protocol.root.flashvars.no_cache = "1";
			protocol.root.request.media.id = MediaID;
            protocol.root.flashvars.player_un = m_tvmUser;
            protocol.root.flashvars.player_pass = m_tvmPass;
			
			if (!string.IsNullOrEmpty(CommentsTypeName))
				protocol.root.request.comments.type = CommentsTypeName;
			else
				protocol.root.request.comments.type = CommentsType == eCommentsType.AllExceptUsers ? AllExceptUsers : AllTypes;

			return protocol;
		}

		protected override MediaComments PreCacheHandling(object retrievedData)
		{
			CommentsList protocol = retrievedData as CommentsList;

			if (protocol == null)
			{
				throw new Exception("Returned object is not a CommentsList protocol");
			}

			MediaComments mediaComments = new MediaComments();
			List<string> commentsTypesList = new List<string>();

			for (int i = 0; i < protocol.response.commentCollection.Count; i++)
			{
				// Create the comment context for each comment returned
				CommentContext context = new CommentContext(
					protocol.response.commentCollection[i].type,
					protocol.response.commentCollection[i].date,
					protocol.response.commentCollection[i].writer,
					protocol.response.commentCollection[i].header,
					protocol.response.commentCollection[i].sub_header,
					protocol.response.commentCollection[i].content,
                    protocol.response.commentCollection[i].media.id,
                    string.Empty);

				mediaComments.commentsList.Add(context);

				//Add every comment to the comments dataset.
				dsComments.CommentsRow itemRow = mediaComments.commentsDS.Comments.NewCommentsRow();

				itemRow.Type = protocol.response.commentCollection[i].type;
				itemRow.Date = protocol.response.commentCollection[i].date;
				itemRow.Writer = protocol.response.commentCollection[i].writer;
				itemRow.Header = protocol.response.commentCollection[i].header;
				itemRow.SubHeader = protocol.response.commentCollection[i].sub_header;
				itemRow.Content = protocol.response.commentCollection[i].content;
                itemRow.MediaId = protocol.response.commentCollection[i].media.id;

				mediaComments.commentsDS.Comments.AddCommentsRow(itemRow);

				if(!commentsTypesList.Contains(itemRow.Type))
				{
					commentsTypesList.Add(itemRow.Type);

					dsComments.CommentTypesRow commentTypeRow = mediaComments.commentsDS.CommentTypes.NewCommentTypesRow();
					commentTypeRow.Type = itemRow.Type;
					mediaComments.commentsDS.CommentTypes.AddCommentTypesRow(commentTypeRow);
				}
			}

            switch (OrderBy)
            {
                case OrderComments.CommentNumber:
                    mediaComments.commentsList = mediaComments.commentsList.OrderBy(commentItem => commentItem.Date).ToList();
                    break;
                case OrderComments.None:
                default:
                    mediaComments.commentsList.Reverse();
                    break;
            }            

			return mediaComments;
		}

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{B958BAD1-4D82-489e-BDA2-B8D741C80A8B}"); }
		}
	}
}

