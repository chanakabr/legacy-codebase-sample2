using CommandLine;
using ConfigurationManager;
using ElasticSearch.Common;
using KLogMonitor;
using Polly;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ElasticsearchIndexCleaner
{

    public class Program
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public class Options
        {
            [Option('g', "groupId", HelpText = "GroupId", Required = true, Min = 1, Separator = ',')]
            public IEnumerable<int> GroupIds { get; set; }

            [Option('s', "save last X indexes", HelpText = "if set won't delete the last X indexes", Required = false)]
            public int SaveLastXIndexes { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
           .WithParsed(o =>
           {
               KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"./");
               ApplicationConfiguration.Init();

               var cleaner = new IndexCleaner();
               cleaner.Clean(o.GroupIds, o.SaveLastXIndexes);

           });
        }
    }
}
