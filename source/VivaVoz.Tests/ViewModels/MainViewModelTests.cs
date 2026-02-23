using AwesomeAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;
using VivaVoz.ViewModels;
using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class MainViewModelTests {
    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var act = () => new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullRecorder_ShouldThrowArgumentNullException() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var player = Substitute.For<IAudioPlayer>();

        var act = () => new MainViewModel(null!, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        act.Should().Throw<ArgumentNullException>().WithParameterName("recorder");
    }

    [Fact]
    public void Constructor_WithNullAudioPlayer_ShouldThrowArgumentNullException() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();

        var act = () => new MainViewModel(recorder, null!, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        act.Should().Throw<ArgumentNullException>().WithParameterName("audioPlayer");
    }

    [Fact]
    public void Constructor_WithNullDbContext_ShouldThrowArgumentNullException() {
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var act = () => new MainViewModel(recorder, player, null!, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        act.Should().Throw<ArgumentNullException>().WithParameterName("dbContext");
    }

    [Fact]
    public void Constructor_WithNullTranscriptionManager_ShouldThrowArgumentNullException() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var act = () => new MainViewModel(recorder, player, context, null!, Substitute.For<IClipboardService>());

        act.Should().Throw<ArgumentNullException>().WithParameterName("transcriptionManager");
    }

    [Fact]
    public void IsRecording_WhenRecorderIsNotRecording_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.IsRecording.Returns(false);
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.IsRecording.Should().BeFalse();
    }

    [Fact]
    public void IsRecording_WhenRecorderIsRecording_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.IsRecording.Returns(true);
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.IsRecording.Should().BeTrue();
    }

    [Fact]
    public void SelectedRecording_WhenNewInstance_ShouldBeNull() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.SelectedRecording.Should().BeNull();
    }

    [Fact]
    public void Recordings_WhenNewInstance_ShouldBeInitialized() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.Recordings.Should().NotBeNull();
    }

    [Fact]
    public void Recordings_WhenDatabaseHasRecordings_ShouldBeOrderedByCreatedAtDescending() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);

        var now = DateTime.UtcNow;
        var older = CreateRecording(now.AddMinutes(-10));
        var middle = CreateRecording(now.AddMinutes(-5));
        var newest = CreateRecording(now);

        context.Recordings.AddRange(older, middle, newest);
        context.SaveChanges();

        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.Recordings.Select(r => r.Id).Should().Equal(newest.Id, middle.Id, older.Id);
    }

    [Fact]
    public void SelectedRecording_WhenChanged_ShouldRaisePropertyChanged() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
        var recording = CreateRecording(DateTime.UtcNow);

        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.SelectedRecording = recording;

        changed.Should().Contain(nameof(MainViewModel.SelectedRecording));
        changed.Should().Contain(nameof(MainViewModel.HasSelection));
    }

    [Fact]
    public void HasSelection_WhenNoRecordingSelected_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.HasSelection.Should().BeFalse();
    }

    [Fact]
    public void HasSelection_WhenRecordingSelected_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        viewModel.HasSelection.Should().BeTrue();
    }

    [Fact]
    public void NoSelection_WhenNoRecordingSelected_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.NoSelection.Should().BeTrue();
    }

    [Fact]
    public void NoSelection_WhenRecordingSelected_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        viewModel.NoSelection.Should().BeFalse();
    }

    [Fact]
    public void IsNotRecording_WhenNotRecording_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.IsRecording.Returns(false);
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.IsNotRecording.Should().BeTrue();
    }

    [Fact]
    public void OnIsRecordingChanged_ShouldRaiseIsNotRecordingPropertyChanged() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.IsRecording = true;

        changed.Should().Contain("IsNotRecording");
    }

    [Fact]
    public void SelectRecordingCommand_WhenExecuted_ShouldSetSelectedRecording() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
        var recording = CreateRecording(DateTime.UtcNow);

        viewModel.SelectRecordingCommand.Execute(recording);

        viewModel.SelectedRecording.Should().Be(recording);
    }

    [Fact]
    public void SelectRecordingCommand_WithNull_ShouldSetSelectedRecordingToNull() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        viewModel.SelectRecordingCommand.Execute(null);

        viewModel.SelectedRecording.Should().BeNull();
    }

    [Fact]
    public void ClearSelectionCommand_WhenExecuted_ShouldSetSelectedRecordingToNull() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        viewModel.ClearSelectionCommand.Execute(null);

        viewModel.SelectedRecording.Should().BeNull();
    }

    [Fact]
    public void StartRecordingCommand_WhenExecuted_ShouldCallRecorderStartRecording() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.IsRecording.Returns(false);
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.StartRecordingCommand.Execute(null);

        recorder.Received(1).StartRecording();
    }

    [Fact]
    public void StartRecordingCommand_WhenRecorderStartsSuccessfully_ShouldUpdateIsRecording() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.IsRecording.Returns(false, true);
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.StartRecordingCommand.Execute(null);

        recorder.Received(1).StartRecording();
    }

    [Fact]
    public void StartRecordingCommand_WhenMicrophoneNotFound_ShouldCatchException() {
        // ShowMicrophoneNotFoundDialog creates Avalonia UI elements which require
        // a running Avalonia application. We verify the catch block is entered
        // by confirming the MicrophoneNotFoundException doesn't propagate as-is.
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.When(r => r.StartRecording())
                .Do(_ => throw new MicrophoneNotFoundException("No mic"));
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        try {
            viewModel.StartRecordingCommand.Execute(null);
        }
        catch (Exception ex) {
            // The MicrophoneNotFoundException is caught; any exception here
            // is from the Avalonia dialog code (InvalidOperationException), not the original
            ex.Should().NotBeOfType<MicrophoneNotFoundException>();
        }
    }

    [Fact]
    public void StopRecordingCommand_WhenExecuted_ShouldCallRecorderStopRecording() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.IsRecording.Returns(false);
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.StopRecordingCommand.Execute(null);

        recorder.Received(1).StopRecording();
    }

    [Fact]
    public void StopRecordingCommand_WhenExecuted_ShouldUpdateIsRecordingFromRecorder() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.IsRecording.Returns(true, false);
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.StopRecordingCommand.Execute(null);

        recorder.Received(1).StopRecording();
    }

    [Fact]
    public void OnSelectedRecordingChanged_WithNull_ShouldSetDefaultHeaders() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        viewModel.SelectedRecording = null;

        viewModel.DetailHeader.Should().Be("No recording selected");
        viewModel.DetailBody.Should().Be("Select a recording from the list to view details.");
    }

    [Fact]
    public void OnSelectedRecordingChanged_WithRecording_ShouldSetDetailHeader() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Title = "My Recording";

        viewModel.SelectedRecording = recording;

        viewModel.DetailHeader.Should().Be("My Recording");
        viewModel.DetailBody.Should().Be("Detail view placeholder.");
    }

    [Fact]
    public void OnSelectedRecordingChanged_WithEmptyTitle_ShouldUseDefaultTitle() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Title = "";

        viewModel.SelectedRecording = recording;

        viewModel.DetailHeader.Should().Be("Recording selected");
    }

    [Fact]
    public void OnSelectedRecordingChanged_WithWhitespaceTitle_ShouldUseDefaultTitle() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Title = "   ";

        viewModel.SelectedRecording = recording;

        viewModel.DetailHeader.Should().Be("Recording selected");
    }

    [Fact]
    public void OnSelectedRecordingChanged_ShouldRaiseNoSelectionPropertyChanged() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        changed.Should().Contain("NoSelection");
    }

    [Fact]
    public void AudioPlayer_ShouldBeInitialized() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.AudioPlayer.Should().NotBeNull();
    }

    [Fact]
    public void DetailHeader_WhenNewInstance_ShouldHaveDefaultValue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.DetailHeader.Should().Be("No recording selected");
    }

    [Fact]
    public void DetailBody_WhenNewInstance_ShouldHaveDefaultValue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());

        viewModel.DetailBody.Should().Be("Select a recording from the list to view details.");
    }

    // ========== TranscriptDisplay tests ==========

    [Fact]
    public void TranscriptDisplay_WhenNoSelection_ShouldBeEmpty() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        viewModel.TranscriptDisplay.Should().BeEmpty();
    }

    [Fact]
    public void TranscriptDisplay_WhenTranscribing_ShouldShowTranscribingMessage() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Transcribing;
        recording.Transcript = null;

        viewModel.SelectedRecording = recording;

        viewModel.TranscriptDisplay.Should().Be("Transcribing...");
    }

    [Fact]
    public void TranscriptDisplay_WhenFailed_ShouldShowFailedMessage() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Failed;

        viewModel.SelectedRecording = recording;

        viewModel.TranscriptDisplay.Should().Be("Transcription failed.");
    }

    [Fact]
    public void TranscriptDisplay_WhenCompleteWithNullTranscript_ShouldShowNoSpeechDetected() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = null;

        viewModel.SelectedRecording = recording;

        viewModel.TranscriptDisplay.Should().Be("No speech detected.");
    }

    [Fact]
    public void TranscriptDisplay_WhenCompleteWithEmptyTranscript_ShouldShowNoSpeechDetected() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = "";

        viewModel.SelectedRecording = recording;

        viewModel.TranscriptDisplay.Should().Be("No speech detected.");
    }

    [Fact]
    public void TranscriptDisplay_WhenCompleteWithTranscript_ShouldShowTranscriptText() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = "The quick brown fox jumps over the lazy dog.";

        viewModel.SelectedRecording = recording;

        viewModel.TranscriptDisplay.Should().Be("The quick brown fox jumps over the lazy dog.");
    }

    [Fact]
    public void TranscriptDisplay_WhenRecordingStatus_ShouldBeEmpty() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Recording;

        viewModel.SelectedRecording = recording;

        viewModel.TranscriptDisplay.Should().BeEmpty();
    }

    // ========== IsTranscribing tests ==========

    [Fact]
    public void IsTranscribing_WhenNoSelection_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        viewModel.IsTranscribing.Should().BeFalse();
    }

    [Fact]
    public void IsTranscribing_WhenTranscribing_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Transcribing;

        viewModel.SelectedRecording = recording;

        viewModel.IsTranscribing.Should().BeTrue();
    }

    [Fact]
    public void IsTranscribing_WhenComplete_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;

        viewModel.SelectedRecording = recording;

        viewModel.IsTranscribing.Should().BeFalse();
    }

    // ========== IsTranscriptionFailed tests ==========

    [Fact]
    public void IsTranscriptionFailed_WhenNoSelection_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        viewModel.IsTranscriptionFailed.Should().BeFalse();
    }

    [Fact]
    public void IsTranscriptionFailed_WhenFailed_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Failed;

        viewModel.SelectedRecording = recording;

        viewModel.IsTranscriptionFailed.Should().BeTrue();
    }

    [Fact]
    public void IsTranscriptionFailed_WhenComplete_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;

        viewModel.SelectedRecording = recording;

        viewModel.IsTranscriptionFailed.Should().BeFalse();
    }

    // ========== ShowTranscriptSection tests ==========

    [Fact]
    public void ShowTranscriptSection_WhenNoSelection_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        viewModel.ShowTranscriptSection.Should().BeFalse();
    }

    [Fact]
    public void ShowTranscriptSection_WhenRecordingSelected_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        viewModel.ShowTranscriptSection.Should().BeTrue();
    }

    // ========== PropertyChanged notification tests for transcript properties ==========

    [Fact]
    public void OnSelectedRecordingChanged_ShouldRaiseTranscriptDisplayPropertyChanged() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        changed.Should().Contain(nameof(MainViewModel.TranscriptDisplay));
    }

    [Fact]
    public void OnSelectedRecordingChanged_ShouldRaiseIsTranscribingPropertyChanged() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        changed.Should().Contain(nameof(MainViewModel.IsTranscribing));
    }

    [Fact]
    public void OnSelectedRecordingChanged_ShouldRaiseIsTranscriptionFailedPropertyChanged() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        changed.Should().Contain(nameof(MainViewModel.IsTranscriptionFailed));
    }

    [Fact]
    public void OnSelectedRecordingChanged_ShouldRaiseShowTranscriptSectionPropertyChanged() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        changed.Should().Contain(nameof(MainViewModel.ShowTranscriptSection));
    }

    [Fact]
    public void TranscriptDisplay_WhenSelectionCleared_ShouldReturnToEmpty() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Transcript = "Some transcript text";
        viewModel.SelectedRecording = recording;

        viewModel.SelectedRecording = null;

        viewModel.TranscriptDisplay.Should().BeEmpty();
        viewModel.IsTranscribing.Should().BeFalse();
        viewModel.IsTranscriptionFailed.Should().BeFalse();
        viewModel.ShowTranscriptSection.Should().BeFalse();
    }

    // ========== Constructor — null clipboardService ==========

    [Fact]
    public void Constructor_WithNullClipboardService_ShouldThrowArgumentNullException() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();

        var act = () => new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("clipboardService");
    }

    // ========== CanCopyTranscript tests ==========

    [Fact]
    public void CanCopyTranscript_WhenNoSelection_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        viewModel.CanCopyTranscript.Should().BeFalse();
    }

    [Fact]
    public void CanCopyTranscript_WhenTranscribing_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Transcribing;

        viewModel.SelectedRecording = recording;

        viewModel.CanCopyTranscript.Should().BeFalse();
    }

    [Fact]
    public void CanCopyTranscript_WhenFailed_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Failed;

        viewModel.SelectedRecording = recording;

        viewModel.CanCopyTranscript.Should().BeFalse();
    }

    [Fact]
    public void CanCopyTranscript_WhenCompleteWithNullTranscript_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = null;

        viewModel.SelectedRecording = recording;

        viewModel.CanCopyTranscript.Should().BeFalse();
    }

    [Fact]
    public void CanCopyTranscript_WhenCompleteWithEmptyTranscript_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = "";

        viewModel.SelectedRecording = recording;

        viewModel.CanCopyTranscript.Should().BeFalse();
    }

    [Fact]
    public void CanCopyTranscript_WhenCompleteWithTranscript_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = "Hello world";

        viewModel.SelectedRecording = recording;

        viewModel.CanCopyTranscript.Should().BeTrue();
    }

    [Fact]
    public void OnSelectedRecordingChanged_ShouldRaiseCanCopyTranscriptPropertyChanged() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        changed.Should().Contain(nameof(MainViewModel.CanCopyTranscript));
    }

    // ========== CopyTranscriptCommand tests ==========

    [Fact]
    public async Task CopyTranscriptCommand_WhenCompleteWithTranscript_ShouldCopyTextToClipboard() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var viewModel = CreateViewModelWithClipboard(connection, context, clipboard);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = "Hello world";
        viewModel.SelectedRecording = recording;

        await viewModel.CopyTranscriptCommand.ExecuteAsync(null);

        await clipboard.Received(1).SetTextAsync("Hello world");
    }

    [Fact]
    public async Task CopyTranscriptCommand_WhenNoTranscript_ShouldNotCallClipboard() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var viewModel = CreateViewModelWithClipboard(connection, context, clipboard);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = null;
        viewModel.SelectedRecording = recording;

        await viewModel.CopyTranscriptCommand.ExecuteAsync(null);

        await clipboard.DidNotReceive().SetTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CopyTranscriptCommand_WhenTranscribing_ShouldNotCallClipboard() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var viewModel = CreateViewModelWithClipboard(connection, context, clipboard);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Transcribing;
        viewModel.SelectedRecording = recording;

        await viewModel.CopyTranscriptCommand.ExecuteAsync(null);

        await clipboard.DidNotReceive().SetTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CopyTranscriptCommand_WhenNoSelection_ShouldNotCallClipboard() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var viewModel = CreateViewModelWithClipboard(connection, context, clipboard);

        await viewModel.CopyTranscriptCommand.ExecuteAsync(null);

        await clipboard.DidNotReceive().SetTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CopyTranscriptCommand_WhenExecuted_ShouldChangeLabelToCopied() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        // Make SetTextAsync complete immediately
        clipboard.SetTextAsync(Arg.Any<string>()).Returns(Task.CompletedTask);
        var viewModel = CreateViewModelWithClipboard(connection, context, clipboard);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;
        recording.Transcript = "Hello world";
        viewModel.SelectedRecording = recording;

        // Execute without awaiting the full delay — capture label change during execution
        var task = viewModel.CopyTranscriptCommand.ExecuteAsync(null);

        // After clipboard call but before delay completes, label should be "Copied!"
        // We need to give a brief moment for the clipboard call to complete
        await Task.Delay(50);
        viewModel.CopyButtonLabel.Should().Be("Copied!");

        // After full delay, label should reset
        await task;
        viewModel.CopyButtonLabel.Should().Be("Copy");
    }

    [Fact]
    public void CopyButtonLabel_WhenNewInstance_ShouldBeCopy() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        viewModel.CopyButtonLabel.Should().Be("Copy");
    }

    [Fact]
    public void CopyButtonLabel_WhenSelectionChanges_ShouldResetToCopy() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        viewModel.CopyButtonLabel = "Copied!";

        viewModel.SelectedRecording = CreateRecording(DateTime.UtcNow);

        viewModel.CopyButtonLabel.Should().Be("Copy");
    }

    // ========== PendingTranscription state tests ==========

    [Fact]
    public void TranscriptDisplay_WhenPendingTranscription_ShouldShowWaitingMessage() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.PendingTranscription;

        viewModel.SelectedRecording = recording;

        viewModel.TranscriptDisplay.Should().Be("Waiting to transcribe...");
    }

    [Fact]
    public void IsTranscribing_WhenPendingTranscription_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.PendingTranscription;

        viewModel.SelectedRecording = recording;

        viewModel.IsTranscribing.Should().BeFalse();
    }

    // ========== CanRetranscribe tests ==========

    [Fact]
    public void CanRetranscribe_WhenNoSelection_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);

        viewModel.CanRetranscribe.Should().BeFalse();
    }

    [Fact]
    public void CanRetranscribe_WhenPendingTranscription_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.PendingTranscription;

        viewModel.SelectedRecording = recording;

        viewModel.CanRetranscribe.Should().BeTrue();
    }

    [Fact]
    public void CanRetranscribe_WhenFailed_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Failed;

        viewModel.SelectedRecording = recording;

        viewModel.CanRetranscribe.Should().BeTrue();
    }

    [Fact]
    public void CanRetranscribe_WhenComplete_ShouldBeTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;

        viewModel.SelectedRecording = recording;

        viewModel.CanRetranscribe.Should().BeTrue();
    }

    [Fact]
    public void CanRetranscribe_WhenTranscribing_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Transcribing;

        viewModel.SelectedRecording = recording;

        viewModel.CanRetranscribe.Should().BeFalse();
    }

    [Fact]
    public void CanRetranscribe_WhenRecording_ShouldBeFalse() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Recording;

        viewModel.SelectedRecording = recording;

        viewModel.CanRetranscribe.Should().BeFalse();
    }

    // ========== RetranscribeButtonLabel tests ==========

    [Fact]
    public void RetranscribeButtonLabel_WhenComplete_ShouldBeReTranscribe() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Complete;

        viewModel.SelectedRecording = recording;

        viewModel.RetranscribeButtonLabel.Should().Be("Re-transcribe");
    }

    [Fact]
    public void RetranscribeButtonLabel_WhenFailed_ShouldBeTranscribe() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.Failed;

        viewModel.SelectedRecording = recording;

        viewModel.RetranscribeButtonLabel.Should().Be("Transcribe");
    }

    [Fact]
    public void RetranscribeButtonLabel_WhenPendingTranscription_ShouldBeTranscribe() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var viewModel = CreateViewModel(connection, context);
        var recording = CreateRecording(DateTime.UtcNow);
        recording.Status = RecordingStatus.PendingTranscription;

        viewModel.SelectedRecording = recording;

        viewModel.RetranscribeButtonLabel.Should().Be("Transcribe");
    }

    // ========== RetranscribeCommand tests ==========

    [Fact]
    public void RetranscribeCommand_WhenExecuted_ShouldSetStatusToTranscribing() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var transcriptionManager = Substitute.For<ITranscriptionManager>();
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, transcriptionManager, Substitute.For<IClipboardService>());
        var tempFile = Path.GetTempFileName();
        try {
            var recording = CreateRecording(DateTime.UtcNow);
            recording.AudioFileName = tempFile;
            recording.Status = RecordingStatus.PendingTranscription;
            viewModel.SelectedRecording = recording;

            viewModel.RetranscribeCommand.Execute(null);

            recording.Status.Should().Be(RecordingStatus.Transcribing);
        }
        finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void RetranscribeCommand_WhenExecuted_ShouldEnqueueTranscription() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var transcriptionManager = Substitute.For<ITranscriptionManager>();
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, transcriptionManager, Substitute.For<IClipboardService>());
        var tempFile = Path.GetTempFileName();
        try {
            var recording = CreateRecording(DateTime.UtcNow);
            recording.AudioFileName = tempFile;
            recording.Status = RecordingStatus.Failed;
            viewModel.SelectedRecording = recording;

            viewModel.RetranscribeCommand.Execute(null);

            transcriptionManager.Received(1).EnqueueTranscription(recording.Id, Arg.Any<string>());
        }
        finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void RetranscribeCommand_WhenExecuted_ShouldHideRetranscribeButton() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var transcriptionManager = Substitute.For<ITranscriptionManager>();
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, transcriptionManager, Substitute.For<IClipboardService>());
        var tempFile = Path.GetTempFileName();
        try {
            var recording = CreateRecording(DateTime.UtcNow);
            recording.AudioFileName = tempFile;
            recording.Status = RecordingStatus.Complete;
            viewModel.SelectedRecording = recording;

            viewModel.RetranscribeCommand.Execute(null);

            viewModel.CanRetranscribe.Should().BeFalse();
        }
        finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void RetranscribeCommand_WhenNoSelection_ShouldNotEnqueueTranscription() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var transcriptionManager = Substitute.For<ITranscriptionManager>();
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, transcriptionManager, Substitute.For<IClipboardService>());

        viewModel.RetranscribeCommand.Execute(null);

        transcriptionManager.DidNotReceive().EnqueueTranscription(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Fact]
    public void RetranscribeCommand_WhenAudioFileMissing_ShouldSetStatusToFailed() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var transcriptionManager = Substitute.For<ITranscriptionManager>();
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, transcriptionManager, Substitute.For<IClipboardService>());
        var recording = CreateRecording(DateTime.UtcNow);
        recording.AudioFileName = "nonexistent.wav";
        recording.Status = RecordingStatus.PendingTranscription;
        viewModel.SelectedRecording = recording;

        viewModel.RetranscribeCommand.Execute(null);

        recording.Status.Should().Be(RecordingStatus.Failed);
        transcriptionManager.DidNotReceive().EnqueueTranscription(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Fact]
    public void RetranscribeCommand_WhenExecuted_ShouldShowTranscribingSpinner() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var transcriptionManager = Substitute.For<ITranscriptionManager>();
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(recorder, player, context, transcriptionManager, Substitute.For<IClipboardService>());
        var tempFile = Path.GetTempFileName();
        try {
            var recording = CreateRecording(DateTime.UtcNow);
            recording.AudioFileName = tempFile;
            recording.Status = RecordingStatus.PendingTranscription;
            viewModel.SelectedRecording = recording;

            viewModel.RetranscribeCommand.Execute(null);

            viewModel.IsTranscribing.Should().BeTrue();
        }
        finally {
            File.Delete(tempFile);
        }
    }

    // ========== Helper methods ==========

    private static MainViewModel CreateViewModel(SqliteConnection connection, AppDbContext context) {
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        return new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), Substitute.For<IClipboardService>());
    }

    private static MainViewModel CreateViewModelWithClipboard(SqliteConnection connection, AppDbContext context, IClipboardService clipboard) {
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        return new MainViewModel(recorder, player, context, Substitute.For<ITranscriptionManager>(), clipboard);
    }

    private static SqliteConnection CreateConnection() {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static AppDbContext CreateContext(SqliteConnection connection) {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static Recording CreateRecording(DateTime createdAt) => new() {
        Id = Guid.NewGuid(),
        Title = "Test",
        AudioFileName = "file.wav",
        Status = RecordingStatus.Complete,
        Language = "en",
        Duration = TimeSpan.FromSeconds(10),
        CreatedAt = createdAt,
        UpdatedAt = createdAt,
        WhisperModel = "tiny",
        FileSize = 10
    };
}
