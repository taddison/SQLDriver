using System;
using System.Data.SqlClient;
using System.Diagnostics;
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
        private SqlConnection[] _connections;
        private SqlCommand[] _commands;

        public SQLRunner(int numberOfThreads, int numberOfRepetitionsPerThread, string targetServerConnectionString, string commandText)
        {
            _numberOfThreads = numberOfThreads;
            _numberOfRepetitionsPerThread = numberOfRepetitionsPerThread;
            _connectionString = targetServerConnectionString;
            _commandText = commandText;

            _threadTimings = new int[numberOfThreads][];
            _connections = new SqlConnection[numberOfThreads];
            _commands = new SqlCommand[numberOfThreads];
            
            for(var i = 0; i < _threadTimings.Length; i++)
            {
                _threadTimings[i] = new int[numberOfRepetitionsPerThread];
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

        internal void Run()
        {
            // Execute workload on required number of threads, and capture details
            Parallel.For(0, _numberOfThreads, threadId =>
            {
                RunThread(threadId);
            });
        }

        private void RunThread(int threadId)
        {
            var sw = new Stopwatch();
            var command = _commands[threadId];

            for(var i = 0; i < _numberOfRepetitionsPerThread; i++)
            {
                sw.Restart();
                // TODO: What to do with success?
                var success = RunRepetition(command);
                sw.Stop();
                _threadTimings[threadId][i] = (int)sw.ElapsedMilliseconds;
            }

        }

        private bool RunRepetition(SqlCommand command)
        {
            var random = new Random();
            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}