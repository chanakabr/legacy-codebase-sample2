using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models.General;

namespace Validator
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly asm = Assembly.Load("WebAPI");

            foreach (Type type in asm.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("WebAPI.Models")))
            {
                if (!type.IsInterface && !type.IsEnum && !typeof(KalturaOTTObject).IsAssignableFrom(type))
                    throw new Exception(string.Format("Model {0} doesn't inherit from {1}", type.Name, typeof(KalturaOTTObject).Name));
            }
        }
    }
}
