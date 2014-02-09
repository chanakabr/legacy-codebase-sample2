using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using TVPPro.Configuration.PlatformServices;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.Helper;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMRentalMultiMediaLoader : CustomAdapter<dsItemInfo>
    {
        private string m_tvmUser;
        private string m_tvmPass;

        #region Load properties
		
		/// <summary>
		/// A unique token which gets Medias Ids stirng : mediaID|MediaID"
		/// </summary>
		public string SearchTokenSignature
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "SearchTokenSignature", null);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "SearchTokenSignature", value);
			}
		}

        public PermittedMediaContainer[] MediasIdCotainer
        {
            get
            {
                return Parameters.GetParameter<PermittedMediaContainer[]>(eParameterType.Retrieve, "MediasIdCotainer", null);
            }
            set
            {
                Parameters.SetParameter<PermittedMediaContainer[]>(eParameterType.Retrieve, "MediasIdCotainer", value);
            }
        }

		public bool IsPosterPic
		{
			get
			{
				return Parameters.GetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", false);
			}
			set
			{
				Parameters.SetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", value);
			}
		}

        public string PicSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "PicSize", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "PicSize", value);
            }
        }

        public int MediaType
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "MediaType", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "MediaType", value);
            }
        }
        public int PageSize
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Filter, "PageSize", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Filter, "PageSize", value);

            }
        }
        public int PageIndex
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Filter, "PageIndex", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Filter, "PageIndex", value);
            }
        }

        #endregion properties

        #region Constractor
        public TVMRentalMultiMediaLoader(string PictureSize, int MediaTypeId)
        {
            PicSize = PictureSize;
            MediaType = MediaTypeId;
            if (string.IsNullOrEmpty(PicSize))
            {
                throw new Exception("PicSize must be supplied");
            }
        }

        public TVMRentalMultiMediaLoader(string tvmUn, string tvmPass, string PictureSize, int MediaTypeId)
        {
            PicSize = PictureSize;
            MediaType = MediaTypeId;
            if (string.IsNullOrEmpty(PicSize))
            {
                throw new Exception("PicSize must be supplied");
            }

            m_tvmUser = tvmUn;
            m_tvmPass = tvmPass;
        }
        #endregion Constractor

        protected override dsItemInfo CreateSourceResult()
        {
            int index = 0;

            dsItemInfo result = new dsItemInfo();

            if (MediasIdCotainer != null)
            {
                string[] MediaArrayID = new string[MediasIdCotainer.Count()];
                foreach (PermittedMediaContainer MediaObj in MediasIdCotainer)
                {
                    MediaArrayID[index] = MediaObj.m_nMediaID.ToString();
                    index++;
                }

                dsItemInfo MediasFromLoader = new dsItemInfo();

                if (!string.IsNullOrEmpty(m_tvmUser) && !string.IsNullOrEmpty(m_tvmPass))
                {
                    //Get the media information from the current tvmAccount.
                    MediasFromLoader = new TVMMultiMediaLoader(m_tvmUser, m_tvmPass, MediaArrayID, PicSize, MediaType) { IsPosterPic = IsPosterPic  }.Execute();
                }
                else
                {
                    //Get the media information for all the medias.
                    MediasFromLoader = new TVMMultiMediaLoader(MediaArrayID, PicSize, MediaType) { IsPosterPic = IsPosterPic }.Execute();
                }

                foreach (dsItemInfo.ItemRow loaderRow in MediasFromLoader.Item.Rows)
                {
                    // Get the permited item from the array by mediaId
                    var ItemPermited = (from m in MediasIdCotainer where m.m_nMediaID ==int.Parse(loaderRow.ID) select m).First();

                    dsItemInfo.ItemRow rowAllInfo = result.Item.NewItemRow();
                    rowAllInfo.ID = loaderRow.ID;
                    rowAllInfo.ImageLink = loaderRow.ImageLink;
                    rowAllInfo.Title = loaderRow.Title;
                    rowAllInfo.DescriptionShort = loaderRow.DescriptionShort;
                    rowAllInfo.MediaTypeID = loaderRow.MediaTypeID;
                    //add the information from the permited array to the media info row
                    rowAllInfo.PurchaseDate = ItemPermited.m_dPurchaseDate;
                    rowAllInfo.EndPurchaseDate = ItemPermited.m_dEndDate;
                    rowAllInfo.CurrentDate = ItemPermited.m_dCurrentDate;
                    rowAllInfo.FileID = loaderRow.FileID != "0" ? loaderRow.FileID : ItemPermited.m_nMediaFileID.ToString();
                    rowAllInfo.Rate = loaderRow.Rate;
                    rowAllInfo.LastWatchedDeviceName = loaderRow.LastWatchedDeviceName;
                    rowAllInfo.Likes = loaderRow.Likes;
                    result.Item.AddItemRow(rowAllInfo);
                }
            }

            return result;
        }

        public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Never;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{90F93B47-2C9A-4dcf-B9CB-0F82990D2BB6}"); }
        }
    }
}
