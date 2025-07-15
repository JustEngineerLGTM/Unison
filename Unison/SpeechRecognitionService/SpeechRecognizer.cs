using System;
using System.Globalization;
using System.Threading.Tasks;
using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.Abstractions.VoiceActivityDetection;
using EchoSharp.NAudio;
using EchoSharp.SpeechTranscription;
using WebRtcVadSharp;
using EchoSharp.WebRtc.WebRtcVadSharp;
using EchoSharp.Whisper.net;
using Whisper.net;
namespace Unison.SpeechRecognitionService;

public class SpeechRecognizer : ISpeechRecognizer
{
    private string _textTranscribed="";
    private readonly MicrophoneInputSource _micAudioSource = new(deviceNumber: 1);
    IVadDetectorFactory GetWebRtcVadSharpDetector()
    {
        return new WebRtcVadSharpDetectorFactory(new WebRtcVadSharpOptions()
        {
            OperatingMode = OperatingMode.HighQuality
            
        });
    }
    ISpeechTranscriptorFactory GetWhisperTranscriptor()
    {
        var processorBuilder = WhisperFactory.FromPath("ggml-base.bin")
            .CreateBuilder()
            .WithTemperature(0f)
            .WithTokenTimestamps()
            .WithNoSpeechThreshold(0.6f);

        return new WhisperSpeechTranscriptorFactory(processorBuilder);
    }



    
    public async Task RecognitionStart()
    {
        
        _micAudioSource.StartRecording();
        var vadDetectorFactory = GetWebRtcVadSharpDetector();
        var speechTranscriptorFactory = GetWhisperTranscriptor();
        var realTimeFactory = new EchoSharpRealtimeTranscriptorFactory(speechTranscriptorFactory, vadDetectorFactory, echoSharpOptions: new EchoSharpRealtimeOptions()
        {
            ConcatenateSegmentsToPrompt = false // Flag to concatenate segments to prompt when new segment is recognized (for the whole session)
        });
        var realTimeTranscriptor = realTimeFactory.Create(new RealtimeSpeechTranscriptorOptions()
        {
            AutodetectLanguageOnce = false, // Flag to detect the language only once or for each segment
            IncludeSpeechRecogizingEvents = false, // Flag to include speech recognizing events (RealtimeSegmentRecognizing)
            RetrieveTokenDetails = false, // Flag to retrieve token details
            LanguageAutoDetect = false, // Flag to auto-detect the language
            Language = new CultureInfo("ru-RU"), // Language to use for transcription
        });

        await foreach (var transcription in realTimeTranscriptor.TranscribeAsync(_micAudioSource))
        {
            var textToWrite = transcription switch
            {
                RealtimeSegmentRecognized segmentRecognized => $"{segmentRecognized.Segment.StartTime}-{segmentRecognized.Segment.StartTime + segmentRecognized.Segment.Duration}:{segmentRecognized.Segment.Text}",
                RealtimeSessionStarted sessionStarted => $"SessionId: {sessionStarted.SessionId}",
                RealtimeSessionStopped sessionStopped => $"SessionId: {sessionStopped.SessionId}",
                _ => null
            };
            if(textToWrite is not null)
                _textTranscribed+=textToWrite+Environment.NewLine;
            Console.WriteLine(_textTranscribed);
        }
    }

    public Task RecognitionStop()
    {
        _micAudioSource.StopRecording();
        return Task.CompletedTask;
    }
}
