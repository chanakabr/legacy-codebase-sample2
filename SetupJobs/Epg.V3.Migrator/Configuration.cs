using ApiObjects;
using Core.Api;
using System.Collections.Generic;

namespace Epg.V3.Migrator
{
    public class Configuration
    {
        public int PartnerId { get; set; }
        public int BatchSize { get; set; } = 10000;
    }
}