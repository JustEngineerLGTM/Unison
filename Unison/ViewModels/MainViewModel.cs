using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Unison.SpeechRecognitionService;

namespace Unison.ViewModels;

public partial class MainViewModel : ObservableObject
{

    [ObservableProperty] private string _recognizedText = "";

    public IRelayCommand StartCommand { get; }
    public IAsyncRelayCommand StopCommand { get; }

    public MainViewModel()
    {
        ISpeechRecognizer speechRecognizer = new SpeechRecognizer();
        if (speechRecognizer == null)
            throw new ArgumentNullException(nameof(speechRecognizer));

        StartCommand = new RelayCommand(() =>
        {
            Console.WriteLine("▶️ StartCommand вызван");

            try
            {
                speechRecognizer.RecognitionStart();
                Console.WriteLine("🎙️ Распознавание запущено");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Ошибка при запуске: " + ex.Message);
            }
        });

        StopCommand = new AsyncRelayCommand(async () =>
        {
            Console.WriteLine("⏹️ StopCommand вызван");

            try
            {
                await speechRecognizer.RecognitionStop();
                Console.WriteLine("✅ Распознанный текст: " + _recognizedText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Ошибка при остановке: " + ex.Message);
            }
        });
    }

}