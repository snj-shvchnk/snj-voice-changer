namespace SnjVoiceChanger;

public sealed record VirtualMicrophoneStatus(bool IsAvailable, string DeviceName, string Message);
