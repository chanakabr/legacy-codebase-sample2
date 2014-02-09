using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public abstract class TemplatePart : GalleryPart
    {
        #region Properties
      
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public abstract ITemplate Template { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate SeperatorTemplate { get; set; }
        #endregion        
        
        public void HandleItem(Control item, bool pushToStartOfCollection)
        {
            Template.InstantiateIn(item);
            if (pushToStartOfCollection)
            {
                this.Controls.AddAt(0, (item));
            }
            else
            {
                this.Controls.Add(item);

            }
            

        }
        public virtual void HandleItem(Control item)
        {
            HandleItem(item, false);            
        }

        public virtual void HandleSeperator()
        {
            if (SeperatorTemplate != null)
            {
                Control item = new Control();
                SeperatorTemplate.InstantiateIn(item);
                this.Controls.Add(item);
            }
        }
    }
}
