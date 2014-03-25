using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder
{
    public class BuzzAbstract
    {
        protected BaseBuzzImpl m_oImplementer;

        public BuzzAbstract()
        {

        }

        public BaseBuzzImpl Implementer
        {
            set { m_oImplementer = value; }
        }
    }
}
