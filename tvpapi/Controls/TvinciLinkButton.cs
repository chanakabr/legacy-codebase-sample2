using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Tvinci.Web.Controls
{
    public class TvinciLinkButton : LinkButton
    {
        protected override void AddAttributesToRender(System.Web.UI.HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            return;
            //if (this.Page != null)
            //{
            //    this.Page.VerifyRenderingInServerForm(this);
            //}

            //string onClick = EnsureEndWithSemiColon(this.OnClientClick);
            
            //if (base.HasAttributes)
            //{
            //    string str2 = base.Attributes["onclick"];
            //    if (str2 != null)
            //    {
            //        onClick = onClick + EnsureEndWithSemiColon(str2);
            //        base.Attributes.Remove("onclick");
            //    }
            //}
           
            //bool isEnabled = base.IsEnabled;
            //if (this.Enabled && !isEnabled)
            //{
            //    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            //}
            
            //if (isEnabled && (this.Page != null))
            //{
            //    PostBackOptions postBackOptions = this.GetPostBackOptions();
            //    string postBackEventReference = null;
            //    if (postBackOptions != null)
            //    {
            //        postBackEventReference = this.Page.ClientScript.GetPostBackEventReference(postBackOptions, true);
            //    }

            //    writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");

            //    if (string.IsNullOrEmpty(postBackEventReference))
            //    {
            //        //postBackEventReference = "javascript:void(0)";
            //        postBackEventReference = "";
            //    }
            //    else
            //    {
            //        if (postBackEventReference.StartsWith("javascript:"))
            //        {
            //            postBackEventReference = postBackEventReference.Remove(0, 11);
            //        }

            //        onClick = string.Format("{0}{1}",onClick,EnsureEndWithSemiColon(postBackEventReference));
            //    }
            //}

            //if (onClick.Length > 0)
            //{
            //    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
            //}

            //base.AddAttributesToRender(writer);
        }

        private static string EnsureEndWithSemiColon(string value)
        {
            if (value != null)
            {
                int length = value.Length;
                if ((length > 0) && (value[length - 1] != ';'))
                {
                    return (value + ";");
                }
            }
            return value;
        }


    }
}
