using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Tvinci.Web.Controls.Gallery.Part;
using System.Data;

namespace Tvinci.Web.Controls.ContainerControl
{
    public class GenericTemplate : TemplatedContainer
    {
        [TemplateContainer(typeof(ContentPartItem<object>))]
        public override ITemplate Template { get; set; }
    }

	public class DataRowTemplate : TemplatedContainer
	{
		[TemplateContainer(typeof(ContentPartItem<DataRow>))]
		public override ITemplate Template { get; set; }
	}

	public class DataRowViewTemplate : TemplatedContainer
	{				
		[TemplateContainer(typeof(ContentPartItem<DataRowView>))]
		public override ITemplate Template { get; set; }
	}

    public abstract class TemplatedContainer : XHtmlContainer
    {
        [PersistenceMode(PersistenceMode.InnerProperty)]        
        public abstract ITemplate Template { get; set; }

        public void HandleItem(Control control)
        {            
            Template.InstantiateIn(control);
            this.Controls.Add(control);            
        }
    }
}
