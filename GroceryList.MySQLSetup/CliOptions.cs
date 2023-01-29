using CommandLine;
using System;

namespace GroceryList.MySQLSetup
{
    public class CliOptions
    {
        [Option('d', "data", Required = false, HelpText = "Path to previous data files (database) to load.")]
        public string? DataFilesPath { get; set; }

        [Option('h', "homeid", Required = true, HelpText = "Default Home ID to be created for app.")]
        public string? HomeId { get; set; }

        public string? Connection { get; set; }
    }
}
