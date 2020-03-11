using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Web.Controls.Gallery.Part;
using System.Web.UI;
using Tvinci.Web.TVS.Controls.Menu;

namespace Tvinci.Web.TVS.Controls.Menu
{
    public class MenuDataPart : DataContentPart<dsMenu.CategoryRow>
    {
        [TemplateContainer(typeof(ContentPartItem<dsMenu.CategoryRow>))]
        public override ITemplate Template { get; set; }

		public string InnerPartContainerID { get; set; }

        public override void OnItemAdded(ContentPartItem container, ContentPartMetadata itemMetadata)
        {
            dsMenu.CategoryRow item = (dsMenu.CategoryRow)container.ContentItem;

            DataItemsTemplate itemsTemplate = (DataItemsTemplate)container.FindControl(DataItemsTemplate.Name);

            if (itemsTemplate != null)
            {
				Tvinci.Web.TVS.Controls.Menu.dsMenu.ItemRow[] items = item.GetItemRows();
				if (items != null && items.Length != 0)
				{
					Control control;

					if (itemsTemplate.Pre != null)
					{
						control = new Control();
						itemsTemplate.Pre.InstantiateIn(control);
						itemsTemplate.Controls.Add(control);
					}

					foreach (dsMenu.ItemRow itemRow in items)
					{
						Control itemControl = new ContentPartItem<dsMenu.ItemRow>(itemRow, itemMetadata);
						itemsTemplate.HandleItem(itemControl);
					}

					if (itemsTemplate.Post != null)
					{
						control = new Control();
						itemsTemplate.Post.InstantiateIn(control);
						itemsTemplate.Controls.Add(control);
					}
				}
				else if(!string.IsNullOrEmpty(InnerPartContainerID))
				{
					if(container.FindControl(InnerPartContainerID) != null)
						container.FindControl(InnerPartContainerID).Visible = false;
				}
                
            }

            base.OnItemAdded(container, itemMetadata);
        }
    }       
}
