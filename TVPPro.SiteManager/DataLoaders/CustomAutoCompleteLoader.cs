using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.DataEntities;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class CustomAutoCompleteLoader : CustomAdapter<List<String>>
    {
        public string[] MetaNames
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "MetaNames", null);
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "MetaNames", value);
            }
        }

        public string[] TagNames
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "TagNames", null);
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "TagNames", value);
            }
        }

        public int? MediaTypeID
        {
            get
            {
                return Parameters.GetParameter<int?>(eParameterType.Retrieve, "MediaTypeID", null);
            }
            set
            {
                Parameters.SetParameter<int?>(eParameterType.Retrieve, "MediaTypeID", value);
            }
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

        private List<String> CreateList()
        {
            AutoCompleteLoader loaderAutoComplete = new AutoCompleteLoader() { MediaTypeID = MediaTypeID, MetaNames = MetaNames };
            List<String> lstResponse = new List<String>(loaderAutoComplete.Execute());
            
            TVMTagLoader tagsLoader = new TVMTagLoader( TagNames );
            dsTags.ItemsDataTable dtTagItem = tagsLoader.Execute();

            lstResponse.AddRange(dtTagItem.Select(r => r["Name"].ToString()).ToArray());

            lstResponse.Remove("");
            lstResponse = lstResponse.Distinct().ToList();

            return lstResponse;
        }

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{934C4E3F-D2ED-43bf-9D41-453087DEAE21}"); }
		}
    }
}
