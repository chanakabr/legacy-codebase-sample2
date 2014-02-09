using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Data;
using System.Web.UI.WebControls;
using System.Collections;

namespace Tvinci.Web.Controls.Gallery.Part
{  
    public abstract class ContentPartItem : Control, INamingContainer
    {
        public object ContentItem { get; private set; }
        #region Constructor
        public ContentPartItem(object contentItem, ContentPartMetadata itemMetadata)
        {
            ContentItem = contentItem;
            Metadata = itemMetadata;
        }

        #endregion

        #region Properties
        public ContentPartMetadata Metadata { get; set; }

        #endregion
    }

    public class ContentPartItem<TItem> : ContentPartItem
    {
        public ContentPartItem(object contentItem, ContentPartMetadata itemMetadata)
            : base(contentItem, itemMetadata)
        { }

        public virtual TItem Item
        {
            get
            {
                return (TItem)base.ContentItem;
            }
        }


    }
}
