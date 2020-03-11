using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

namespace Tvinci.Web.Controls
{
    #region JavaFunctionManager
    [ParseChildren(true)]
    [PersistChildren(false)]
    public class JavaFunctionManager : PlaceHolder
    {
        private const string itemKey = "JavaFunctionManager";

        private class JavaFunction
        {
            public string Name { get; set; }
            public string Args { get; set; }
            public int Counter { get; set; }
            public StringBuilder Content { get; set; }

            public JavaFunction()
            {
                Content = new StringBuilder();
                Counter = 0;
            }
        }

        protected override void OnInit(EventArgs e)
        {
			if (HttpContext.Current.Items[itemKey] == null)
				HttpContext.Current.Items[itemKey] = this;
			//else
			//    throw new Exception("A control of type 'JavaFunctionManager' was already created. cannot have multiple controls on page of this type");

            base.OnInit(e);
        }

        public static bool TryGetManager(out JavaFunctionManager control)
        {
            control = null;
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                control = HttpContext.Current.Items[itemKey] as JavaFunctionManager;
                if (control != null)
                {
                    return true;
                }
            }

            return false;
        }

        Dictionary<string, JavaFunction> m_functions = new Dictionary<string, JavaFunction>();
        protected override void Render(HtmlTextWriter writer)
        {
            // backward competability
            JavaPageloadControl control;
            if (JavaPageloadControl.TryGetPageControl(out control))
            {
                this.RegisterFunction("pageLoad", string.Empty, control.GetContent());
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script type='text/javascript'>");

            foreach (JavaFunction item in m_functions.Values)
            {
                sb.AppendLine(string.Concat("function ", item.Name, "(", item.Args, ")"));
                sb.AppendLine("{");
                sb.AppendLine(string.Concat("\t", item.Content.ToString()));
                for (int i = 1; i <= item.Counter; i++)
                {
                    sb.AppendLine(string.Concat("\t", item.Name, "_", i, "();"));
                }

                sb.AppendLine("}");
                sb.AppendLine();
            }
            sb.AppendLine("</script>");

            writer.Write(sb.ToString());
            base.Render(writer);
        }

        internal void RegisterFunction(string name, string args, string content)
        {
            string key = string.Concat(name, "|", args);
            JavaFunction function;

            if (!m_functions.TryGetValue(key, out function))
            {
                function = new JavaFunction() { Name = name, Args = args };
                m_functions.Add(key, function);
            }

            function.Content.AppendLine(content);
        }

        internal void RegisterFunction(string name, string args, out string newName)
        {
            string key = string.Concat(name, "|", args);
            JavaFunction function;

            if (!m_functions.TryGetValue(key, out function))
            {
                function = new JavaFunction() { Name = name, Args = args };
                m_functions.Add(key, function);
            }

            function.Counter++;
            newName = string.Concat(name, "_", function.Counter);
            return;
        }
    } 
    #endregion

    #region JavaFunctionWrapper
    [ParseChildren(true)]
    [PersistChildren(false)]
    public class JavaFunctionWrapper : PlaceHolder
    {
        public static void RegisterFunction(string functionName, string functionArgs, string functionContent)
        {
            JavaFunctionManager manager;
            if (JavaFunctionManager.TryGetManager(out manager))
            {
                manager.RegisterFunction(functionName, functionArgs, functionContent);
            }
            else
            {
                throw new Exception("Failed to find java function manager (did you remember to add the control to the page?)");
            }
        }

        private class JavaFunction
        {
            public string Name { get; set; }
            public string Args { get; set; }
        }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string Content { get; set; }

        private string m_modifiedContent = string.Empty;
        protected override void OnPreRender(EventArgs e)
        {
            m_modifiedContent = registerFunction(Content);
            base.OnPreRender(e);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            if (!string.IsNullOrEmpty(m_modifiedContent))
            {
                writer.Write(m_modifiedContent);
            }

            base.Render(writer);
        }

        private string registerFunction(string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                return string.Empty;
            }

            JavaFunctionManager manager;
            if (JavaFunctionManager.TryGetManager(out manager))
            {
                List<JavaFunction> functions = new List<JavaFunction>();

                Content = Regex.Replace(Content, "function (?<name>.*?)[(](?<args>.*?)[)]", delegate(Match match)
                {
                    string name = match.Groups[1].Value;
                    string args = match.Groups[2].Value;
                    functions.Add(new JavaFunction() { Name = name, Args = args });

                    manager.RegisterFunction(name, args, out name);

                    return string.Concat("function ", name, "(", args, ")");
                });

                return Content;
            }

            //throw new Exception("Failed to find java function manager (did you remember to add the control to the page?)");
            return string.Empty;
        }
    } 
    #endregion

    #region JavaPageloadControl
    public class JavaPageloadControl : Control
    {
        private const string itemKey = "JavaPageloadHandler";
        public static bool TryGetPageControl(out JavaPageloadControl control)
        {
            control = null;
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                control = HttpContext.Current.Items[itemKey] as JavaPageloadControl;
                if (control != null)
                {
                    return true;
                }
            }

            return false;

        }

        internal static void RegisterControlToRequest(JavaPageloadControl control)
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                HttpContext.Current.Items[itemKey] = control;
            }
            else
            {
                throw new Exception("Cannot find relevent request to add the control to");
            }
        }

        protected override void OnInit(EventArgs e)
        {
            JavaPageloadControl.RegisterControlToRequest(this);
            base.OnInit(e);
        }

        public class PageLoadContext
        {
            public string Content { get; set; }
            public eExecuteOn ExecuteOn { get; set; }

            public PageLoadContext(string content, eExecuteOn executeOn)
            {
                Content = content;
                ExecuteOn = executeOn;
            }
        }
        private Dictionary<string, PageLoadContext> m_methodContent = new Dictionary<string, PageLoadContext>();

        public bool IsContentRegistered(string key)
        {
            return m_methodContent.ContainsKey(key);
        }

        [Obsolete]
        public void RegisterContent(string key, string content)
        {
            RegisterContent(key, content, eExecuteOn.Always);
        }

        public enum eExecuteOn
        {
            Always,
            PostBack,
            NewPage
        }

        public void RegisterContent(string key, string content, eExecuteOn executeOn)
        {
            m_methodContent[key] = new PageLoadContext(content, executeOn);
        }

        internal string GetContent()
        {
            if (m_methodContent.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (PageLoadContext item in m_methodContent.Values)
                {
                    switch (item.ExecuteOn)
                    {
                        case eExecuteOn.Always:
                            sb.AppendLine(item.Content);
                            break;
                        case eExecuteOn.PostBack:
                            sb.AppendFormat("if (isPostBack) {{{0}}}", item.Content);
                            sb.AppendLine();
                            break;
                        case eExecuteOn.NewPage:
                            sb.AppendFormat("if (!isPostBack) {{{0}}}", item.Content);
                            sb.AppendLine();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }

                sb.AppendLine("isPostBack = true;");
                return sb.ToString();
            }

            return string.Empty;
        }
        protected override void Render(HtmlTextWriter writer)
        {
            string result = GetContent();

            if (!string.IsNullOrEmpty(result))
            {
                writer.WriteLine(@"<script type=""text/javascript"">var isPostBack = false;</script>");
            }
            base.Render(writer);
        }
    } 
    #endregion
}
