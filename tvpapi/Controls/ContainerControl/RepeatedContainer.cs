using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Tvinci.Web.Controls.ContainerControl
{
    
    public class RepeatedContainerItem : Control, INamingContainer
    {
        public class ItemMetadata
        {
            public int ItemNumber { get; set; }
            public int ItemCount { get; set; }
        }

        
        ItemMetadata m_metadata = new ItemMetadata();
        public object Item { get; set; }
        
        public ItemMetadata Metadata
        {
            get
            {
                return m_metadata;
            }
        }

        public RepeatedContainerItem(int itemNumber, int itemCount)
        {            
            m_metadata.ItemCount = itemCount;
            m_metadata.ItemNumber = itemNumber;
        }    
	}

	public class ItemAddedEventArgs : EventArgs
	{
		public RepeatedContainerItem Control { get; set; }

	}	
    public class RepeatedContainer : TemplatedContainer
    {
        public enum eDirection
        {
            Forward,
            Reverse
        }

        public eDirection Direction { get; set; }
		public event EventHandler<ItemAddedEventArgs> ItemAddedEvent;

        [TemplateContainer(typeof(RepeatedContainerItem))]
        public override ITemplate Template { get; set; }

        public int RepeatingTime { get; set; }
        public object Item { get; set; }

        public RepeatedContainer()
        {
            Direction = eDirection.Forward;
        }
        public override void DataBind()
        {
            if (Item != null)
            {
                this.Controls.Clear();

                if (Direction == eDirection.Forward)
                {
                    for (int i = 1; i <= RepeatingTime; i++)
                    {
                        addItemToTemplate(i);
                    }
                }
                else
                {
                    for (int i = RepeatingTime; i >=1 ; i--)
                    {
                        addItemToTemplate(i);
                    }
                }
            }            

            base.DataBind();                        
        }

        private void addItemToTemplate(int i)
        {
            Control control = new RepeatedContainerItem(i, RepeatingTime) { Item = Item };
            control.ID = string.Format("item{0}", i);
            base.HandleItem(control);

            if (ItemAddedEvent != null)
            {
                ItemAddedEvent(this, new ItemAddedEventArgs { Control = (RepeatedContainerItem)control });
            }
        }
    }
}
