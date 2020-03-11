using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Core.Catalog.Request;
using Core.Catalog.Response;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataEntities;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class CategoryLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int CategoryID { get; set; }
        public string PicSize { get; set; }

        public CategoryLoader(int groupID, string userIP, int categoryID)
            : base(groupID, userIP, 0, 0)
        {
            CategoryID = categoryID;
        }

        public CategoryLoader(string userName, string userIP, int categoryId)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, categoryId)
        {
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new CategoryRequest()
            {
                m_nCategoryID = CategoryID
            };
        }

        public object Execute()
        {
            dsCategory retVal = null;
            CategoryResponse categoryResponse = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                categoryResponse = m_oResponse as CategoryResponse;
                if (categoryResponse != null)
                    retVal = CategoryResponseToDsCategory(categoryResponse);
            }
            return retVal;
        }

        private dsCategory CategoryResponseToDsCategory(CategoryResponse categoryResponse)
        {
            dsCategory retVal = new dsCategory();
            MakeCategory(categoryResponse, null, retVal, PicSize);
            return retVal;
        }

        private void MakeCategory(CategoryResponse cat, dsCategory.CategoriesRow parent, dsCategory dsCat, string picSize)
        {
            if (cat == null)
                return;

            dsCategory.CategoriesRow currentRow = dsCat.Categories.NewCategoriesRow();
            currentRow.ID = cat.ID.ToString();
            currentRow.Title = cat.m_sTitle;
            var pic = cat.m_lPics != null ? cat.m_lPics.Where(p => p.m_sSize == picSize).FirstOrDefault() : null;
            currentRow.PicURL = pic != null ? pic.m_sURL : string.Empty;
            //If we are root, there is no parent
            currentRow.ParentCatID = parent != null ? parent.ID.ToString() : null;
            dsCat.Categories.AddCategoriesRow(currentRow);

            foreach (var rootChannel in cat.m_oChannels)
            {
                dsCategory.ChannelsRow currentChannelRow = dsCat.Channels.NewChannelsRow();
                currentChannelRow.CategoryID = cat.ID.ToString();
                currentChannelRow.ID = rootChannel.m_nChannelID;
                currentChannelRow.Title = rootChannel.m_sTitle;
                var channelPic = rootChannel.m_lPic != null ? rootChannel.m_lPic.Where(p => p.m_sSize == picSize).FirstOrDefault() : null;
                currentChannelRow.PicURL = channelPic != null ? pic.m_sURL : string.Empty;
                dsCat.Channels.AddChannelsRow(currentChannelRow);
            }

            foreach (var innerCat in cat.m_oChildCategories)
            {
                MakeCategory(innerCat, currentRow, dsCat, picSize);
            }
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                if (obj is CategoryRequest)
                    sText.AppendFormat("AssetStatsRequest: groupID = {0}, userIP = {1}, CategoryID = {2}", GroupID, m_sUserIP, CategoryID);
                else if (obj is CategoryResponse)
                {
                    CategoryResponse res = obj as CategoryResponse;
                    sText.AppendFormat("CategoryResponse: ID = {0}, ParentCategoryID = {1}, CoGuid = {2}, Title = {3}", res.ID, res.m_nParentCategoryID, res.m_sCoGuid, res.m_sTitle);
                }
            }
            logger.Debug(sText.ToString());
        }

        #region ISupportPaging method
        public bool TryGetItemsCount(out long count)
        {
            count = 0;

            if (m_oResponse == null)
                return false;

            count = m_oResponse.m_nTotalItems;

            return true;
        }
        #endregion

        #region ILoaderAdapter not implemented methods
        public bool IsPersist()
        {
            throw new NotImplementedException();
        }

        public object Execute(eExecuteBehaivor behaivor)
        {
            throw new NotImplementedException();
        }

        public object LastExecuteResult
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

    }
}
