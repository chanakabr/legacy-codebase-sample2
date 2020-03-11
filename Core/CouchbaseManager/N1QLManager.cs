using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CouchbaseManager
{
    public class N1QLManager
    {
        /// <summary>
        /// THE statement, the query script itself
        /// </summary>
        public string statement;

        /// <summary>
        /// Named parameters to be used in query statement - each parameter should have '$' before its definition in query. e.g.:
        /// ... WHERE user_id = $userId ...
        /// </summary>
        public KeyValuePair<string, object>[] namedParameters;

        /// <summary>
        /// Positional parameters to be used in query statement - each parameter should have a number in query, e.q. $1, $2 etc.
        /// </summary>
        public List<object> positionalParameters;

        public N1QLManager(string statement, Dictionary<string, object> namedParametersDictionary = null, List<object> positionalParameters = null)
        {
            this.statement = statement.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(";", "");
            this.statement = Regex.Replace(statement, @"\s+", " ");

            if (namedParametersDictionary != null)
            {
                this.namedParameters = namedParametersDictionary.ToArray();
            }

            if (positionalParameters != null)
            {
                this.positionalParameters = positionalParameters;
            }
        }
    }
}
