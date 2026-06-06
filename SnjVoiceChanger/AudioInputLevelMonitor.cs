namespace SnjVoiceChanger;

public sealed class AudioInputLevelMonitor : IDisposable
{
    private IAudioMeterInformation? _meter;

    public AudioInputLevelMonitor(AudioInputDevice inputDevice)
    {
        Device = inputDevice;
        _meter = CoreAudioInterop.CreateAudioMeter(inputDevice.Id);
    }

    public AudioInputDevice Device { get; }

    public float GetPeakLevel()
    {
        if (_meter is null)
        {
            return 0;
        }

        try
        {
            _meter.GetPeakValue(out var peak);
            return Math.Clamp(peak, 0, 1);
        }
        catch
        {
            return 0;
        }
    }

    public void Dispose()
    {
        CoreAudioInterop.ReleaseComObject(_meter);
        _meter = null;
    }
}
