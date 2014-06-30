using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class HomeNetwork
    {
        public string name { get; set; }
        public string uid { get; set; }
        public string description { get; set; }
        public DateTime create_date { get; set; }
        public bool is_active { get; set; }
    }
}
