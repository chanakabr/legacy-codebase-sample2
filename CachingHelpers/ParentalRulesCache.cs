using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachingHelpers
{
    public class ParentalRulesCache : BaseCacheHelper<ParentalRule>
    {

        #region Singleton

        private static ParentalRulesCache instance;

        public static ParentalRulesCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new ParentalRulesCache();
                    }
                }
            }

            return instance;
        }

        #endregion
        
        #region Ctor and initialization

        private ParentalRulesCache()
            : base()
        {
        }

        #endregion

        protected override ParentalRule BuildValue(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        protected override List<ParentalRule> MultiBuildValue(List<int> indexes, params object[] parameters)
        {
            List<ParentalRule> rules = new List<ParentalRule>();

            int groupId = (int)parameters[0];
            List<long> fullIds = (List<long>)parameters[1];
            List<long> ids = BuildPartialList(fullIds, indexes);

            var dictionary = DAL.ApiDAL.Get_Group_ParentalRules_ByID(groupId, ids);
            rules = dictionary.Values.ToList();

            return rules;
        }

        public List<ParentalRule> Get(int groupId, List<long> ids)
        {
            List<ParentalRule> rules = new List<ParentalRule>();

            List<string> keys = ids.Select(id => string.Format("{0}_parental_rule_{1}", version, id)).ToList();
            string mutexName = string.Concat("ParntalRules GID_", groupId);

            rules = base.MultiGet(keys, mutexName, groupId, ids);

            return rules;
        }
    }
}
