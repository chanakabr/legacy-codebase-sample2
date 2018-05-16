using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PermissionsDeployment
{
    class Program
    {
        private const string IMPORT_FILE_NAME = "import";
        private const string EXPORT_FILE_NAME = "export";
        private const string DELETE_FILE_NAME = "delete";
        private const string HELP = "help";

        static void Main(string[] args)
        {
            bool result = false;

            Dictionary<string, string> arguments = ResolveArguments(args);

            string fileName = string.Empty;

            if (arguments == null || arguments.Count == 0 || arguments.ContainsKey(HELP))
            {

                string version = string.Empty;

                try
                {
                    version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch
                {
                }

                Console.WriteLine("Permissions deployment tool.");

                if (!string.IsNullOrEmpty(version))
                {
                    Console.WriteLine(string.Format("Current version is {0}", version));
                }

                Console.WriteLine("Possible command line arguments, which are not case sensitive:");
                Console.WriteLine("export: File path to export permissions data into. Shortcut: e");
                Console.WriteLine("import: File path to import permissions data from. Shortcut: i");
                Console.WriteLine("delete: File path to delete permissions data from. Shortcut: d");
            }
            if (arguments.ContainsKey(IMPORT_FILE_NAME))
            {
                fileName = arguments[IMPORT_FILE_NAME];
                result = PermissionsManager.PermissionsManager.Import(fileName);
            }
            else if (arguments.ContainsKey(EXPORT_FILE_NAME))
            {
                fileName = arguments[EXPORT_FILE_NAME];
                result = PermissionsManager.PermissionsManager.Export(fileName);
            }
            else if (arguments.ContainsKey(DELETE_FILE_NAME))
            {
                fileName = arguments[DELETE_FILE_NAME];
                result = PermissionsManager.PermissionsManager.Delete(fileName);
            }
            
            if (result)
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

                        if (key == "i")
                        {
                            key = IMPORT_FILE_NAME;
                        }
                        else if (key == "e")
                        {
                            key = EXPORT_FILE_NAME;
                        }
                        else if (key == "d")
                        {
                            key = DELETE_FILE_NAME;
                        }
                        else if (key == "h")
                        {
                            key = HELP;
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
