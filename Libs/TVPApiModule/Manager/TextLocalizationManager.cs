using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;
using TVPApi;
using Tvinci.Localization;
using System.Threading;
using TVPApiModule.Objects;
using KLogMonitor;
using System.Reflection;

namespace TVPApiModule.Manager
{

    public class TextLocalizationManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static ReaderWriterLockSlim m_Lock = new ReaderWriterLockSlim();
        private Dictionary<string, TextLocalization> m_dTextLocalization = new Dictionary<string, TextLocalization>();

        private static TextLocalizationManager m_Instance = null;

        public static TextLocalizationManager Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new TextLocalizationManager();

                return m_Instance;
            }
        }

        private TextLocalizationManager()
        {
        }

        public TextLocalization GetTextLocalization(int groupID, PlatformType platform)
        {
            string sKey = m_Instance.GetKey(groupID, platform);

            TextLocalization retTextLocalization = null;

            // read TextLocalization from shared dictionary
            if (m_Lock.TryEnterReadLock(1000))
            {
                try
                {
                    m_dTextLocalization.TryGetValue(sKey, out retTextLocalization);
                }
                catch (Exception ex)
                {
                    logger.Error("GetTextLocalization->", ex);
                }
                finally
                {
                    m_Lock.ExitReadLock();
                }
            }

            // if not exsist in shared dictionary create one
            if (retTextLocalization == null)
            {
                retTextLocalization = new TextLocalization(groupID, platform);
            }

            // add TextLocalization to shared dictionary if not exsist
            if (m_Lock.TryEnterWriteLock(1000))
            {
                try
                {
                    if (!m_dTextLocalization.Keys.Contains(sKey))
                    {
                        m_dTextLocalization.Add(sKey, retTextLocalization);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("TextLocalization->", ex);
                }
                finally
                {
                    m_Lock.ExitWriteLock();
                }
            }

            return retTextLocalization;
        }

        private string GetKey(int groupID, PlatformType platform)
        {
            string keyStr = groupID.ToString();
            if (platform != PlatformType.Unknown)
            {
                keyStr = string.Concat(keyStr, platform.ToString().ToLower());
            }

            return keyStr;
        }
    }
}
