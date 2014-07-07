using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchbaseWrapper
{
    public class ClientConfig
    {
        public string Bucket { get; set; }
        public List<string> URLs { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
