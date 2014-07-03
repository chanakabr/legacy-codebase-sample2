using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatisticsBL
{
    public static class Utils
    {
        public static BaseStaticticsBL GetInstance(int nGroupID)
        {
            switch (nGroupID)
            {
                default:
                    {
                        return new TvinciStaticticsBL(nGroupID);
                    }
            }
        }
    }
}
