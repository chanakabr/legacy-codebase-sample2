using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols;
using TagSearch = Tvinci.Data.TVMDataLoader.Protocols.TagSearch;
using Tvinci.Data.TVMDataLoader.Protocols.TagSearch;
using TVPPro.SiteManager.DataEntities;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMTagLoader : TVMAdapter<dsTags.ItemsDataTable>
    {

        private string m_tvmUser;
        private string m_tvmPass;

        public string[] TagName
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "TagName", null);
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "TagName", value);
            }
        }

        public string OveriddenLanguage
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

        public TVMTagLoader(string[] tagName) : this(tagName, string.Empty, string.Empty)
        {
        }

        public TVMTagLoader(string[] tagName, string user, string pass)
        {
            m_tvmPass = pass;
            m_tvmUser = user;
            TagName = tagName;
        }

        protected override IProtocol CreateProtocol()
        {
            TagSearch.TagSearchProtocol protocol = new TagSearch.TagSearchProtocol();

            protocol.root.flashvars.player_un = m_tvmUser;
            protocol.root.flashvars.player_pass = m_tvmPass;

			foreach(string key in TagName)
			{
				protocol.root.request.tags.Add(new TagSearch.tag_type() { name = key });
			}

            if (!string.IsNullOrEmpty(OveriddenLanguage))
            {
                protocol.OverridenLanguage = OveriddenLanguage;
            }

            return protocol;
        }

		protected override dsTags.ItemsDataTable PreCacheHandling(object retrievedData)
        {
			dsTags.ItemsDataTable result = new dsTags.ItemsDataTable();
            TagSearch.TagSearchProtocol data = (TagSearch.TagSearchProtocol)retrievedData;

            if (data != null)
            {
                if (data.response != null && data.response.tag_typeCollection.Count > 0)
                {
					List<string> tempValues = new List<string>();
                    foreach (responsetag_type tagCollection in data.response.tag_typeCollection)
                    {
                        foreach (TagSearch.tag tag in tagCollection)
                        {
                            if (!tempValues.Contains(tag.name))
                            {
                                tempValues.Add(tag.name);

                                dsTags.ItemsRow row = result.NewItemsRow();
                                row.Name = tag.name;
                                result.AddItemsRow(row);
                            }
                        }
                    }                                        
                }
            }

            return result;
        }

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{FC775BA1-8883-4196-8952-722B225A541E}"); }
		}

        //#region IBrowseByLoader Members

        //public string Letter
        //{
        //    get
        //    {
        //        return Parameters.GetParameter<string>(eParameterType.Retrieve, "Letter", "");
        //    }
        //    set
        //    {
        //        Parameters.SetParameter<string>(eParameterType.Retrieve, "Letter", value);
        //    }
        //}

        //#endregion
    }
}
