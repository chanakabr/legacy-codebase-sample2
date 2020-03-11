using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class GroupInfo
    {
        #region Public Properties
        public string Name { get; set; }

        public long ID { get; set; }

        public long ParentID { get; set; }
        #endregion
    }
}
