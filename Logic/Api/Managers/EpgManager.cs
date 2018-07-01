using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace APILogic.Api.Managers
{
    public class EpgManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static string GetEpgChannelId(int mediaId, int groupId)
        {
            string allLinearMediaIdsKey = LayeredCacheKeys.GetAllLinearMediaKey(groupId);
            //TODO SHIR - FIND RELEVENT INVALIDATIONS KEYS - ask ira (LayeredCacheKeys.GetDeviceConcurrencyPriorityInvalidationKey(groupId);)
            string invalidationKey = null;
            Dictionary<long, string> allLinearMedia = null;

            if (!LayeredCache.Instance.Get<Dictionary<long, string>>(allLinearMediaIdsKey,
                                                                    ref allLinearMedia,
                                                                    GetAllLinearMedia,
                                                                    new Dictionary<string, object>() { { "groupId", groupId } },
                                                                    groupId,
                                                                    LayeredCacheConfigNames.GET_ALL_LINEAR_MEDIA,
                                                                    new List<string>() { invalidationKey }))
            {
                log.ErrorFormat("GetEpgChannelId - GetAllLinearMedia - Failed get data from cache. groupId: {0}", groupId);
            }

            long lMediaId = long.Parse(mediaId.ToString());
            if (allLinearMedia != null && allLinearMedia.ContainsKey(lMediaId))
            {
                return allLinearMedia[lMediaId];
            }

            return null;
        }

        private static Tuple<Dictionary<long, string>, bool> GetAllLinearMedia(Dictionary<string, object> funcParams)
        {
            Dictionary<long, string> allLinearMedia = null;
            bool res = true;
            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;

                        if (groupId.HasValue)
                        {
                            DataTable dtAllLinearMedia = EpgDal.GetAllLinearMedia(groupId.Value);

                            if (dtAllLinearMedia != null && dtAllLinearMedia.Rows != null)
                            {
                                allLinearMedia = new Dictionary<long, string>();

                                foreach (DataRow drLinearMedia in dtAllLinearMedia.Rows)
                                {
                                    long linearMediaId = ODBCWrapper.Utils.GetLongSafeVal(drLinearMedia, "ID");
                                    string channelId = ODBCWrapper.Utils.GetSafeStr(drLinearMedia, "EPG_IDENTIFIER");

                                    allLinearMedia.Add(linearMediaId, channelId);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res = false;
                log.Error(string.Format("GetAllLinearMedia faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, string>, bool>(allLinearMedia, res);
        }
    }
}
