using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Validator
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly asm = Assembly.LoadFrom(args[0]);

            if (asm == null)
                throw new Exception("DLL not found");

            var tt = asm.GetType("WebAPI.Models.General.KalturaOTTObject");

            bool found = false;
            foreach (Type type in asm.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("WebAPI.Models")))
            {
                if (!type.IsInterface && !type.IsEnum && !tt.IsAssignableFrom(type))
                {
                    Console.WriteLine(string.Format("Model {0} doesn't inherit from {1}", type.Name, tt.Name));
                    found = true;
                }
            }

            if (!found)
            {
                //Console.WriteLine("SUCCESS!");
                Environment.Exit(0);
            }
            else
                Environment.Exit(-1);
        }
    }
}
