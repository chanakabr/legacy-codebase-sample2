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

namespace Tvinci.Web.Controls.Gallery.Part
{

    
    [Obsolete("Use 'PartContainer' instead")]
    public class TabPartContainer : PartWrapper
    {

    }

    public enum eTabVisibleMode
    {
        Always,
        IfMultiple
    }

    public class TabPart : TemplatePart
    {
        
        #region Override Members

        public eTabVisibleMode VisibleMode { get; set; }
        

        public TabPart()
        {            
            VisibleMode = eTabVisibleMode.IfMultiple;
        }

        [TemplateContainer(typeof(TabPartItem))]
        public override ITemplate Template { get; set; }
        #endregion

        public override string HandlerID
        {
            get { return TabPartHandler.Identifier; }
        }

        public class PreviewTabEventArgs : EventArgs
        {
            public TabPartHandler.TabItem Tab { get; private set; }

            public PreviewTabEventArgs(TabPartHandler.TabItem tab)
            {
                Tab = tab;
            }
        }

        public event EventHandler<PreviewTabEventArgs> PreviewTab;

        internal void OnPreviewTab(TabPartHandler.TabItem tab)
        {
            if (PreviewTab != null)
            {
                PreviewTab(this, new PreviewTabEventArgs(tab));
            }
            
        }
    }
}
