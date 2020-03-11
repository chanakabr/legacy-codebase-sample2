using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class PagingPartHandler : GalleryPartHandler
    {
        public const string Identifier = "PagingHandler";

        public void HandlePaging(PagingPart part, int activePageIndex, long pagesCount, long totalItemsCount, int pageSize)
        {            
            part.Controls.Clear();

            Control partWrapper = FindPartWrapper(part,null);
            if (partWrapper != null)
            {
                if (pagesCount <= 1)
                {
                    partWrapper.Visible = false;
                    return;
                }
                else
                {
                    partWrapper.Visible = true;
                }
            }
            
            PagingPartItem container = new PagingPartItem(activePageIndex, pagesCount, totalItemsCount, pageSize);
            container.ID = "p";

            part.HandleItem(container);

            part.SyncNavigationInformation(activePageIndex + 1, pageSize, totalItemsCount, pagesCount);

            if (part.Layout == null)
            {
                return;
            }
            
            PlaceHolder ph = (PlaceHolder)container.FindControl(part.Layout.PlaceHolderID);

            ph.Controls.Clear();

            if (ph != null)
            {
                ph.Controls.Add(part.Layout.Generate(activePageIndex, pagesCount, (part.Direction == PagingPart.eDirection.Reverse)));
            }
            else
            {
                throw new Exception("Value of 'PlaceHolderID' on property 'Layout' is not set of control not found");
            }
        }

        public override string GetIdentifier()
        {
            return Identifier;
        }
    }
}
