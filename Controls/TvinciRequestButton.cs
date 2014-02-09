using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls
{
    public class TvinciRequestButton : HyperLink
    {
        public string Action { get; set; }
        public string Parameters { get; set; }
        public string HandlerName { get; set; }
        public string CallBack { get; set; }
        public string CustomID { get; set; }
        public string BeforeSend { get; set; }

        public override string ClientID
        {
            get
            {
                return CustomID ?? base.ClientID;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (string.IsNullOrEmpty(CallBack) || string.IsNullOrEmpty(HandlerName))
                return;

            Attributes.Add("onClick",
                           string.Format("{0};{1}TvinciRequestButtonSend('{2}','{3}','{4}')", BeforeSend, CallBack, HandlerName, Action, Parameters));

            if (!Page.ClientScript.IsClientScriptBlockRegistered(CallBack + "_TvinciRequestButtonFunction"))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof (TvinciRequestButton),
                                                            CallBack + "_TvinciRequestButtonFunction",
                                                            string.Format(
                                                                @"
                        var {0}TvinciRequestButtonXhttp;

                        function {0}TvinciRequestButtonSend(ClientHandler, ActionName, Parameters)
                        {{
                          {0}TvinciRequestButtonXhttp = createXMLHTTP(ClientHandler + "".axd"" + ""?requestType="" + ActionName + ""&Parameters="" + Parameters, true, ""post"");
                          {0}TvinciRequestButtonXhttp.onreadystatechange = {0}TvinciRequestButtonStateChange;
                          {0}TvinciRequestButtonXhttp.send(null);
                        }}

                        function {0}TvinciRequestButtonStateChange()
                        {{
                            if ({0}TvinciRequestButtonXhttp.readyState==4 && {0}TvinciRequestButtonXhttp.status == 200)
                            {{
                                if(typeof {0} != 'function') 
                                {{
                                    return; 
                                }}

                                if ({0}TvinciRequestButtonXhttp.responseText)
                                {{
                                    var resArray = {0}TvinciRequestButtonXhttp.responseText.split("";"");
                                    if (resArray.length != 2)
                                    {{
                                        {0}(true, null);
                                        return;
                                    }}
                                    else
                                    {{
                                        if (resArray[0] == ""Success"")
                                        {{
                                            {0}(false, resArray[1]);
                                            return;
                                        }}
                                        else
                                        {{
                                            {0}(true, resArray[1]);
                                            return;
                                        }}
                                    }}
                                }}
                            }}
                        }}",
                    CallBack), true);
            }

            base.OnPreRender(e);
        }
    }
}
