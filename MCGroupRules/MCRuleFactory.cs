using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;
using TVinciShared;

namespace MCGroupRules
{
    public class MCRuleFactory
    {

        public List<MCRule> GetGroupRules(int groupID)
        {
            List<MCRule> ruleList = new List<MCRule>();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT ID, Type, min_limit_id, subscription_id, start_date, end_date FROM groups_mail_rules WHERE Type <> 8 AND STATUS=1 AND IS_ACTIVE=1 AND";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            selectQuery.Execute("query", true);
            DataTable dt = selectQuery.Table("query");
            selectQuery.Finish();

            return RuleDtToList(dt, groupID);
        }

        public List<MCRule> GetGroupRulesByType(int groupID, RuleType type)
        {
            
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += string.Format("SELECT ID, Type, min_limit_id, subscription_id, start_date, end_date, group_id FROM groups_mail_rules WHERE Type == {0} AND STATUS=1 AND IS_ACTIVE=1 AND", (int)type);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            selectQuery.Execute("query", true);
            DataTable dt = selectQuery.Table("query");
            selectQuery.Finish();

            return RuleDtToList(dt, groupID);

        }

        private List<MCRule> RuleDtToList(DataTable dt, int groupID)
        {
            List<MCRule> ruleList = new List<MCRule>();
            int limitID;
            int subscriptionID;

            foreach (DataRow ruleRow in dt.Rows)
            {
                if (DateTime.Parse(ruleRow["start_date"].ToString()) < DateTime.UtcNow)
                {
                    DateTime endDate = DateTime.MaxValue;
                    DateTime.TryParse(ruleRow["end_date"].ToString(), out endDate);
                    if (endDate > DateTime.UtcNow)
                    {
                        if (ruleRow["min_limit_id"] != DBNull.Value)
                        {
                            limitID = int.Parse(ruleRow["min_limit_id"].ToString());
                        }
                        else limitID = -1;
                        if (ruleRow["subscription_id"] != DBNull.Value)
                        {
                            subscriptionID = int.Parse(ruleRow["subscription_id"].ToString());
                        }
                        else subscriptionID = -1;
                        MCRule rule = new MCRule(groupID, int.Parse(ruleRow["ID"].ToString()), (RuleType)int.Parse(ruleRow["Type"].ToString()), limitID, subscriptionID);
                        ruleList.Add(rule);
                    }
                }
            }

            return ruleList;
        }


        public MCImplementationBase GetRuleImplementation(MCRule rule)
        {
            MCImplementationBase impl;

            switch (rule.RuleType)
            {
                case RuleType.HaventWatched:
                    impl = new MCHaventWatched(rule.GroupID, rule.RuleID, (int)rule.RuleType, rule.FrequencyID, rule.SubscriptionID);
                    return impl;
                case RuleType.PaymentFailed:
                    impl = new MCPaymentFailed(rule.GroupID, rule.RuleID, (int)rule.RuleType, rule.FrequencyID, rule.SubscriptionID);
                    return impl;
                case RuleType.RegistrationDate:
                    impl = new MCRegistrationDate(rule.GroupID, rule.RuleID, (int)rule.RuleType, rule.FrequencyID, rule.SubscriptionID);
                    return impl;
                case RuleType.SubscriptionTypeAfterEnd:
                    impl = new MCTrialSubscriptionAfterEnd(rule.GroupID, rule.RuleID, (int)rule.RuleType, rule.FrequencyID, rule.SubscriptionID);
                    return impl;
                case RuleType.SubscriptionTypeBeforeEnd:
                    impl = new MCTrialSubscriptionBeforeEnd(rule.GroupID, rule.RuleID, (int)rule.RuleType, rule.FrequencyID, rule.SubscriptionID);
                    return impl;
                case RuleType.HaventWatchedSinceRegistration:
                    impl = new MCHaventWatchedSinceRegistration(rule.GroupID, rule.RuleID, (int)rule.RuleType, rule.FrequencyID, rule.SubscriptionID);
                    return impl;
                case RuleType.SubscriptionEnded:
                    impl = new MCSubscriptionEnded(rule.GroupID, rule.RuleID, (int)rule.RuleType, rule.FrequencyID, rule.SubscriptionID);
                    return impl;
                default:
                    return null;
            }
        }

    }
}
