using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Catalog.Searchers
{
    public static class Helper
    {
        private static ConcurrentDictionary<string, object> channelFactories = new ConcurrentDictionary<string,object>();
        
        public static T GetFactoryChannel<T>(string address) where T : class
        {
            string key = typeof(T).ToString();
            T channel = null;

            if (!channelFactories.ContainsKey(address))//channel factory not cached
            {
                ChannelFactory<T> factory = new ChannelFactory<T>();
                factory.Endpoint.Address = new EndpointAddress(new System.Uri(address));
                factory.Endpoint.Binding = new BasicHttpBinding();
                channelFactories.TryAdd(key, factory);

            }
            object value;
            if(channelFactories.TryGetValue(key, out value))
            {
                channel = ((ChannelFactory<T>)value).CreateChannel();
                ((IClientChannel)channel).Open();
            }            

            return channel;
        }

        public static void CloseChannel(IClientChannel channel)
        {
            try
            {
                if (channel != null && channel.State != CommunicationState.Closed)
                {
                    channel.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Close channel request", string.Format("exception thrown when closing channel"), "Catalog");
                channel.Abort();
            }
        }

        public static void AbortChannel(IClientChannel channel)
        {
            try
            {
                if (channel != null && channel.State != CommunicationState.Closed)
                {
                    channel.Abort();
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Abort channel request", string.Format("exception thrown when aborting channel"), "Catalog");
                channel.Abort();
            }
        }
    }
}
