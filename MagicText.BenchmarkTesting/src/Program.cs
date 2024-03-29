using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using System;

namespace MagicText.BenchmarkTesting
{
    public static class Program
    {
        public const int ExitSuccess = 0;
        public const int ExitFailure = -1;

        public static Int32 Main(String[]? args = null)
        {
            BenchmarkRunner.Run<StreamBenchmarking>();

            return ExitSuccess;
        }
    }
}
