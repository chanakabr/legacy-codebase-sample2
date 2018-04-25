using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    public class AssetRulesResponse
    {
        public List<AssetRule> AssetRules { get; set; }
        public Status Status { get; set; }
    }
}
