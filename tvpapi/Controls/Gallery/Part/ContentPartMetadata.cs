using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Web.Controls.Gallery.Part
{
   

    public sealed class ContentPartMetadata
    {        
        public int ItemNumber { get; set; }
        public int ItemsCount { get; set; }
        public int SelectedItemNumber { get; set; }
        public int ItemsInColumn { get; set; }
        public bool IsSelected { get; private set; }
                        
        public ContentPartMetadata(int itemNumber, int itemsCount, int itemsInColumn, int selectedItemNumber)
        {
            SelectedItemNumber = selectedItemNumber;
            IsSelected = itemNumber == selectedItemNumber;
            ItemNumber = itemNumber;
            ItemsCount = itemsCount;
            ItemsInColumn = itemsInColumn;                                   

        }

        #region Methods
        public bool IsFirst()
        {
            return (ItemNumber == 1);
        }

        public bool IsEven()
        {
            return (ItemNumber % 2 == 0);
        }

        public bool IsLast()
        {
            return ItemNumber == ItemsCount;
        }

        public bool ShouldAddColumnTag(eTagType type)
        {
            if (ItemsInColumn != 0)
            {
                switch (type)
            {
                case eTagType.Begin:
                    return ((ItemNumber -1) % ItemsInColumn == 0) ; // IsFirst() - not needed since the calculation will return true on 1 always                    
                case eTagType.End:
                    return IsLast() || (ItemNumber % ItemsInColumn == 0);
                default:
                    throw new NotImplementedException();
                }
            }
            else
            {
                return false;
            }            
        }
        #endregion

		public object GetItemNumber(int minDigits)
		{
			return ItemNumber.ToString().PadLeft(minDigits, '0');			
		}
	}
}
