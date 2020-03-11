using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.CrowdsourceItems;
using CrowdsourcingFeeder.DataCollector;
using CrowdsourcingFeeder.DataCollector.Base;
using KLogMonitor;
using ScheduledTasks;

namespace CrowdsourcingFeeder
{
    public class CrowdsourcingTask : BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CrowdsourcingTask(int nTaskID, int nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {

        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new CrowdsourcingTask(nTaskID, nIntervalInSec, sParameters);
        }

        protected override bool DoTheTaskInner()
        {
            try
            {
                string[] paramsList = m_sParameters.Split('|');
                eCrowdsourceType collectorType;
                log.Debug("Crowdsource - " + string.Format("Recieved params: GroupId={0} Collector:{1} Asset={2}", paramsList[2], paramsList[0], paramsList[1]));
                if (Enum.TryParse(paramsList[0], true, out collectorType) && paramsList.Length == 3)
                {
                    BaseDataCollector collector = DataCollectorFactory.GetInstance(collectorType, paramsList[1], int.Parse(paramsList[2]));
                    if (collector != null)
                        return collector.ProccessCroudsourceTask();
                }
                log.Debug("Crowdsource - " + string.Format("{0} - Error parsing collectorType", paramsList[0]));
                return false;
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("params={0}, ex={1}, st:{2}", m_sParameters, ex.Message, ex.StackTrace), ex);
                return false;
            }
        }
    }
}
