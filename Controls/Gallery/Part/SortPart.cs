using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Tvinci.Web.Controls.Gallery.Part
{
  
    public class SortPartItem : Control, INamingContainer
    {
        public bool IsLast { get; set; }
        public bool IsActive { get; set; }
        public string DisplayText { get; set; }
        public string ActiveContext { get; set; }
        public int ItemNumber { get; set; }
                
        public SortPartItem(string displayText, bool isActive, int itemNumber, string activeContext)
        {
            DisplayText = displayText;
            ItemNumber = itemNumber;
            IsActive = isActive;
            ActiveContext = activeContext;
        }        
    }
    
    public class SortPart : GalleryPart
    {
        private string m_sort;
        public string SortByArray { get { return m_sort; } set { m_sort = value; } }
        public int ActiveSortNumber { get; set; }
        public string ActiveContext { get; set; }
        public string InitialValue { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(SortPartItem))]
        public ITemplate FirstItemTemplate { get; set; }

        public object FirstItemData { get; set; }


        public SortPart()
        {
            ActiveSortNumber = 1;
            ActiveContext = string.Empty;
        }
        
        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }
        
        protected override void LoadControlState(object savedState)
        {
            object[] values = (object[])savedState;
            ActiveSortNumber = (int)values[0];
            SortByArray = (string)values[1];
            ActiveContext = (string)values[2];

            base.LoadControlState(values[3]);
        }

        protected override object SaveControlState()
        {
            return new object[] { ActiveSortNumber, SortByArray,ActiveContext,  base.SaveControlState() };            
        }

        public override string HandlerID
        {
            get { return SortPartHandler.Identifier; }
        }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate SeperatorTemplate { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(SortPartItem))]
        public ITemplate Template {get;set;}
        
    }
}
