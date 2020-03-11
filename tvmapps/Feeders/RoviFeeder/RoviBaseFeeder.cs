using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoviFeeder
{
    public abstract class RoviBaseFeeder
    {
        public abstract bool Ingest();

        protected string m_url;
        protected int m_fromID;
        protected int m_groupID;
    }
}
