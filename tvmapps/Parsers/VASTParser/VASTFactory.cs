using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VASTParser
{
    public class VASTFactory
    {
        public static VASTParser GetVASTImpl(int mediaID, int groupID, string adType)
        {
            VASTParser retVal = null;
            switch (groupID)
            {
                case 109:
                    {
                        retVal = new FilmoVASTParser(mediaID, groupID, adType);
                        break;
                    }
                default:
                    break;
                    
            }
            return retVal;
        }
    }
}
