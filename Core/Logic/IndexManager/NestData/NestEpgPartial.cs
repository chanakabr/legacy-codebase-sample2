using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.IndexManager.NestData
{
    [ElasticsearchType(RelationName = "epg")]
    public class NestEpgPartial
    {
        [PropertyName("regions")]
        public List<int> Regions { get; set; }
    }
}
