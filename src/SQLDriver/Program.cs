using System;
using System.Diagnostics;
using System.Linq;
using CommandLine;

namespace SQLDriver
{
    class Program
    {
        static Options _options;

        static void Main(string[] args)
        {
            _options = new Options();
            Parser.Default.ParseArgumentsStrict(args, _options, () => Environment.Exit(-2));

            // Run benchmark
            WriteOutput("Initialising test runner and opening connections...");
            var runner = new SQLRunner(_options.NumberOfThreads, _options.NumberOfRepetitionsPerThread, _options.TargetServerConnectionString, _options.CommandText);
            WriteOutput("Done");
            WriteOutput($"Running load test with {_options.NumberOfThreads} threads and {_options.NumberOfRepetitionsPerThread} repetitions per thread...");

            var sw = new Stopwatch();
            sw.Start();
            runner.Run();
            sw.Stop();

            WriteOutput("Done");

            // Gather results & output
            var results = runner.GetTimingsArray();
            var failures = runner.GetFailureArray();
            Array.Sort(results);

            // Unreliable with < 1000 results for 99.9 percentile
            var length = results.Length;
            var median = results[length / 2];
            var eightyPercentile = results[(length / 10) * 8];
            var ninetyPercentile = results[(length / 10) * 9];
            var ninetyFivePercentile = results[(int)(length / 100.0 * 95)];
            var ninetyNinePercentile = results[(int)(length / 100.0 * 99)];
            var ninetyNineNinePercentile = results[(int)(length / 1000.0 * 999)];

            var failureCount = failures.Count(isFailure => isFailure);
            var failurePercentage = failureCount / (float)length;

            WriteOutput();
            WriteOutput($"Completed {results.Length} executions in {sw.ElapsedMilliseconds}ms for benchmark ref {_options.Id}");
            WriteOutput($"{failureCount} errors ({failurePercentage:P0})");
            WriteOutput($"Min: {results[0]} | Avg: {results.Average():N0} | Max: {results[length-1]}");
            WriteOutput();
            WriteOutput("  50P  |  80P  |  90P  |  95P  |  99P  | 99.9P");
            WriteOutput($"{median,7}|{eightyPercentile,7}|{ninetyPercentile,7}|{ninetyFivePercentile,7}|{ninetyNinePercentile,7}|{ninetyNineNinePercentile,7}");
            WriteOutput();

            if(_options.MinimalOutput)
            {
                WriteMinimalOutput(sw.ElapsedMilliseconds, results.Length, failureCount, median, ninetyPercentile, ninetyFivePercentile, ninetyNinePercentile, ninetyNineNinePercentile, results[length-1]);
            }

            if(!String.IsNullOrEmpty(_options.OutputFilePath))
            {
                var resultsAsString = results.Select(i => i.ToString()).ToArray();

                try
                {
                    System.IO.File.WriteAllLines(_options.OutputFilePath, resultsAsString);
                    WriteOutput($"Wrote all results to {_options.OutputFilePath}");
                }
                catch (Exception e)
                {
                    WriteOutput("Exception writing output.");
                    WriteOutput(e.Message);
                }
            }

            if(_options.Wait)
            {
                WriteOutput("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void WriteMinimalOutput(long duration, int completed, int failed, int median, int p90, int p95, int p99, int p999, int max)
        {
            Console.Write($"{_options.Id},{_options.NumberOfThreads},{_options.NumberOfRepetitionsPerThread},\"{_options.CommandText}\",{duration},{completed},{failed},{median},{p90},{p95},{p99},{p999},{max}");
        }

        private static void WriteOutput()
        {
            WriteOutput(string.Empty);
        }

        private static void WriteOutput(string output)
        {
            if(_options.MinimalOutput)
            {
                return;
            }
            Console.WriteLine(output);
        }
    }
}
