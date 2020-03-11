using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Helpers.Link
{
    public enum eValueType
    {
        Token,
        Link
    }

    public sealed class MappingPair
    {
        public string Token { get; set; }
        public string Link { get; set; }
    }

    public interface IMappingAdapter
    {                
        MappingPair ExtractMapping(string value, eValueType type);        
    }

    public class MappingManager     
    {
        public static MappingManager Instance { get; private set; }
        Dictionary<string, List<IMappingAdapter>> m_clientAdapters = new Dictionary<string, List<IMappingAdapter>>();
        List<IMappingAdapter> m_defaultAdapters = new List<IMappingAdapter>();

        static MappingManager()
        {
            Instance = new MappingManager();            
        }

        public void AddAdapter(params IMappingAdapter[] adapters)
        {
            m_defaultAdapters.AddRange(adapters);            
        }

        public void AddAdapter(string client, params IMappingAdapter[] adapters)
        {            
            if (!string.IsNullOrEmpty(client))
            {
                List<IMappingAdapter> clientAdapters;
                if (!m_clientAdapters.TryGetValue(client, out clientAdapters))
                {
                    clientAdapters = new List<IMappingAdapter>();
                    m_clientAdapters.Add(client,clientAdapters);
                }

                clientAdapters.AddRange(adapters);
            }else
            {
                throw new ArgumentNullException("client");
            }            
        }

        public string GetLink(string client, string token)
        {
            MappingPair pair = getPair(client, token, eValueType.Token);

            if (pair != null)
            {
                return LinkHelper.ParseURL(pair.Link);
            }

            return string.Empty;
        }

        public string getToken(string client, string link)
        {
            MappingPair pair = getPair(client, link, eValueType.Link);

            if (pair != null)
            {
                return pair.Token;
            }

            return string.Empty;
        }

        private MappingPair getPair(string client, string value, eValueType valueType)        
        {
            List<IMappingAdapter> adapters = new List<IMappingAdapter>();
            if (m_clientAdapters.TryGetValue(client, out adapters))
            {
                foreach(IMappingAdapter adapter in adapters)
                {
                    MappingPair pair = adapter.ExtractMapping(value, valueType);

                    if (pair != null)
                    {
                        return pair;
                    }
                }

                foreach (IMappingAdapter adapter in m_defaultAdapters)
                {
                    MappingPair pair = adapter.ExtractMapping(value, valueType);

                    if (pair != null)
                    {
                        return pair;
                    }
                }
            }

            return null;
        }
    }
}
