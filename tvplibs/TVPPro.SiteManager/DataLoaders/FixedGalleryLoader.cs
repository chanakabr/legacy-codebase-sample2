using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Helpers;
using TVPPro.SiteManager.Context;
using Tvinci.Data.DataLoader;
using System.Data;

namespace TVPPro.SiteManager.DataLoaders
{
	[Serializable]
	public class FixedGalleryLoader : DatabaseAdapter<DataTable, dsSideGalleries.SideGalleriesDataTable>
    {
		public int GalleryID
		{
			get
			{
				return Parameters.GetParameter<int>(eParameterType.Retrieve, "GalleryID", 0);
			}
			set
			{
				Parameters.SetParameter<int>(eParameterType.Retrieve, "GalleryID", value);
			}
		}

		public FixedGalleryLoader(int galleryID)
		{
			GalleryID = galleryID;
		}

		protected override void InitializeQuery(ODBCWrapper.DataSetSelectQuery query)
		{
			//query += "select SG.ID, SGM.Header, SGM.Title,SGM.[Text], SGM.Link, PC.BASE_URL 'PictureURL'" +
			//            "from dbo.SideGalleries SG inner join dbo.SideGalleriesMetadata SGM on(SG.ID = SGM.SideGalleryID)" +
			//            "left join pics PC on SGM.PictureID=PC.ID where SG.[NAME] = 'FreeGallery' and  ";
			//query += DatabaseHelper.AddCommonFields("SGM.status", "SGM.Is_active", eExecuteLocation.Application, false);

			query += "select fg.id, fg.name, fg.title, fg.header, fg.text, fg.linktext, fg.link, fg.OpenNewWindow, p.base_url 'PictureURL'";
			query += "from FixedGalleries fg left join pics p on fg.PictureID = p.id ";
			query += "where ";
			query += ODBCWrapper.Parameter.NEW_PARAM("fg.id", "=", GalleryID);
			query += "and ";
			query += DatabaseHelper.AddCommonFields("fg.status", "fg.Is_active", eExecuteLocation.Application, false);
		}

		protected override dsSideGalleries.SideGalleriesDataTable FormatResults(DataTable originalObject)
		{
			dsSideGalleries.SideGalleriesDataTable result = new dsSideGalleries.SideGalleriesDataTable();

			foreach (DataRow dr in originalObject.Rows)
			{
				dsSideGalleries.SideGalleriesRow row = result.NewSideGalleriesRow();

				row.ID = long.Parse(dr["ID"].ToString());
				row.Name = dr["Name"].ToString();
				row.Title = dr["Title"].ToString();
				row.Header = dr["Header"].ToString();
				row.Text = dr["Text"].ToString();
				row.Link = dr["Link"].ToString();
				row.LinkText = dr["LinkText"].ToString();
				row.OpenNewWindow = bool.Parse(dr["openNewWindow"].ToString());

				if (dr["PictureURL"] != null)
					row.PictureURL = dr["PictureURL"].ToString();

				result.AddSideGalleriesRow(row);
			}

			return result;
		}

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{A1EB828E-7F6F-4cf3-8DB3-2A5491CA4735}"); }
        }
    }
}
