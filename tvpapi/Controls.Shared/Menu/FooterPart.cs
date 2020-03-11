//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.ComponentModel;
//using Tvinci.Data.DataLoader;
//using System.IO;
//using Tvinci.Web.Controls.ContainerControl;
//using Tvinci.Web.Controls.Gallery;
//using Tvinci.Web.Controls.Gallery.Part;

//namespace Tvinci.Web.TVS.Controls.Menu
//{
//    public class FooterPart : ContentPart<FooterCategory>
//    {
//        [TemplateContainer(typeof(ContentPartItem<FooterCategory>))]
//        public override ITemplate Template { get; set; }

//        public override void OnItemAdded(ContentPartItem container, ContentPartMetadata itemMetadata)
//        {            
//            FooterCategory item = (FooterCategory)container.ContentItem;

//            ItemsTemplate itemsTemplate = (ItemsTemplate)container.FindControl(ItemsTemplate.Name);

//            if (itemsTemplate != null)
//            {
//                foreach (FooterCategory.Item footerItem in item.Items)
//                {
//                    Control itemControl = new ContentPartItem<FooterCategory.Item>(footerItem, itemMetadata);
//                    itemsTemplate.HandleItem(itemControl);
//                }
//            }

//            base.OnItemAdded(container, itemMetadata);
//        }
     
                        
//    }

    
//}
