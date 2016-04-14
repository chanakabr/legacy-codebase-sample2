using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdapterControllers.CDVR
{
    public class CdvrAdapterController
    {
        #region Consts
        private const int STATUS_OK = 0;
        private const int STATUS_NO_CONFIGURATION_FOUND = 3;
                
        private const string PARAMETER_GROUP_ID = "group_id";
        private const string PARAMETER_ENGINE = "engine";
        #endregion

        #region Static Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Locker for the entire class
        /// </summary>
        private static readonly object generalLocker = new object();

        #endregion

        #region Singleton

        private static CdvrAdapterController instance;

        /// <summary>
        /// Gets the singleton instance of the adapter controller
        /// </summary>     
        /// <returns></returns>
        public static CdvrAdapterController GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new CdvrAdapterController();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor

        private CdvrAdapterController()
        {
            //configurationSynchronizer = new CouchbaseSynchronizer(100);
            //configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

        #endregion

        #region Public Method

        #endregion

        #region Private Method
        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {                
                int groupId = 0;
                CDVRAdapter engine = null;

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    groupId = (int)parameters[PARAMETER_GROUP_ID];
                }

                if (parameters.ContainsKey(PARAMETER_ENGINE))
                {
                    engine = (CDVRAdapter)parameters[PARAMETER_ENGINE];
                }

                // get the right 
                result = this.SendConfiguration(engine, groupId);
            }

            return result;
        }

        public bool SendConfiguration(CDVRAdapter engine, int groupId)
        {
            bool result = false;

            CdvrEngineAdapter.IService client = new CdvrEngineAdapter.ServiceClient();
            client.SetConfiguration(engine.
          
                /*

                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Empty;

                try
                {
                    //call Adapter Transact
                    RecommendationEngineAdapter.AdapterStatus adapterResponse =
                        client.SetConfiguration(engine.ID,
                        engine.Settings != null ? engine.Settings.Select(setting => new RecommendationEngineAdapter.KeyValue()
                        {
                            Key = setting.key,
                            Value = setting.value
                        }).ToArray() : null,
                        groupId,
                        unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(engine.SharedSecret, EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null)
                        log.DebugFormat("Recommendations Engine Adapter Send Configuration Result = {0}", adapterResponse);
                    else
                        log.Debug("Adapter response is null");

                    if (adapterResponse != null && adapterResponse.Code == STATUS_OK)
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed ex = {0}, engine id = {1}", ex, engine != null ? engine.ID : 0);
                }
            }*/

            return result;
        }
        #endregion
    }
}
