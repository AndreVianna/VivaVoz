using AwesomeAssertions;

using VivaVoz.Services.Audio;

using Xunit;

namespace VivaVoz.Tests.Services.Audio;

public class MicrophoneNotFoundExceptionTests {
    [Fact]
    public void DefaultConstructor_ShouldCreateException() {
        var exception = new MicrophoneNotFoundException();

        exception.Should().NotBeNull();
    }

    [Fact]
    public void MessageConstructor_ShouldSetMessage() {
        var exception = new MicrophoneNotFoundException("Missing");

        exception.Message.Should().Be("Missing");
    }

    [Fact]
    public void MessageAndInnerConstructor_ShouldSetBothProperties() {
        var inner = new InvalidOperationException("Inner");

        var exception = new MicrophoneNotFoundException("Missing", inner);

        exception.Message.Should().Be("Missing");
        exception.InnerException.Should().BeSameAs(inner);
    }
}
