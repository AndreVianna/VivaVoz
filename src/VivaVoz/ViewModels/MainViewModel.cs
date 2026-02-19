using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VivaVoz.Constants;
using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services.Audio;

namespace VivaVoz.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext;
    private readonly IAudioRecorder _recorder;

    [ObservableProperty]
    private ObservableCollection<Recording> recordings = new();

    [ObservableProperty]
    private Recording? selectedRecording;

    [ObservableProperty]
    private string detailHeader = "No recording selected";

    [ObservableProperty]
    private string detailBody = "Select a recording from the list to view details.";

    [ObservableProperty]
    private bool isRecording;

    public MainViewModel(IAudioRecorder recorder, AppDbContext dbContext)
    {
        _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        Recordings = new ObservableCollection<Recording>(
            _dbContext.Recordings.OrderByDescending(recording => recording.CreatedAt).ToList());

        IsRecording = _recorder.IsRecording;
        _recorder.RecordingStopped += OnRecordingStopped;
    }

    public bool HasSelection => SelectedRecording is not null;
    public bool NoSelection => SelectedRecording is null;
    public bool IsNotRecording => !IsRecording;

    partial void OnIsRecordingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotRecording));
    }

    [RelayCommand]
    private void SelectRecording(Recording? recording)
    {
        SelectedRecording = recording;
    }

    [RelayCommand]
    private void StartRecording()
    {
        try
        {
            _recorder.StartRecording();
            IsRecording = _recorder.IsRecording;
        }
        catch (MicrophoneNotFoundException ex)
        {
            ShowMicrophoneNotFoundDialog(ex.Message);
        }
    }

    [RelayCommand]
    private void StopRecording()
    {
        _recorder.StopRecording();
        IsRecording = _recorder.IsRecording;
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedRecording = null;
    }

    partial void OnSelectedRecordingChanged(Recording? value)
    {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(NoSelection));

        if (value is null)
        {
            DetailHeader = "No recording selected";
            DetailBody = "Select a recording from the list to view details.";
            return;
        }

        var title = string.IsNullOrWhiteSpace(value.Title) ? "Recording selected" : value.Title;
        DetailHeader = title;
        DetailBody = "Detail view placeholder.";
    }

    private void OnRecordingStopped(object? sender, AudioRecordingStoppedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsRecording = _recorder.IsRecording;

            var now = DateTime.Now;
            var audioFileName = GetRelativeAudioPath(e.FilePath);
            var fileSize = GetFileSize(e.FilePath);

            var recording = new Recording
            {
                Id = Guid.NewGuid(),
                Title = $"Recording {now:MMM dd, yyyy HH:mm}",
                AudioFileName = audioFileName,
                Transcript = null,
                Status = RecordingStatus.Transcribing,
                Language = "auto",
                Duration = e.Duration,
                CreatedAt = now,
                UpdatedAt = now,
                WhisperModel = "tiny",
                FileSize = fileSize
            };

            _dbContext.Recordings.Add(recording);
            _dbContext.SaveChanges();
            Recordings.Insert(0, recording);
        });
    }

    private static string GetRelativeAudioPath(string filePath)
    {
        try
        {
            return Path.GetRelativePath(FilePaths.AudioDirectory, filePath);
        }
        catch
        {
            return filePath;
        }
    }

    private static long GetFileSize(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Exists ? fileInfo.Length : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static void ShowMicrophoneNotFoundDialog(string message)
    {
        var text = string.IsNullOrWhiteSpace(message)
            ? "No microphone device detected."
            : message;

        var window = new Window
        {
            Title = "Microphone not found",
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new TextBlock
            {
                Text = text,
                Margin = new Thickness(24),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            }
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            window.ShowDialog(desktop.MainWindow);
            return;
        }

        window.Show();
    }
}
