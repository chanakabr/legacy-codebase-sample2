using System.Collections.Generic;
using System.Text;
using TVinciShared;

namespace ApiLogic.Api.Managers.Rule
{
    public class KsqlBuilder
    {
        public static string And(params string[] ksql)
        {
            return And((IEnumerable<string>) ksql);
        }

        public static string And(IEnumerable<string> ksql)
        {
            return new StringBuilder()
                .Append("(and ")
                .AppendJoin(" ", ksql)
                .Append(")")
                .ToString();
        }
    }
}