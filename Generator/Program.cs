using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validator.Managers.Scheme;

namespace Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Stream stream = new FileStream("KalturaClient.xml", FileMode.Create, FileAccess.Write);
            SchemeManager.Generate(stream);
        }
    }
}
