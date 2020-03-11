using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using System.Data;
using TVPPro.SiteManager.DataEntities;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class ProfileLoader : DatabaseAdapter<DataTable, dsProfiles.ProfileDataTable>
    {
        protected override void InitializeQuery(TVPApi.ODBCWrapper.DataSetSelectQuery query)
        {
            query += "SELECT ID, DESCRIPTION as [Desc], MENU_COLOR as MenuColor ";
            query += "FROM lu_pages_profiles_types ";
            query += "WHERE STATUS = 1";
        }

        protected override dsProfiles.ProfileDataTable FormatResults(DataTable originalObject)
        {
            dsProfiles.ProfileDataTable res = new dsProfiles.ProfileDataTable();

            if (originalObject == null)
                return null;

            res.Merge(originalObject);

            return res;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{7B409BC3-16EB-4299-AEB1-9205E69BA5FC}"); }
        }
    }
}
