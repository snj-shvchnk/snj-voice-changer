namespace SnjVoiceChanger;

public sealed class AudioInputDeviceScanner
{
    public IReadOnlyList<AudioInputDevice> GetInputDevices()
    {
        return CoreAudioInterop.GetActiveCaptureDevices();
    }
}
