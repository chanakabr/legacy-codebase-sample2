using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader;
using Tvinci.Helpers;
using TVPApi.ODBCWrapper;
using System.Data;
//using Tvinci.Projects.TVP.Core.Manager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TabularLinksLoader : CustomAdapter<dsTabularLinks>
    {
        private long PageMetaDataID;

		#region Load properties
		public string Token
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "Token", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "Token", value);
            }
        }

        public long? PageID
        {
            get
            {
                return Parameters.GetParameter<long?>(eParameterType.Retrieve, "PageID", null);
            }
            set
            {
                Parameters.SetParameter<long?>(eParameterType.Retrieve, "PageID", value);
            }
        }
		#endregion

        public TabularLinksLoader(string token)
        {
            Token = token;
        }

        public TabularLinksLoader(long pageID)
        {
            PageID = pageID;
        }

        public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Application;
        }

        protected override dsTabularLinks CreateSourceResult()
        {
            if (string.IsNullOrEmpty(Token) && !PageID.HasValue)
            {
                // No token or page meta data id
                return null;
            }

            if (PageID.HasValue)
            {
                PageMetaDataID = PageID.Value;
            }
            
            dsTabularLinks ret = new dsTabularLinks();

            // Load tabular links definitions
            new DatabaseDirectAdapter(GetDefinitions, ret.Definitions).Execute();

            if (ret.Definitions.Count <= 0)
            {
                return ret;
            }

            // Load categories
            // remove - DatabaseDirectAdapter.Execute(new DatabaseDirectAdapter(
            new DatabaseDirectAdapter(
                delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
                {
                    query += "select * from TabularLinksCategory where ";
                    query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("TabularLinksID", "=", ret.Definitions[0].ID);
                    query += "and";
                    query += DatabaseHelper.AddCommonFields("status", "is_active", eExecuteLocation.Application, false);
                    query += "order by itemorder";
                }, ret.Categories).Execute();

            // For each category load it's items
            for (int i = 0; i < ret.Categories.Count; i++)
            {
                // Load items
                new DatabaseDirectAdapter(
                    delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
                    {
                        query += "select * from TabularLinksCategoryItem where ";
                        query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("TabularLinksCategoryID", "=", ret.Categories[i].ID);
                        query += "and";
                        query += DatabaseHelper.AddCommonFields("status", "is_active", eExecuteLocation.Application, false);
                        query += "order by itemorder";
                    }, ret.Items).Execute();
            }

            return ret;
        }

        void GetDefinitions(TVPApi.ODBCWrapper.DataSetSelectQuery query)
        {
            query += "select * from TabularLinks where";

            if (!string.IsNullOrEmpty(Token))
            {
                query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("Token", "=", Token);
            }
            else if (PageID.HasValue)
            {
                //query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("SitePageMetaDataID", "=", PageMetaDataID);
				query += "SitePageMetaDataID is not null";
            }

            query += "and";
            query += DatabaseHelper.AddCommonFields("status", "is_active", eExecuteLocation.Application, false);
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{E4DAA6A9-7FDB-4e46-B844-E38F43A54DC9}"); }
        }
    }
}
