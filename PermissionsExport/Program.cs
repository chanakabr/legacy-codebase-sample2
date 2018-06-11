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
        private const string INITIALIZE_FOLDER = "initializefolder";
        private const string LOAD_FOLDER = "loadfolder";
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
                Console.WriteLine("export: File path to export permissions data into in XML format. Shortcut: e");
                Console.WriteLine("import: File path to import permissions data from in XML format. Shortcut: i");
                Console.WriteLine("delete: File path to delete permissions data from in XML format. Shortcut: d");
                Console.WriteLine("initializefolder: Folder path to save all permissions data into in JSON format. Shortcut: n");
                Console.WriteLine("loadfolder: Folder path to load all permissions data from in JSON format. Shortcut: l");
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
            else if (arguments.ContainsKey(INITIALIZE_FOLDER))
            {
                string folderName = arguments[INITIALIZE_FOLDER];
                result = PermissionsManager.PermissionsManager.InitializeFolder(folderName);
            }
            else if (arguments.ContainsKey(LOAD_FOLDER))
            {
                string folderName = arguments[LOAD_FOLDER];
                result = PermissionsManager.PermissionsManager.LoadFolder(folderName);
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
                        else if (key == "n")
                        {
                            key = INITIALIZE_FOLDER;
                        }
                        else if (key == "l")
                        {
                            key = LOAD_FOLDER;
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
