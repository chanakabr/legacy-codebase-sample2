using System.Collections.Generic;

namespace SchemaRegistryEvents
{
    public class SourceService
    {
        public const string HeaderName = "serviceName"; 
        public const string Phoenix = "phoenix";
        public const string Household = "household";
        
        public static readonly IDictionary<string, string> PhoenixHeader = new Dictionary<string, string>
            { { HeaderName, Phoenix } };
    }
}