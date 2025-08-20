using BenchmarkDotNet.Running;

namespace Softalleys.Utilities.Commands.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<InvokerBenchmarks>();
    }
}
