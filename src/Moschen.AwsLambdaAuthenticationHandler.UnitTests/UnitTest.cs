using Serilog;
using Xunit.Abstractions;

namespace Moschen.AwsLambdaAuthenticationHandler.UnitTests
{
    public abstract class UnitTest
    {
        protected ITestOutputHelper Output { get; }
        protected ILogger Logger { get; }

        protected UnitTest(ITestOutputHelper output)
        {
            Output = output;
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Xunit(Output);

            Logger = loggerConfig.CreateLogger();
        }
    }
}
