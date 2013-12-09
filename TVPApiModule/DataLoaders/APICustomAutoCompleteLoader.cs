using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using TVPApi;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.DataEntities;

namespace TVPApi
{
    [Serializable]
    public class APICustomAutoCompleteLoader : CustomAutoCompleteLoader
    {        
        public APICustomAutoCompleteLoader() : base()
        {
        }

        public APICustomAutoCompleteLoader(string tvmUser, string tvmPass)
        {
            TvmUser = tvmUser;
            TvmPass = tvmPass;
        }

        private string TvmUser
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
        private string TvmPass
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

        protected override void PreExecute()
        {
            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();
        }

        protected override List<String> CreateSourceResult()
        {
            List<String> lstResult = CreateList();

            return lstResult;
        }

        protected override List<String> FormatResults(List<String> originalObject)
        {
            List<String> lstReturn = (from item in originalObject orderby item select item).ToList();

            return lstReturn;
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

        private List<String> CreateList()
        {
            APIAutoCompleteLoader loaderAutoComplete = new APIAutoCompleteLoader(TvmUser, TvmPass) { MediaType = MediaType, MetaNames = MetaNames, Platform = this.Platform, GroupID = this.GroupID };
            List<String> lstResponse = new List<String>(loaderAutoComplete.Execute());

            TVMTagLoader tagsLoader = new TVMTagLoader(TagNames);
            dsTags.ItemsDataTable dtTagItem = tagsLoader.Execute();

            lstResponse.AddRange(dtTagItem.Select(r => r["Name"].ToString()).ToArray());

            lstResponse.Remove("");
            lstResponse = lstResponse.Distinct().ToList();

            return lstResponse;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{C5469C5F-A9FA-424d-9B81-B1C827A44BFC}"); }
        }
    }
}
