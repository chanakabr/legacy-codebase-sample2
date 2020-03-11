using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Objects;
using System.Xml.Linq;
using System.Web.Caching;
using System.Web;


namespace TVPPro.SiteManager.Manager
{
    public class RSSManager
    {
        #region Private members

        private static Cache _cache = HttpContext.Current.Cache;
        private static object _locker = new object();

        #endregion

        #region Functions

        /// <summary>
        /// get feed (cached)
        /// </summary>
        /// <param name="url">the url from which to get feed</param>
        /// <param name="buffer">when feed is obsolete</param>
        /// <returns></returns>
        public static RSSEntity[] GetFeed(string url, TimeSpan buffer)
        {
            return GetFeed(url, buffer, string.Empty);
        }
        /// <summary>
        /// get feed (cached)
        /// </summary>
        /// <param name="url">the url from which to get feed</param>
        /// <param name="buffer">when feed is obsolete</param>
        /// <param name="uniqueElementsParams">unique elements to RSS (params)</param>
        /// <returns></returns>
        public static RSSEntity[] GetFeed(string url, TimeSpan buffer, params string[] uniqueElementsParams)
        {
            RSSEntity[] arrFeeds = (RSSEntity[])_cache[url];
            if (arrFeeds == null)
            {
                lock (_locker)
                {
                    if (arrFeeds == null)
                    {
                        arrFeeds = GetFeedFromUrl(url, uniqueElementsParams);
                        _cache.Add(url, arrFeeds, null, DateTime.Now.Add(buffer), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                    }
                }
            }
            return arrFeeds;
        }


        /// <summary>
        /// Getting RSS feed from the internet
        /// </summary>
        /// <param name="url">the url from which to get feed</param>
        /// <returns></returns>
        private static RSSEntity[] GetFeedFromUrl(string url)
        {
            return GetFeedFromUrl(url, string.Empty);
        }
        private static RSSEntity[] GetFeedFromUrl(string url, params string[] uniqueElementsParams)
        {
            XDocument feedXML = XDocument.Load(url);
            List<RSSEntity> feedList = new List<RSSEntity>();
            Dictionary<string, List<string>> uniqueElementsDic;
            var feeds = from feed in feedXML.Descendants("item")
                        select new
                        {
                            Title = feed.Element("title") == null ? string.Empty : feed.Element("title").Value,
                            Link = feed.Element("link") == null ? string.Empty : feed.Element("link").Value,
                            Description = feed.Element("description") == null ? string.Empty : feed.Element("description").Value,
                            UniqueElementArr = (from element in uniqueElementsParams
                                                select feed.Elements(element))
                        };
            foreach (var feed in feeds)
            {
                uniqueElementsDic = new Dictionary<string, List<string>>();
                string lastKey = string.Empty;
                RSSEntity ent = new RSSEntity()
                {
                    Title = feed.Title,
                    Description = feed.Description,
                    Link = feed.Link
                };
                foreach (var elements in feed.UniqueElementArr)
                {
                    var elementsArr = elements.ToArray();
                    foreach (var item in elementsArr)
                    {
                        if (lastKey != item.Name.LocalName)
                        {
                            uniqueElementsDic.Add(item.Name.LocalName, new List<string>());
                            lastKey = item.Name.LocalName;
                        }
                        uniqueElementsDic[lastKey].Add(item.Value);
                    }
                }

                ent.UniqueEmelementDic = uniqueElementsDic;
                feedList.Add(ent);
            }
            return feedList.ToArray();

        }


        #endregion

    }
}
