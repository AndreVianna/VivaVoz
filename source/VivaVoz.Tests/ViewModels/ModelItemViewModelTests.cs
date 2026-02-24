using AwesomeAssertions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using VivaVoz.Services.Transcription;
using VivaVoz.ViewModels;

using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class ModelItemViewModelTests {
    // ========== Constructor tests ==========

    [Fact]
    public void Constructor_WithNullModelId_ShouldThrow() {
        var manager = Substitute.For<IModelManager>();

        var act = () => new ModelItemViewModel(null!, manager);

        act.Should().Throw<ArgumentNullException>().WithParameterName("modelId");
    }

    [Fact]
    public void Constructor_WithNullModelManager_ShouldThrow() {
        var act = () => new ModelItemViewModel("tiny", null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("modelManager");
    }

    [Fact]
    public void Constructor_WithValidArgs_ShouldNotThrow() {
        var manager = Substitute.For<IModelManager>();

        var act = () => new ModelItemViewModel("tiny", manager);

        act.Should().NotThrow();
    }

    // ========== ModelId and DisplayName ==========

    [Fact]
    public void Constructor_ShouldSetModelId() {
        var vm = CreateViewModel("tiny");

        vm.ModelId.Should().Be("tiny");
    }

    [Theory]
    [InlineData("tiny", "Tiny")]
    [InlineData("base", "Base")]
    [InlineData("small", "Small")]
    [InlineData("medium", "Medium")]
    [InlineData("large-v3", "Large (v3)")]
    public void Constructor_ShouldSetDisplayNameForKnownModels(string modelId, string expectedDisplayName) {
        var vm = CreateViewModel(modelId);

        vm.DisplayName.Should().Be(expectedDisplayName);
    }

    [Fact]
    public void Constructor_WithUnknownModelId_ShouldUseModelIdAsDisplayName() {
        var vm = CreateViewModel("unknown-model");

        vm.DisplayName.Should().Be("unknown-model");
    }

    // ========== ExpectedSize ==========

    [Theory]
    [InlineData("tiny", "~75 MB")]
    [InlineData("base", "~142 MB")]
    [InlineData("small", "~466 MB")]
    [InlineData("medium", "~1.5 GB")]
    [InlineData("large-v3", "~2.9 GB")]
    public void Constructor_ShouldSetExpectedSizeForKnownModels(string modelId, string expectedSize) {
        var vm = CreateViewModel(modelId);

        vm.ExpectedSize.Should().Be(expectedSize);
    }

    [Fact]
    public void Constructor_WithUnknownModelId_ShouldSetUnknownExpectedSize() {
        var vm = CreateViewModel("unknown-model");

        vm.ExpectedSize.Should().Be("Unknown");
    }

    // ========== IsInstalled ==========

    [Fact]
    public void Constructor_WhenModelInstalled_ShouldSetIsInstalledTrue() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(true);

        var vm = new ModelItemViewModel("tiny", manager);

        vm.IsInstalled.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WhenModelNotInstalled_ShouldSetIsInstalledFalse() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(false);

        var vm = new ModelItemViewModel("tiny", manager);

        vm.IsInstalled.Should().BeFalse();
    }

    // ========== StatusText ==========

    [Fact]
    public void StatusText_WhenNotInstalled_ShouldReturnNotInstalled() {
        var vm = CreateViewModel("tiny", isInstalled: false);

        vm.StatusText.Should().Be("Not installed");
    }

    [Fact]
    public void StatusText_WhenInstalled_ShouldReturnInstalled() {
        var vm = CreateViewModel("tiny", isInstalled: true);

        vm.StatusText.Should().Be("Installed");
    }

    [Fact]
    public void StatusText_WhenDownloading_ShouldIncludeProgress() {
        var vm = CreateViewModel("tiny");
        vm.IsDownloading = true;
        vm.DownloadProgress = 0.5;

        vm.StatusText.Should().Contain("50%");
    }

    // ========== CanDownload / CanCancel / CanDelete ==========

    [Fact]
    public void CanDownload_WhenNotInstalledAndNotDownloading_ShouldBeTrue() {
        var vm = CreateViewModel("tiny", isInstalled: false);

        vm.CanDownload.Should().BeTrue();
    }

    [Fact]
    public void CanDownload_WhenInstalled_ShouldBeFalse() {
        var vm = CreateViewModel("tiny", isInstalled: true);

        vm.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void CanDownload_WhenDownloading_ShouldBeFalse() {
        var vm = CreateViewModel("tiny");
        vm.IsDownloading = true;

        vm.CanDownload.Should().BeFalse();
    }

    [Fact]
    public void CanCancel_WhenNotDownloading_ShouldBeFalse() {
        var vm = CreateViewModel("tiny");

        vm.CanCancel.Should().BeFalse();
    }

    [Fact]
    public void CanCancel_WhenDownloading_ShouldBeTrue() {
        var vm = CreateViewModel("tiny");
        vm.IsDownloading = true;

        vm.CanCancel.Should().BeTrue();
    }

    [Fact]
    public void CanDelete_WhenInstalled_ShouldBeTrue() {
        var vm = CreateViewModel("tiny", isInstalled: true);

        vm.CanDelete.Should().BeTrue();
    }

    [Fact]
    public void CanDelete_WhenNotInstalled_ShouldBeFalse() {
        var vm = CreateViewModel("tiny", isInstalled: false);

        vm.CanDelete.Should().BeFalse();
    }

    [Fact]
    public void CanDelete_WhenDownloading_ShouldBeFalse() {
        var vm = CreateViewModel("tiny", isInstalled: true);
        vm.IsDownloading = true;

        vm.CanDelete.Should().BeFalse();
    }

    // ========== Delete command ==========

    [Fact]
    public void DeleteCommand_WhenInstalled_ShouldCallModelManagerDeleteModel() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(true);
        var vm = new ModelItemViewModel("tiny", manager);

        vm.DeleteCommand.Execute(null);

        manager.Received(1).DeleteModel("tiny");
    }

    [Fact]
    public void DeleteCommand_WhenInstalled_ShouldSetIsInstalledFalse() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(true);
        var vm = new ModelItemViewModel("tiny", manager);

        vm.DeleteCommand.Execute(null);

        vm.IsInstalled.Should().BeFalse();
    }

    [Fact]
    public void DeleteCommand_WhenInstalled_ShouldUpdateStatusTextToNotInstalled() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(true);
        var vm = new ModelItemViewModel("tiny", manager);

        vm.DeleteCommand.Execute(null);

        vm.StatusText.Should().Be("Not installed");
    }

    [Fact]
    public void DeleteCommand_ShouldRaisePropertyChangedForIsInstalled() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(true);
        var vm = new ModelItemViewModel("tiny", manager);

        var changed = new List<string?>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        vm.DeleteCommand.Execute(null);

        changed.Should().Contain(nameof(ModelItemViewModel.IsInstalled));
    }

    // ========== Download command ==========

    [Fact]
    public async Task DownloadCommand_WhenSuccessful_ShouldSetIsInstalledTrue() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(false);
        manager.DownloadModelAsync("tiny", Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var vm = new ModelItemViewModel("tiny", manager);

        await vm.DownloadCommand.ExecuteAsync(null);

        vm.IsInstalled.Should().BeTrue();
    }

    [Fact]
    public async Task DownloadCommand_ShouldSetIsDownloadingDuringDownload() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(false);

        var tcs = new TaskCompletionSource();
        var downloadingDuringExecution = false;

        manager.DownloadModelAsync("tiny", Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(async _ => {
                downloadingDuringExecution = true;
                await tcs.Task;
            });

        var vm = new ModelItemViewModel("tiny", manager);
        var downloadTask = vm.DownloadCommand.ExecuteAsync(null);

        await Task.Delay(50); // let DownloadAsync start
        downloadingDuringExecution.Should().BeTrue();
        vm.IsDownloading.Should().BeTrue();

        tcs.SetResult();
        await downloadTask;

        vm.IsDownloading.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadCommand_WhenCancelled_ShouldNotSetIsInstalled() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(false);
        manager.DownloadModelAsync("tiny", Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var vm = new ModelItemViewModel("tiny", manager);

        await vm.DownloadCommand.ExecuteAsync(null);

        vm.IsInstalled.Should().BeFalse();
        vm.IsDownloading.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadCommand_WhenFails_ShouldNotSetIsInstalled() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(false);
        manager.DownloadModelAsync("tiny", Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));
        var vm = new ModelItemViewModel("tiny", manager);

        await vm.DownloadCommand.ExecuteAsync(null);

        vm.IsInstalled.Should().BeFalse();
        vm.IsDownloading.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadCommand_ShouldReportProgress() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(false);

        IProgress<double>? capturedProgress = null;
        manager.DownloadModelAsync("tiny", Arg.Do<IProgress<double>?>(p => capturedProgress = p), Arg.Any<CancellationToken>())
            .Returns(_ => {
                capturedProgress?.Report(0.5);
                capturedProgress?.Report(1.0);
                return Task.CompletedTask;
            });

        var vm = new ModelItemViewModel("tiny", manager);

        await vm.DownloadCommand.ExecuteAsync(null);
        await Task.Delay(100); // Progress<T> dispatches callbacks to thread pool asynchronously

        vm.DownloadProgress.Should().Be(1.0);
    }

    // ========== CancelDownload command ==========

    [Fact]
    public async Task CancelDownloadCommand_ShouldCancelOngoingDownload() {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded("tiny").Returns(false);

        CancellationToken capturedToken = default;
        var tcs = new TaskCompletionSource();

        manager.DownloadModelAsync("tiny", Arg.Any<IProgress<double>?>(), Arg.Do<CancellationToken>(ct => {
            capturedToken = ct;
            ct.Register(() => tcs.TrySetResult());
        })).Returns(_ => tcs.Task.ContinueWith(_ => { }, TaskContinuationOptions.None));

        var vm = new ModelItemViewModel("tiny", manager);
        var downloadTask = vm.DownloadCommand.ExecuteAsync(null);

        await Task.Delay(50); // let download start

        vm.CancelDownloadCommand.Execute(null);

        await Task.WhenAny(tcs.Task, Task.Delay(1000));
        tcs.Task.IsCompleted.Should().BeTrue("cancellation should have been triggered");
    }

    // ========== PropertyChanged notifications ==========

    [Fact]
    public void IsInstalled_WhenChanged_ShouldRaisePropertyChangedForStatusText() {
        var vm = CreateViewModel("tiny");

        var changed = new List<string?>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        vm.IsInstalled = true;

        changed.Should().Contain(nameof(ModelItemViewModel.StatusText));
    }

    [Fact]
    public void IsDownloading_WhenChanged_ShouldRaisePropertyChangedForStatusText() {
        var vm = CreateViewModel("tiny");

        var changed = new List<string?>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        vm.IsDownloading = true;

        changed.Should().Contain(nameof(ModelItemViewModel.StatusText));
    }

    [Fact]
    public void DownloadProgress_WhenChanged_ShouldRaisePropertyChangedForStatusText() {
        var vm = CreateViewModel("tiny");
        vm.IsDownloading = true;

        var changed = new List<string?>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        vm.DownloadProgress = 0.75;

        changed.Should().Contain(nameof(ModelItemViewModel.StatusText));
    }

    // ========== Helpers ==========

    private static ModelItemViewModel CreateViewModel(string modelId, bool isInstalled = false) {
        var manager = Substitute.For<IModelManager>();
        manager.IsModelDownloaded(Arg.Any<string>()).Returns(isInstalled);
        return new ModelItemViewModel(modelId, manager);
    }
}
