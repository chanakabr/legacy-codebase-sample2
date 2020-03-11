using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections;
using Tvinci.Data.DataLoader;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Web.Controls.Gallery.Part;

namespace Tvinci.Web.Controls.Gallery
{
    [Serializable()]
    public class GalleryTab
    {
        public string Title { get; set; }        
        public string LoaderTableName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public ePagingMethod PagingMethod { get; set; }
        string m_tabIdentifier;
        public string Identifier { get { return m_tabIdentifier; } 
            set {
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception("UniqueID cannot be empty");
                }

                m_tabIdentifier = value;
            }
        }
        
        public GalleryTab() : this(string.Empty)
        {
            // empty by design
        }

        public GalleryTab(string title)
        {
            Title = title;
            PagingMethod = ePagingMethod.Default;
            m_tabIdentifier = Guid.NewGuid().ToString();            
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GalleryTab))
            {
                return false;
            }
            else
            {
                return this.Identifier.Equals(((GalleryTab)obj).Identifier);
            }
        }        
    }    
}
