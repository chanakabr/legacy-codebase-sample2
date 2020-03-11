using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using KLogMonitor;
using Tvinci.Web.HttpModules.Configuration;

namespace Tvinci.Helpers
{
    public class QueryStringCollection : Dictionary<string,QueryStringPair>
    {
        [Flags]
        public enum eMode
        {            
            Default = 0,
            None = 2,
            All = 4,
            Base64 = 8
        }

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public QueryStringCollection() :base(new CompareCaseInSensitive())
        {            
            // no implementation by design
        }

        public string GenerateQuery(string url, eMode actualMode)
        {                                 
            StringBuilder base64 = new StringBuilder();
            StringBuilder clean = new StringBuilder();

            IOrderedEnumerable<KeyValuePair<string, QueryStringPair>> orderedList = this.OrderBy(item => item.Value.Key);

            foreach (KeyValuePair<string, QueryStringPair> item in orderedList)
            {
                if (string.IsNullOrEmpty(item.Value.Key) || string.IsNullOrEmpty(item.Value.Value))
                {
                    continue;
                }

                if (item.Value.ItemType == eItemType.Base64)
                {
                    if (base64.Length != 0)
                    {
                        base64.Append("&");
                    }

                    base64.Append(string.Concat(item.Value.Key, "=", HttpUtility.UrlEncode(item.Value.Value)));
                }
                else
                {
                    if (clean.Length != 0)
                    {
                        clean.Append("&");
                    }

                    clean.Append(string.Concat(item.Value.Key, "=", HttpUtility.UrlEncode(item.Value.Value)));
                }
            }

            if ((actualMode & eMode.All) == eMode.All)
            {
                actualMode = eMode.All;
            }
            else if ((actualMode & eMode.None) == eMode.None)
            {
                actualMode = eMode.None;
            }
            else if ((actualMode & eMode.Default) == eMode.Default)
            {
                actualMode = eMode.None;
                if (LinkHelper.IsBaseOfApplication(url))
                {
                    if (QueryConfigManager.Base64Mode)
                    {
                        actualMode |= eMode.Base64;
                    }
                }
            }

            // build targeted querystring
            string query;

            //logger.DebugFormat("clean '{0}' | base '{1}' | {2} | {3}", clean.ToString(), base64.ToString(), shouldHandleSpecialTypes, QueryConfigManager.Base64Mode);

            if (actualMode == eMode.All || (actualMode & eMode.Base64) == eMode.Base64)            
            {
                query = QueryStringHelper.EncryptString(base64.ToString());
            }
            else
            {
                query = base64.ToString();
            }
            

            if (clean.Length != 0)
            {
                if (string.IsNullOrEmpty(query))
                {
                    query += clean.ToString();
                }
                else
                {
                    query += string.Concat("&", clean.ToString());
                }
            }

            return query;
        }

        public string GenerateLink(string url)
        {
            return GenerateLink(url, eMode.Default);
        }

        public string GenerateLink(string url, eMode customHandleMode)
        {
            url = LinkHelper.GetLinkWithoutQuerystring(url);

            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }


            string query = GenerateQuery(url, customHandleMode);
                        
            if (!string.IsNullOrEmpty(query))
            {
                return string.Concat(url, "?", (query));
            }
            else
            {
                return url;
            }
        }
    }

    public enum eItemType
    {
        Clear,
        Base64
    }

    public class QueryStringPair
    {        
        

        #region Constructor
        public QueryStringPair(string key)
            : this(key, string.Empty)
        {
            
        }

        public QueryStringPair(string key, string value)
            : this(key, value, eItemType.Clear, true)
        {               
        }

        public QueryStringPair(string key, string value, eItemType itemType)
            : this(key, value, itemType, true)
        {
        }

        public QueryStringPair(string key, string value, eItemType itemType, bool retrieveCurrentIfEmptyValue)
        {
            if (string.IsNullOrEmpty(value) && retrieveCurrentIfEmptyValue)
            {
                string prevValue;
                if (QueryStringHelper.TryGetString(key, out prevValue))
                {
                    value = prevValue;
                }
            }

            ItemType = itemType;
            Key = key;
            Value = value;
        }
        #endregion

        #region Static Methods
        public static implicit operator QueryStringPair(string value)
        {
            string[] temp = value.Split(';');

            if (temp.Length == 1)
            {
                return new QueryStringPair(temp[0]);
            }
            else if (temp.Length > 1)
            {
                return new QueryStringPair(temp[0], temp[1]);
            }
            else
            {
                throw new Exception("Cannot cast implicitly from string.empty or");
            }
        }
        #endregion

        #region Properties

        public eItemType ItemType { get; set; }
        public string Key { get; set; }        
        public string Value { get; set; }
        #endregion
    }
}
