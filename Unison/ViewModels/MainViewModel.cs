using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Unison.SpeechRecognitionService;
using Avalonia.Threading;

namespace Unison.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IModelService _modelService;
    private readonly ISpeechRecognizer _speechRecognizer;

    [ObservableProperty]
    private string _recognizedText = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private ModelEntry? _selectedModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool _isRecognitionActive;

    public ObservableCollection<ModelEntry> Models { get; } = new();

    public IAsyncRelayCommand LoadModelsCommand { get; }
    public IAsyncRelayCommand StartCommand { get; }
    public IAsyncRelayCommand StopCommand { get; }

    public MainViewModel(IModelService modelService, ISpeechRecognizer speechRecognizer)
    {
        _modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        _speechRecognizer = speechRecognizer ?? throw new ArgumentNullException(nameof(speechRecognizer));

        _speechRecognizer.OnTextRecognized += text =>
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                Dispatcher.UIThread.Post(() => RecognizedText += text + " "); 
            }
        };

        LoadModelsCommand = new AsyncRelayCommand(async () =>
        {
            try
            {
                Models.Clear();
                var list = await _modelService.GetAvailableModelsAsync();
                if (list == null) return;

                foreach (var m in list)
                {
                    Models.Add(new ModelEntry(m, _modelService));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке моделей: {ex.Message}");
            }
        });

        StartCommand = new AsyncRelayCommand(async () =>
        {
            if (SelectedModel is not { IsDownloaded: true })
                return;

            try
            {
                var modelPath = Path.Combine("models", SelectedModel.Info.Name,SelectedModel.Info.Name);
                await _speechRecognizer.RecognitionStart(modelPath);
                IsRecognitionActive = true;
                RecognizedText = "Распознавание начато...\n"; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запуске распознавания: {ex.Message}");
                RecognizedText = $"Ошибка: {ex.Message}\n";
                IsRecognitionActive = false;
            }
        }, () => SelectedModel is { IsDownloaded: true } && !IsRecognitionActive); 

        StopCommand = new AsyncRelayCommand(
            async () =>
            {
                try
                {
                    await _speechRecognizer.RecognitionStop();
                    IsRecognitionActive = false;
                    RecognizedText += "\nРаспознавание остановлено."; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при остановке распознавания: {ex.Message}");
                    RecognizedText += $"\nОшибка остановки: {ex.Message}";
                }
            },
            () => IsRecognitionActive
        );
        
        LoadModelsCommand.Execute(null);
    }
}
