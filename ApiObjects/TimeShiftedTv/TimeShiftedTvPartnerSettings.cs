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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("IsCatchUpEnabled: {0}, ", IsCatchUpEnabled.HasValue ? IsCatchUpEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsCdvrEnabled: {0}, ", IsCdvrEnabled.HasValue ? IsCdvrEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsStartOverEnabled: {0}, ", IsStartOverEnabled.HasValue ? IsStartOverEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsTrickPlayEnabled: {0}, ", IsTrickPlayEnabled.HasValue ? IsTrickPlayEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("CatchUpBufferLength: {0}, ", CatchUpBufferLength));
            sb.Append(string.Format("TrickPlayBufferLength: {0}, ", TrickPlayBufferLength));            
                        
            return sb.ToString();
        }

    }
}