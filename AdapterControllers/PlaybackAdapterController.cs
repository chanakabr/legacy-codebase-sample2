using ApiObjects;
using KLogMonitor;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Reflection;
using TVinciShared;

namespace AdapterControllers
{
    public class PlaybackAdapterController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int STATUS_OK = 0;
        private const string PARAMETER_GROUP_ID = "group_id";
        private const string PARAMETER_ADAPTER = "adapter";
        private const string LOCKER_STRING_FORMAT = "Playback_Adapter_Locker_{0}";

        private static PlaybackAdapterController instance;
        private static readonly object generalLocker = new object();


        // Gets the singleton instance of the adapter controller
        public static PlaybackAdapterController GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new PlaybackAdapterController();
                    }
                }
            }

            return instance;
        }

        private CouchbaseSynchronizer configurationSynchronizer;
        public PlaybackAdapterController()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

        public bool SendConfiguration(PlaybackProfile adapter, int partnerId)
        {
            bool result = false;

            if (adapter != null && !string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                PlaybackAdapter.ServiceClient client = new PlaybackAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);

                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Empty;

                try
                {
                    PlaybackAdapter.AdapterStatus adapterResponse =
                        client.SetConfiguration(adapter.Id, adapter.Settings, partnerId, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null)
                        log.DebugFormat("Playback Adapter Send Configuration Result = {0}", adapterResponse);
                    else
                        log.Debug("Adapter response is null");

                    if (adapterResponse != null && adapterResponse.Code == STATUS_OK)
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed ex = {0}, adapter id = {1}", ex, adapter != null ? adapter.Id : 0);
                }
            }

            return result;
        }

        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {
                PlaybackProfile adapter = null;
                int groupId = 0;

                if (parameters.ContainsKey(PARAMETER_ADAPTER))
                {
                    adapter = (PlaybackProfile)parameters[PARAMETER_ADAPTER];
                }

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    groupId = (int)parameters[PARAMETER_GROUP_ID];
                }

                result = this.SendConfiguration(adapter, groupId);
            }

            return result;
        }
    }
}
