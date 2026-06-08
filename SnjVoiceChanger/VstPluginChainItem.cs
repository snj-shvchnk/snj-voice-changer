namespace SnjVoiceChanger;

public sealed class VstPluginChainItem : IDisposable
{
    private readonly object _hostLock = new();
    private bool _disposed;

    public VstPluginChainItem(string name, string path, NativeVstHost host)
    {
        Name = name;
        Path = path;
        Host = host;
    }

    public string Name { get; }

    public string Path { get; }

    public NativeVstHost Host { get; }

    public void SetupProcessing(double sampleRate, int maxBlockSize, int inputChannels, int outputChannels)
    {
        lock (_hostLock)
        {
            EnsureNotDisposed();
            Host.SetupProcessing(sampleRate, maxBlockSize, inputChannels, outputChannels);
        }
    }

    public void ProcessFloat32(ReadOnlySpan<float> inputInterleaved, Span<float> outputInterleaved, int frameCount)
    {
        lock (_hostLock)
        {
            EnsureNotDisposed();
            Host.ProcessFloat32(inputInterleaved, outputInterleaved, frameCount);
        }
    }

    public void OpenEditor(IntPtr parentHwnd)
    {
        lock (_hostLock)
        {
            EnsureNotDisposed();
            Host.OpenEditor(parentHwnd);
        }
    }

    public void CloseEditor()
    {
        lock (_hostLock)
        {
            if (_disposed)
            {
                return;
            }

            Host.CloseEditor();
        }
    }

    public void Dispose()
    {
        lock (_hostLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Host.Dispose();
        }
    }

    public override string ToString() => Name;

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(VstPluginChainItem));
        }
    }
}
