namespace VivaVoz.Services.Audio;

public sealed class MicrophoneNotFoundException : Exception {
    public MicrophoneNotFoundException() {
    }

    public MicrophoneNotFoundException(string message)
        : base(message) {
    }

    public MicrophoneNotFoundException(string message, Exception innerException)
        : base(message, innerException) {
    }
}
