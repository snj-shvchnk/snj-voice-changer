namespace SnjVoiceChanger;

using NAudio.CoreAudioApi;
using NAudio.Wave;

public sealed class AudioRoutingService : IDisposable
{
    private readonly object _levelLock = new();
    private MMDeviceEnumerator? _deviceEnumerator;
    private MMDevice? _inputDevice;
    private MMDevice? _outputDevice;
    private WasapiCapture? _capture;
    private WasapiOut? _output;
    private BufferedWaveProvider? _buffer;
    private MediaFoundationResampler? _resampler;
    private float _outputPeakLevel;

    public bool IsRunning => _capture is not null && _output is not null;

    public void Start(AudioInputDevice inputDevice, AudioOutputDevice outputDevice)
    {
        Stop();

        _deviceEnumerator = new MMDeviceEnumerator();
        _inputDevice = _deviceEnumerator.GetDevice(inputDevice.Id);
        _outputDevice = _deviceEnumerator.GetDevice(outputDevice.Id);

        _capture = new WasapiCapture(_inputDevice);
        _buffer = new BufferedWaveProvider(_capture.WaveFormat)
        {
            BufferDuration = TimeSpan.FromMilliseconds(500),
            DiscardOnBufferOverflow = true,
        };

        using var outputAudioClient = _outputDevice.AudioClient;
        var outputFormat = outputAudioClient.MixFormat;
        _resampler = new MediaFoundationResampler(_buffer, outputFormat)
        {
            ResamplerQuality = 60,
        };

        _output = new WasapiOut(_outputDevice, AudioClientShareMode.Shared, true, 100);
        _output.Init(_resampler);
        _capture.DataAvailable += Capture_DataAvailable;

        _output.Play();
        _capture.StartRecording();
    }

    public float GetOutputPeakLevel()
    {
        lock (_levelLock)
        {
            return _outputPeakLevel;
        }
    }

    public void Stop()
    {
        if (_capture is not null)
        {
            _capture.DataAvailable -= Capture_DataAvailable;
            _capture.StopRecording();
            _capture.Dispose();
        }

        _output?.Stop();
        _output?.Dispose();
        _resampler?.Dispose();
        _inputDevice?.Dispose();
        _outputDevice?.Dispose();
        _deviceEnumerator?.Dispose();

        _capture = null;
        _output = null;
        _buffer = null;
        _resampler = null;
        _inputDevice = null;
        _outputDevice = null;
        _deviceEnumerator = null;
        SetOutputPeakLevel(0);
    }

    private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
    {
        if (_capture is null || _buffer is null || e.BytesRecorded <= 0)
        {
            SetOutputPeakLevel(0);
            return;
        }

        SetOutputPeakLevel(AudioBufferLevelCalculator.CalculatePeakLevel(
            e.Buffer,
            e.BytesRecorded,
            _capture.WaveFormat));

        _buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
    }

    private void SetOutputPeakLevel(float level)
    {
        lock (_levelLock)
        {
            _outputPeakLevel = Math.Clamp(level, 0, 1);
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
