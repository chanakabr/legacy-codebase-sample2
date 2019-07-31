using CommandLine;
using ConfigurationManager;
using ElasticSearch.Common;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ElasticsearchIndexCleaner
{
    public class Program
    {
        private static KLogger _Logger;
        private static ElasticSearchApi _ESClient;

        public class Options
        {
            [Option('g', "groupId", HelpText = "GroupId", Required = true, Min = 1, Separator = ',')]
            public IEnumerable<int> GroupIds { get; set; }

            [Option('c', "commit", HelpText = "if set to false then no changes will be made (fry run)", Required = false, Default = false)]
            public bool Commit { get; set; }

            [Option('s', "save last X indexes", HelpText = "if set won't delete the last X indexes", Required = false)]
            public int SaveLastXIndexes { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
           .WithParsed(o =>
           {
               KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"C:\log\ElasticsearchIndexCleaner\");
               ApplicationConfiguration.Initialize();
               _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
               _ESClient = new ElasticSearchApi();

               foreach (var groupId in o.GroupIds)
               {
                   _Logger.Info($"Starting ElasticsearchIndexCleaner for gropu:[{groupId}]");
                   var indices = _ESClient.ListIndices($"{groupId}_epg_*");
                   var indicesToDelete = indices.Where(i => !i.Aliases.Any()).Select(i => i.Name).ToList();

                   if (o.SaveLastXIndexes < indicesToDelete.Count)
                   {
                       var counter = indicesToDelete.ToDictionary(x => int.Parse(x.Split('_').Last()), y => y);
                       var sorted = new SortedDictionary<int,string>(counter, Comparer<int>.Default);
                       indicesToDelete = sorted.Take(indicesToDelete.Count - o.SaveLastXIndexes).Select(x=>x.Value).ToList();
                   }

                   _Logger.Info($"Deleting indices:[{string.Join(",", indicesToDelete)}] ");
                   if (o.Commit)
                   {
                       _ESClient.DeleteIndices(indicesToDelete);
                       _Logger.Info($"Deleted!");
                   }
               }
           });
        }
    }
}
