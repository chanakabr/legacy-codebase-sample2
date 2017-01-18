﻿using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class PurchaseSessionIdResponse
    {
        public long PurchaseCustomDataId { get; set; }

        public Status Status { get; set; }
    }
}
