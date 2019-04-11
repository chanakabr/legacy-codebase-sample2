using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Reflection;
using KLogMonitor;

namespace TCMClient
{
    public class Settings
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly object locker = new object();
        private static Settings _Instance;
        public static Settings Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (locker)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new Settings();
                        }
                    }
                }
                return _Instance;
            }
        }

        private JObject m_Settings = new JObject();
        
        private string m_URL;
        private string m_Application;
        private string m_Host;
        private string m_Environment;
        private string m_AppID;
        private string m_AppSecret;
        private bool m_VerifySSL;
        private string m_LocalPath;

        #region Constructors

        private Settings()
        {

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes settings with data from local / remote source according to config
        /// </summary>
        /// <param name="fromLocal">determines the source</param>
        public void Init(bool? fromLocal = null)
        {
            TCMConfiguration config = (TCMConfiguration)ConfigurationManager.GetSection("TCMConfig");
            if (fromLocal.HasValue)
            {
                config.FromLocal = fromLocal.Value;
            }
            Init(config);
        }

        /// <summary>
        /// Initializes settings with data from local / remote source according to config
        /// </summary>
        /// <param name="config">all configuration properties in one object</param>
        public void Init(TCMConfiguration config)
        {
            config.OverrideEnvironmentVariable();
            if (config.FromLocal)
            {
                m_LocalPath = config.LocalPath;

                //Populate settings from local
                PopulateSettings(true);
            }
            else
            {
                //Populate settings from remote
                Init(config.URL, config.Application, config.Host, config.Environment, config.AppID, config.AppSecret, config.VerifySSL, config.LocalPath);
            }
        }

        /// <summary>
        /// Initializes settings with data from remote source 
        /// </summary>
        /// <param name="url">Remote Server URL</param>
        /// <param name="application">Application</param>
        /// <param name="host">Host</param>
        /// <param name="environment">Environment</param>
        /// <param name="appID">App ID</param>
        /// <param name="appSecret">App Secret</param>
        /// <param name="localPath">Local Path</param>
        [Obsolete]
        public void Init(string url, string application, string host, string environment, string appID, string appSecret, string localPath = null)
        {
            m_URL = url;
            m_Application = application;
            m_Host = host;
            m_Environment = environment;
            m_AppID = appID;
            m_AppSecret = appSecret;
            m_LocalPath = localPath;

            PopulateSettings(false);
        }

        /// <summary>
        /// Initializes settings with data from remote source 
        /// </summary>
        /// <param name="url">Remote Server URL</param>
        /// <param name="application">Application</param>
        /// <param name="host">Host</param>
        /// <param name="environment">Environment</param>
        /// <param name="appID">App ID</param>
        /// <param name="appSecret">App Secret</param>
        /// <param name="verifySSL">App Secret</param>
        /// <param name="localPath">Local Path</param>
        public void Init(string url, string application, string host, string environment, string appID, string appSecret, bool verifySSL, string localPath = null)
        {
            m_URL = url;
            m_Application = application;
            m_Host = host;
            m_Environment = environment;
            m_AppID = appID;
            m_AppSecret = appSecret;
            m_VerifySSL = verifySSL;
            m_LocalPath = localPath;

            PopulateSettings(false);
        }

        /// <summary>
        /// Returns the value from settings
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public T GetValue<T>(string key)
        {
            try
            {
                JToken token = m_Settings.SelectToken(key.ToLower());
                if (token != null)
                {
                    return token.ToObject<T>();
                }
                else return default(T);
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error while retriving value from TCM. key:[{key}]", ex);
                return default(T);
            }

        }

        /// <summary>
        /// Return the JSON object
        /// </summary>
        /// <returns></returns>
        public JObject Get()
        {
            return m_Settings;
        }

        #endregion

        #region Private Methods

        private string getSettingsFromServer()
        {
            string settings = null;

            HttpWebResponse httpWebResponse = null;

            try
            {
                if (!m_VerifySSL)
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                }

                string tcmRequesturl = $"{m_URL}/{m_Application}/{m_Host}/{m_Environment}?app_id={m_AppID}&app_secret={m_AppSecret}";
                _Logger.Info($"Issuing TCM (GET) [{tcmRequesturl}]");
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(tcmRequesturl);
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Timeout = 5000;

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                _Logger.Info($"TCM Response Status ({httpWebResponse.StatusCode}) [{httpWebResponse.StatusDescription}]");

                using (StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    settings = sr.ReadToEnd();
                }

                string pathToLocalFile = getPathToLocalFile();

                using (StreamWriter sw = new StreamWriter(pathToLocalFile))
                {
                    sw.Write(settings);
                }
            }
            catch
            {
            }
            finally
            {
                if (httpWebResponse != null)
                    httpWebResponse.Close();
            }

            return settings;
        }

        private string getSettingsFromLocal()
        {
            string settings = null;
            string pathToLocalFile = getPathToLocalFile();
            _Logger.Info($"Getting TCM from local file:[{pathToLocalFile}]");
            if (File.Exists(pathToLocalFile))
            {
                using (StreamReader sr = new StreamReader(pathToLocalFile))
                {
                    settings = sr.ReadToEnd();
                }
            }

            return settings;
        }

        private string getSettings(bool fromLocal)
        {
            string settings = null;

            if (fromLocal)
            {
                settings = getSettingsFromLocal();
            }
            else
            {
                settings = getSettingsFromServer();

                //if no data from server, try to take data from local file
                if (string.IsNullOrEmpty(settings))
                {
                    settings = getSettingsFromLocal();
                }
            }

            return settings;
        }

        private void PopulateSettings(bool fromLocal)
        {
            string settings = getSettings(fromLocal);

            if (!string.IsNullOrEmpty(settings) && !settings.ToLower().Equals("null"))
            {
                try
                {
                    foreach (var record in JsonConvert.DeserializeObject<Dictionary<string, object>>(settings))
                    {
                        if (record.Value is JObject)
                        {
                            m_Settings.Add(record.Key.ToLower(), ParseNestedObjects((JObject)record.Value)); 
                        }
                        else
                        {
                            string _key = record.Key;
                            m_Settings.Add(_key.ToLower(), new JValue(record.Value));    
                        }
                        
                    }
                }
                catch(Exception e)
                {
                    _Logger.Error($"Error while trying to populate TCM settings[{settings}]", e);
                    throw new Exception("Source is corrupted.");
                }
            }
            else
            {
                throw new Exception("Source is empty / not avaiable.");
            }
        }

        private JObject ParseNestedObjects(JObject settings)
        {
            JObject retJObject = new JObject();
            //Iterate siblings
            foreach (var record in settings)
            {
                if (record.Value is JObject)
                {
                    //if JObject (complex), go deeper in the hierarchy
                    JObject parsedJObject = ParseNestedObjects((JObject) record.Value);
                    retJObject.Add(record.Key.ToLower(), parsedJObject);
                }
                else
                {
                    JProperty token = new JProperty(record.Key.ToLower(), record.Value);
                    retJObject.Add(token);
                }
            }
            return retJObject;
        }

        private string getPathToLocalFile()
        {
            string pathToLocalFile = null;

            if (string.IsNullOrEmpty(m_LocalPath))
            {
                pathToLocalFile = AppDomain.CurrentDomain.BaseDirectory + "settings.json";
            }
            else
            {
                pathToLocalFile = m_LocalPath + "/settings.json";
            }

            return pathToLocalFile;
        }

        #endregion
    }
}
