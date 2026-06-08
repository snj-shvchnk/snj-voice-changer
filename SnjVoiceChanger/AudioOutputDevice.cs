namespace SnjVoiceChanger;

public sealed record AudioOutputDevice(string Id, string Name)
{
    public override string ToString() => Name;
}
