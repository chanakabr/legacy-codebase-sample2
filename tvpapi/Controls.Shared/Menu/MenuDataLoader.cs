using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Data.DataLoader;
using Tvinci.Helpers;

namespace Tvinci.Web.TVS.Controls.Menu
{
	[Serializable]
    public class MenuDataLoader : CustomAdapter<dsMenu.CategoryDataTable>
	{
        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{D1EAACB9-DCDF-4e7e-8419-339EA66AB5B9}"); }
        }

		public enum eMenuType
		{
			None = 0,
			Footer =1,
			Menu = 2
		}

		public enum eMenuDBTable
		{ 
			UnKnown,
			Footer,
			Menu
		}

        public eMenuDBTable TableName
        {
            get
            {
                return Parameters.GetParameter<eMenuDBTable>(eParameterType.Retrieve, "TableName", eMenuDBTable.UnKnown);
            }
            set
            {
                Parameters.SetParameter<eMenuDBTable>(eParameterType.Retrieve, "TableName", value);
            }
        }

        public eMenuType m_MenuType
        {
            get
            {
                return Parameters.GetParameter<eMenuType>(eParameterType.Retrieve, "m_MenuType", eMenuType.None);
            }
            set
            {
                Parameters.SetParameter<eMenuType>(eParameterType.Retrieve, "m_MenuType", value);
            }
        }

        public object ValueInDB
        {
            get
            {
                return Parameters.GetParameter<object>(eParameterType.Retrieve, "LanguageID", 0);
            }
            private set
            {
                Parameters.SetParameter<object>(eParameterType.Retrieve, "LanguageID", value);
            }
        }			
					
        public MenuDataLoader(eMenuType menuType, object valueInDB)
		{
            ValueInDB = valueInDB;
			m_MenuType = menuType;

			//To support the Db table seperetion in the orange DB
			if (TableName == eMenuDBTable.UnKnown)
				TableName = eMenuDBTable.Menu;

		}

        protected override dsMenu.CategoryDataTable CreateSourceResult()
		{
			dsMenu result = GetData();

			return result.Category;
		}

		private dsMenu GetData()
		{
			dsMenu result = new dsMenu();

			DatabaseDirectAdapter.Execute(new DatabaseDirectAdapter(GetMenuCategory, result.Category));
			DatabaseDirectAdapter.Execute(new DatabaseDirectAdapter(GetCategoryItems, result.Item));

			return result;
		}

		void GetMenuCategory(TVPApi.ODBCWrapper.DataSetSelectQuery query)
		{
			query += "select fc.ID 'ID', fc.Name 'Name', fc.SpecialStyle 'SpecialStyle', fc.link 'Link' from ";

			if (TableName == eMenuDBTable.Menu)
			{
				query += "MenuCategory fc where ";
			}
			else if (TableName == eMenuDBTable.Footer)
			{
				query += "FooterCategory fc where ";
			}

			query += DatabaseHelper.AddCommonFields("fc.Status", "fc.Is_Active", eExecuteLocation.Application, true);

			if (m_MenuType != eMenuType.None)
			{
				query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("fc.type", "=", (int) Enum.Parse(typeof(eMenuType), m_MenuType.ToString()));
				query += " and ";
			}
			query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("fc.languageID", "=", ValueInDB);
			query += " order by fc.itemorder";
		}

		void GetCategoryItems(TVPApi.ODBCWrapper.DataSetSelectQuery query)
		{
			if (TableName == eMenuDBTable.Menu)
				query += "select fci.MenuCategoryID 'CategoryID', fci.Name 'Name', fci.Link 'Link' from MenuCategoryItem fci join MenuCategory fc on fc.id = fci.MenuCategoryID and ";
			else if (TableName == eMenuDBTable.Footer)
				query += "select fci.FooterCategoryID 'CategoryID', fci.Name 'Name', fci.Link 'Link' from FooterCategoryItem fci join FooterCategory fc on fc.id = fci.FooterCategoryID and ";

			query += DatabaseHelper.AddCommonFields("fci.Status", "fci.Is_Active", eExecuteLocation.Application, true);
			query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("fc.LanguageID", "=", ValueInDB);
			query += " order by fci.itemorder";
		}

	}
}
