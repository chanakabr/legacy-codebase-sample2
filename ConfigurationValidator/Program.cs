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
        private const string APPLICATION = "application";
        private const string HOST = "host";
        private const string ENVIRONMENT = "environment";
        private const string OUTPUT_FILE = "outputfile";
        private const string MIGRATE = "migrate";

        static void Main(string[] args)
        {
            Dictionary<string, string> arguments = ResolveArguments(args);

            string application = string.Empty;
            string host = string.Empty;
            string environment = string.Empty;
            string outputFile = string.Empty;
            string migrate = string.Empty;

            if (arguments.ContainsKey("help") || arguments.ContainsKey("h"))
            {
                Console.WriteLine("The purpose of this application is to validation the configuration in TCM.");
                Console.WriteLine("Possible command line arguments, which are not case sensitive::");
                Console.WriteLine("host: TCM value for host. If empty, app.config value will be used. Shortcut: o");
                Console.WriteLine("application: TCM value for application. If empty, app.config value will be used. Shortcut: a");
                Console.WriteLine("environemnt: TCM value for environment. If empty, app.config value will be used. Shortcut: e");
                Console.WriteLine("interactive: If application/host/environment shall be defined during runtime. Shortcut: i");
                Console.WriteLine("wait: If validator shall wait for key press when finishing validatiog. Shortcut: w");
                Console.WriteLine("outputfile: If validator should write its output in a file, this is the file path. Shortcut: f");
                Console.WriteLine("migrate: If validator should create a JSON-formatted file which shows the changes from the previous version. Shortcut: m");

                Environment.Exit(0);
            }
            else if (arguments.ContainsKey("interactive") || arguments.ContainsKey("i"))
            {
                Console.WriteLine("Enter application (or press enter if you wish to use app.config): ");
                application = Console.ReadLine();
                Console.WriteLine("Enter host (or press enter if you wish to use app.config): ");
                host = Console.ReadLine();
                Console.WriteLine("Enter environment (or press enter if you wish to use app.config):");
                environment = Console.ReadLine();
            }
            else
            {
                if (arguments.ContainsKey(APPLICATION))
                {
                    application = arguments[APPLICATION];
                }

                if (arguments.ContainsKey(HOST))
                {
                    host = arguments[HOST];
                }

                if (arguments.ContainsKey(ENVIRONMENT))
                {
                    environment = arguments[ENVIRONMENT];
                }
            }

            if (arguments.ContainsKey(OUTPUT_FILE))
            {
                outputFile = arguments[OUTPUT_FILE];
            }

            bool valid = ApplicationConfiguration.Validate(application, host, environment, outputFile);

            if (arguments.ContainsKey(MIGRATE))
            {
                migrate = arguments[MIGRATE];

                ApplicationConfiguration.Migrate(migrate);
            }

            if (arguments.ContainsKey("wait") || arguments.ContainsKey("w"))
            {
                Console.Read();
            }

            if (valid)
                Environment.Exit(0);

            Environment.Exit(-1);
        }

        private static Dictionary<string, string> ResolveArguments(string[] args)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (args != null && args.Length > 0)
            {
                foreach (string argument in args)
                {
                    int index = argument.IndexOf('=');

                    string key = string.Empty;
                    string value = string.Empty;

                    if (index > 0)
                    {
                        key = argument.Substring(0, index).Trim().ToLower();
                        value = argument.Substring(index + 1).Trim();

                        if (key == "a")
                        {
                            key = APPLICATION;
                        }
                        else if (key == "o")
                        {
                            key = HOST;
                        }
                        else if (key == "e")
                        {
                            key = ENVIRONMENT;
                        }
                        else if (key == "f")
                        {
                            key = OUTPUT_FILE;
                        }
                        else if (key == "m")
                        {
                            key = MIGRATE;
                        }
                    }
                    else
                    {
                        key = argument.Trim().ToLower();
                    }

                    result[key] = value;
                }
            }

            return result;
        }
    }
}
