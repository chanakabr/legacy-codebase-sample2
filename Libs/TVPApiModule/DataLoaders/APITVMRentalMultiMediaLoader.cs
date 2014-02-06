using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.DataLoader;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.SingleMedia;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace TVPApiModule.DataLoaders
{
    public class APITVMRentalMultiMediaLoader : TVMRentalMultiMediaLoader
    {        
        public int GroupID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "GroupID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "GroupID", value);
            }
        }

        public PlatformType Platform
        {
            get
            {
                return Parameters.GetParameter<PlatformType>(eParameterType.Retrieve, "Platform", PlatformType.Unknown);
            }
            set
            {
                Parameters.SetParameter<PlatformType>(eParameterType.Retrieve, "Platform", value);
            }
        }

        protected string TvmUser
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmUser", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmUser", value);
            }

        }
        protected string TvmPass
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmPass", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmPass", value);
            }

        }

        #region Constractor
        public APITVMRentalMultiMediaLoader(string[] MediaList, string PictureSize, int MediaTypeId) : base (PictureSize, MediaTypeId)
        {
        }

        public APITVMRentalMultiMediaLoader(string tvmUn, string tvmPass, string PictureSize, int MediaTypeId)
            : base(tvmUn, tvmPass, PictureSize, MediaTypeId)
        {
            TvmUser = tvmUn;
            TvmPass = tvmPass;
        }

        
        #endregion Constractor

        protected override void PreExecute()
        {
            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();
        }

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

                if (!string.IsNullOrEmpty(TvmUser) && !string.IsNullOrEmpty(TvmPass))
                {
                    //Get the media information from the current tvmAccount.
                    MediasFromLoader = new APIMultiMediaLoader(TvmUser, TvmPass, MediaArrayID, PicSize, MediaType) { IsPosterPic = IsPosterPic, GroupID = GroupID, Platform = Platform }.Execute();
                }
                else
                {
                    //Get the media information for all the medias.
                    MediasFromLoader = new APIMultiMediaLoader(MediaArrayID, PicSize, MediaType) {GroupID = GroupID, Platform = Platform, IsPosterPic = IsPosterPic }.Execute();
                }

                foreach (dsItemInfo.ItemRow loaderRow in MediasFromLoader.Item.Rows)
                {
                    // Get the permited item from the array by mediaId
                    var ItemPermited = (from m in MediasIdCotainer where m.m_nMediaID == int.Parse(loaderRow.ID) select m).First();

                    dsItemInfo.ItemRow rowAllInfo = result.Item.NewItemRow();
                    rowAllInfo.ID = loaderRow.ID;
                    rowAllInfo.ImageLink = loaderRow.ImageLink;
                    rowAllInfo.Title = loaderRow.Title;
                    rowAllInfo.MediaTypeID = loaderRow.MediaTypeID;
                    //add the information from the permited array to the media info row
                    rowAllInfo.PurchaseDate = ItemPermited.m_dPurchaseDate;
                    rowAllInfo.EndPurchaseDate = ItemPermited.m_dEndDate;
                    rowAllInfo.CurrentDate = ItemPermited.m_dCurrentDate;
                    rowAllInfo.FileID = loaderRow.FileID;
                    result.Item.AddItemRow(rowAllInfo);
                }
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{4742020D-6927-46DA-8916-456CA98159C9}"); }
        }
    }
}
