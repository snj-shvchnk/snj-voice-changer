namespace SnjVoiceChanger;

using NAudio.CoreAudioApi;
using NAudio.Wave;

public sealed class AudioInputLevelMonitor : IDisposable
{
    private readonly object _levelLock = new();
    private MMDeviceEnumerator? _deviceEnumerator;
    private MMDevice? _device;
    private WasapiCapture? _capture;
    private float _peakLevel;

    public AudioInputLevelMonitor(AudioInputDevice inputDevice)
    {
        Device = inputDevice;
        _deviceEnumerator = new MMDeviceEnumerator();
        _device = _deviceEnumerator.GetDevice(inputDevice.Id);
        _capture = new WasapiCapture(_device);
        _capture.DataAvailable += Capture_DataAvailable;
        _capture.StartRecording();
    }

    public AudioInputDevice Device { get; }

    public float GetPeakLevel()
    {
        lock (_levelLock)
        {
            return _peakLevel;
        }
    }

    private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
    {
        if (_capture is null || e.BytesRecorded <= 0)
        {
            SetPeakLevel(0);
            return;
        }

        SetPeakLevel(CalculatePeakLevel(e.Buffer, e.BytesRecorded, _capture.WaveFormat));
    }

    private void SetPeakLevel(float level)
    {
        lock (_levelLock)
        {
            _peakLevel = Math.Clamp(level, 0, 1);
        }
    }

    private static float CalculatePeakLevel(byte[] buffer, int bytesRecorded, WaveFormat waveFormat)
    {
        return waveFormat.BitsPerSample switch
        {
            16 => CalculatePcm16Peak(buffer, bytesRecorded),
            24 => CalculatePcm24Peak(buffer, bytesRecorded),
            32 when waveFormat.Encoding != WaveFormatEncoding.Pcm => CalculateFloat32Peak(buffer, bytesRecorded),
            32 => CalculatePcm32Peak(buffer, bytesRecorded),
            _ => 0,
        };
    }

    private static float CalculateFloat32Peak(byte[] buffer, int bytesRecorded)
    {
        var peak = 0f;

        for (var offset = 0; offset <= bytesRecorded - 4; offset += 4)
        {
            var sample = Math.Abs(BitConverter.ToSingle(buffer, offset));
            peak = Math.Max(peak, sample);
        }

        return peak;
    }

    private static float CalculatePcm16Peak(byte[] buffer, int bytesRecorded)
    {
        var peak = 0f;

        for (var offset = 0; offset <= bytesRecorded - 2; offset += 2)
        {
            var sample = Math.Abs(BitConverter.ToInt16(buffer, offset) / 32768f);
            peak = Math.Max(peak, sample);
        }

        return peak;
    }

    private static float CalculatePcm24Peak(byte[] buffer, int bytesRecorded)
    {
        var peak = 0f;

        for (var offset = 0; offset <= bytesRecorded - 3; offset += 3)
        {
            var sample = buffer[offset] | buffer[offset + 1] << 8 | buffer[offset + 2] << 16;
            if ((sample & 0x800000) != 0)
            {
                sample |= unchecked((int)0xFF000000);
            }

            peak = Math.Max(peak, Math.Abs(sample / 8388608f));
        }

        return peak;
    }

    private static float CalculatePcm32Peak(byte[] buffer, int bytesRecorded)
    {
        var peak = 0f;

        for (var offset = 0; offset <= bytesRecorded - 4; offset += 4)
        {
            var sample = Math.Abs(BitConverter.ToInt32(buffer, offset) / 2147483648f);
            peak = Math.Max(peak, sample);
        }

        return peak;
    }

    public void Dispose()
    {
        if (_capture is not null)
        {
            _capture.DataAvailable -= Capture_DataAvailable;
            _capture.StopRecording();
            _capture.Dispose();
        }

        _device?.Dispose();
        _deviceEnumerator?.Dispose();
        _capture = null;
        _device = null;
        _deviceEnumerator = null;
    }
}
