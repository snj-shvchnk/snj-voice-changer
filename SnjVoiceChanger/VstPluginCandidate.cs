namespace SnjVoiceChanger;

public sealed record VstPluginCandidate(string Path, string Name)
{
    public override string ToString() => Name;
}
