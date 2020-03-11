using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols.Search;
using TVPPro.SiteManager.DataEntities;
using System.Data;

namespace TVPApi
{
    [Serializable]
    public class APIAutoCompleteLoader : AutoCompleteLoader
    {        
        public APIAutoCompleteLoader()
            : base()
        {
        }

        public APIAutoCompleteLoader(string tvmUser, string tvmPass)
        {
            TvmUser = tvmUser;
            TvmPass = tvmPass;
        }

        private string TvmUser
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmUser", "");
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmUser", value);
            }
        }

        private string TvmPass
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmPass", "");
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmPass", value);
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

        public int? MediaType
        {
            get
            {
                return Parameters.GetParameter<int?>(eParameterType.Retrieve, "MediaType", null);
            }
            set
            {
                Parameters.SetParameter<int?>(eParameterType.Retrieve, "MediaType", value);
            }
        }

        protected override void PreExecute()
        {
            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SearchProtocol protocol = new SearchProtocol();

            protocol.root.request.search_data.channel.start_index = "0";
            protocol.root.request.search_data.channel.media_count = ConfigManager.GetInstance().GetConfig(GroupID, Platform).SiteConfiguration.Data.Features.MovieFinder.MaxItems.ToString();
            protocol.root.flashvars.player_un = TvmUser;
            protocol.root.flashvars.player_pass = TvmPass;
            protocol.root.request.@params.with_info = "true";
            if (MediaType.HasValue)
                protocol.root.request.search_data.cut_values.type.value = MediaType.ToString();

            //protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();

            foreach (string key in MetaNames)
            {
                protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = key });
            }

            return protocol;
        }

        protected override List<String> PreCacheHandling(object retrievedData)
        {
            List<String> lstRet = new List<String>();
            List<String> lstMetas = new List<String>();

            SearchProtocol result = (retrievedData as SearchProtocol);

            if (result != null)
            {
                dsItemInfo dsItems = new dsItemInfo();

                foreach (media resMedia in result.response.channel.mediaCollection)
                {
                    lstRet.Add(resMedia.title.ToString());
                    TVPPro.SiteManager.Helper.DataHelper.CollectMetasInfo(ref dsItems, resMedia);
                }

                foreach (DataColumn column in dsItems.Metas.Columns)
                {
                    if (!column.ColumnName.ToLower().Equals("id")) lstRet.AddRange(dsItems.Metas.Select(r => r[column].ToString()).ToArray());
                }

                // remove empty and duplicates
                lstRet.Remove("");
                lstRet = lstRet.Distinct().ToList();
            }

            return lstRet;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{E96B28DD-640C-4dda-A340-CE1DD0043F35}"); }
        }
    }
}
