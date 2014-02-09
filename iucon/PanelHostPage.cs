// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.IO;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Web;

namespace iucon.web.Controls
{
    /// <summary>
    /// Hosts the UserControl and renderes the output
    /// </summary>
    internal class PanelHostPage : Page
    {
        private HtmlForm _mainForm = null;
        private ScriptManager _scriptManager = null;
        private ClientScriptManager _serverClientScript = null;
        private PageStatePersister _persister = null;

        private string _controlPath = null;
        private string _pageViewState = null;
        private string _renderedContent = null;
        private bool _hasData;
        private string _controlClientID = null;
        public ParameterCollection Parameters { get; set; }
        internal ScriptManager ScriptManager
        {
            get { return _scriptManager; }
        }

        public PanelHostPage(string ControlPath, string ControlClientID)
            : this(ControlPath, ControlClientID, null)
        {
        }


        public PanelHostPage(string ControlPath, string ControlClientID, ClientScriptManager clientScript)
        {
            EnableEventValidation = false;
            _controlPath = ControlPath;
            _controlClientID = ControlClientID;
            _serverClientScript = clientScript;

            Controls.Add(new HtmlHead());

            _mainForm = new HtmlForm();
            Controls.Add(_mainForm);

            _scriptManager = new ScriptManager();
            _mainForm.Controls.Add(_scriptManager);
        }


        /// <summary>
        /// This content is written to the client by PartialUpdatePanelHandler
        /// </summary>        
        public string GetHtmlContent()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<!-- {0} -->",(_hasData ? "1" : "0"));
            
            
            // insert new viewstate
            sb.AppendFormat("<input type=\"hidden\" name=\"{0}_ViewState\" id=\"{0}_ViewState\" value=\"{1}\" />", _controlClientID, _pageViewState);
            

            // process PostBackLinks
            string content = PostProcessPostBackLinks(_renderedContent);

            //sb.AppendFormat("<input type=\"hidden\" name=\"{0}_HasData\" id=\"{0}_HasData\" value=\"{1}\" />", _controlClientID, (_hasData ? "1" : "0"));

            // append content            
            sb.Append(content);

            return sb.ToString();
        }
        
        protected override void CreateChildControls()
        {
            // Load Control
            if (_controlPath != null)
            {
                Control control = LoadControl(ResolveUrl(_controlPath));

                // generate a global unique id from the parent hierarchie
                // replace _ by . to generate a id that ASP.NET can handle                
                control.ID = _controlClientID.Replace('_', '.');

                Form.Controls.Add(control);
            }

            base.CreateChildControls();
        }

        protected override void OnPreRenderComplete(EventArgs e)
        {
            base.OnPreRenderComplete(e);

            foreach (Control c in Controls)
                ProcessPostBackControls(c);

            SetRenderMethodDelegate(new RenderMethod(RenderPage));
            _mainForm.SetRenderMethodDelegate(new RenderMethod(RenderForm));
        }

        /// <summary>
        /// This custom delegate is necessary, because otherwise the page
        /// would send it's form-tag, script-tags etc. directly to the the response
        /// when a control is rendered in the Serverside initial startup mode.
        /// Here we simply discard the default output, because we only want to have the
        /// contents of our _mainForm
        /// </summary>
        private void RenderPage(HtmlTextWriter writer, Control control)
        {
            StringWriter controlWriter = new StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(controlWriter);

            foreach (Control child in control.Controls)
                child.RenderControl(htmlWriter);
        }

        private void RenderForm(HtmlTextWriter writer, Control control)
        {
            StringBuilder controlBuilder = new StringBuilder();
            StringWriter controlWriter = new StringWriter(controlBuilder);
            HtmlTextWriter htmlWriter = new HtmlTextWriter(controlWriter);

            foreach (Control child in control.Controls)
            {
                if (!(child is ScriptManager))
                {
                    child.RenderControl(htmlWriter);
                }
            }

            _renderedContent = controlBuilder.ToString();

            _hasData = (Regex.Replace(_renderedContent, "[\r\n ]", "").Length != 0);

            if (_serverClientScript == null)
            {
                // append JScript elements
                ScriptRenderer script = new ScriptRenderer(_scriptManager, _controlClientID);
                _renderedContent += script.GetScriptBlock();
            }
            else
            {
                ClientScriptRegister cs = new ClientScriptRegister();
                cs.RegisterScripts(_scriptManager, _serverClientScript);
            }
        }

