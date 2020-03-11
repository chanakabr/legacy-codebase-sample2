using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class DataPartContainer : PlaceHolder
    {
        public string ReleventForTabs { get; set; }
    }

    [ParseChildren(true)]
    [PersistChildren(false)]
    public class MultipleDataPartContainer : Control
    {
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public PlaceHolder NoActiveTabContent {get;set;}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<DataPartContainer> Parts { get; set; }

        public MultipleDataPartContainer()
        {
            Parts = new List<DataPartContainer>();
            NoActiveTabContent = null;
        }

        internal void SyncActiveTab(string tabIdentifier)
        {
            this.Visible = true;
            this.Controls.Clear();

            tabIdentifier = string.Format(";{0};", tabIdentifier);
            DataPartContainer part = Parts.Find(p => string.Format(";{0};",p.ReleventForTabs).Contains(tabIdentifier));

            if (part != null)
            {
                this.Controls.Add(part);
            }
            else
            {
                if (NoActiveTabContent != null)
                {
                    this.Controls.Add(NoActiveTabContent);
                }
                else
                {
                    this.Visible = false;
                }
            }
        }
    }
}
