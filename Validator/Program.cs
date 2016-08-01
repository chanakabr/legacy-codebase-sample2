using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Validator.Managers.Scheme;

namespace Validator
{
    class Program
    {
        static void Main(string[] args)
        {
            bool valid = SchemeManager.Validate();
            Console.Read();

            if (valid)
                Environment.Exit(0);

            Environment.Exit(-1);
        }
    }
}
