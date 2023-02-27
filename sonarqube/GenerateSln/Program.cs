using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace GenerateSln
{
    class Program
    {
        static void Main(string[] args)
        {
            var workingDir = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
            if (!Directory.Exists(workingDir))
            {
                Console.WriteLine($"Backend directory [{workingDir}] doesn't exist");
                return;
            }

            workingDir += Path.DirectorySeparatorChar;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Backend directory [{workingDir}]");
            Console.ResetColor();

            var projects = GetProjects(workingDir).ToList();
            //GenerateCSV(projects, workingDir);

            var exclude = new[] { "ApiLogicNetCore", "ImageManagerCore", "GenerateSln" };
            var dotnetCoreProjects = projects.Where(_ => _.NetCore && !exclude.Any(e => _.fileName.Contains(e)) );
            var netCoreSln = GenerateSln("ott-backend-netcore", Path.Combine("sonarqube", "netcore"), workingDir, dotnetCoreProjects);
            //Validate(netCoreSln, workingDir);

            var exclude2 = new [] { "ApiLogicNetCore", "ImageManagerCore", "Tester", Path.Combine("tvpapi","QueueWrapper"), "Tvinci.Web.Controls", "Tvinci.Web.TVS.Controls",
                "ESIndexRebuildHandler", "ESIndexUpdateHandler", "EpgFeeder", "CommonWithSL", "PermissionsDeployment", Path.Combine("tvmapps", "enums"), 
                "SeriesRecordingTaskHandler1", "BuzzFeeder", Path.Combine("PendingTransactionHandler", "PendingTransactionHandler"),
                "PendingChargeHandler", "ExpiredRecordingsHandler", "GenerateSln"};
            var dotnetFrameworkProjects = projects.Where(_ => _.NetFramework && !exclude2.Any(e => _.fileName.Contains(e, StringComparison.OrdinalIgnoreCase)));
            var netFrameworkSln = GenerateSln("ott-backend-netframework", Path.Combine("sonarqube", "netframework"), workingDir, dotnetFrameworkProjects);
            //Validate(netFrameworkSln, workingDir);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Finished");
            Console.ResetColor();
        }

        private static IEnumerable<Project> GetProjects(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories);
            foreach (var fileName in files)
            {
                var shortName = fileName.Replace(directory, "");
                var folderName = shortName.Split(Path.DirectorySeparatorChar)[0];
                var content = File.ReadAllText(fileName);

                bool isNetFramework = false;
                bool isNetCore = false;
                if (content.Contains(">v4.8<") || content.Contains(">v4.0<") || content.Contains(">v3.5<") || content.Contains("net48"))
                {
                    isNetFramework = true;
                }
                if (content.Contains("netcoreapp3.1"))
                {
                    isNetCore = true;
                }

                if (!isNetCore && !isNetFramework)
                {
                    Console.WriteLine("Unknown target framework: " + fileName);
                }
                yield return new Project
                {
                    folderName = folderName,
                    fileName = shortName,
                    NetCore = isNetCore,
                    NetFramework = isNetFramework
                };
            }
        }

        private static void GenerateCSV(IEnumerable<Project> projects, string workingDir)
        {
            var lines = new List<string> { "Folder,Filename,NetCore,NetFramework", "ccproxy,TVPAPI_AsyncApiProxy,nodejs" };

            foreach (var project in projects)
            {
                lines.Add($"{project.folderName},{project.fileName},{project.NetCore},{project.NetFramework}");
            }

            File.WriteAllLines(Path.Combine(workingDir, "projects.csv"), lines);
        }

        private const string dotnet = "dotnet";
        // dotnet sln[< SOLUTION_FILE >] add[--in-root][-s | --solution-folder<PATH>] < PROJECT_PATH > [< PROJECT_PATH > ...]
        private static string GenerateSln(string solutionFileName, string solutionFolder, string workingDir, IEnumerable<Project> projects)
        {
            Directory.CreateDirectory(Path.Combine(workingDir, solutionFolder));
            var newSln = @$"new sln -n {solutionFileName} --force";
            Process.Start(new ProcessStartInfo(dotnet, newSln) { WorkingDirectory = Path.Combine(workingDir, solutionFolder) }).WaitForExit();

            var solutionFilePath = Path.Combine(solutionFolder, $"{solutionFileName}.sln");
            foreach (var group in projects.GroupBy(_ => _.folderName))
            {
                var addProjects = @$"sln {solutionFilePath} add -s {group.Key} {string.Join(' ', group.Select(_ => "\"" + _.fileName + "\""))}";
                Process.Start(new ProcessStartInfo(dotnet, addProjects) { WorkingDirectory = workingDir }).WaitForExit();
            }
            return solutionFilePath;
        }

        private static void Validate(string solutionFile, string workingDir)
        {
            var p = Process.Start(new ProcessStartInfo(dotnet, $"build {solutionFile}") { WorkingDirectory = workingDir });
            p.WaitForExit();
            Debug.Assert(p.ExitCode == 0);
        }
    }

    public class Project
    {
        public string folderName;
        public string fileName;
        public bool NetCore;
        public bool NetFramework;
    }
}