using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApi
{
    public class ParserHelper
    {
        public static IParser GetParser(int groupID)
        {
            switch (groupID)
            {
                case 121:
                case 122:
                case 123:
                case 124:
                    return new AbertisJSONParser();
                default:
                    return null;
            }
        }
    }
}
