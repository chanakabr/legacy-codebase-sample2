using System;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public partial class KalturaCategoryVersionFilter : KalturaFilter<KalturaCategoryVersionOrderBy>
    {
        public override KalturaCategoryVersionOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryVersionOrderBy.UPDATE_DATE_DESC;
        }
    }
}