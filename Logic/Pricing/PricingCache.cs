using CachingProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

namespace Core.Pricing
{
    public class PricingCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static internal Dictionary<string, Subscription> TryGetSubscriptions(List<string> keys)
        {
            Dictionary<string, Subscription> subscriptions = null;
            try
            {
                Dictionary<string, object> values = GetValues(keys);
                if (values != null && values.Count > 0)
                {
                    subscriptions = new Dictionary<string, Subscription>();
                    foreach (KeyValuePair<string, object> pair in values)
                    {
                        if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                        {
                            Subscription sub = (Subscription)pair.Value;
                            if (sub != null &&  !string.IsNullOrEmpty(sub.m_sObjectCode) && !subscriptions.ContainsKey(sub.m_sObjectCode))
                            {
                                subscriptions.Add(sub.m_sObjectCode, sub);
                            }
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                if (keys != null && keys.Count > 0)
                {                    
                    foreach (string key in keys)
                    {
                        sb.Append(key + ", ");
                    }
                }
                log.ErrorFormat("Error getting subscriptions from cache, keys: {0}. Exception: {1}", sb.ToString(), ex.Message);
            }

            return subscriptions;
        }

        static internal Dictionary<string, PPVModule> TryGetPPVmodules(List<string> keys)
        {
            Dictionary<string, PPVModule> ppvModules = null;
            try
            {
                Dictionary<string, object> values = GetValues(keys);
                if (values != null && values.Count > 0)
                {
                    ppvModules = new Dictionary<string, PPVModule>();
                    foreach (KeyValuePair<string, object> pair in values)
                    {
                        if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                        {
                            PPVModule ppvModule = (PPVModule)pair.Value;
                            if (ppvModule != null && !string.IsNullOrEmpty(ppvModule.m_sObjectCode) && !ppvModules.ContainsKey(ppvModule.m_sObjectCode))
                            {
                                ppvModules.Add(ppvModule.m_sObjectCode, ppvModule);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                if (keys != null && keys.Count > 0)
                {
                    foreach (string key in keys)
                    {
                        sb.Append(key + ", ");
                    }
                }
                log.ErrorFormat("Error getting PPVmodules from cache, keys: {0}. Exception: {1}", sb.ToString(), ex.Message);
            }

            return ppvModules;
        }

        static internal bool TryGetSubscription(string key, out Subscription sub)
        {
            bool res = false;
            Subscription temp = Get<Subscription>(key);
            if (temp != null)
            {
                sub = temp;
                res = true;
            }
            else
            {
                sub = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddSubscription(string key, Subscription sub)
        {
            return sub != null && Add(key, sub);
        }

        static internal bool TryGetCollection(string key, out Collection coll)
        {
            bool res = false;
            Collection temp = Get<Collection>(key);
            if (temp != null)
            {
                coll = temp;
                res = true;
            }
            else
            {
                coll = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddCollection(string key, Collection coll)
        {
            return coll != null && Add(key, coll);
        }

        static internal bool TryGetPPVModule(string key, out PPVModule ppv)
        {
            bool res = false;
            PPVModule temp = Get<PPVModule>(key);
            if (temp != null)
            {
                ppv = temp;
                res = true;
            }
            else
            {
                ppv = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddPPVModule(string key, PPVModule ppv)
        {
            return ppv != null && Add(key, ppv);
        }

        static internal bool TryGetMediaFilePPVModuleObj(string key, out MediaFilePPVModule mfpm)
        {
            bool res = false;
            MediaFilePPVModule temp = Get<MediaFilePPVModule>(key);
            if (temp != null)
            {
                mfpm = temp;
                res = true;
            }
            else
            {
                mfpm = null;
                res = false;
            }

            return res;
        }

        static internal bool TryGetUsageModule(string key, out UsageModule um)
        {
            bool res = false;
            UsageModule temp = Get<UsageModule>(key);
            if (temp != null)
            {
                um = temp;
                res = true;
            }
            else
            {
                um = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddUsageModule(string key, UsageModule um)
        {
            return um != null && Add(key, um);
        }

        static internal bool TryAddMediaFilePPVModuleObj(string key, MediaFilePPVModule mfpm)
        {
            return mfpm != null && Add(key, mfpm);
        }

        static internal bool TryGetPrePaidModule(string key, out PrePaidModule ppm)
        {
            bool res = false;
            PrePaidModule temp = Get<PrePaidModule>(key);
            if (temp != null)
            {
                res = true;
                ppm = temp;
            }
            else
            {
                res = false;
                ppm = null;
            }

            return res;
        }

        static internal bool TryAddPrePaidModule(string key, PrePaidModule ppm)
        {
            return ppm != null && Add(key, ppm);
        }

        static internal bool TryGetPriceCode(string key, out PriceCode pc)
        {
            bool res = false;
            PriceCode temp = Get<PriceCode>(key);
            if (temp != null)
            {
                pc = temp;
                res = true;
            }
            else
            {
                pc = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddPriceCode(string key, PriceCode pc)
        {
            return pc != null && Add(key, pc);
        }

        static internal bool TryGetMediaFilePPVContainer(string key, out MediaFilePPVContainer mfpc)
        {
            bool res = false;
            MediaFilePPVContainer temp = Get<MediaFilePPVContainer>(key);
            if (temp != null)
            {
                mfpc = temp;
                res = true;
            }
            else
            {
                mfpc = null;
                res = false;
            }

            return res;
        }

        static internal bool TryAddMediaFilePPVContainer(string key, MediaFilePPVContainer mfpc)
        {
            return mfpc != null && Add(key, mfpc);
        }


        static internal void LogCachingError(string msg, string key, object obj, string methodName, string logFile)
        {
            StringBuilder sb = new StringBuilder(msg);
            sb.Append(String.Concat(" Key: ", key));
            sb.Append(String.Concat(" Val: ", obj != null ? obj.ToString() : "null"));
            sb.Append(String.Concat(" Method Name: ", methodName));
            //sb.Append(String.Concat(" Cache Data: ", ToString()));
            log.Error("CacheError - "+ sb.ToString());
        }

        /*
        public override string ToString()
        {
            return cache.ToString();
        }
        */

        private static bool Add(string key, object obj)
        {
            return TvinciCache.WSCache.Instance.Add(key, obj);
        }

        private static T Get<T>(string key)
        {
            return TvinciCache.WSCache.Instance.Get<T>(key);
        }

        private static Dictionary<string, object> GetValues (List<string> keys)
        {
            Dictionary<string, object> values = null;
            if (keys != null && keys.Count > 0)
            {
                values = TvinciCache.WSCache.Instance.GetValues(keys) as Dictionary<string, object>;
            }

            return values;
        }
    }
}
