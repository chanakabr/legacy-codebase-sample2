using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Web.Controls.ContainerControl;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class ClientPagingNavigation : XHtmlContainer
    {        
        public override void DataBind()
        {            
            base.DataBind();
            
        }

        protected override void OnInit(EventArgs e)
        {
            registerPart();
            base.OnInit(e);
        }

        private void registerPart()
        {
            Control tempParent = this.Parent;

            while (tempParent != null)
            {
                ClientPagingPart gallery = tempParent as ClientPagingPart;

                if (gallery != null)
                {
                    gallery.Navigation = this;                                        
                    return;
                }
                else
                {
                    tempParent = tempParent.Parent;
                }
            }

            throw new Exception(string.Format("Control '{0}' of type 'GalleryPanel' must be a child of control which implements 'IGallery' (Did you used a wrong gallery panel or used it outside of gallery control?)", this.ID));

        }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(ClientPagingNavigationItem))]
        public ITemplate PageButtonTemplate { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        public PlaceHolder PrevContainer { get; set; }
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public PlaceHolder NextContainer { get; set; }
    }
}
