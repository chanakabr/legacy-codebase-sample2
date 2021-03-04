using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Reflection;
using KLogMonitor;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Polly;
using Polly.Retry;

namespace TCMClient
{
    public class Settings
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly HttpClient httpClient;

        static Settings()
        {
            SslProtocols enabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
            DecompressionMethods enabledDecompressionMethod = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            int maxConnectionsPerServer = 5;
            bool checkCertificateRevocationList = false;
            System.TimeSpan timeout = System.TimeSpan.FromMilliseconds(100000);

#if NETCOREAPP3_1
            SocketsHttpHandler httpHandler = new SocketsHttpHandler() { SslOptions = new System.Net.Security.SslClientAuthenticationOptions() };
            httpHandler.SslOptions.EnabledSslProtocols = enabledSslProtocols;
            httpHandler.AutomaticDecompression = enabledDecompressionMethod;
            httpHandler.MaxConnectionsPerServer = maxConnectionsPerServer;
            httpHandler.SslOptions.CertificateRevocationCheckMode = checkCertificateRevocationList ? X509RevocationMode.Online : X509RevocationMode.NoCheck;
            if (!checkCertificateRevocationList)
            {
                httpHandler.SslOptions.RemoteCertificateValidationCallback = delegate { return true; };
            }

            httpClient = new HttpClient(httpHandler) { Timeout = timeout };
#elif NETFRAMEWORK
            HttpClientHandler httpHandler = new HttpClientHandler() { SslProtocols = new SslProtocols() };
            httpHandler.SslProtocols = enabledSslProtocols;
            httpHandler.AutomaticDecompression = enabledDecompressionMethod;
            httpHandler.MaxConnectionsPerServer = maxConnectionsPerServer;
            httpHandler.CheckCertificateRevocationList = checkCertificateRevocationList;
            if (!checkCertificateRevocationList)
            {
                httpHandler.ServerCertificateCustomValidationCallback = delegate { return true; };
            }

            httpClient = new HttpClient(httpHandler) { Timeout = timeout };
#endif
        }

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

        private JObject m_SettingsLoweredKeys = new JObject();
        private JObject m_SettingsOriginalKeys = new JObject();

        private string m_URL;
        private string m_Application;
        private string m_Host;
        private string m_Environment;
        private string m_AppID;
        private string m_AppSecret;
        private bool m_VerifySSL;
        private string m_LocalPath;

        public static bool IsInitilized { get; set; }

        #region Constructors

        private Settings()
        {
            IsInitilized = false;
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
            return GetValue<T>(key, false);
        }

