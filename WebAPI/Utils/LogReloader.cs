using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using CouchbaseManager;
using KLogMonitor;
using KlogMonitorHelper;
using ConfigurationManager;
using System.Threading;
using System.Reflection;
using Newtonsoft.Json;

namespace WebAPI.Utils
{
    public class LogReloader
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Members

        private BackgroundWorker worker;
        private int interval;
        private CouchbaseManager.CouchbaseManager couchbaseManager;
        private KLoggerConfiguration configuration;

        #endregion

        #region Singleton

        /// <summary>
        /// Locker for the entire class
        /// </summary>
        private static readonly object generalLocker = new object();

        private static LogReloader instance;

        /// <summary>
        /// Gets the singleton instance of the adapter controller
        /// </summary>     
        /// <returns></returns>
        public static LogReloader GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new LogReloader();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Main Methods

        public void Initiate()
        {
            interval = ApplicationConfiguration.LogReloadInterval.IntValue;

            if (interval > 0)
            {
                couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

                string configurationXml = KLogger.GetConfigurationXML();
                configuration = new KLoggerConfiguration()
                {
                    configuration = configurationXml,
                    timeStamp = TVinciShared.DateUtils.GetUtcUnixTimestampNow()
                };

                var cbConfiguration = couchbaseManager.Get<KLoggerConfiguration>("log4net.config");

                if (cbConfiguration == null)
                {
                    couchbaseManager.Set("log4net.config", configuration);
                }

                worker = new BackgroundWorker();
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerAsync();
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Timer timer = new Timer((x) =>
            {
                Reload();
            },
            null, interval, interval);
        }

        public void Reload()
        {
            try
            {
                var cbConfiguration = couchbaseManager.Get<KLoggerConfiguration>("log4net.config");

                // if configuration was updated
                if (cbConfiguration != null && (configuration == null || cbConfiguration.timeStamp > configuration.timeStamp))
                {
                    log.Debug($"Reconfiguring logger with config from Couchbase.");
                    KLogger.Reconfigure(cbConfiguration.configuration);

                    configuration = cbConfiguration;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed reconfiguring logger with config from Couchbase. ex={ex}");
            }
        } 

        #endregion
    }

    [JsonObject()]
    public class KLoggerConfiguration
    {
        [JsonProperty()]
        public long timeStamp;
        [JsonProperty()]
        public string configuration;
    }
}
