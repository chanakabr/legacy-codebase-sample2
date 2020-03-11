using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using Tvinci.Helpers.Link.Configuration;

namespace Tvinci.Web.HttpModules.Configuration
{
    public class QueryConfigManager : ConfigurationManager<QueryStringConfig>
    {
        public static QueryConfigManager Instance = new QueryConfigManager();

        Dictionary<string, string> m_globalList = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, string>> m_pageList = new Dictionary<string, Dictionary<string, string>>();
        private List<string> m_IgnoredPages = new List<string>();

        public bool IsPageIgnored(string page)
        {
            return m_IgnoredPages.Contains(page.ToLower());
        }

        public bool TryGetRule(string page, string key, out string rule)
        {
            Dictionary<string, string> list;
            if (m_pageList.TryGetValue(page.ToLower(), out list))
            {
                if (list.TryGetValue("*", out rule))
                {
                    return true;
                }
                else
                {
                    return list.TryGetValue(key.ToLower(), out rule);
                }
            }
            else
            {
                rule = string.Empty;
                return false;
            }
        }

        public bool TryGetRule(string key, out string rule)
        {
            return m_globalList.TryGetValue(key.ToLower(), out rule);
        }

        private QueryConfigManager()
        {
            // no implementation needed by design
            base.DataModified = dataModified;
        }

        public void Initialize(QueryStringConfig configuration)
        {
            base.SyncFromConfiguration(configuration);
        }

        public void Initialize(string virtualPath)
        {            
            base.SyncFromFile(virtualPath,true);
        }

        void dataModified(QueryStringConfig data)
        {
            m_globalList = new Dictionary<string, string>();
            m_pageList = new Dictionary<string, Dictionary<string, string>>();
            m_IgnoredPages = new List<string>();

            foreach (QueryItem item in data.ValidQueryList.QueryItems.GlobalScope)
            {
                m_globalList[item.Key.ToLower()] =  item.Rule;
            }

            foreach (QueryItem item in data.Base64.BypassBase64.QueryItems.GlobalScope)
            {
                m_globalList[item.Key.ToLower()] = item.Rule;
            }

            foreach (PageItem page in data.ValidQueryList.QueryItems.PageScope)
            {
                Dictionary<string, string> pageItems = new Dictionary<string, string>();

                foreach (QueryItem pageItem in page)
                {
                    pageItems[pageItem.Key.ToLower()] =  pageItem.Rule;
                }

                m_pageList.Add(page.PagePath.ToLower(), pageItems);
            }

            foreach (PageItem page in data.Base64.BypassBase64.QueryItems.PageScope)
            {
                Dictionary<string,string> pageItems = new Dictionary<string,string>();
                
                foreach(QueryItem pageItem in page)
                {
                    pageItems[pageItem.Key.ToLower()] = pageItem.Rule;
                }

                m_pageList.Add(page.PagePath.ToLower(), pageItems);

                if (page.Ignore)
                    m_IgnoredPages.Add(page.PagePath.ToLower());
            }
        }

        public static bool Base64Mode
        {
            get
            {
                return Instance.Data.General.Base64Mode;
            }
        }

    }
}
