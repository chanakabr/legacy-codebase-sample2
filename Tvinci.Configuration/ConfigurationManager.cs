using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;
using System.Web;
using Tvinci.Configuration.ConfigSvc;
using System.Security;
using KLogMonitor;
using System.Reflection;
using TVinciShared;

namespace Tvinci.Configuration
{
   
    public class ConfigurationManager<TConfiguration> : ISupportProvider, IDisposable where TConfiguration : class 
    {

        [Flags]
        public enum eBehaivor
        {
            Default = 0,
            AllowWrite = 2,
            Encrypt = 4
        }

        public enum eSource
        {
            None,
            File,
            Object,
            Service
        }

        public delegate void DataModifiedDelegate(TConfiguration data);

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        FileSystemWatcher m_watcher = null;
        Timer m_timer = null;
        string m_file = string.Empty;
        private eSource m_source = eSource.None;
        private event DataModifiedDelegate m_postDataModified;
        protected string m_syncFile;

        public event DataModifiedDelegate PostDataModified
        {
            add
            {                
                m_postDataModified += value;

                if (SyncMode == eMode.Synced && m_data != null)
                {
                    value(m_data);
                }
            }
            remove
            {
                m_postDataModified -= value;
            }
        }

        public void ReSyncFromFile(string fileName)
        {
            if (!string.Equals(fileName, m_syncFile))
            {
                SyncFromFile(fileName, true);
                m_syncFile = fileName;
            }
        }
        

        /// <summary>
        /// Used in data sync algorithm only! don't expose to inheritence.
        /// </summary>
        ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();
        TConfiguration m_data;
        public eBehaivor Behaivor { get; set; }
        protected DataModifiedDelegate DataModified { get; set; }
              
        public eMode SyncMode { get; private set; }

        public ConfigurationManager(eSource source = eSource.File)
        {
            SyncMode = source == eSource.Service ? eMode.Synced : eMode.NotSynced;
        }

        public ConfigurationManager(string virtualPath)
            : this(virtualPath, null, eBehaivor.Default)
        {
        }

        public ConfigurationManager(string virtualPath, DataModifiedDelegate dataModified)
            : this(virtualPath, dataModified, eBehaivor.Default)
        {

        }

        public ConfigurationManager(string virtualPath, DataModifiedDelegate dataModified, eBehaivor behaivor) : this()
        {
            Behaivor = behaivor;
            DataModified = dataModified;
            SyncFromFile(virtualPath,true);
        }


        public TConfiguration Data
        {
            get
            {
                if (m_locker.TryEnterReadLock(4000))
                {
                    try
                    {

                        if (SyncMode == eMode.NotSynced)
                        {
                            throw new Exception("The data is not synced");
                        }

                        return m_data;
                    }
                    finally
                    {
                        m_locker.ExitReadLock();
                    }
                }
                else
                {
                    return default(TConfiguration);
                }
            }
        }

        public void SaveToFile()
        {
            if (m_source != ConfigurationManager<TConfiguration>.eSource.File)
            {
                string message = string.Format("Save operation is permitted only when monitoring file. current source type '{0}'", m_source);
                //logger.Error(message);
                throw new Exception(message);
            }

            if (string.IsNullOrEmpty(m_file))
            {
                throw new Exception("Invalid situation. if source is set to 'File' then path must be set");
            }

            //logger.InfoFormat("Attempting to save current configuration to file '{0}'", m_file);            
            bool previousStatus = (m_watcher != null);
            string sourcePath = m_file;
            
            try
            {                
                if (((Behaivor & ConfigurationManager<TConfiguration>.eBehaivor.AllowWrite) != ConfigurationManager<TConfiguration>.eBehaivor.AllowWrite))
                {
                    //logger.Error("Saving is disabled for this configuration. contact programmer to enable this action. Operation aborted");
                }
                else
                {
                    StopSync();

                    // try to save to file
                    XmlSerializer xs = new XmlSerializer(typeof(TConfiguration));

                    using (XmlTextWriter xt = new XmlTextWriter(sourcePath, Encoding.UTF8))
                    {
                        xt.Formatting = Formatting.Indented;
                        xs.Serialize(xt, m_data);
                    }
                }                
            }
            catch (Exception)
            {
                //logger.Error(string.Format("Failed to save current configuration to file. user '{0}'",System.Security.Principal.WindowsIdentity.GetCurrent().Name),ex);                
            }
            finally
            {
                //logger.Info("Re-syncing configuration from file");
                SyncFromFile(sourcePath,previousStatus);
            }           
        }

        public void SyncFromConfiguration(TConfiguration configuration)
        {
            //logger.InfoFormat("Handling request to read configuration from object");

            if (m_source != ConfigurationManager<TConfiguration>.eSource.None)
            {
                StopSync();
            }
            
            if (configuration == null)
            {
                //logger.ErrorFormat("Cannot handle null configuration object.");
                return;
            }

            try
            {                
                sync(configuration);
            }
            catch (Exception)
            {
                //logger.ErrorFormat("failed handling sync request from configuration object.",ex);                
            }            
        }

