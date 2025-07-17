using System;
using System.Threading.Tasks;
using NAudio.Wave;
using Vosk;

namespace Unison.SpeechRecognitionService;

public class SpeechRecognizer : ISpeechRecognizer
{
    private Model? _model;
    private VoskRecognizer? _recognizer;
    private WaveInEvent? _waveIn;

    public event Action<string>? OnTextRecognized;

    public Task RecognitionStart(string path)
    {
        _model = new Model(path);
        _recognizer = new VoskRecognizer(_model, 16000.0f);
        _recognizer.SetWords(true); 
        _recognizer.SetPartialWords(true);

        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(rate: 16000, bits: 16, channels: 1)
        };

        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.StartRecording();

        return Task.CompletedTask;
    }

    public Task RecognitionStop()
    {
        if (_waveIn != null)
        {
            _waveIn.StopRecording();
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.Dispose();
            _waveIn = null;
        }

   
        if (_recognizer != null)
        {
            var finalResult = _recognizer.FinalResult();
            var text = VoskParserUtility.VoskResultParser.Parse(finalResult);
            if (!string.IsNullOrWhiteSpace(text))
            {
                OnTextRecognized?.Invoke(text + Environment.NewLine);
            }
        }
        

        _recognizer?.Dispose();
        _recognizer = null;

        _model?.Dispose();
        _model = null;

        return Task.CompletedTask;
    }


    private void OnDataAvailable(object? sender, WaveInEventArgs args)
    {
        if (_recognizer == null) return;
        

        if (_recognizer.AcceptWaveform(args.Buffer, args.BytesRecorded))
        {
            var result = _recognizer.Result();
            var text = VoskParserUtility.VoskResultParser.Parse(result);
            if (!string.IsNullOrWhiteSpace(text))
                OnTextRecognized?.Invoke(text);
        }

    }
}