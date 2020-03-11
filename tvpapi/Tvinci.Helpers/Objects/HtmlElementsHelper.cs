using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Helpers.Objects
{
    public static class HtmlElementsHelper
    {
        public static string CreateDivElement(bool addCloseTag, string body)
        {
            return CreateElement("div", addCloseTag, body, null, null, null, null);
        }
        public static string CreateDivElement(bool addCloseTag, string body,string Class)
        {
            return CreateElement("div", addCloseTag, body, Class, null, null, null);
        }
        public static string CreateDivElement(bool addCloseTag, string body, string Class, string id)
        {
            return CreateElement("div", addCloseTag, body, Class, id, null, null);
        }
        public static string CreateDivElement(bool addCloseTag, string body, string Class, string id, string onclick)
        {
            return CreateElement("div", addCloseTag, body, Class, id, onclick, null);
        }
        public static string CreateElement(string tagName, bool addCloseTag, string body, string Class, string id, string onclick, string style)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("<{0}", tagName);
            if (!string.IsNullOrEmpty(Class))
                builder.AppendFormat(" class='{0}'", Class);
            if (!string.IsNullOrEmpty(id))
                builder.AppendFormat(" id='{0}'", id);
            if (!string.IsNullOrEmpty(onclick))
                builder.AppendFormat(" onclick='{0}'", onclick);
            if (!string.IsNullOrEmpty(style))
                builder.AppendFormat(" style='{0}'", style);

            builder.Append('>');

            if (!string.IsNullOrEmpty(body))
                builder.Append(body);

            if (addCloseTag)
                builder.AppendFormat("</{0}>",tagName);

            return builder.ToString();
        }

        public static string CreateDtElement(bool addCloseTag, string body)
        {
            return CreateElement("dt",addCloseTag, body, null, null, null, null);
        }
        public static string CreateDtElement(bool addCloseTag, string body, string Class)
        {
            return CreateElement("dt", addCloseTag, body, Class, null, null, null);
        }
        public static string CreateDtElement(bool addCloseTag, string body, string Class, string id)
        {
            return CreateElement("dt", addCloseTag, body, Class, id, null, null);
        }
        public static string CreateDtElement(bool addCloseTag, string body, string Class, string id, string onclick)
        {
            return CreateElement("dt", addCloseTag, body, Class, id, onclick, null);
        }
        
        

    }
}
