using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects;
using ApiObjects.MediaIndexingObjects;
using KLogMonitor;
using QueueWrapper;
using Tvinci.Core.DAL;

namespace EpgFeeder
{
    public class EpgFeederObj : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        const char spliter = '|';
        string sGroupID;
        string sEPGChannel;
        string sPathType;
        string sPath;
        Dictionary<string, string> sExtraParamter = new Dictionary<string, string>();
        EPGAbstract oEPGFeed;
        

        public EpgFeederObj(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            InitParamter();
            oEPGFeed = new EPGAbstract();

        }
        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new EpgFeederObj(nTaskID, nIntervalInSec, sParameters);
        }

        protected override bool DoTheTaskInner()
        {
            try
            {
                switch (sEPGChannel)
                {
                    case "EPGxmlTv":
                        oEPGFeed.Implementer = new EPGGeneral(sGroupID, sPathType, sPath, sExtraParamter);
                        break;
                    case "EPG_MediaCorp":
                        oEPGFeed.Implementer = new EPGMediaCorp(sGroupID, sPathType, sPath, sExtraParamter);
                        break;
                    case "EPG_Yes":
                        oEPGFeed.Implementer = new EPGYes(sGroupID, sPathType, sPath, sExtraParamter);
                        break;
                    case "EPG_KabelKiosk":
                        oEPGFeed.Implementer = new EPGEutelsat(sGroupID, sPathType, sPath, sExtraParamter);
                        break;

                }

                if (oEPGFeed != null)
                {
                    Dictionary<DateTime, List<int>> datesWithChannelIds = oEPGFeed.SaveChannel();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("group:{0}, ex:{1}", sGroupID, ex.Message));
            }
            return true;
        }

        protected void InitParamter()
        {
            try
            {
                string[] item = m_sParameters.Split(spliter);
                sGroupID = item[0];
                sEPGChannel = item[1];
                sPathType = item[2];
                sPath = item[3];

                for (int i = 4; i < item.Length; i++)
                {
                    string[] strspliter = { ";#" };
                    string[] extraitem = item[i].Split(strspliter, StringSplitOptions.RemoveEmptyEntries);
                    if (extraitem.Length > 1)
                    {
                        sExtraParamter.Add(extraitem[0], extraitem[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO
            }
        }
    }
}
