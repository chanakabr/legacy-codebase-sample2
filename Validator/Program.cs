using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;

namespace Validator
{
    class Program
    {
        static void Main(string[] args)
        {
            bool valid = SchemaManager.Validate();
            Console.Read();

            if (valid)
                Environment.Exit(0);

            Environment.Exit(-1);
        }
    }
}
