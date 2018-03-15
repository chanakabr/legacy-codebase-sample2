using ConfigurationManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter application (or press enter if you wish to use app.config): ");
            string application = Console.ReadLine();
            Console.WriteLine("Enter host (or press enter if you wish to use app.config): ");
            string host = Console.ReadLine();
            Console.WriteLine("Enter environment (or press enter if you wish to use app.config):");
            string environment = Console.ReadLine();

            bool valid = ApplicationConfiguration.Validate(application, host, environment);

            Console.Read();

            if (valid)
                Environment.Exit(0);

            Environment.Exit(-1);
        }
    }
}
