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
    public class PagingPart : TemplatePart
    {
        public enum eDirection
        {
            Normal,
            Reverse
        }
        #region Properties                
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public PagingLayout Layout { get; set; }
        public eDirection Direction { get; set; }
        #endregion


        #region Override members
               
        [TemplateContainer(typeof(PagingPartItem))]
        public override ITemplate Template { get; set; }
        #endregion

        public override string HandlerID
        {
            get { return PagingPartHandler.Identifier; }
        }

        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public long PagesCount { get; private set; }
        public long ItemsInSource { get; private set; }

        internal void SyncNavigationInformation(int pageNumber, int pageSize, long itemsInSource, long pagesCount)
        {
            ItemsInSource = itemsInSource;
            PageNumber = pageNumber;
            PageSize = pageSize;
            PagesCount = pagesCount;
        }
    }
}
