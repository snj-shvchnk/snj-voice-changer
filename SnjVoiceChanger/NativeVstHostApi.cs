using System.Runtime.InteropServices;

namespace SnjVoiceChanger;

internal static class NativeVstHostApi
{
    private const string LibraryName = "SnjVstHostNative";

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern int SnjVstHost_GetApiVersion();

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern IntPtr SnjVstHost_Create();

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern void SnjVstHost_Destroy(IntPtr host);

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern int SnjVstHost_LoadPlugin(IntPtr host, string pluginPath);

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern int SnjVstHost_SetupProcessing(
        IntPtr host,
        double sampleRate,
        int maxBlockSize,
        int inputChannels,
        int outputChannels);

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern int SnjVstHost_ProcessFloat32(
        IntPtr host,
        [In] float[] inputInterleaved,
        [Out] float[] outputInterleaved,
        int frameCount);

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern int SnjVstHost_OpenEditor(IntPtr host, IntPtr parentHwnd);

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern void SnjVstHost_CloseEditor(IntPtr host);

    [DllImport(
        LibraryName,
        CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Unicode,
        ExactSpelling = true)]
    internal static extern IntPtr SnjVstHost_GetLastError(IntPtr host);
}
