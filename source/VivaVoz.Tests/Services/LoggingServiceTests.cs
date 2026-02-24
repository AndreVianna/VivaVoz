using AwesomeAssertions;

using Serilog;

using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class LoggingServiceTests {
    [Fact]
    public void Configure_ShouldNotThrow() {
        var act = LoggingService.Configure;

        act.Should().NotThrow();
    }

    [Fact]
    public void Configure_ShouldSetNonSilentLogger() {
        LoggingService.Configure();

        Log.Logger.GetType().Name.Should().NotBe("SilentLogger");
    }

    [Fact]
    public void CloseAndFlush_AfterConfigure_ShouldNotThrow() {
        LoggingService.Configure();

        var act = Log.CloseAndFlush;

        act.Should().NotThrow();
    }
}
