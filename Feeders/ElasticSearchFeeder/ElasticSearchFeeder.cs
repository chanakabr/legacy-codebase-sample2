using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace ElasticSearchFeeder
{
    public class ElasticSearchFeeder : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region members
        const char spliter = '|';
        int nGroupID;
        string sQueueName;
        string sType;
        bool bRebuildIndex;
        bool bSwitchIndex;
        DateTime dStartDate, dEndDate;
        ElasticSearchAbstract oESFeed;
        eESFeederType eFeederType;

        List<int> epgChannelIDsToDelete;
        #endregion

        public ElasticSearchFeeder(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            InitParamter();
            this.oESFeed = new ElasticSearchAbstract(eFeederType);
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new ElasticSearchFeeder(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            try
            {
                if (nGroupID == 0)
                    return false;

                log.DebugFormat("Params - Start Task {0} ESFeeder", sType);
                switch (sType)
                {
                    case "media":
                        oESFeed.Implementer = new BaseESMedia(nGroupID, sQueueName, bRebuildIndex) { bSwitchIndex = this.bSwitchIndex };
                        oESFeed.Start();
                        break;
                    case "epg":
                        oESFeed.Implementer = new BaseESMedia(nGroupID, sQueueName, bRebuildIndex, dStartDate, dEndDate) { bSwitchIndex = this.bSwitchIndex };
                        oESFeed.Start();
                        break;
                    case "epg_delete_channels":
                        oESFeed.Implementer = new ESDeleteEPGChannelsImplementor(epgChannelIDsToDelete, nGroupID, sQueueName, bRebuildIndex);
                        oESFeed.Start();
                        break;
                    default:
                        log.Error("Error - Unknown type in elastic search feeder. ElasticSearchFeeder");
                        break;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error - group:{0}, ex:{1} st:{2}", nGroupID, ex.Message, ex.StackTrace);
            }

            return true;
        }

        protected void InitParamter()
        {
            log.Debug("Start Init - Start Init :" + m_sParameters + " ESFeeder");

            try
            {

                string[] items = m_sParameters.Split(spliter);

                eFeederType = eESFeederType.MEDIA;

                if (items.Length >= 3)
                {
                    log.Debug("Start Init - items.length" + items.Length + " ESFeeder");

                    int.TryParse(items[0], out nGroupID);
                    sType = items[1];
                    sQueueName = items[2];

                    if (items.Length > 4)
                    {
                        bSwitchIndex = (items[3].Equals("1")) ? true : false;
                        bRebuildIndex = (items[4].Equals("1")) ? true : false;
                    }

                    if (sType == "epg")
                    {
                        eFeederType = eESFeederType.EPG;
                        int nOffset = 7;

                        if (items.Length > 5)
                        {
                            if (int.TryParse(items[5], out nOffset))
                            {
                                nOffset = (nOffset >= 0) ? nOffset : 7;
                            }
                        }

                        DateTime dTempDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
                        dStartDate = dTempDate.AddDays(nOffset * -1);
                        dEndDate = dTempDate.AddDays(nOffset);
                    }

                    if (sType == "epg_delete_channels")
                    {
                        eFeederType = eESFeederType.EPG;
                        epgChannelIDsToDelete = new List<int>();
                        if (items.Length == 4)
                        {
                            string epgChannelsAsStr = items[3];
                            if (!string.IsNullOrEmpty(epgChannelsAsStr))
                            {
                                string[] channels = epgChannelsAsStr.Split(';');
                                if (channels != null && channels.Length > 0)
                                {
                                    for (int i = 0; i < channels.Length; i++)
                                    {
                                        int epgChannelID = 0;
                                        if (Int32.TryParse(channels[i], out epgChannelID))
                                        {
                                            epgChannelIDsToDelete.Add(epgChannelID);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                log.Debug("Params - " + nGroupID.ToString() + " " + sType + " " + sQueueName + " ESFeeder");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error - " + string.Format("Error while parsing init parameters. params={0}. Ex={1} ST: {2}", m_sParameters, ex.Message, ex.StackTrace) + " ElasticSearchFeeder");
                throw new ArgumentException("Invalid arguments passed to Elastic search feeder");
            }
        }
    }
}
