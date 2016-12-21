using EventManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class NotificationCondition
    {
        public virtual bool Evaluate(KalturaEvent kalturaEvent)
        {
            return true;
        }
    }
}
