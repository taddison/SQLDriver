# SQLDriver
Benchmark and load test MS SQL Server.

## Example Usage
```SQLDriver -r 5 -t 10 - c"server=localhost;initial catalog=master;integrated security=SSPI" -s "select @@servername"

SQLDriver -r 5 -t 10 -c "server=localhost;initial catalog=master;integrated security=SSPI" -s "select @@servername" -o "c:\results.csv"```

Both of these commands will:
- Use 10 threads
- To execute the specified command (select @@servername) 5 times (50 times in total)
- On the server localhost
- Ouput percentile timings for the run, as well as the number of errors
The second command also
- Outputs the timings (ms per execution) to the file c:\results.csv

Run the program without any parameters for a list of all options.

The program will also print summary statistics once execution is complete:
![sample output](/SampleOutput.png)

## Building the project
Clone the repository and execute a build.  Tested in Visual Studio 2015 on Windows 10.

Optionally run an ILMerge on the final output:
```ILMerge SQLDriver.exe CommandLine.dll /out:SQLDriverMerged.exe```

## Motivation
Although there are plenty of other tools out there (OStress, HammerDB, SQLQueryStress - to name just a few) none of them were an exact fit for what I'm trying to do in a [benchmarking project](https://github.com/taddison/sql-tables-as-queue-benchmarks), so I built this tool and I'm open sourcing it in the hope someone else will benefit from it.

## Contributing
Contributions welcome, though please file an issue first rather than dropping a stealth PR directly.
