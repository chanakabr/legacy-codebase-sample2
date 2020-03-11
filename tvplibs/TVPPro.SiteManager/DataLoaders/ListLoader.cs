using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.DataEntities;
using System.Web;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.Helper;
using Tvinci.Data.TVMDataLoader;

namespace TVPPro.SiteManager.DataLoaders 
{
    [Serializable]
    public class ListLoader : CustomAdapter<DataView>
    {
        private string m_tvmUser;
        private string m_tvmPass;

        #region Load properties
        public int PageIndex
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "PageIndex", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "PageIndex", value);
            }
        }

        public int PageSize
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "PageSize", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "PageSize", value);
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

        public Enums.eOrderBy OrderBy
        {
            get
            {
                return Parameters.GetParameter<Enums.eOrderBy>(eParameterType.Retrieve, "OrderBy", Enums.eOrderBy.Added);
            }
            set
            {
                Parameters.SetParameter<Enums.eOrderBy>(eParameterType.Retrieve, "OrderBy", value);
            }
        }

        public string FilterBySign
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Filter, "FilterBySign", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Filter, "FilterBySign", value);
            }
        }

        public bool FilterByStartsWith
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Filter, "FilterByStartsWith", true);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Filter, "FilterByStartsWith", value);
            }
        }

        public TVPPro.SiteManager.Context.Enums.MediaListType ListType
        {
            get
            {
                return Parameters.GetParameter<TVPPro.SiteManager.Context.Enums.MediaListType>(eParameterType.Retrieve, "MediaList", TVPPro.SiteManager.Context.Enums.MediaListType.MediaList);
            }
            set
            {
                Parameters.SetParameter<TVPPro.SiteManager.Context.Enums.MediaListType>(eParameterType.Retrieve, "MediaList", value);
            }
        }

        public string TagPair
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TagPair", String.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TagPair", value);
            }
        }

        public bool WithInfo
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "WithInfo", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "WithInfo", value);
            }
        }
        #endregion

        public ListLoader()
        {

        }

        public ListLoader(string user, string pass)
        {
            m_tvmUser = user;
            m_tvmPass = pass;
        }

        protected override DataView CreateSourceResult()
        {
            DataView dvResult = null;
            //If the list type is a tag list - use the Tag Loader
            if (ListType == Enums.MediaListType.TagList)
            {
                dsTags.ItemsDataTable result = DoTagSearch();
                dvResult = result.DefaultView;
            }
            //Otherwise - use the SearchMediaLoader
            else
            {
                dsItemInfo results = DoSearch();
                dvResult = results.Item.DefaultView;
            }
			return dvResult;
        }

		protected override DataView FormatResults(DataView originalObject)
		{
			originalObject.RowFilter = "";

			if (!string.IsNullOrEmpty(FilterBySign))
			{
                string nameCol = "Title";
                if (ListType == Enums.MediaListType.TagList)
                {
                    nameCol = "Name";
                }
				switch (FilterBySign)
				{
					case "All":
						break;
					case "#":
						originalObject.RowFilter = string.Format("{0} like '0%' or {0} like '1%' or {0} like '2%' or {0} like '3%' or {0} like '4%' or {0} like '5%' or {0} like '6%' or {0} like '7%' or {0} like '8%' or {0} like '9%'", nameCol);
						break;
					default:
                        originalObject.RowFilter = string.Format(FilterByStartsWith ? "{0} like '{1}*'" : "{0} like '*{1}*'", nameCol, FilterBySign.Replace("'", "''"));
						break;
				}
			}

			return originalObject;
		}


        //ArikG : Add the ability to filter tag searches as well
        private TVPPro.SiteManager.DataEntities.dsTags.ItemsDataTable DoTagSearch()
        {
            TVPPro.SiteManager.DataEntities.dsTags.ItemsDataTable idtRet = new dsTags.ItemsDataTable();
            string[] tagArr = new string[] { TagPair };
            TVMAdapter<dsTags.ItemsDataTable> TagLoader = new TVMTagLoader(tagArr, m_tvmUser, m_tvmPass) { TagName = tagArr };
            idtRet = TagLoader.Execute();

            return idtRet;
        }

        private dsItemInfo DoSearch()
        {
            TVMAdapter<dsItemInfo> MediaLoader = null;
            dsItemInfo MediaResult = null;
            //TVM Media search by list type
            if (ListType == Enums.MediaListType.MediaList)
            {
                MediaLoader = new SearchMediaLoader(m_tvmUser, m_tvmPass) { PageIndex = PageIndex, PageSize = PageSize, PictureSize = PicSize, OrderBy = OrderBy, WithInfo = this.WithInfo };
            }
            else if (ListType == Enums.MediaListType.TagPairList)
            {
                Dictionary<string, string> dictTag = DataHelper.GetDictionaryFromTagPairs(TagPair);
                MediaLoader = new SearchMediaLoader(dictTag) { SearchTokenSignature = TagPair, PageIndex = PageIndex, PageSize = PageSize, PictureSize = PicSize, OrderBy = OrderBy, WithInfo = this.WithInfo };
            }

            MediaResult = MediaLoader.Execute();
			return MediaResult;
        }
 
        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{E8794AE4-6D83-4484-B022-9082FF92B5B4}"); }
        }
    }
}
