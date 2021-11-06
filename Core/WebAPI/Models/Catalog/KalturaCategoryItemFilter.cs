using System;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public partial class KalturaCategoryItemFilter : KalturaFilter<KalturaCategoryItemOrderBy>
    {
        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.CREATE_DATE_ASC;
        }
    }
}