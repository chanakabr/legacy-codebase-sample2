using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class ContentPartHandler : GalleryPartHandler
    {
        public const string Identifier = "ContentPartHandler";

        public void HandleItems(ContentPart part, IEnumerable items, int itemsCount)
        {
            part.Controls.Clear();
            part.OnPreExecute(items, itemsCount);

            Control partWrapper = FindPartWrapper(part,part);
            
            if (itemsCount == 0)
            {
                if (part.NoDataPanel != null)
                {
                    if (partWrapper != null)
                    {
                        foreach (Control control in partWrapper.Controls)
                        {
                            control.Visible = false;
                        }

                        partWrapper.Controls.AddAt(0, part.NoDataPanel); 
                    }
                    else
                    {
                        part.Controls.Add(part.NoDataPanel);
                    }
                }

                return;
            }

            int itemNumber = 1;                        
            int itemsInColumn = part.ItemsInColumn;

            if (itemsInColumn == 0 && part.ExpectedColumns != 0)
            {
                itemsInColumn = (int)Math.Ceiling(((double)itemsCount / part.ExpectedColumns));
            }
                        
            int selectedItemNumber = part.SelectedItemNumber;

            bool isReverse = (part.Behaivor & ContentPart.eBehaivor.Reverse) == ContentPart.eBehaivor.Reverse;
            if (!isReverse)
            {
                handleVirtualFirstItem(part, ref itemNumber, ref itemsCount, itemsInColumn, selectedItemNumber);                                
            }

            foreach (object item in items)
            {
                if (itemNumber != 1)
                {
                    part.HandleSeperator();
                }

                ContentPartMetadata itemMetadata = new ContentPartMetadata(itemNumber, itemsCount, itemsInColumn, part.SelectedItemNumber);
                ContentPartItem control = part.CreateItem(item, itemMetadata);
                control.ID = itemNumber.ToString();
                part.HandleItem(control, isReverse);
                part.OnItemAdded(control, itemMetadata);
                itemNumber++;
            }

            if (isReverse)
            {
                handleVirtualFirstItem(part, ref itemNumber, ref itemsCount, itemsInColumn, selectedItemNumber);
            }

            

        }
        private void handleVirtualFirstItem(ContentPart part, ref int itemNumber, ref int itemsCount,int itemsInColumn, int selectedItemNumber)
        {
            if (part.FirstItemData != null && part.FirstItemTemplate != null)
            {
                // increase items count by one since pushing item to as first item
                itemsCount++;

                ContentPartMetadata itemMetadata = new ContentPartMetadata(itemNumber, itemsCount, itemsInColumn, selectedItemNumber);
                ContentPartItem firstItemControl = new ContentPartItem<object>(part.FirstItemData.ItemData, itemMetadata);
                firstItemControl.ID = itemNumber.ToString();
                part.FirstItemTemplate.InstantiateIn(firstItemControl);
                part.Controls.Add(firstItemControl);

                itemNumber++;
            }
          
        }
        
        public override string GetIdentifier()
        {
            return Identifier;
        }

    }
}
