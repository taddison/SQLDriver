# SQLDriver
Benchmark and load test MS SQL Server.

## Example Usage
```
SQLDriver -r 5 -t 10 - c"server=localhost;initial catalog=master;integrated security=SSPI" -s "select @@servername"

SQLDriver -r 5 -t 10 -c "server=localhost;initial catalog=master;integrated security=SSPI" -s "select @@servername" -o "c:\results.csv"
```

Both of these commands will:
- Use 10 threads
- To execute the specified command (select @@servername) 5 times (50 times in total)
- On the server localhost
- Ouput percentile timings for the run, as well as the number of errors

The second command also:
- Outputs the timings (ms per execution) to the file c:\results.csv

Run the program without any parameters for a list of all options.

The program will also print summary statistics once execution is complete:
![sample output](/SampleOutput.png)

## Building the project
Clone the repository and execute a build.  Tested in Visual Studio 2017 on Windows 10.

Optionally run an ILMerge on the final output:
```ILMerge SQLDriver.exe CommandLine.dll /out:SQLDriverMerged.exe```

## Motivation
Although there are plenty of other tools out there (OStress, HammerDB, SQLQueryStress - to name just a few) none of them were an exact fit for what I'm trying to do in a [benchmarking project](https://github.com/taddison/sql-tables-as-queue-benchmarks), so I built this tool and I'm open sourcing it in the hope someone else will benefit from it.

## Benchmarking
Using the MinimalOutput argument (-m) SQLDriver can be used to produce summary data.  The following script runs the benchmark 10 times and outputs the results to a csv.

```powershell
"id,threads,repeats,command,duration,completed,failed,median,p90,p95,p99,p999,max" | Out-File results.csv

for($threads = 1; $threads -le 8; $threads++) {
    .\SQLDriver.exe -r 5 -t $threads -c "server=localhost;initial catalog=master;integrated security=sspi" -s "select @@servername" -m -i "sample" *>> results.csv
}
```

The CSV can then be analysed in Power BI, Excel, etc.

## Contributing
Contributions welcome, please raise an issue before submitting a PR.
