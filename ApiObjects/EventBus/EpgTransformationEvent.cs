using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    public class EpgTransformationEvent : ServiceEvent
    {
        public string Data { get; set; }
    }

}
