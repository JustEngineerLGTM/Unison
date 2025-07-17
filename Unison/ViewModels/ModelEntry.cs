using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Unison.SpeechRecognitionService;

namespace Unison.ViewModels;

public partial class ModelEntry : ObservableObject
{
    public VoskModel Info { get; }
    private readonly IModelService _modelService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
    private bool _isDownloaded;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadCommand))]
    private bool _isDownloading;

    [ObservableProperty] private int _downloadProgress;

    public IAsyncRelayCommand DownloadCommand { get; }

    public ModelEntry(VoskModel info, IModelService modelService)
    {
        Info = info ?? throw new ArgumentNullException(nameof(info));
        _modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));

        if (string.IsNullOrWhiteSpace(info.Name))
            throw new ArgumentException("Model name is required.", nameof(info));

        DownloadCommand = new AsyncRelayCommand(DownloadModelAsync, () => !IsDownloaded && !IsDownloading);
        InitializeStatusAsync();
    }
    
    private async void InitializeStatusAsync()
    {
        IsDownloaded = await _modelService.IsModelDownloadedAsync(Info.Name);
    }

    private async Task DownloadModelAsync()
    {
        IsDownloading = true;

        var progress = new Progress<int>(p => DownloadProgress = p);
        await _modelService.DownloadModelAsync(Info, progress);

        IsDownloading = false;
        IsDownloaded = true;
        DownloadProgress = 0; 
    }
}