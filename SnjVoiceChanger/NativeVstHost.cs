using System.Drawing;
using System.Runtime.InteropServices;

namespace SnjVoiceChanger;

public sealed class NativeVstHost : IAudioPluginHost
{
    private IntPtr _handle;
    private int _inputChannels;
    private int _outputChannels;
    private bool _disposed;

    public NativeVstHost()
    {
        try
        {
            _handle = NativeVstHostApi.SnjVstHost_Create();
        }
        catch (Exception ex) when (IsNativeLoadException(ex))
        {
            throw CreateUnavailableException(ex);
        }

        if (_handle == IntPtr.Zero)
        {
            throw new NativeVstHostException("Native VST host could not be created.");
        }
    }

    ~NativeVstHost()
    {
        Dispose(false);
    }

    public static int ApiVersion
    {
        get
        {
            try
            {
                return NativeVstHostApi.SnjVstHost_GetApiVersion();
            }
            catch (Exception ex) when (IsNativeLoadException(ex))
            {
                throw CreateUnavailableException(ex);
            }
        }
    }

    public void LoadPlugin(string pluginPath)
    {
        EnsureNotDisposed();

        try
        {
            ThrowIfFailed(NativeVstHostApi.SnjVstHost_LoadPlugin(_handle, pluginPath));
        }
        catch (Exception ex) when (IsNativeLoadException(ex))
        {
            throw CreateUnavailableException(ex);
        }
    }

    public void SetupProcessing(double sampleRate, int maxBlockSize, int inputChannels, int outputChannels)
    {
        EnsureNotDisposed();

        try
        {
            ThrowIfFailed(NativeVstHostApi.SnjVstHost_SetupProcessing(
                _handle,
                sampleRate,
                maxBlockSize,
                inputChannels,
                outputChannels));
        }
        catch (Exception ex) when (IsNativeLoadException(ex))
        {
            throw CreateUnavailableException(ex);
        }

        _inputChannels = inputChannels;
        _outputChannels = outputChannels;
    }

    public void ProcessFloat32(
        ReadOnlySpan<float> inputInterleaved,
        Span<float> outputInterleaved,
        int frameCount)
    {
        EnsureNotDisposed();

        if (frameCount < 0)
        {
            throw new NativeVstHostException("Frame count must not be negative.");
        }

        var inputSampleCount = frameCount * _inputChannels;
        var outputSampleCount = frameCount * _outputChannels;

        if (inputInterleaved.Length < inputSampleCount || outputInterleaved.Length < outputSampleCount)
        {
            throw new NativeVstHostException("Input or output buffer is too small for the configured channel count.");
        }

        var input = inputInterleaved[..inputSampleCount].ToArray();
        var output = new float[outputSampleCount];

        try
        {
            ThrowIfFailed(NativeVstHostApi.SnjVstHost_ProcessFloat32(_handle, input, output, frameCount));
        }
        catch (Exception ex) when (IsNativeLoadException(ex))
        {
            throw CreateUnavailableException(ex);
        }

        output.CopyTo(outputInterleaved);
    }

    public void OpenEditor(IntPtr parentHwnd)
    {
        EnsureNotDisposed();

        try
        {
            ThrowIfFailed(NativeVstHostApi.SnjVstHost_OpenEditor(_handle, parentHwnd));
        }
        catch (Exception ex) when (IsNativeLoadException(ex))
        {
            throw CreateUnavailableException(ex);
        }
    }

    public Size? GetEditorSize()
    {
        EnsureNotDisposed();
        return null;
    }

    public void EditorIdle()
    {
        EnsureNotDisposed();
    }

    public void CloseEditor()
    {
        if (_disposed || _handle == IntPtr.Zero)
        {
            return;
        }

        try
        {
            NativeVstHostApi.SnjVstHost_CloseEditor(_handle);
        }
        catch (Exception ex) when (IsNativeLoadException(ex))
        {
            throw CreateUnavailableException(ex);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        var handle = _handle;
        _handle = IntPtr.Zero;
        _disposed = true;

        if (handle == IntPtr.Zero)
        {
            return;
        }

        try
        {
            NativeVstHostApi.SnjVstHost_Destroy(handle);
        }
        catch
        {
        }
    }

    private void ThrowIfFailed(int result)
    {
        if (result == 0)
        {
            return;
        }

        var message = GetLastError();
        throw new NativeVstHostException(string.IsNullOrWhiteSpace(message)
            ? $"Native VST host call failed with code {result}."
            : message);
    }

    private string GetLastError()
    {
        try
        {
            var errorPointer = NativeVstHostApi.SnjVstHost_GetLastError(_handle);
            return Marshal.PtrToStringUni(errorPointer) ?? string.Empty;
        }
        catch (Exception ex) when (IsNativeLoadException(ex))
        {
            return CreateUnavailableException(ex).Message;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(NativeVstHost));
        }
    }

    private static bool IsNativeLoadException(Exception ex)
    {
        return ex is DllNotFoundException ||
            ex is EntryPointNotFoundException ||
            ex is BadImageFormatException;
    }

    private static NativeVstHostException CreateUnavailableException(Exception ex)
    {
        return new NativeVstHostException($"Native VST host unavailable: {ex.Message}", ex);
    }
}
