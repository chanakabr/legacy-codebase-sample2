using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.DataLoader;
using System.Data;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class CustomLayoutLoader : CustomAdapter<TVPPro.SiteManager.DataLoaders.CustomLayoutLoader.CustomLayout>
    {
        #region propeties
        public long ItemID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "ItemID", -1);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "ItemID", value);
            }
        }

        public Enums.eCustomLayoutItemType ItemType
        {
            get
            {
                return Parameters.GetParameter<Enums.eCustomLayoutItemType>(eParameterType.Retrieve, "ItemType", Enums.eCustomLayoutItemType.Show);
            }
            set
            {
                Parameters.SetParameter<Enums.eCustomLayoutItemType>(eParameterType.Retrieve, "ItemType", value);
            }
        }
        #endregion properties

        protected override CustomLayout CreateSourceResult()
        {
            DataTable dt = new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
            {
                query += @"select cl.SpaceBetweenHeaderAndPlayer, cl.HTMLPictureRepeat, cl.FlashComponent, cl.IsDarkBackground, pc1.BASE_URL 'HTMLPicture', pc2.BASE_URL 'BodyPicture' 
                    from CustomLayout cl left outer join pics pc1 on cl.HTMLPictureID=pc1.ID left outer join pics pc2 on cl.BodyPictureID=pc2.ID where";
                query += ODBCWrapper.Parameter.NEW_PARAM("CustomLayoutItemTypeID", "=", (int)ItemType);
                query += "and";
                query += ODBCWrapper.Parameter.NEW_PARAM("LanguageID", "=", TextLocalization.Instance.UserContext.ValueInDB);

                if (ItemID > 0)
                {
                    query += "and";
                    query += ODBCWrapper.Parameter.NEW_PARAM("ItemID", "=", ItemID);
                }
            }).Execute(Tvinci.Data.DataLoader.eExecuteBehaivor.ForceRetrieve);

            if (dt == null || dt.Rows.Count != 1)
                return null;

            CustomLayout layout = new CustomLayout();

            if (!dt.Rows[0].IsNull("SpaceBetweenHeaderAndPlayer"))
                layout.SpaceBetweenHeaderAndPlayer = (int)dt.Rows[0]["SpaceBetweenHeaderAndPlayer"];

            if (!dt.Rows[0].IsNull("HTMLPictureRepeat") &&
                Enum.IsDefined(typeof(Enums.eCustomLayoutPictureRepeat), dt.Rows[0]["HTMLPictureRepeat"].ToString()))
            {
                layout.HTMLPictureRepeat =
                    (Enums.eCustomLayoutPictureRepeat)Enum.Parse(typeof(Enums.eCustomLayoutPictureRepeat), dt.Rows[0]["HTMLPictureRepeat"].ToString());
            }

            if (!dt.Rows[0].IsNull("FlashComponent"))
                layout.FlashComponent = dt.Rows[0]["FlashComponent"].ToString();

            if (!dt.Rows[0].IsNull("HTMLPicture"))
                layout.HTMLPicture = dt.Rows[0]["HTMLPicture"].ToString();

            if (!dt.Rows[0].IsNull("BodyPicture"))
                layout.BodyPicture = dt.Rows[0]["BodyPicture"].ToString();

            layout.IsDarkBackground = (bool)dt.Rows[0]["IsDarkBackground"];

            return layout;
        }

        public class CustomLayout
        {
            public int SpaceBetweenHeaderAndPlayer { get; set; }
            public string HTMLPicture { get; set; }
            public TVPPro.SiteManager.Context.Enums.eCustomLayoutPictureRepeat HTMLPictureRepeat { get; set; }
            public string BodyPicture { get; set; }
            public string FlashComponent { get; set; }
            public bool IsDarkBackground { get; set; }
        }

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{75222B3D-F2C0-4bfd-AE4C-18F74ACB9C45}"); }
		}
    }
}
