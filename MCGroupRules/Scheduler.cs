using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace MCGroupRules
{
    public class Scheduler : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public int m_nGroupID { get; set; }

        protected override bool DoTheTaskInner()
        {
            try
            {
                MCRuleFactory factory = new MCRuleFactory();
                List<MCRule> rules = factory.GetGroupRules(m_nGroupID);
                foreach (MCRule rule in rules)
                {
                    log.Debug("Rule Found - Rule Type: " + ((int)(rule.RuleType)).ToString() + " Rule ID: " + rule.RuleID.ToString());
                    MCImplementationBase impl = factory.GetRuleImplementation(rule);
                    impl.InitMCObj();
                    impl.Send();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        public Scheduler(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {
            m_nGroupID = int.Parse(engrameters);
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new Scheduler(nTaskID, nIntervalInSec, engrameters);
        }

    }
}
