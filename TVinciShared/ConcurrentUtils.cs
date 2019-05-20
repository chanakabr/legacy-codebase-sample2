using ApiObjects.MediaMarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Core.DAL;

namespace TVinciShared
{
    public class ConcurrentUtils
    {

        static public bool IsConcurrent(string sSiteGUID, string sUDID, int nGroupID)
        {
            /*
            * Method is deprecated. Use WS_Domains's ValidateLimitationModule instead.
            */

            throw new NotImplementedException("Deprecated");
        }


        static public int GetConcurrentCount(int nGroupID, int nDomainID, string sUDID, ref int nFamilyConcurrentCount, int nDeviceFamilyID = 0)
        {
            throw new NotImplementedException("Deprecated");
        }

    }
}
