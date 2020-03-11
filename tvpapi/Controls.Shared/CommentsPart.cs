using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Tvinci.Web.Controls.Gallery;
using Tvinci.Web.Controls.Gallery.Part;
using Tvinci.Web.TVS.Controls.Comment;
using Tvinci.Data.DataLoader;
using System.Data;

namespace Tvinci.Web.TVS.Controls.Comment
{
	public class CommentControl
	{




	}

    public class CommentsPart : DataContentPart<dsComments.CommentsRow>
    {
        [TemplateContainer(typeof(ContentPartItem<dsComments.CommentsRow>))]
        public override ITemplate Template { get; set; }

        public override ContentPartItem CreateItem(object contentItem, ContentPartMetadata itemMetadata)
        {
            return new ContentPartItem<dsComments.CommentsRow>(contentItem, itemMetadata);
        }    
    }

    //[PersistChildren(true)]
    //public class CommentsControl : GalleryControl
    //{
    //    [PersistenceMode(PersistenceMode.InnerProperty)]
    //    public PlaceHolder NewItem { get; set; }
    //    [PersistenceMode(PersistenceMode.InnerProperty)]
    //    public GalleryControl Gallery { get; set; }
    //}

    public class CommentsGallery : GalleryControl
    {
        protected override bool IsValidGalleryPart(IGalleryPart part)
        {
            if (part.HandlerID == ContentPartHandler.Identifier)
            {
                return (part is CommentsPart);
            }

            return base.IsValidGalleryPart(part);
        }
    }
}
