using System;
using System.Text;

namespace NPVR
{
    public class NPVRCancelDeleteByObj : NPVRParamsObj
    {
        protected string bySeriesId; //allows filtering only the bookings associated to a given series.
        protected string bySeasonNumber; //allows filtering only the bookings associated to a given season number of the series specified with the bySeriesId criteria.

        public virtual string BySeriesId
        {
            get
            {
                return bySeriesId;
            }
            set
            {
                bySeriesId = value;
            }
        }

        public virtual string BySeasonNumber
        {
            get
            {
                return bySeasonNumber;
            }
            set
            {
                bySeasonNumber = value;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(String.Concat("NPVRCancelDeleteByObj. Base Obj:", base.ToString()));
            sb.Append(String.Concat(" BySeriesId: ", bySeriesId));
            sb.Append(String.Concat(" BySeasonNumber: ", bySeasonNumber));

            return sb.ToString();
        }
    }
}
