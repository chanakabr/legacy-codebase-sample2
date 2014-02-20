using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Financial
{
    public class ContentOwnerContract : BaseContract
    {

        public override bool IsContracatValid(double nPrice, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRT)
        {

            bool bIsRelated = false;

            if (eRT == m_eRelatedTo || m_eRelatedTo == RelatedTo.BOTH)
                bIsRelated = true;

            return (base.IsContracatValid(nPrice, nCurrenyCD, sCountryName, dDate, eRT) && bIsRelated);
        }
        

    }
}
