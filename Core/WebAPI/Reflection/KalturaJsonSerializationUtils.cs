using System.Net;
using System.Text;

namespace WebAPI.Reflection
{
    public static class KalturaJsonSerializationUtils
    {
        public static StringBuilder AppendEscapedJsonString(this StringBuilder stringBuilder, string value)
        {
            stringBuilder.Append("\"");
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\"':
                        stringBuilder.Append("\\\"");
                        break;
                    case '\\':
                        stringBuilder.Append(@"\\");
                        break;
                    case '\0':
                        stringBuilder.Append(@"\0");
                        break;
                    case '\a':
                        stringBuilder.Append(@"\a");
                        break;
                    case '\b':
                        stringBuilder.Append(@"\b");
                        break;
                    case '\f':
                        stringBuilder.Append(@"\f");
                        break;
                    case '\n':
                        stringBuilder.Append(@"\n");
                        break;
                    case '\r':
                        stringBuilder.Append(@"\r");
                        break;
                    case '\t':
                        stringBuilder.Append(@"\t");
                        break;
                    case '\v':
                        stringBuilder.Append(@"\v");
                        break;
                    default:
                        stringBuilder.Append(c);
                        break;
                }
            }

            stringBuilder.Append("\"");

            return stringBuilder;
        }

        public static StringBuilder AppendEscapedXmlString(this StringBuilder stringBuilder, string value)
        {
            stringBuilder.Append(WebUtility.HtmlEncode(value));

            return stringBuilder;
        }
    }
}