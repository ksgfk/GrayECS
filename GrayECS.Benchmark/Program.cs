using BenchmarkDotNet.Running;

namespace GrayECS.Benchmark
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkEcs>();
        }
    }
}