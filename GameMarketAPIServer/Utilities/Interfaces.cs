using Xunit.Abstractions;

namespace GameMarketAPIServer.Utilities
{
    public class Interfaces
    {
    }
    public interface ILogger
    {
        public void WriteLine(string message);
    }
    public class ConsoleLogger : ILogger
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
    public class TestOutputHelperLogger : ILogger
    {
        private readonly ITestOutputHelper output;
        public TestOutputHelperLogger(ITestOutputHelper outputHelper) { output = outputHelper; }
        public void WriteLine(string message) {
            output.WriteLine(message);
        }
    }
}
