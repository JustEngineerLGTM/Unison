using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace Unison.SpeechRecognitionService
{
    public class NAudioInputProvider : IAudioInputProvider
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _writer;
        private MemoryStream? _stream;
        private int _deviceNumber = 0;

        public Stream StartDefaultDevice()
        {
            _stream = new MemoryStream();

            _waveIn = new WaveInEvent
            {
                DeviceNumber = _deviceNumber,
                WaveFormat = new WaveFormat(16000, 16, 1)
            };

            _writer = new WaveFileWriter(new IgnoreDisposeStream(_stream), _waveIn.WaveFormat);

            _waveIn.DataAvailable += (s, e) =>
            {
                if (_writer != null)
                {
                    _writer.Write(e.Buffer, 0, e.BytesRecorded);
                }
            };

            _waveIn.RecordingStopped += (s, e) =>
            {
                Console.WriteLine("🛑 Recording stopped");
                _writer?.Dispose();
                _waveIn?.Dispose();
            };

            Console.WriteLine("🎙️ Начинаем запись");
            _waveIn.StartRecording();

            return _stream;
        }

        public void Stop()
        {
            _waveIn?.StopRecording();
        }

        public List<string> GetAvailableDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture,
                DeviceState.Active);
            var list = new List<string>();
            foreach (var device in devices)
                list.Add(device.FriendlyName);
            return list;
        }
    }

    public class IgnoreDisposeStream : Stream
    {
        private readonly Stream _inner;
        public IgnoreDisposeStream(Stream inner) => _inner = inner;
        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer,
            offset,
            count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset,
            origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer,
            offset,
            count);
        protected override void Dispose(bool disposing) { }
    }
}
