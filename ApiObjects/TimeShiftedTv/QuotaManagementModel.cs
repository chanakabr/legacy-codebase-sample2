using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class QuotaManagementModel
    {
        #region Properties

        public int Id
        {
            get;
            set;
        }

        public int Minutes
        {
            get;
            set;
        }

        #endregion

        #region Ctor

        public QuotaManagementModel()
        {

        }

        #endregion
    }
}
