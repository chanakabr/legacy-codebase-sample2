using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using TVPPro.SiteManager.DataEntities;
using System.Data;
using Tvinci.Helpers;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class SideGalleriesLoader : CustomAdapter<dsSideGalleries>
    {
        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{8D879C7E-CF9C-4178-8BB9-5DF61C8DD5F8}"); }
        }

        protected override dsSideGalleries CreateSourceResult()
        {
            dsSideGalleries res = new dsSideGalleries();

            new DatabaseDirectAdapter(
                delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
                {
					query += "select sd.ID,sd.Name,sd.GalleryTypeID, sdm.Title, sdm.TVMChannel, sdm.Header, sdm.Text, sdm.Link, sdm.OpenNewWindow, pc.BASE_URL 'PictureURL', lg.Culture, sd.ShownOnAllPages ";
					query += "from SideGalleries sd left join SideGalleriesMetadata sdm on sd.ID=sdm.SideGalleryID left join lu_languages lg on sdm.LanguageID=lg.ID left join pics pc on sdm.PictureID=pc.ID where";
					query += DatabaseHelper.AddCommonFields("sd.status", "sdm.Is_active", eExecuteLocation.Application, false);
					query += " order by sd.ItemOrder";
                }, res.SideGalleries).Execute();

            new DatabaseDirectAdapter(
                delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
                {
					query += "select spl.SideGalleryID, spl.PageID from SideGalleriesPageLink spl, SideGalleries sd, SideGalleriesMetadata sdm where ";
					query += "spl.SideGalleryID = sd.ID and spl.SideGalleryID = sdm.SideGalleryID and";
					query += DatabaseHelper.AddCommonFields("sd.status", "sdm.Is_active", eExecuteLocation.Application, true);
                    query += DatabaseHelper.AddCommonFields("spl.status", "", eExecuteLocation.Application, false);
                }, res.SideGalleriesPageLink).Execute();

            return res;
        }
    }
}
