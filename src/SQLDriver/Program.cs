using System;
using System.Diagnostics;
using System.Linq;
using CommandLine;

namespace SQLDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArgumentsStrict(args, options, () => Environment.Exit(-2));
            
            // Run benchmark
            Console.Write("Initialising test runner and opening connections...");
            var runner = new SQLRunner(options.NumberOfThreads, options.NumberOfRepetitionsPerThread, options.TargetServerConnectionString, options.CommandText);
            Console.WriteLine("Done");
            Console.Write($"Running load test with {options.NumberOfThreads} threads and {options.NumberOfRepetitionsPerThread} repetitions per thread...");

            var sw = new Stopwatch();
            sw.Start();
            runner.Run();
            sw.Stop();

            Console.WriteLine("Done");

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
            var failurePercentage = failureCount / length;

            Console.WriteLine();
            Console.WriteLine($"Completed {results.Length} executions in {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"{failureCount} errors ({failurePercentage:P0})");
            Console.WriteLine($"Min: {results[0]} | Avg: {results.Average():N0} | Max: {results[length-1]}");
            Console.WriteLine();
            Console.WriteLine("  50P  |  80P  |  90P  |  95P  |  99P  | 99.9P");
            Console.WriteLine($"{median,7}|{eightyPercentile,7}|{ninetyPercentile,7}|{ninetyFivePercentile,7}|{ninetyNinePercentile,7}|{ninetyNineNinePercentile,7}");
            Console.WriteLine();

            if(!String.IsNullOrEmpty(options.OutputFilePath))
            {
                var resultsAsString = results.Select(i => i.ToString()).ToArray();

                try
                {
                    System.IO.File.WriteAllLines(options.OutputFilePath, resultsAsString);
                    Console.WriteLine($"Wrote all results to {options.OutputFilePath}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception writing output.");
                    Console.WriteLine(e.Message);
                }
            }

            if(options.Wait)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
    }
}
