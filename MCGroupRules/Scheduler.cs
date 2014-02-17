using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCGroupRules
{
    public class Scheduler : ScheduledTasks.BaseTask
    {
        public int m_nGroupID { get; set; }

        protected override bool DoTheTaskInner()
        {
            try
            {
                MCRuleFactory factory = new MCRuleFactory();
                List<MCRule> rules = factory.GetGroupRules(m_nGroupID);
                foreach (MCRule rule in rules)
                {
                    Logger.Logger.Log("Rule Found", "Rule Type: " + ((int)(rule.RuleType)).ToString() + " Rule ID: " + rule.RuleID.ToString(), "MailRules");
                    MCImplementationBase impl = factory.GetRuleImplementation(rule);
                    impl.InitMCObj();
                    impl.Send();
                }
                return true;
            }
            catch (Exception ex)
            {
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
