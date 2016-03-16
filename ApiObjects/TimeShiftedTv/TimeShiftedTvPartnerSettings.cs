using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class TimeShiftedTvPartnerSettings
    {

        public bool? IsCatchUpEnabled { get; set; }
        public bool? IsCdvrEnabled { get; set; }

        public TimeShiftedTvPartnerSettings()
        {
        }

        public TimeShiftedTvPartnerSettings(bool? isCatchUpEnabled, bool? isCdvrEnabled)
        {
            this.IsCatchUpEnabled = isCatchUpEnabled;
            this.IsCdvrEnabled = isCdvrEnabled;
        }

    }
}