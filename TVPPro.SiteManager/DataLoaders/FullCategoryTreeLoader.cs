using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.CategoriesTree;
using Tvinci.Data.TVMDataLoader.Protocols.MH_CategoriesTree;
using System.Configuration;
using TVPPro.SiteManager.Helper;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class FullCategoryTreeLoader : Tvinci.Data.TVMDataLoader.TVMAdapter<dsCategory>
    {

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

        public string TVMUser
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TVMUser", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TVMUser", value);

            }
        }

        public string TVMPass
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TVMPass", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TVMPass", value);

            }
        }

        public override dsCategory Execute()
        {
            bool shouldUseNewCache;
            if (bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["ShouldUseNewCache"], out shouldUseNewCache) && shouldUseNewCache)
            {
                CatalogLoaders.CategoryLoader categoryLoader = new CatalogLoaders.CategoryLoader(TVMUser, SiteHelper.GetClientIP(), CategoryId);
                return categoryLoader.Execute() as dsCategory;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{B76F4BE8-0474-4113-8556-F6EAB2367B79}"); }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            MH_CategoriesTree result = new MH_CategoriesTree();

            result.root.flashvars.category_id = CategoryId;
            result.root.flashvars.with_channels = true;
            result.root.flashvars.player_un = TVMUser;
            result.root.flashvars.player_pass = TVMPass;
            if (!string.IsNullOrEmpty(PicSize))
            {
                result.root.flashvars.pic_size1 = PicSize;
            }

            result.root.flashvars.no_cache = "1";

            return result;
        }

        public FullCategoryTreeLoader(string user, string pass, int CatID)
        {
            TVMUser = user;
            TVMPass = pass;
            CategoryId = CatID;
        }

        protected override dsCategory PreCacheHandling(object retrievedData)
        {
            MH_CategoriesTree data = retrievedData as MH_CategoriesTree;

            if (data == null)
            {
                throw new Exception("");
            }

            dsCategory retVal = new dsCategory();
            makeCategory(data.response.category, null, retVal);

            return retVal;
        }

        private void makeCategory(categoryType cat, dsCategory.CategoriesRow parent, dsCategory dsCat)
        {
            if (cat == null)
                return;

            dsCategory.CategoriesRow currentRow = dsCat.Categories.NewCategoriesRow();
            currentRow.ID = cat.id.ToString();
            currentRow.Title = cat.title;
            currentRow.PicURL = cat.pic_size1;
            //If we are root, there is no parent
            currentRow.ParentCatID = parent != null ? parent.ID.ToString() : null;
            dsCat.Categories.AddCategoriesRow(currentRow);

            foreach (channelType rootChannel in cat.channelCollection)
            {
                dsCategory.ChannelsRow currentChannelRow = dsCat.Channels.NewChannelsRow();
                currentChannelRow.CategoryID = cat.id.ToString();
                currentChannelRow.ID = rootChannel.id;
                currentChannelRow.Title = rootChannel.title;
                currentChannelRow.PicURL = rootChannel.pic_size1;
                dsCat.Channels.AddChannelsRow(currentChannelRow);
            }

            foreach (categoryType innerCat in cat.categoryCollection)
            {
                makeCategory(innerCat, currentRow, dsCat);
            }
        }
    }
}
