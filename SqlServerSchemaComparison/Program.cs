using CommandLine;
using Microsoft.SqlServer.Dac.Compare;
using System;
using System.Linq;

namespace SqlServerSchemaComparison
{
    public class Options
    {
        [Option('s', "source", Required = true, HelpText = "The source .dacpac file")]
        public string? SourceDacPac { get; set; }

        [Option('t', "target", Required = true, HelpText = "The target database connection string")]
        public string? TargetConnectionString { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunSchemaCompare);         
        }

        private static void RunSchemaCompare(Options options)
        {
            if (options.SourceDacPac is null)
                throw new ArgumentNullException("source", "The source .dacpac file is required");

            if (options.TargetConnectionString is null)
                throw new ArgumentNullException("target", "The target database connection string is required");

            var sourceDacpac = new SchemaCompareDacpacEndpoint(options.SourceDacPac);
            var targetDatabase = new SchemaCompareDatabaseEndpoint(options.TargetConnectionString);

            var comparison = new SchemaComparison(sourceDacpac, targetDatabase);

            Console.WriteLine("Running schema comparison...");
            SchemaComparisonResult compareResult = comparison.Compare();

            foreach (SchemaDifference diff in compareResult.Differences)
            {
                string objectType = diff.Name;
                string sourceObject = diff.SourceObject?.Name.ToString() ?? "null";
                string targetObject = diff.TargetObject?.Name.ToString() ?? "null";
                Console.WriteLine($"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");
            }

            if (compareResult.Differences.Count() == 0)
                Console.WriteLine("No differences detected");
        }
    }
}
