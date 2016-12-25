using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    public abstract class KalturaEvent
    {
        #region Properties

        public int PartnerId
        {
            get;
            set;
        }

        #endregion

        #region Ctor

        public KalturaEvent(int groupId = 0)
        {
            this.PartnerId = groupId;
        } 

        #endregion
    }
}
