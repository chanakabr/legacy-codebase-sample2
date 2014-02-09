using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader.PredefinedAdapters;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class MediasContentTypeLoader : CustomAdapter<dsMediaTypes>
    {
        #region Empty Constractor
        public MediasContentTypeLoader()
        {

        }
        #endregion Empty Constractor

        protected override dsMediaTypes CreateSourceResult()
        {
            dsMediaTypes dsTypes = new dsMediaTypes();

            new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
            {
                query += "select mct.TvmTypeID, mct.Name 'TypeName', mct.Source 'Source', mct.TemplateName 'TemplateName', mct.TVMAccountID, mct.OrderNumber, psbt.PicSize 'PictureSize', psbt.IsPoster, pt.URL 'PageFilename' ";
                query += "from lu_MediasContentType mct inner join lu_PicSizeByType psbt on (mct.TvmTypeID = psbt.TvmTypeID) left join lu_page_types pt on (mct.PageID = pt.ID)";
                query += " where Active = 'true' order by mct.OrderNumber";
                //query += DatabaseHelper.AddCommonFields("spgl.status", "", eExecuteLocation.Application, true);
            }, dsTypes.MediaTypes).Execute();

            return dsTypes;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{C5F3817C-E783-4cd7-B643-F651243AD37B}"); }
        }
    }
}
