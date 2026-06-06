namespace SnjVoiceChanger;

public sealed class VirtualMicrophoneService
{
    public const string TargetDeviceName = "Snj Voice Changer";

    public VirtualMicrophoneStatus GetStatus(IReadOnlyList<AudioInputDevice> inputDevices)
    {
        var device = inputDevices.FirstOrDefault(device =>
            device.Name.Contains(TargetDeviceName, StringComparison.OrdinalIgnoreCase) ||
            device.Name.Contains("SnjVoiceChanger", StringComparison.OrdinalIgnoreCase));

        if (device is not null)
        {
            return new VirtualMicrophoneStatus(true, device.Name, "Detected");
        }

        return new VirtualMicrophoneStatus(
            false,
            TargetDeviceName,
            "Driver required");
    }
}
