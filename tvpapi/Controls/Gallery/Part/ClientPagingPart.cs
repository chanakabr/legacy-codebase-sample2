using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Tvinci.Web.Controls.ContainerControl;
using System.Web.UI.WebControls;
using System.Web;
using Tvinci.Web.Controls.Infrastructure;

namespace Tvinci.Web.Controls.Gallery.Part
{      
    [PersistChildren(true)]
    [ParseChildren(false)]
    public class ClientPagingPart : GalleryPart
    {
        public override void HandleAddedToGallery(GalleryBase gallery)
        {
            if (Direction == eDirection.RTL)
            {
                gallery.HandleInnerControlParts(this, delegate(IGalleryPart part)
                {
                    ContentPart contentPart = part as ContentPart;
                    if (contentPart != null)
                    {
                        contentPart.Behaivor = ContentPart.eBehaivor.Reverse;
                    }
                });
            }
        }

        public enum eDirection
        {
            LTR,
            RTL
        }
        public enum ePagingType
        {
            None,
            Client
        }

        public enum eInitializeMode
        {
            PageLoad,
            Ajax
        }

        public eInitializeMode InitializeMode { get; set; }
        public eDirection Direction { get; set; }
        public ClientPagingNavigation Navigation { get; set; }
		public int ItemInRow { get; set; }
		public int ItemsParPage { get; set; }	//For vertical galleries, sets the paging parameter. 
		public int RowCount { get; set; }
        public string CssClass { get; set; }
        public string CustomStyle { get; set; }        
        public string CustomCssOnNoPaging { get; set; }
        public ePagingType PagingType { get; set; }

        public ClientPagingPart()
        {
            Direction = eDirection.LTR;
            InitializeMode = eInitializeMode.PageLoad;

			RowCount = 1;
            GalleryIdentifier = Guid.NewGuid().ToString();            
            PagingType = ePagingType.Client;            
        }

        protected override void OnInit(EventArgs e)
        {            
            //this.Controls.Add(Content);
            base.OnInit(e);
        }
        
        protected override ControlCollection CreateControlCollection()
        {
            return base.CreateControlCollection();
            
        }
              
        public override string HandlerID
        {
            get { return ClientPagingHandler.Identifier; }
        }

        public string GalleryIdentifier {get;private set;}

        protected override void OnPreRender(EventArgs e)
        {
            if (InitializeMode != eInitializeMode.PageLoad)
            {
                return;
            }

            JavaPageloadControl control;
            if (JavaPageloadControl.TryGetPageControl(out control))
            {
                
                if (!control.IsContentRegistered("ClientPagingHandling"))
                {
                    control.RegisterContent("ClientPagingHandling", @"

    //FireFox
    var CarouselCollection = document.getElementsByName(""CarousleControl"");        
    if(CarouselCollection.length > 0)
    {
        for(var i=0; i<CarouselCollection.length; i++)
        {                
            if (CarouselCollection[i].value != '')
            {
                var values =  CarouselCollection[i].defaultValue.split(';');            
                InitializeCarousel(values[0], parseInt(values[1]),values[2]);                
                if (values.length < 4)
                {         
                    CarouselCollection[i].value += ';executed';
                }
            }

        }
    }
    else    //IE
    {
        var inputCollection = document.getElementsByTagName(""input"");
                        
        for(var i=0; i<inputCollection.length; i++)
        {
            if(inputCollection[i].name == ""CarousleControl"")
            {
                var values =  CarouselCollection[i].defaultValue.split(';');            
                InitializeCarousel(values[0], parseInt(values[1]),values[2]);    
                if (values.length < 4)
                {         
                    CarouselCollection[i].value += ';executed';            
                }
            }            
        }
    }", JavaPageloadControl.eExecuteOn.NewPage);
                }

                if (!control.IsContentRegistered("ClientPagingHandlingPostback"))
                {

                    control.RegisterContent("ClientPagingHandlingPostBack", @"

    //FireFox
    var CarouselCollection = document.getElementsByName(""CarousleControl"");        
    if(CarouselCollection.length > 0)
    {
        for(var i=0; i<CarouselCollection.length; i++)
        {                
            if (CarouselCollection[i].value != '')
            {
                var values =  CarouselCollection[i].defaultValue.split(';');   
                if (values.length != 4)
                {         
                    InitializeCarousel(values[0], parseInt(values[1]),values[2]);                
                    CarouselCollection[i].value += ';executed';
                }
            }

        }
    }
    else    //IE
    {
        var inputCollection = document.getElementsByTagName(""input"");
                        
        for(var i=0; i<inputCollection.length; i++)
        {
            if(inputCollection[i].name == ""CarousleControl"")
            {
                var values =  CarouselCollection[i].defaultValue.split(';');            
                if (values.length != 4)
                {         
                    InitializeCarousel(values[0], parseInt(values[1]),values[2]);    
                    CarouselCollection[i].value += ';executed';            
                }   
            }            
        }
    }", JavaPageloadControl.eExecuteOn.PostBack);



                }
            }
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
			if (ItemsParPage == 0)
				ItemsParPage = ItemInRow;

            writer.WriteBeginTag("div");
            writer.WriteAttribute("id", GalleryIdentifier);
            writer.WriteAttribute("name", "ClientGalleryPart");

            if (!string.IsNullOrEmpty(CustomCssOnNoPaging) && !currentGalleryHasPaging)
            {
                writer.WriteAttribute("class", CustomCssOnNoPaging);
            }
            else
            {
                writer.WriteAttribute("class", CssClass);
            }

            if (!string.IsNullOrEmpty(CustomStyle))
                writer.WriteAttribute("style", CustomStyle);

            writer.Write(HtmlTextWriter.TagRightChar);
            base.Render(writer);
            writer.WriteEndTag("div");

            if (PagingType == ePagingType.Client)
            {
                switch (InitializeMode)
                {
                    case eInitializeMode.PageLoad:
						writer.WriteLine(string.Format("<div style='display:none'><input name='CarousleControl' value='{0};{1};{2}'/></div>", GalleryIdentifier, ItemsParPage, Direction.ToString()));
                        break;
                    case eInitializeMode.Ajax:
						ScriptManager.RegisterClientScriptBlock(this, typeof(ClientPagingPart), "cp" + Guid.NewGuid().ToString(), string.Format(@"InitializeCarousel('{0}', {1}, '{2}');", GalleryIdentifier, ItemsParPage, Direction.ToString()), true);                                                
                        break;
                    default:
                        break;
                }
            }
        }


        bool currentGalleryHasPaging;

        internal void previewPagesCount(long pagesCount)
        {
            if (pagesCount <= 1)
            {
                currentGalleryHasPaging = false;
                Navigation.Visible = false;
            }
            else
            {
                currentGalleryHasPaging = true;
                Navigation.Visible = true;
            }
        }
    }
}
