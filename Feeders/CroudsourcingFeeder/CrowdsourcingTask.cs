using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.CrowdsourceItems;
using CrowdsourcingFeeder.DataCollector;
using CrowdsourcingFeeder.DataCollector.Base;
using ScheduledTasks;

namespace CrowdsourcingFeeder
{
    public class CrowdsourcingTask : BaseTask
    {
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
                Logger.Logger.Log("Crowdsource", string.Format("Recieved params: GroupId={0} Collector:{1} Asset={2}", paramsList[2], paramsList[0], paramsList[1]), "Crowdsourcing");
                if (Enum.TryParse(paramsList[0], true, out collectorType) && paramsList.Length == 3)
                {
                    BaseDataCollector collector = DataCollectorFactory.GetInstance(collectorType, paramsList[1], int.Parse(paramsList[2]));
                    if (collector != null)
                        return collector.ProccessCroudsourceTask();
                }
                Logger.Logger.Log("Crowdsource", string.Format("{0} - Error parsing collectorType", paramsList[0]), "Crowdsourcing");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("params={0}, ex={1}, st:{2}", m_sParameters, ex.Message, ex.StackTrace), "Crowdsourcing");
                return false;
            }
        }
    }
}
