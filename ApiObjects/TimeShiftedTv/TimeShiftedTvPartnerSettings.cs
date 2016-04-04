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
        public bool? IsStartOverEnabled { get; set; }
        public bool? IsTrickPlayEnabled { get; set; }
        public int CatchUpBufferLength { get; set; }
        public int TrickPlayBufferLength { get; set; }

        public TimeShiftedTvPartnerSettings()
        {
        }

        public TimeShiftedTvPartnerSettings(bool? isCatchUpEnabled, bool? isCdvrEnabled, bool? isStartOverEnabled, bool? isTrickPlayEnabled, int catchUpBufferLength, int trickPlayBufferLength)
        {
            this.IsCatchUpEnabled = isCatchUpEnabled;
            this.IsCdvrEnabled = isCdvrEnabled;
            this.IsStartOverEnabled = isStartOverEnabled;
            this.IsTrickPlayEnabled = isTrickPlayEnabled;
            this.CatchUpBufferLength = catchUpBufferLength;
            this.TrickPlayBufferLength = trickPlayBufferLength;
        }

    }
}