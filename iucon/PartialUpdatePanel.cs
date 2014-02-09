// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections.ObjectModel;
using System.IO;
using System.Web.UI.Design;
using System.Drawing.Design;

namespace iucon.web.Controls
{
    public enum InitialRenderBehaviour
    {
        Serverside,
        Clientside,        
        None
    }


    public delegate object GetRequestLanguageIDDelegate();

    [ParseChildren(true)]
    [PersistChildren(false)]
    [ToolboxData("<{0}:PartialUpdatePanel runat=\"server\"><LoadingTemplate></LoadingTemplate><ErrorTemplate></ErrorTemplate></{0}:PartialUpdatePanel>")]
    [Designer("iucon.web.Controls.PartialUpdatePanelDesigner", typeof(IDesigner))]
    public class PartialUpdatePanel : CompositeControl, IScriptControl
    {
        private Panel _hostPanel = null;
        private Panel _loadingPanel = null;
        private Panel _errorPanel = null;
        private Panel _contentPanel = null;

        private InitialRenderBehaviour _initialRenderBehaviour = InitialRenderBehaviour.Clientside;

        private ITemplate _loadingTemplate = null;
        private ITemplate _errorTemplate = null;
        private ITemplate _initialTemplate = null;

        public static GetRequestLanguageIDDelegate GetRequestLanguageIDMethod { get; set; }
        public static bool TryGetSiteLanguage(out string languageKey)
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                languageKey = HttpContext.Current.Request.Form["__SiteLanguageKey"];
                return !string.IsNullOrEmpty(languageKey);
            }
            else
            {
                languageKey = string.Empty;
                return false;
            }
        }

        #region Properties

        /// <summary>
        /// Relative path to the UserControl that should be
        /// rendered within this PartialUpdatePanel
        /// </summary>
        [Editor(typeof(UserControlFileEditor), typeof(UITypeEditor))]
        public string UserControlPath
        {
            get;
            set;
        }

        /// <summary>
        /// Forces the Panel to refresh every n milliseconds
        /// </summary>
        [DefaultValue(-1)]
        public int AutoRefreshInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Display loading information after n milliseconds
        /// </summary>
        [DefaultValue(0)]
        public int DisplayLoadingAfter
        {
            get;
            set;
        }

        /// <summary>
        /// Render this panel after another
        /// </summary>
        [IDReferenceProperty(typeof(PartialUpdatePanel))]
        public string RenderAfterPanel
        {
            get;
            set;
        }

        /// <summary>
        /// Parameters that should be passed via HTTP-GET to the
        /// UserControl
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ParameterCollection Parameters
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the initial rendermode to
        /// - Serverside
        /// - Clientside
        /// - None
        /// </summary>
        [DefaultValue("Clientside")]
        public InitialRenderBehaviour InitialRenderBehaviour
        {
            get { return _initialRenderBehaviour; }
            set { _initialRenderBehaviour = value; }
        }

        /// <summary>
        /// Shown when updating is in progress
        /// </summary>
        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate LoadingTemplate
        {
            get
            {
                return _loadingTemplate;
            }
            set
            {
                _loadingTemplate = value;
            }
        }

        /// <summary>
        /// Shown when updating fails
        /// </summary>
        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate ErrorTemplate
        {
            get
            {
                return _errorTemplate;
            }
            set
            {
                _errorTemplate = value;
            }
        }

        /// <summary>
        /// Shown when the initial rendermode is "None"
        /// </summary>
        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate InitialTemplate
        {
            get
            {
                return _initialTemplate;
            }
            set
            {
                _initialTemplate = value;
            }
        }

        #endregion

        public PartialUpdatePanel() 
        {

        }

        

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _loadingPanel = new Panel();
            _loadingPanel.ID = "LoadingPanel";
            _loadingPanel.Style.Add(HtmlTextWriterStyle.Display, "none");

            _contentPanel = new Panel();
            _contentPanel.ID = "ContentPanel";

            _errorPanel = new Panel();
            _errorPanel.ID = "ErrorPanel";
            _errorPanel.Style.Add(HtmlTextWriterStyle.Display, "none");

            _hostPanel = new Panel();
            _hostPanel.ID = "HostPanel";
            Controls.Add(_hostPanel);

            _hostPanel.Controls.Add(_loadingPanel);
            _hostPanel.Controls.Add(_contentPanel);
            _hostPanel.Controls.Add(_errorPanel);

            if (_loadingTemplate != null)
                _loadingTemplate.InstantiateIn(_loadingPanel);

            if (_errorTemplate != null)
                _errorTemplate.InstantiateIn(_errorPanel);

            // render the usercontrol
            if (InitialRenderBehaviour == InitialRenderBehaviour.Serverside)
            {
                PanelHostPage page = new PanelHostPage(UserControlPath, ClientID, Page.ClientScript);
                page.Parameters = this.Parameters;
                ((IHttpHandler)page).ProcessRequest(HttpContext.Current);
                _contentPanel.Controls.Add(new LiteralControl(page.GetHtmlContent()));
            }
            else
            {
                if (InitialRenderBehaviour == InitialRenderBehaviour.None)
                {
                    // show initial template
                    if (_initialTemplate != null)
                        _initialTemplate.InstantiateIn(_contentPanel);
                }

                // To preserve viewstate from normal PostBack add a dummy hidden field
                // that will transport the viewstate during async postbacks
                HiddenField viewState = new HiddenField();
                viewState.ID = "ViewState";
                viewState.Value = Page.Request.Form[ClientID + "_ViewState"];
                _contentPanel.Controls.Add(viewState);
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }
  
        protected override void OnPreRender(EventArgs e)
        {
            if (!this.DesignMode)
            {
                // Test for ScriptManager and register if it exists
                ScriptManager sm = ScriptManager.GetCurrent(Page);

                if (sm == null)
                    throw new HttpException("A ScriptManager control must exist on the current page.");

                sm.RegisterScriptControl(this);
            }

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {      
            if (!this.DesignMode)
            {
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                sm.RegisterScriptDescriptors(this);                
            }

            base.Render(writer);
        }

        internal void EnsureChildControlsForDesigner()
        {
            EnsureChildControls();
        }

        private Control FindControl(Control control, string controlID)
        {          
            Control currentContainer = control;
            Control foundControl = null;

            if (control == control.Page)
            {
                // If we get to the Page itself while we're walking up the
                // hierarchy, just return whatever item we find (if anything) 
                // since we can't walk any higher. 
                return control.FindControl(controlID);
            }

            while (foundControl == null && currentContainer != control.Page)
            {
                currentContainer = currentContainer.NamingContainer;
                if (currentContainer == null)
                {
                    throw new HttpException();
                }
                foundControl = currentContainer.FindControl(controlID);
            }

            return foundControl;
        }

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("iucon.web.Controls.PartialUpdatePanel", this.ClientID);
            descriptor.AddProperty("UserControlPath", UserControlPath);
            descriptor.AddProperty("Parameters", Parameters);
            descriptor.AddProperty("ShowLoading", _loadingTemplate != null);
            descriptor.AddProperty("AutoRefreshInterval", AutoRefreshInterval);
            descriptor.AddProperty("DisplayLoadingAfter", DisplayLoadingAfter);
            if (!string.IsNullOrEmpty(RenderAfterPanel))
                descriptor.AddProperty("RenderAfterPanel", FindControl(this, RenderAfterPanel).ClientID);
            descriptor.AddProperty("InitiallyRenderFromClient", InitialRenderBehaviour == InitialRenderBehaviour.Clientside);
            descriptor.AddProperty("CurrentUICulture", System.Threading.Thread.CurrentThread.CurrentUICulture.ToString());
            descriptor.AddProperty("CurrentCulture", System.Threading.Thread.CurrentThread.CurrentCulture.ToString());

            if (GetRequestLanguageIDMethod != null)
            {
                descriptor.AddProperty("SiteLanguageKey", GetRequestLanguageIDMethod().ToString());
            }

            yield return descriptor;
        }

        // Generate the script reference
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("iucon.web.Controls.PartialUpdatePanel.js", this.GetType().Assembly.FullName);
        }

        #endregion
    }
}