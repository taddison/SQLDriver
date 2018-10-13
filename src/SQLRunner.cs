using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SQLDriver
{
    internal class SQLRunner
    {
        private int _numberOfThreads;
        private int _numberOfRepetitionsPerThread;
        private string _connectionString;
        private string _commandText;

        // Threads x Number of Executions
        // For each thread, store the time taken to complete the workload
        private int[][] _threadTimings;
        // Store any failures
        private bool[][] _failures;
        private SqlConnection[] _connections;
        private SqlCommand[] _commands;

        public SQLRunner(int numberOfThreads, int numberOfRepetitionsPerThread, string targetServerConnectionString, string commandText)
        {
            _numberOfThreads = numberOfThreads;
            _numberOfRepetitionsPerThread = numberOfRepetitionsPerThread;
            _connectionString = targetServerConnectionString;
            _commandText = commandText;

            _threadTimings = new int[numberOfThreads][];
            _failures = new bool[numberOfThreads][];
            _connections = new SqlConnection[numberOfThreads];
            _commands = new SqlCommand[numberOfThreads];
            
            for(var i = 0; i < _threadTimings.Length; i++)
            {
                _threadTimings[i] = new int[numberOfRepetitionsPerThread];
                _failures[i] = new bool[_numberOfRepetitionsPerThread];
                _connections[i] = new SqlConnection(_connectionString);
                _connections[i].Open();
                _commands[i] = new SqlCommand(_commandText, _connections[i]);
            }
        }

        internal int[] GetTimingsArray()
        {
            var timingsArray = new int[_numberOfRepetitionsPerThread * _numberOfThreads];

            for (var threadId = 0; threadId < _numberOfThreads; threadId++)
            {
                _threadTimings[threadId].CopyTo(timingsArray, threadId * _numberOfRepetitionsPerThread);
            }

            return timingsArray;
        }

        internal bool[] GetFailureArray()
        {
            var failureArray = new bool[_numberOfRepetitionsPerThread * _numberOfThreads];
            
            for (var threadId = 0; threadId < _numberOfThreads; threadId++)
            {
                _failures[threadId].CopyTo(failureArray, threadId * _numberOfRepetitionsPerThread);
            }

            return failureArray;
        }

        internal async Task Run()
        {
            var threads = Enumerable.Range(0,_numberOfThreads)
                    .Select(i => RunThread(i))
                    .ToArray();
            
            await Task.WhenAll(threads);
        }

        private async Task RunThread(int threadId)
        {
            var sw = new Stopwatch();
            var command = _commands[threadId];

            for(var i = 0; i < _numberOfRepetitionsPerThread; i++)
            {
                sw.Restart();
                var success = await RunRepetition(command);
                sw.Stop();
                _threadTimings[threadId][i] = (int)sw.ElapsedMilliseconds;
                _failures[threadId][i] = !success;
            }

        }

        private async Task<bool> RunRepetition(SqlCommand command)
        {
            var random = new Random();
            try
            {
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}