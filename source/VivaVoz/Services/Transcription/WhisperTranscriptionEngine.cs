using System.Text;

using Whisper.net;

namespace VivaVoz.Services.Transcription;

public sealed class WhisperTranscriptionEngine : ITranscriptionEngine, IDisposable {
    private const string _defaultModelId = "tiny";
    private static readonly string[] _supportedLanguages = [
        "auto", "en", "zh", "de", "es", "ru", "ko", "fr", "ja", "pt", "tr",
        "pl", "ca", "nl", "ar", "sv", "it", "id", "hi", "fi", "vi", "he",
        "uk", "el", "ms", "cs", "ro", "da", "hu", "ta", "no", "th", "ur",
        "hr", "bg", "lt", "la", "mi", "ml", "cy", "sk", "te", "fa", "lv",
        "bn", "sr", "az", "sl", "kn", "et", "mk", "br", "eu", "is", "hy",
        "ne", "mn", "bs", "kk", "sq", "sw", "gl", "mr", "pa", "si", "km",
        "sn", "yo", "so", "af", "oc", "ka", "be", "tg", "sd", "gu", "am",
        "yi", "lo", "uz", "fo", "ht", "ps", "tk", "nn", "mt", "sa", "lb",
        "my", "bo", "tl", "mg", "as", "tt", "haw", "ln", "ha", "ba", "jw",
        "su",
    ];

    private readonly WhisperModelManager _modelManager;
    private readonly Lock _sync = new();
    private WhisperFactory? _factory;
    private string? _loadedModelPath;

    public WhisperTranscriptionEngine(WhisperModelManager modelManager) {
        ArgumentNullException.ThrowIfNull(modelManager);
        _modelManager = modelManager;
    }

    public IReadOnlyList<string> SupportedLanguages => _supportedLanguages;

    public bool IsAvailable {
        get {
            lock (_sync) {
                return _factory is not null || _modelManager.IsModelDownloaded(_defaultModelId);
            }
        }
    }

    public async Task<TranscriptionResult> TranscribeAsync(
        string audioFilePath,
        TranscriptionOptions options,
        CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioFilePath);

        if (!File.Exists(audioFilePath)) {
            throw new FileNotFoundException($"Audio file not found: '{audioFilePath}'.", audioFilePath);
        }

        var modelId = options.ModelId ?? _defaultModelId;
        var language = options.Language ?? "auto";

        var modelPath = await _modelManager.EnsureModelAsync(modelId, cancellationToken).ConfigureAwait(false);
        var factory = GetOrCreateFactory(modelPath);

        Log.Debug("[WhisperEngine] Transcribing '{FilePath}' with model '{ModelId}', language '{Language}'.",
            audioFilePath, modelId, language);

        var stopwatch = Stopwatch.StartNew();

        await using var processor = factory.CreateBuilder()
            .WithLanguage(language)
            .Build();

        await using var fileStream = File.OpenRead(audioFilePath);

        var textBuilder = new StringBuilder();
        var detectedLanguage = language;

        await foreach (var segment in processor.ProcessAsync(fileStream, cancellationToken).ConfigureAwait(false)) {
            textBuilder.Append(segment.Text);
        }

        stopwatch.Stop();

        var text = textBuilder.ToString().Trim();

        Log.Information("[WhisperEngine] Transcription completed in {Elapsed}ms. Length: {Length} chars.",
            stopwatch.ElapsedMilliseconds, text.Length);

        return new TranscriptionResult(
            Text: text,
            DetectedLanguage: detectedLanguage,
            Duration: stopwatch.Elapsed,
            ModelUsed: modelId
        );
    }

    private WhisperFactory GetOrCreateFactory(string modelPath) {
        lock (_sync) {
            if (_factory is not null && _loadedModelPath == modelPath) {
                return _factory;
            }

            _factory?.Dispose();

            Log.Information("[WhisperEngine] Loading model from '{ModelPath}'.", modelPath);
            _factory = WhisperFactory.FromPath(modelPath);
            _loadedModelPath = modelPath;

            return _factory;
        }
    }

    public void Dispose() {
        lock (_sync) {
            _factory?.Dispose();
            _factory = null;
            _loadedModelPath = null;
        }
    }
}
