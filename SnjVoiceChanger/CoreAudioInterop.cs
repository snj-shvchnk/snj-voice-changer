using System.Runtime.InteropServices;

namespace SnjVoiceChanger;

internal static class CoreAudioInterop
{
    private static readonly Guid MMDeviceEnumeratorId = new("BCDE0395-E52F-467C-8E3D-C4579291692E");
    private static readonly Guid AudioMeterInformationId = new("C02216F6-8C67-4B5B-9D00-D008E73E0064");
    private static readonly PropertyKey DeviceFriendlyNameKey = new(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 14);

    public static IReadOnlyList<AudioInputDevice> GetActiveCaptureDevices()
    {
        var devices = new List<AudioInputDevice>();
        IMMDeviceEnumerator? enumerator = null;
        IMMDeviceCollection? collection = null;

        try
        {
            enumerator = CreateDeviceEnumerator();
            enumerator.EnumAudioEndpoints(EDataFlow.Capture, DeviceState.Active, out collection);
            collection.GetCount(out var count);

            for (uint index = 0; index < count; index++)
            {
                IMMDevice? device = null;

                try
                {
                    collection.Item(index, out device);
                    device.GetId(out var id);
                    var name = GetFriendlyName(device);
                    devices.Add(new AudioInputDevice(id, name));
                }
                finally
                {
                    ReleaseComObject(device);
                }
            }
        }
        finally
        {
            ReleaseComObject(collection);
            ReleaseComObject(enumerator);
        }

        return devices;
    }

    public static IAudioMeterInformation CreateAudioMeter(string deviceId)
    {
        IMMDeviceEnumerator? enumerator = null;
        IMMDevice? device = null;

        try
        {
            enumerator = CreateDeviceEnumerator();
            enumerator.GetDevice(deviceId, out device);
            var audioMeterInformationId = AudioMeterInformationId;
            device.Activate(ref audioMeterInformationId, ClsCtx.InprocServer, IntPtr.Zero, out var meter);
            return (IAudioMeterInformation)meter;
        }
        finally
        {
            ReleaseComObject(device);
            ReleaseComObject(enumerator);
        }
    }

    public static void ReleaseComObject(object? instance)
    {
        if (instance is not null && Marshal.IsComObject(instance))
        {
            Marshal.ReleaseComObject(instance);
        }
    }

    private static IMMDeviceEnumerator CreateDeviceEnumerator()
    {
        var enumeratorType = Type.GetTypeFromCLSID(MMDeviceEnumeratorId, throwOnError: true)!;
        return (IMMDeviceEnumerator)Activator.CreateInstance(enumeratorType)!;
    }

    private static string GetFriendlyName(IMMDevice device)
    {
        IPropertyStore? propertyStore = null;

        try
        {
            device.OpenPropertyStore(StorageAccessMode.Read, out propertyStore);
            var friendlyNameKey = DeviceFriendlyNameKey;
            propertyStore.GetValue(ref friendlyNameKey, out var value);

            try
            {
                var name = value.GetString();
                return string.IsNullOrWhiteSpace(name) ? "Unnamed input device" : name.Trim();
            }
            finally
            {
                PropVariantClear(ref value);
            }
        }
        finally
        {
            ReleaseComObject(propertyStore);
        }
    }

    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PropVariant propVariant);
}

[ComImport]
[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceEnumerator
{
    void EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask, out IMMDeviceCollection devices);
    void GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice endpoint);
    void GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
    void RegisterEndpointNotificationCallback(IntPtr client);
    void UnregisterEndpointNotificationCallback(IntPtr client);
}

[ComImport]
[Guid("0BD7A1BE-7A1A-44DB-8397-C0F2CB9F482F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceCollection
{
    void GetCount(out uint count);
    void Item(uint index, out IMMDevice device);
}

[ComImport]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDevice
{
    void Activate(ref Guid iid, ClsCtx clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.Interface)] out object instance);
    void OpenPropertyStore(StorageAccessMode accessMode, out IPropertyStore propertyStore);
    void GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
    void GetState(out DeviceState state);
}

[ComImport]
[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPropertyStore
{
    void GetCount(out uint propertyCount);
    void GetAt(uint propertyIndex, out PropertyKey key);
    void GetValue(ref PropertyKey key, out PropVariant value);
    void SetValue(ref PropertyKey key, ref PropVariant value);
    void Commit();
}

[ComImport]
[Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioMeterInformation
{
    void GetPeakValue(out float peak);
    void GetMeteringChannelCount(out uint channelCount);
    void GetChannelsPeakValues(uint channelCount, [Out] float[] peakValues);
    void QueryHardwareSupport(out uint hardwareSupportMask);
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct PropertyKey
{
    public PropertyKey(Guid formatId, uint propertyId)
    {
        FormatId = formatId;
        PropertyId = propertyId;
    }

    public readonly Guid FormatId;
    public readonly uint PropertyId;
}

[StructLayout(LayoutKind.Explicit)]
internal struct PropVariant
{
    [FieldOffset(0)]
    private ushort _valueType;

    [FieldOffset(8)]
    private IntPtr _value;

    public string? GetString()
    {
        const ushort vtLpwstr = 31;
        return _valueType == vtLpwstr ? Marshal.PtrToStringUni(_value) : null;
    }
}

internal enum EDataFlow
{
    Render,
    Capture,
    All,
}

internal enum ERole
{
    Console,
    Multimedia,
    Communications,
}

[Flags]
internal enum DeviceState
{
    Active = 0x00000001,
}

[Flags]
internal enum ClsCtx
{
    InprocServer = 0x1,
}

internal enum StorageAccessMode
{
    Read = 0,
}
