using CommandLine;

namespace SQLDriver
{
    public class Options
    {
        [Option('o', "OutputFilePath", Required = false, HelpText = "Output file path to write all results")]
        public string OutputFilePath { get; set; }

        [Option('m', "MinimalOutput", Required = false, HelpText = "Output a single line of condensed results only")]
        public bool MinimalOutput { get; set; }

        [Option('r', "RepetitionsPerThread", Required = true, HelpText = "Number of times to execute the command per thread")]
        public int NumberOfRepetitionsPerThread { get; set; }

        [Option('t', "NumberOfThreads", Required = true, HelpText = "Number of threads to use")]
        public int NumberOfThreads { get; set; }

        [Option('c', "ConnectionString", Required = true, HelpText = "Connection string of target SQL server")]
        public string TargetServerConnectionString { get; set; }

        [Option('s', "SQL", Required = true, HelpText = "The SQL to execute on the target server")]
        public string CommandText { get; set; }
        
        [Option('w', "Wait", Required = false, DefaultValue = false, HelpText = "Whether the process should wait for user input before terminating after a benchmark run")]
        public bool Wait { get; set; }

        [Option('i', "Id", Required = false, DefaultValue = "", HelpText = "Benchmark Id, output as part of the results")]
        public string Id { get; set; }
    }
}
