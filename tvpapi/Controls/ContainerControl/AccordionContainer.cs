using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace Tvinci.Web.Controls.ContainerControl
{
    public class AccordionHeaderItem : Control, INamingContainer
    {
        public string Header { get; set; }
        public bool IsSelected { get; set; }

        public AccordionHeaderItem(string header, bool isSelected)
        {
            Header = header;
            IsSelected = isSelected;
        }
    }



    [ParseChildren(false)]
    [PersistChildren(true)]
    public class AccordionPanel : Control, INamingContainer
    {
        public string Header { get; set; }
    }

    [ParseChildren(true)]
    [PersistChildren(false)]
    public class AccordionContainer : PlaceHolder, INamingContainer
    {
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(AccordionHeaderItem))]
        public ITemplate ItemHeaderTemplate { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<AccordionPanel> Items { get; set; }

        public string CssClass { get; set; }        

        public override void DataBind()       
        {            

            this.Visible = true;
            this.Controls.Clear();

            if (ItemHeaderTemplate == null)
            {
                this.Visible = false;
                return;
            }

            bool isFirst = true;
            foreach (AccordionPanel item in Items)
            {
                if (!string.IsNullOrEmpty(item.Header))                
                {
                    AccordionHeaderItem templateItem = new AccordionHeaderItem(item.Header, isFirst);
                    isFirst = false;
                    ItemHeaderTemplate.InstantiateIn(templateItem);
                    this.Controls.Add(templateItem);
                    this.Controls.Add(item);
                }
            }

            base.DataBind();

            // show only if has inner controls
            this.Visible = (this.Controls.Count != 0);
        }


        protected override void OnPreRender(EventArgs e)
        {         
//            JavaPageloadControl control;
//            if (JavaPageloadControl.TryGetPageControl(out control))
//            {
//                if (!control.IsContentRegistered("AccordionContainer"))
//                {
//                    control.RegisterContent("AccordionContainer", @"
// //FireFox
//                        var CarouselCollection = document.getElementsByName('AccordionContainer');        
//                        if(CarouselCollection.length > 0)
//                        {
//                            for(var i=0; i<CarouselCollection.length; i++)
//                            {                
//                                if (CarouselCollection[i].value != '')
//                                {
//                                    var values =  CarouselCollection[i].value.split(';');            
//                                    jQuery('#' + values[0]).accordion({header: values[1],autoheight: false});                
//                                }
//                            }
//                        }
//                        else    //IE
//                        {
//                            var inputCollection = document.getElementsByTagName(""input"");
//                                            
//                            for(var i=0; i<inputCollection.length; i++)
//                            {
//                                if(inputCollection[i].name == ""AccordionContainer"")
//                                {
//                                    var values =  CarouselCollection[i].value.split(';');            
//                                    jQuery('#' + values[0]).accordion({header: values[1],autoheight: false});                
//                                    inputCollection[i].value = '';
//                                }            
//                            }
//                        }                    
//                    ");
//                }
//            }
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {            
            if (!string.IsNullOrEmpty(CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);                
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);            
            writer.RenderBeginTag(HtmlTextWriterTag.Div);                        
            base.Render(writer);            
            writer.RenderEndTag();

            if (!Page.IsPostBack)
            {
                writer.WriteLine(string.Format("<input type='hidden' name='AccordionContainer' value='{0};{1}'/>", this.ClientID, "a.title"));
            }
        }
    }


}
