using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRParamsObj
    {
        private string assetID;
        private string entityID;

        public virtual string AssetID
        {
            get
            {
                return assetID;
            }
            set
            {
                this.assetID = value;
            }
        }

        public virtual string EntityID
        {
            get
            {
                return entityID;
            }
            set
            {
                this.entityID = value;
            }
        }
    }
}
