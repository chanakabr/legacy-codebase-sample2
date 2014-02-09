using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls.ContainerControl
{
    public class ContainerContent : PlaceHolder
    {
        public string UniqueName { get; set; }
    }

    [ParseChildren(true)]
    [PersistChildren(false)]
    public class ConditionalContainer2 : PlaceHolder
    {
        string m_ContainerToShow = string.Empty;

        public string ContainerToShow
        {
            get
            {
                return m_ContainerToShow;
            }
            set
            {
                if (m_ContainerToShow != value)
                {
                    m_ContainerToShow = value;

                    if (m_afterIniitialized)
                    {
                        syncControl();
                    }
                }

            }
        }

        bool m_afterIniitialized = false;
        protected override void OnInit(EventArgs e)
        {
            m_afterIniitialized = true;
            syncControl();
            base.OnInit(e);
        }
                        
        private void syncControl()
        {
            this.Controls.Clear();
            ClearChildViewState();
            if (Container != null && !string.IsNullOrEmpty(ContainerToShow))
            {
                ContainerContent item = Container.Find(delegate(ContainerContent obj)
                {
                    return (obj.UniqueName == ContainerToShow);
                });

                if (item != null)
                {
                    this.Controls.Add(item);
                }                                                
            }
        }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<ContainerContent> Container { get; set; }
    }


    [ParseChildren(true)]
    public class ConditionalContainer : PlaceHolder
    {
        int m_selectedIndex = -1;

        public int SelectedIndex
        {
            get
            {                
                return m_selectedIndex;
            }
            set
            {
                if (m_selectedIndex != value)             
                {
                    m_selectedIndex = value;
                    syncControl();
                }
            }
        }

        public override void DataBind()
        {
            base.DataBind();
        }
        protected override void OnInit(EventArgs e)
        {
            syncControl();
            base.OnInit(e);
        }

        private void syncControl()
        {
            if (Container != null && m_selectedIndex != -1)
            {
                this.Controls.Clear();
                ClearChildViewState();
                this.Controls.Add(Container[m_selectedIndex]);
            }
        }
        
        

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<PlaceHolder> Container { get; set; }
    }

    


}
