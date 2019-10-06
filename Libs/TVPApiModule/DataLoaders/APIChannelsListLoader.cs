using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsList;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Context;
using System.Data;
using TVPApiModule.CatalogLoaders;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;
using ConfigurationManager;

namespace TVPApi
{
    public class APIChannelsListLoader : TVPPro.SiteManager.DataLoaders.ChannelsListLoader
    {
        
        private APIChannelsListsLoader m_oCatalogChannelsListsLoader;


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

        public string Language
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "Language", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "Language", value);
            }
        }

        public APIChannelsListLoader(int groupID, string tvmUN, string tvmPass, string picSize)
            : base(groupID, tvmUN, tvmPass, picSize)
        {
            // Do nothing.
        }

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oCatalogChannelsListsLoader = new APIChannelsListsLoader(0, SiteMapManager.GetInstance.GetPageData(GroupID, Platform).GetTVMAccountByUser(TvmUser).BaseGroupID, GroupID, Platform.ToString(), SiteHelper.GetClientIP(), PageSize, PageIndex, PicSize)
                {
                    Culture = Language,
                    SiteGuid = SiteGuid
                };
                return m_oCatalogChannelsListsLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }

        public override bool TryGetItemsCount(out long count)
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return m_oCatalogChannelsListsLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }
        protected override bool TryGetItemsCountInSource(object retrievedData, out long count)
        {
            count = 0;

            if (retrievedData == null)
                return false;

            ChannelsList result = retrievedData as ChannelsList;

            if (result.response.category.channelCollection.Count == 0)
            {
                count = 0;
                return true;
            }

            count = result.response.category.channelCollection.Count;

            return true;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{F1163BC6-5B81-4457-BAA0-919F9AD56CF1}"); }
        }
    }
}
