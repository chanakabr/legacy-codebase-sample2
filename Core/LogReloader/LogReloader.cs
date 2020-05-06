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

namespace KLogMonitor.ConfigurationReloader
{
    public class LogReloader
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Members

        private BackgroundWorker worker;
        private int interval;
        private CouchbaseManager.CouchbaseManager couchbaseManager;
        private KLoggerConfiguration configuration;

        public string DocumentKey { get; set; }
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

        public void Initiate(string documentKey)
        {
            interval = ApplicationConfiguration.Current.LogReloadInterval.Value;

            if (interval > 0)
            {
                log.Info($"Initiaiting log reloading mechanism with interval of {interval}.");
                this.DocumentKey = documentKey;
                couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

                string configurationXml = KLogger.GetConfigurationXML();
                configuration = new KLoggerConfiguration()
                {
                    configuration = configurationXml,
                    timeStamp = 0
                };

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
            // start immediately
            null, 0, interval);
        }

        public void Reload()
        {
            try
            {
                log.Info("Log reloading mechanism - going to get document from couchbase.");
                var cbConfiguration = couchbaseManager.Get<KLoggerConfiguration>(this.DocumentKey);

                // if configuration was updated
                if (cbConfiguration != null && (configuration == null || cbConfiguration.timeStamp > configuration.timeStamp))
                {
                    log.Info($"Reconfiguring logger with config from Couchbase. Configuration = {cbConfiguration.configuration}");
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
