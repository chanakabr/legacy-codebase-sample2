using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Web.Caching;
using TVPPro.SiteManager.BaseClass;

namespace TVPPro.SiteManager.Helper
{

    public class BitlyHelper : BaseUrlShortenHelper<BitlyHelper>
    {
        private static readonly string API_KEY = System.Configuration.ConfigurationManager.AppSettings["BitlySecret"];
        private static readonly string LOGIN = System.Configuration.ConfigurationManager.AppSettings["BitlyLogin"];


        public override string ShortenUrl(string longUrl)
        {

            BitlyResults res = (BitlyResults)_cache[longUrl];
            if (res == null)
            {
                lock (_locker)
                {
                    if (res == null)
                    {
                        res = GetShortenLinkFromApi(longUrl);
                        if (string.IsNullOrEmpty(res.ShortUrl))
                            res.ShortUrl = longUrl;
                        else
                            _cache.Add(longUrl, res, null, DateTime.Now.Add(new TimeSpan(365, 0, 0, 0)), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                    }
                }
            }

            return res.ShortUrl;
        }

        private BitlyResults GetShortenLinkFromApi(string longUrl)
        {
            var url =
                string.Format("http://api.bit.ly/shorten?format=xml&version=2.0.1&longUrl={0}&login={1}&apiKey={2}",
                              HttpUtility.UrlEncode(longUrl), LOGIN, API_KEY);
            var resultXml = XDocument.Load(url);
            var x = (from result in resultXml.Descendants("nodeKeyVal")
                     select new BitlyResults
                     {
                         UserHash = result.Element("userHash").Value,
                         ShortUrl = result.Element("shortUrl").Value
                     }
                    );

            try
            {
                return x.Single();
            }
            catch { return new BitlyResults() { ShortUrl = string.Empty, UserHash = string.Empty }; }

        }
    }
    public class BitlyResults
    {
        public string UserHash { get; set; }

        public string ShortUrl { get; set; }
    }
}
