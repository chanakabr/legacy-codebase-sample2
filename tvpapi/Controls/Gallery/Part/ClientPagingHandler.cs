using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class ClientPagingHandler : GalleryPartHandler
    {
        public const string Identifier = "ClientPagingHandler";


        public void Process(ClientPagingPart part, long itemsCount)
        {            
            long pagesCount = claculatePageCount(part.ItemInRow*part.RowCount, itemsCount);
                        
            if (part.Navigation != null)
            {
                part.Navigation.Controls.Clear();
                part.Navigation.Visible = true;
                
                part.previewPagesCount(pagesCount);
                                
                if (pagesCount > 1)
                {
                    if (part.Direction == ClientPagingPart.eDirection.LTR)
                    {
                        part.Navigation.Controls.Add(part.Navigation.NextContainer);
                        for (int i = 1; i < (int)pagesCount+1; i++)
                        {
                            ClientPagingNavigationItem pb = new ClientPagingNavigationItem() { ButtonNumber = i, GalleryID = part.GalleryIdentifier };

                            part.Navigation.PageButtonTemplate.InstantiateIn(pb);
                            part.Navigation.Controls.Add(pb);
                        }
                        part.Navigation.Controls.Add(part.Navigation.PrevContainer);
                    }
                    else
                    {
                        part.Navigation.Controls.Add(part.Navigation.PrevContainer);
                        for (int i = (int)pagesCount; i >= 1; i--)
                        {
                            ClientPagingNavigationItem pb = new ClientPagingNavigationItem() { ButtonNumber = i, GalleryID = part.GalleryIdentifier };

                            part.Navigation.PageButtonTemplate.InstantiateIn(pb);
                            part.Navigation.Controls.Add(pb);
                        }
                        part.Navigation.Controls.Add(part.Navigation.NextContainer);
                    }
                }
            }
        }

        private long claculatePageCount(int pageSize, long itemsCount)
        {
            if (pageSize == 0)
            {
                return 1;
            }
            else
            {
                if ((itemsCount % pageSize) != 0)
                {
                    return (itemsCount / pageSize) + 1;
                }
                else
                {
                    return (itemsCount / pageSize);
                }

            }

        }
        public override string GetIdentifier()
        {
            return Identifier;
        }
    }

}