        /// <summary>
        /// Returns the value from settings
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public T GetValue<T>(string key, bool exactCase)
        {
            try
            {
                JToken token = null;

                if (!exactCase)
                {
                    token = m_SettingsLoweredKeys.SelectToken(key.ToLower());
                }
                else
                {
                    token = m_SettingsOriginalKeys.SelectToken(key);
                }

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

        public JToken GetJsonString(string[] keys, bool isLowercase = true)
        {
            JToken token = isLowercase ? m_SettingsLoweredKeys : m_SettingsOriginalKeys;

            if (keys != null && keys.Any())
            {
                foreach (var key in keys)
                {
                    token = token.SelectToken(key.ToLower());

                    if (token == null)
                    {
                        return null;
                    }
                }
            }

            return token;
        }

        /// <summary>
        /// Return the JSON object
        /// </summary>
        /// <returns></returns>
        public JObject Get()
        {
            return m_SettingsOriginalKeys;
        }

        #endregion

        #region Private Methods

        private string getSettingsFromServer()
        {
            string settings = null;

            try
            {
                if (!m_VerifySSL)
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.CheckCertificateRevocationList = false;
                }

                string tcmRequesturl = $"{m_URL}/{m_Application}/{m_Host}/{m_Environment}?app_id={m_AppID}&app_secret={m_AppSecret}";
                _Logger.Info($"Issuing TCM (GET) [{tcmRequesturl}]");

                // GEN-533 - add retry mechanism
                var policy = RetryPolicy
                      .Handle<Exception>()
                      .WaitAndRetry(3, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                        {
                            _Logger.Warn($"Error while trying to get TCM data {ex}");
                            _Logger.Warn($"Waiting for:[{time.TotalSeconds}] seconds until next retry");
                        }
                      );

                policy.Execute(() =>
                {
                    var response = Task.Run(() => httpClient.GetAsync(tcmRequesturl)).ConfigureAwait(false).GetAwaiter().GetResult();

                    _Logger.Info($"TCM Response Status: ({response.StatusCode})");
                    response.EnsureSuccessStatusCode();

                    settings = Task.Run(() => response.Content.ReadAsStringAsync()).ConfigureAwait(false).GetAwaiter().GetResult();

                    // validate that returned string from TCM Server is a valid JSON. 
                    // If it's not, it should throw an error and retry
                    JToken.Parse(settings);
                }
                );
            }
            catch (JsonException e)
            {
                _Logger.Error($"Error while parsing JSON data from TCM server:", e);
            }
            catch (Exception e)
            {
                _Logger.Error($"Error while trying to get TCM data:", e);
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
                    YamlDotNet.Serialization.Deserializer yamlDeserializer = new YamlDotNet.Serialization.Deserializer();
                    YamlDotNet.Serialization.DeserializerBuilder ds = new YamlDotNet.Serialization.DeserializerBuilder();
                    var yaml = yamlDeserializer.Deserialize(sr);
                    JsonSerializer jsonSerializer = new Newtonsoft.Json.JsonSerializer();
                    using (var writer = new StringWriter())
                    {
                        jsonSerializer.Serialize(writer, yaml);
                        settings = writer.ToString();
                    }

                    yamlDeserializer = null;
                    jsonSerializer = null;
                    yaml = null;
                }
            }

            if (!string.IsNullOrEmpty(settings))
            {
                _Logger.Debug($"local settings are {settings}");
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

            //if no data from local or we don't want to get from local, try to take data from server
            if (string.IsNullOrEmpty(settings))
            {
                settings = getSettingsFromServer();
            }

            return settings;
        }

        private void PopulateSettings(bool fromLocal)
        {
            if (IsInitilized)
            {
                _Logger.Warn($"TCM Client does not support multiple initializations, leaving settings as they are loaded previously, ignoring init call.");
                return;
            }

            IsInitilized = true;
            string settings = getSettings(fromLocal);

            if (!string.IsNullOrEmpty(settings) && !settings.ToLower().Equals("null"))
            {
                try
                {
                    foreach (var record in JsonConvert.DeserializeObject<Dictionary<string, object>>(settings))
                    {
                        if (record.Value is JObject)
                        {
                            m_SettingsLoweredKeys.Add(record.Key.ToLower(), ParseNestedObjects((JObject)record.Value, true));
                            m_SettingsOriginalKeys.Add(record.Key, ParseNestedObjects((JObject)record.Value, false));
                        }
                        else if (record.Value is JArray)
                        {
                            m_SettingsLoweredKeys.Add(record.Key.ToLower(), record.Value as JArray);
                            m_SettingsOriginalKeys.Add(record.Key, record.Value as JArray);
                        }
                        else
                        {
                            
                            string _key = record.Key;
                            m_SettingsLoweredKeys.Add(_key.ToLower(), new JValue(record.Value));
                            m_SettingsOriginalKeys.Add(_key, new JValue(record.Value));
                        }
                    }
                }
                catch (Exception e)
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

        private JObject ParseNestedObjects(JObject settings, bool shouldLower)
        {
            JObject retJObject = new JObject();
            //Iterate siblings
            foreach (var record in settings)
            {
                if (record.Value is JObject)
                {
                    //if JObject (complex), go deeper in the hierarchy
                    JObject parsedJObject = ParseNestedObjects((JObject)record.Value, shouldLower);

                    string key = record.Key;

                    if (shouldLower)
                    {
                        key = key.ToLower();
                    }

                    retJObject.Add(key, parsedJObject);
                }
                else
                {
                    string key = record.Key;

                    if (shouldLower)
                    {
                        key = key.ToLower();
                    }

                    JProperty token = new JProperty(key, record.Value);
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
                pathToLocalFile = AppDomain.CurrentDomain.BaseDirectory + "config.yaml";
            }
            else
            {
                pathToLocalFile = m_LocalPath + "/config.yaml";
            }

            return pathToLocalFile;
        }

        #endregion
    }
}