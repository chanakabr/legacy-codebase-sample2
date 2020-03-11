//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Tvinci.Web.Controls.ContainerControl;
//using System.Web.UI;
//using Tvinci.Web.Controls.Gallery.Part;

//namespace Tvinci.Web.TVS.Controls.Menu
//{
//    public class ItemsTemplate : TemplatedContainer
//    {
//        public const string Name = "Items";

//        public ItemsTemplate()
//        {
//            base.ID = Name;           
//        }

//        protected override void OnInit(EventArgs e)
//        {
//            base.ID = Name;
//            base.OnInit(e);
//        }

//        [TemplateContainer(typeof(ContentPartItem<FooterCategory.Item>))]
//        public override ITemplate Template { get; set; }        
//    }
//}
