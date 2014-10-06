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

        protected override bool DoTheTaskInner()
        {
            try
            {
                string[] paramsList = m_sParameters.Split('|');
                eCrowdsourceType collectorType;
                Logger.Logger.Log("Crowdsource", string.Format("{0}: Recieved params: GroupId={1} Collector:{2} Asset={3}", DateTime.UtcNow, paramsList[2], paramsList[0], paramsList[1]), "Crowdsourcing");
                if (Enum.TryParse(paramsList[0], true, out collectorType) && paramsList.Length == 3)
                {
                    BaseDataCollector collector = DataCollectorFactory.GetInstance(collectorType, paramsList[1], int.Parse(paramsList[2]));
                    if (collector != null)
                        return collector.ProccessCroudsourceTask();
                }
                Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error parsing collectorType", DateTime.UtcNow, paramsList[0]), "Crowdsourcing");
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
