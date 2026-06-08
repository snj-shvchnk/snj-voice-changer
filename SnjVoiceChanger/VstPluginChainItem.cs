namespace SnjVoiceChanger;

public sealed class VstPluginChainItem : IDisposable
{
    public VstPluginChainItem(string name, string path, NativeVstHost host)
    {
        Name = name;
        Path = path;
        Host = host;
    }

    public string Name { get; }

    public string Path { get; }

    public NativeVstHost Host { get; }

    public void Dispose()
    {
        Host.Dispose();
    }

    public override string ToString() => Name;
}
