using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Statistics
{
    public class ChannelBucket
    {
        #region Members

        public int CurrentViews { get; set; }
        public int PreviousViews { get; set; }
        public DateTime CurrentDate { get; set; }

        #endregion

        #region CTOR

        public ChannelBucket()
        {
            this.CurrentViews = 0;
            this.PreviousViews = 0;
            this.CurrentDate = DateTime.UtcNow;
        }

        #endregion
    }
}