        /// <summary>
        /// Replaces javascript PostBack-commands "__doPostBack" with the internal 
        /// PartialUpdatePanel-PostBack
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string PostProcessPostBackLinks(string content)
        {
            if (content != null)
            {
                Regex regex = new Regex(@"__doPostBack\('(?<control>[^']*)','(?<argument>[^']*)'\)");
                foreach (Match match in regex.Matches(content))
                    content = content.Replace(match.Value, GetPostBackReference(match.Groups["control"].Value, match.Groups["argument"].Value, false));

                regex = new Regex(@"javascript:setTimeout\('__doPostBack\(\\'(?<control>[^']*)\\',\\'(?<argument>[^']*)\\'\)', 0\)");
                foreach (Match match in regex.Matches(content))
                    content = content.Replace(match.Value, GetPostBackReference(match.Groups["control"].Value, match.Groups["argument"].Value, true));
            }

            return content;
        }

        /// <summary>
        /// Adds OnClientClick to buttons.
        /// The result is that by clicking on these controls
        /// only a partial PostBack is made. It prevents the whole page from doing a PostBack
        /// </summary>        
        private void ProcessPostBackControls(Control control)
        {
            if (control is Button)
            {
                ((Button)control).OnClientClick = GetPostBackReference(control.ClientID.Replace('_', '$'), "null", true);
                ((Button)control).OnClientClick += "; return false;";
            }
            else if (control is ImageButton)
            {
                ((ImageButton)control).OnClientClick = GetPostBackReference(control.ClientID.Replace('_', '$'), "null", true);
                ((ImageButton)control).OnClientClick += "; return false;";
            }
            else if (control is LinkButton)
            {
                ((LinkButton)control).OnClientClick = GetPostBackReference(control.ClientID.Replace('_', '$'), "null", true);
                ((LinkButton)control).OnClientClick += "; return false;";
            }

            foreach (Control child in control.Controls)
                ProcessPostBackControls(child);
        }

        /// <summary>
        /// Gets the internal PostBack command for the hosted control
        /// </summary>        
        private string GetPostBackReference(string eventTarget, string eventArgument, bool enclosed)
        {
            string reference = "";

            if (enclosed)
                reference = string.Format("javascript:setTimeout('$find(\\'{0}\\').doPostBack(\\'{1}\\',\\'{2}\\')',0)",
                                              _controlClientID,
                                              eventTarget,
                                              eventArgument);
            else
                reference = string.Format("$find('{0}').doPostBack('{1}','{2}')",
                                              _controlClientID,
                                              eventTarget,
                                              eventArgument);

            return reference;
        }

        #region ViewState Management

        protected override PageStatePersister PageStatePersister
        {
            get
            {
                if (_persister == null)
                    _persister = new PartialPageStatePersister(this);

                return _persister;
            }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (Request.Form["__CURRENTCULTURE"] != null)
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(Request.Form["__CURRENTCULTURE"]);
            if (Request.Form["__CURRENTUICULTURE"] != null)
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Request.Form["__CURRENTUICULTURE"]);

            // load fake viewstate as it is provided by the "Serverside" initial rendering
            if (Request.Form[_controlClientID + "_VIEWSTATE"] != null)
                _pageViewState = HttpUtility.UrlDecode(Request.Form[_controlClientID + "_VIEWSTATE"]);

            // load viewstate requested by async load
            else if (Request.Form["__VIEWSTATE"] != null)
                _pageViewState = Request.Form["__VIEWSTATE"];

            if (!string.IsNullOrEmpty(_pageViewState))
            {
                _pageViewState = _pageViewState.Replace(' ', '+');
            }
        }

        protected override object LoadPageStateFromPersistenceMedium()
        {
            PartialPageStatePersister persister = PageStatePersister as PartialPageStatePersister;
            persister.PageState = _pageViewState;
            persister.Load();

            return new Pair(persister.ControlState, persister.ViewState);
        }

        protected override void SavePageStateToPersistenceMedium(object state)
        {
            PartialPageStatePersister pageStatePersister = this.PageStatePersister as PartialPageStatePersister;
            if (state is Pair)
            {
                Pair pair = (Pair)state;
                pageStatePersister.ControlState = pair.First;
                pageStatePersister.ViewState = pair.Second;
            }
            else
            {
                pageStatePersister.ViewState = state;
            }
            pageStatePersister.Save();
            
            _pageViewState = HttpUtility.UrlEncode(pageStatePersister.PageState);
        }

        #endregion
    }
}
