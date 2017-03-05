using ApiObjects;
using Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess
{
    public class AdsData
    {
        public string AdsParam { get; set; }

        public AdsPolicy? AdsPolicy { get; set; }
    }
}
