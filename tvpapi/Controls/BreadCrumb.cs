using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Web.Controls.ContainerControl;
using System.Web.UI;

namespace Tvinci.Web.Controls
{
    public enum eDirection
    {
        ltr,
        rtl    
    }

    public class BreadCrumbContainer : Control, INamingContainer
    {
        public BreadCrumbItem Item { get; set; }
        
        public BreadCrumbContainer (BreadCrumbItem item)
        {
            Item = item;            
        }
    }

    public class BreadCrumbItem
    {
		public string Name { get; set; }
        public string Display { get; set; }
        public string Link { get; set; }
        public object Tag { get; set; }

        public BreadCrumbItem(string display, string link) : this(string.Empty,display,link)
        {            
        }

        public BreadCrumbItem(string name, string display, string link) : this(name, display, link, null)
        {
        }

        public BreadCrumbItem(string name, string display, string link, object tag)
        {
			Name = name;
            Display = display;
            Link = link;
            Tag = tag;
        }

        public static implicit operator BreadCrumbItem(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
				return new BreadCrumbItem(string.Empty, string.Empty, string.Empty);
            }

            string[] temp = value.Split(new char[] { '|' }, StringSplitOptions.None);

            if (temp.Length == 1)
            {
                return new BreadCrumbItem(temp[0], temp[0], "");
            }
            else if (temp.Length > 1)
            {
                return new BreadCrumbItem(temp[0], temp[0], temp[1]);
            }
            else
            {
                throw new Exception("");
            }
        }
    }

    public class BreadCrumb : XHtmlContainer
    {
        List<BreadCrumbItem> m_items = new List<BreadCrumbItem>();

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(BreadCrumbContainer))]
        public ITemplate ItemTemplate { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(BreadCrumbContainer))]
        public ITemplate EmptyLinkItemTemplate { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate SeperateTemplate { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(BreadCrumbContainer))]
        public ITemplate ActiveItemTemplate { get; set; }

        public eDirection Direction { get; set; }

        public void Add(IEnumerable<BreadCrumbItem> items)
        {            
            m_items.AddRange(items);
        }

        public void Add(BreadCrumbItem item)
        {
            m_items.Add(item);
        }
        
        public override void DataBind()
        {
            if (m_items.Count == 0)
            {
                return;
            }

            this.Controls.Clear();
            this.ClearChildControlState();

            if (m_items.Count == 1)
            {
                BreadCrumbContainer container = new BreadCrumbContainer(m_items[0]);
                ActiveItemTemplate.InstantiateIn(container);
                this.Controls.Add(container);
            }
            else
            {
                for (int i = 0; i < m_items.Count - 1; i++)
                {
                    BreadCrumbContainer container = new BreadCrumbContainer(m_items[i]);

                    if ((string.IsNullOrEmpty(container.Item.Link) || container.Item.Link == "#") && EmptyLinkItemTemplate != null)
                    {                        
                        EmptyLinkItemTemplate.InstantiateIn(container);                                             
                    }
                    else
                    {
                        ItemTemplate.InstantiateIn(container);
                    }

                    this.Controls.Add(container);

                    Control seperator = new Control();
                    SeperateTemplate.InstantiateIn(seperator);

                    this.Controls.Add(seperator);
                }

                BreadCrumbContainer last = new BreadCrumbContainer(m_items[m_items.Count-1]);
                ActiveItemTemplate.InstantiateIn(last);
                this.Controls.Add(last);
            }
            
            base.DataBind();

        }
    }    
}
