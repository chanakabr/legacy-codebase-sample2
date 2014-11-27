using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using log4net;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataEntities;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class CategoryLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(CategoryLoader));

        public int CategoryID { get; set; }

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
            dsCategory.CategoriesRow rootRow = retVal.Categories.NewCategoriesRow();
            rootRow.ID = categoryResponse.ID.ToString();
            rootRow.Title = "Root";
            foreach (channelObj rootChannel in categoryResponse.m_oChannels)
            {
                dsCategory.ChannelsRow rootChannelRow = retVal.Channels.NewChannelsRow();
                rootChannelRow.CategoryID = categoryResponse.ID.ToString();
                rootChannelRow.ID = rootChannel.m_nChannelID;
                rootChannelRow.Title = rootChannel.m_sTitle;
                retVal.Channels.AddChannelsRow(rootChannelRow);
            }
            retVal.Categories.AddCategoriesRow(rootRow);
            if (categoryResponse.m_oChildCategories != null)
            {
                foreach (CategoryResponse cat in categoryResponse.m_oChildCategories)
                {
                    dsCategory.CategoriesRow catRow = retVal.Categories.NewCategoriesRow();
                    catRow.ID = cat.ID.ToString();
                    catRow.Title = cat.m_sTitle;
                    catRow.ParentCatID = cat.m_nParentCategoryID.ToString();
                    if (cat.m_oChannels != null)
                    {
                        foreach (channelObj catChannel in cat.m_oChannels)
                        {
                            dsCategory.ChannelsRow channelRow = retVal.Channels.NewChannelsRow();
                            channelRow.CategoryID = cat.ID.ToString();
                            channelRow.ID = catChannel.m_nChannelID;
                            channelRow.Title = catChannel.m_sTitle;
                            retVal.Channels.AddChannelsRow(channelRow);
                            //channelRow.NumOfItems = catChannel.media_count
                        }
                    }
                    retVal.Categories.AddCategoriesRow(catRow);
                }
            }

            return retVal;
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
                    sText.AppendFormat("CategoryResponse: ID = {0}, ParentCategoryID = {1}, CoGuid = {2}, Title = {3}", res.ID,  res.m_nParentCategoryID, res.m_sCoGuid, res.m_sTitle);
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
