using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Helpers;
using Tvinci.Data.DataLoader;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
	public class GalleryLinksLoader : CustomAdapter<dsGalleryLinks>
    {
        #region properties
        public long GalleryID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "GalleryID", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "GalleryID", value);
            }
        }
        #endregion properties

        #region empty constractor
        public GalleryLinksLoader()
        {

        }
        #endregion constractor

        protected override dsGalleryLinks CreateSourceResult()
        {
            dsGalleryLinks dsGalleryLinks = new dsGalleryLinks();

            new DatabaseDirectAdapter(delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
                {
                    query += "select spgl.GalleryID , spgl.Name, spgl.Link, spgl.ItemOrder, spg.LinksTitle,spg.ButtonText, spg.ButtonLink ";
                    query += "from SitePageGallery spg inner join SitePageGalleryLink spgl on spgl.GalleryID = spg.ID";
                    query += " where spgl.GalleryID =" + GalleryID + " and spgl.status = 1";
                    //query += DatabaseHelper.AddCommonFields("spgl.status", "", eExecuteLocation.Application, true);
                    //query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("spgl.GalleryID", GalleryID);
                }, dsGalleryLinks.GallryLinks).Execute();

            return dsGalleryLinks;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{8A73D794-3DEE-4930-B545-AB0B5FBC1FA4}"); }
        }
    }
}
