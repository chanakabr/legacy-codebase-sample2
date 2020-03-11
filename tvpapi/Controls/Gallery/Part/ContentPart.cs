using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Web.Controls.Gallery;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Tvinci.Web.Controls.Gallery.Part;
using Tvinci.Web.Controls.ContainerControl;
using System.Collections;
using System.ComponentModel;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class FirstItemDataContainer
    {
        public bool PersistInPostback { get; set; }
        public object ItemData { get; set; }

        public FirstItemDataContainer(bool persistInPostback, object itemData)
        {
            PersistInPostback = persistInPostback; 
            ItemData = itemData;
        }

    }
    
    public abstract class ContentPart : TemplatePart
    {
        [Flags]
        public enum eBehaivor
        {
            None = 0,
            Reverse = 2
        }
        public int ExpectedColumns { get; set; }
        public int ItemsInColumn { get; set; }
        public int SelectedItemNumber { get; set; }
        public eBehaivor Behaivor { get; set; }
        

        [PersistenceMode(PersistenceMode.InnerProperty)]
        public PlaceHolder NoDataPanel { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(ContentPartItem<object>))]
        public ITemplate FirstItemTemplate { get; set; }

        [Browsable(false)]
        public FirstItemDataContainer FirstItemData { get; set; }
                
        public ContentPart()
        {
            SelectedItemNumber = int.MinValue;
        }
        
        protected internal virtual void OnPreExecute(IEnumerable items, int itemsCount)
        {
            return;
        }

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override void LoadControlState(object savedState)
        {
            object[] data = (object[])savedState;            
            SelectedItemNumber = (int)data[1];
            if (data[2] != null)
            {
                FirstItemData = new FirstItemDataContainer(true, data[2]);
            }

            base.LoadControlState(data[0]);
        }

        protected override object SaveControlState()
        {
            object firstItem = (FirstItemData != null && FirstItemData.PersistInPostback) ? FirstItemData.ItemData : null;

            return new object[] { base.SaveControlState(), SelectedItemNumber, firstItem };
        }

        public override string HandlerID
        {
            get { return ContentPartHandler.Identifier; }
        }

        public override void HandleItem(Control item)
        {
            base.HandleItem(item);
            
        }
        public abstract ContentPartItem CreateItem(object item, ContentPartMetadata itemMetadata);
        public abstract void OnItemAdded(ContentPartItem container, ContentPartMetadata itemMetadata);        
    }

    public abstract class ContentPart<TItem> : ContentPart
    {        
        public delegate void ItemAddedDelegate(ContentPartItem container, TItem item, ContentPartMetadata itemMetadata);

        public event ItemAddedDelegate ItemAdded;

        [Browsable(false)]
        public Func<TItem, object, bool> FindSelectedItemFunction { get; set; }
        [Browsable(false)]
        public object FindSelectedItemParameter { get; set; }

        private object m_lastParameterUsedToFindSelected = new object(); // initialize using dummy object to support !ispostback
        protected override void LoadControlState(object savedState)
        {
            object[] data = (object[])savedState;
            FindSelectedItemParameter = data[1];
            
            base.LoadControlState(data[0]);
        }

        protected override object SaveControlState()
        {
            return new object[] { base.SaveControlState(), FindSelectedItemParameter };
        }


        protected internal override void OnPreExecute(IEnumerable items, int itemsCount)
        {
                        
            if (FindSelectedItemFunction != null)
            {
                if (m_lastParameterUsedToFindSelected == FindSelectedItemParameter)
                {
                    return;
                }

                bool hasVirtualFirstItem = (this.FirstItemData != null && this.FirstItemTemplate != null);
                
                m_lastParameterUsedToFindSelected = FindSelectedItemParameter;

                if (FindSelectedItemParameter == null)
                {
                    if (hasVirtualFirstItem)
                    {
                        SelectedItemNumber = 1;
                        return;
                    }
                    else
                    {
                        SelectedItemNumber = 0;
                        return;
                    }
                }
                else
                {
                    int itemNumber = 0;
                    foreach (object item in items)
                    {
                        itemNumber++;
                        if (FindSelectedItemFunction((TItem)item, FindSelectedItemParameter))
                        {
                            if (hasVirtualFirstItem)
                            {
                                SelectedItemNumber = itemNumber + 1;
                                return;
                            }
                            else
                            {
                                SelectedItemNumber = itemNumber;
                                return;
                            }
                        }
                    }    
                }
            }                        
        }

        public override ContentPartItem CreateItem(object contentItem, ContentPartMetadata itemMetadata)
        {
            return new ContentPartItem<TItem>(contentItem, itemMetadata);
        }
        
        public override void OnItemAdded(ContentPartItem container, ContentPartMetadata itemMetadata)
        {
            if (ItemAdded != null)
            {
                ItemAdded(container, (TItem)container.ContentItem, itemMetadata);
            }
        }        
    }
}
