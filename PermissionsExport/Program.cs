using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionsDeployment
{
    class Program
    {
        private const string IMPORT_FILE_NAME = "import";
        private const string EXPORT_FILE_NAME = "export";

        static void Main(string[] args)
        {
            Dictionary<string, string> arguments = ResolveArguments(args);

            string importFileName = string.Empty;
            string exportFileName = string.Empty;

            if (arguments.ContainsKey(IMPORT_FILE_NAME))
            {
                importFileName = arguments[IMPORT_FILE_NAME];
            }
            else if (arguments.ContainsKey(EXPORT_FILE_NAME))
            {
                exportFileName = arguments[EXPORT_FILE_NAME];
            }

            bool result = false;

            if (!string.IsNullOrEmpty(exportFileName))
            {
                result = PermissionsManager.PermissionsManager.Export(exportFileName);
            }
            else if (!string.IsNullOrEmpty(importFileName))
            {
                result = PermissionsManager.PermissionsManager.Import(importFileName);
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
                        //else if (key == "e")
                        //{
                        //    key = ENVIRONMENT;
                        //}
                        //else if (key == "f")
                        //{
                        //    key = OUTPUT_FILE;
                        //}
                        //else if (key == "m")
                        //{
                        //    key = MIGRATE;
                        //}
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
