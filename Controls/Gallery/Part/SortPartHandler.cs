using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class SortPartHandler : GalleryPartHandler
    {
        private const char splitChar = ';';
        public const string Identifier = "SortPartHandler";

        public void Process(SortPart part)
        {
            int itemNumber = 1;
            string activeContext = part.ActiveContext;

            part.Controls.Clear();

            if (string.IsNullOrEmpty(part.SortByArray))
            {
                FindPartWrapper(part, part).Visible = false;
                return;
            }
                        
            FindPartWrapper(part,part).Visible = true;
            string[] sortList = part.SortByArray.Split(new char[] {splitChar},StringSplitOptions.RemoveEmptyEntries); 

            int activeItemNumber = part.ActiveSortNumber;

            if (!string.IsNullOrEmpty(part.InitialValue) && !part.Page.IsPostBack)
            {                
                int tempNumber = 1;
                foreach (string value in sortList)
                {
                    if (value == part.InitialValue)
                    {
                        activeItemNumber = tempNumber;
                        break;
                    }
                    tempNumber++;
                }                
            }

            bool hasSeperator = part.SeperatorTemplate != null;
            for (int i = 0; i < sortList.Length; i++)
            {
                string sortItem = sortList[i];

                if (hasSeperator && itemNumber != 1)
                {
                    Control seperator = new Control();
                    part.SeperatorTemplate.InstantiateIn(seperator);
                    part.Controls.Add(seperator);
                }

                SortPartItem item = new SortPartItem(sortItem, itemNumber == activeItemNumber, itemNumber, activeContext);
                item.ID = itemNumber.ToString();
                
                if (itemNumber == 1 && part.FirstItemTemplate != null)
                {
                    part.FirstItemTemplate.InstantiateIn(item);
                    part.Controls.Add(item);
                }
                else
                {
                    part.Template.InstantiateIn(item);
                    part.Controls.Add(item);
                }

                itemNumber++;
            }
        }

      

        public override string GetIdentifier()
        {
            return Identifier;
        }
    }

}