        public void SyncFromFile(string virtualPath, bool shouldMonitor)
        {                        
            //logger.InfoFormat("Handling request to read configuration from file '{0}'", virtualPath);

            if (m_source != ConfigurationManager<TConfiguration>.eSource.None)
            {
                StopSync();
            }
            
            //// create absolute file path and store for later use
            //if (!Path.IsPathRooted(virtualPath))
            //{
            //    m_file = HttpContext.Current.ServerMapPath(virtualPath);
            //}
            //else
            {
                m_file = virtualPath;
            }
            
            performSyncFromFile();

            if (shouldMonitor)
            {
                m_source = ConfigurationManager<TConfiguration>.eSource.File;
                m_watcher = new FileSystemWatcher();
                m_watcher.Path = Path.GetDirectoryName(m_file);
                m_watcher.Filter = Path.GetFileName(m_file);
                m_watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;
                m_watcher.Changed += new FileSystemEventHandler(this.OnFileChanged);
                m_watcher.Created += new FileSystemEventHandler(this.OnFileChanged);
                m_watcher.Deleted += new FileSystemEventHandler(this.OnFileChanged);
                m_watcher.EnableRaisingEvents = true;
                m_timer = new Timer(new TimerCallback(delegate(object state)
                {
                    performSyncFromFile();
                }), null, -1, -1);

                //logger.Info(string.Concat("Start monitoring file '", m_file, "'."));
            }
        }

        private void performSyncFromFile()
        {
            TConfiguration configFromFile = null;

            try
            {
                // creates instance of configuration. the called method should handles error if occured and return null in those cases
                bool isEncrypted = ((Behaivor & ConfigurationManager<TConfiguration>.eBehaivor.Encrypt) == ConfigurationManager<TConfiguration>.eBehaivor.Encrypt);
                configFromFile = ConfigurationHelper.ExtractFromFile<TConfiguration>(m_file, isEncrypted);
                                    
                if (configFromFile != null)
                {
                    sync(configFromFile);
                }                
            }
            catch(Exception)
            {
                //logger.Error("Failed to perform the configuration sync from file");
            }
        }

        public void StopSync()
        {
            switch (m_source)
            {                
                case ConfigurationManager<TConfiguration>.eSource.File:
                    if (m_watcher != null)
                    {
                        //logger.InfoFormat("Stop monitoring file '{0}'", m_file);
                        m_watcher.EnableRaisingEvents = false;
                        m_watcher.Dispose();
                        m_watcher = null;
                    }

                    m_file = string.Empty;                
                    break;
                case ConfigurationManager<TConfiguration>.eSource.Object:                                        
                case ConfigurationManager<TConfiguration>.eSource.None:
                default:
                    break;
            }

            sync(null);
        }
        
        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            //logger.InfoFormat("Change occured on file '{0}'. change type '{1}'", m_file, e.ChangeType);
            m_timer.Change(500, -1);
        }

        

        void sync(TConfiguration configuration)
        {            
            bool shouldRaiseWriteLock = !m_locker.IsWriteLockHeld;

            try
            {
                if (shouldRaiseWriteLock)
                {
                    if (!m_locker.TryEnterWriteLock(4000))
                        return;
                }

                if (configuration == null)
                {
                    //logger.Info("Null configuration passed - Setting mode to 'Not synced'");
                    SyncMode = eMode.NotSynced;                    
                    return;
                }
                else
                {
                    //logger.Info("Valid configuration passed - Setting mode to 'Synced'");
                    m_data = configuration;
                    SyncMode = eMode.Synced;
                }

                try
                {
                    if (DataModified != null)
                    {
                        DataModified(m_data);
                        //logger.Info("Executing 'DataModified' finished sucessfully");
                    }
                }
                catch (Exception)
                {
                    //logger.Error("Executing 'DataModified' finished sucessfully failed", ex);
                    throw;                    
                }
            }
            catch
            {
                //logger.Error("Error occured - Setting mode to 'Not Synced'");
                m_data = null;
                SyncMode = eMode.NotSynced;                
            }
            finally
            {                                            
                if (shouldRaiseWriteLock)
                {
                    m_locker.ExitWriteLock();
                }                                
            }

            try
            {                
                if (m_postDataModified != null)
                {
                    m_postDataModified(m_data);
                }
            }
            catch { }
        }
        
        #region IDisposable Members

        public void Dispose()
        {
            StopSync();
        }

        #endregion

        #region ISupportProvider Members

        public void SyncFromConfigurationFile(string virtualPath)
        {
            SyncFromFile(virtualPath,true);
        }

        #endregion

        //public delegate TConfiguration MapToObjectDelegate(ConfigKeyVal[] keyVals);

        public void SyncFromService(int nGroupID, string sPlatform, string sEnvironment, eConfigType type, Func<IEnumerable<ConfigKeyVal>,TConfiguration> mapObjectMethod)
        {
            DbConfigManager db = new DbConfigManager(nGroupID, sPlatform, sEnvironment, type);
            m_data = mapObjectMethod(db.source);
        }



    }
}
