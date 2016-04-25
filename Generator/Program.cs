using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Managers;

namespace Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Stream stream = new FileStream("KalturaClient.xml", FileMode.Create, FileAccess.Write);
            SchemaManager.Generate(stream);
        }
    }
}
