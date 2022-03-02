using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace WebAPI.Models.General
{
    public class KalturaSerializable : IKalturaSerializable
    {
        private static Dictionary<string, string> escapeMapping = new Dictionary<string, string>()
        {
            {"\"", @"\"""},
            {"\\\\", @"\\"},
            {"\a", @"\a"},
            {"\b", @"\b"},
            {"\f", @"\f"},
            {"\n", @"\n"},
            {"\r", @"\r"},
            {"\t", @"\t"},
            {"\v", @"\v"},
            {"\0", @"\0"},
        };
        private static Regex escapeRegex = new Regex(string.Join("|", escapeMapping.Keys.ToArray()));

        public string EscapeJson(string str)
        {
            return escapeRegex.Replace(str, EscapeMatchEval);
        }

        private static string EscapeMatchEval(Match m)
        {
            if (escapeMapping.ContainsKey(m.Value))
            {
                return escapeMapping[m.Value];
            }
            return escapeMapping[Regex.Escape(m.Value)];
        }

        public string EscapeXml(string str)
        {
            return WebUtility.HtmlEncode(str);
        }

        public virtual string ToJson(Version currentVersion, bool omitObsolete, bool responseProfile = false)
        {
            return "{" + String.Join(", ", PropertiesToJson(currentVersion, omitObsolete, responseProfile).Values) + "}";
        }

        public virtual string ToXml(Version currentVersion, bool omitObsolete, bool responseProfile = false)
        {
            return String.Join("", PropertiesToXml(currentVersion, omitObsolete, responseProfile).Values);
        }

        protected virtual Dictionary<string, string> PropertiesToXml(Version currentVersion, bool omitObsolete, bool responseProfile = false)
        {
            return new Dictionary<string, string>();
        }

        protected virtual Dictionary<string, string> PropertiesToJson(Version currentVersion, bool omitObsolete, bool responseProfile = false)
        {
            return new Dictionary<string, string>();
        }

    }
}
