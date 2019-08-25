using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.CategoriesTree;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects;
namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class CategoryTreeLoader : Tvinci.Data.TVMDataLoader.TVMAdapter<dsCategory>
    {

        private string m_tvmUser;
        private string m_tvmPass;

        public int CategoryId
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "CategoryId", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "CategoryId", value);

            }
        }

        public override dsCategory Execute()
        {
            bool shouldUseNewCache;
            if (bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["ShouldUseNewCache"], out shouldUseNewCache) && shouldUseNewCache)
            {
                CatalogLoaders.CategoryLoader categoryLoader = new CatalogLoaders.CategoryLoader(m_tvmUser, SiteHelper.GetClientIP(), CategoryId);
                return categoryLoader.Execute() as dsCategory;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{A15FBF3A-155C-48d8-9CFA-2700F0FB4A09}"); }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            CategoriesTree result = new CategoriesTree();

            result.root.flashvars.category_id = CategoryId;
            result.root.flashvars.with_channels = true;
            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;
            return result;
        }

        public CategoryTreeLoader(string user, string pass, int CatID)
        {
            m_tvmUser = user;
            m_tvmPass = pass;
            CategoryId = CatID;
        }

        protected override dsCategory PreCacheHandling(object retrievedData)
        {
            CategoriesTree data = retrievedData as CategoriesTree;
         
            if (data == null)
            {
                throw new Exception("");
            }

            dsCategory retVal = new dsCategory();
            dsCategory.CategoriesRow rootRow = retVal.Categories.NewCategoriesRow();
            rootRow.ID = CategoryId.ToString();
            rootRow.Title = "Root";
            foreach (Tvinci.Data.TVMDataLoader.Protocols.CategoriesTree.channel rootChannel in data.response.channelCollection)
            {
                dsCategory.ChannelsRow rootChannelRow = retVal.Channels.NewChannelsRow();
                rootChannelRow.CategoryID = CategoryId.ToString();
                rootChannelRow.ID = rootChannel.id;
                rootChannelRow.Title = rootChannel.title;
                retVal.Channels.AddChannelsRow(rootChannelRow);
            }
            retVal.Categories.AddCategoriesRow(rootRow);
            if (data.response != null && data.response.categoryCollection != null)
            {
               
                foreach (Tvinci.Data.TVMDataLoader.Protocols.CategoriesTree.category cat in data.response.categoryCollection)
                {
                    dsCategory.CategoriesRow catRow = retVal.Categories.NewCategoriesRow();
                    catRow.ID = cat.id;
                    catRow.Title = cat.title;
                    if (cat.channelCollection != null)
                    {
                        foreach (categorychannel catChannel in cat.channelCollection)
                        {
                            dsCategory.ChannelsRow channelRow = retVal.Channels.NewChannelsRow();
                            channelRow.CategoryID = cat.id;
                            channelRow.ID = catChannel.id;
                            channelRow.Title = catChannel.title;
                            retVal.Channels.AddChannelsRow(channelRow);
                            //channelRow.NumOfItems = catChannel.media_count
                        }
                    }
                    retVal.Categories.AddCategoriesRow(catRow);
                }
            }

            return retVal;
        }


    }
}
