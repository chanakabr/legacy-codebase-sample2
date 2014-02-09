using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Web.Controls.ContainerControl;
using System.Web.UI;
using Tvinci.Web.Controls.Gallery.Part;
//using Tvinci.Web.TVS.Controls.Menu;

namespace Tvinci.Web.TVS.Controls.Menu
{
    public class DataItemsTemplate : TemplatedContainer
    {
		[TemplateContainer(typeof(ContentPartItem<dsMenu.ItemRow>))]
		public override ITemplate Template { get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		public ITemplate Pre { get; set; }
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public ITemplate Post { get; set; }

        public const string Name = "DataItems";

        public DataItemsTemplate()
        {
            base.ID = Name;
        }

        protected override void OnInit(EventArgs e)
        {
            base.ID = Name;
            base.OnInit(e);
        }

        
    }    
}
