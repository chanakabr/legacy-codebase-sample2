using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.DataLoader;

namespace TVPApi
{
    public class APIMenuLoader : MenuLoader
    {

        public int GroupID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "GroupID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "GroupID", value);
            }
        }

        public PlatformType Platform
        {
            get
            {
                return Parameters.GetParameter<PlatformType>(eParameterType.Retrieve, "Platform", PlatformType.Unknown);
            }
            set
            {
                Parameters.SetParameter<PlatformType>(eParameterType.Retrieve, "Platform", value);
            }
        }

        protected override void InitializeQuery(ODBCWrapper.DataSetSelectQuery query)
        {
            ConnectionManager mngr = new ConnectionManager(GroupID, Platform, false);
            query.SetConnectionString(mngr.GetClientConnectionString());
            base.InitializeQuery(query);

            //query += "select mm.ID, mm.ParentID, mm.URL, mm.DefaultItem, mm.SitePageID, lg.Culture, mmd.Title from MainMenu mm left join MainMenuMetaData mmd on mm.ID=mmd.MainMenuID left join lu_Languages lg on mmd.LanguageID=lg.ID where ";
            //query += DatabaseHelper.AddCommonFields("mm.Status", "mm.Is_active", eExecuteLocation.Application, false);
            //query += "order by mm.ItemOrder";
        }

        

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{231EE097-7D5A-48f4-9115-E4CEF5FECA41}"); }
        }
    }
}
