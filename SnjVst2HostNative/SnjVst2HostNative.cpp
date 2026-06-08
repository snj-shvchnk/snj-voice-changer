#include "SnjVst2HostNative.h"

#include <algorithm>
#include <cstdint>
#include <memory>
#include <string>
#include <string_view>
#include <vector>
#include <windows.h>

namespace
{
constexpr int kOk = 0;
constexpr int kErrorInvalidArgument = -1;
constexpr int kErrorFileNotFound = -2;
constexpr int kErrorLoadLibrary = -3;
constexpr int kErrorEntryPoint = -4;
constexpr int kErrorPluginLoad = -5;
constexpr int kErrorUnsupported = -6;
constexpr int kErrorNotConfigured = -7;

constexpr int32_t kEffectMagic = 0x56737450; // 'VstP'
constexpr int32_t effOpen = 0;
constexpr int32_t effClose = 1;
constexpr int32_t effSetSampleRate = 10;
constexpr int32_t effSetBlockSize = 11;
constexpr int32_t effMainsChanged = 12;
constexpr int32_t effEditGetRect = 13;
constexpr int32_t effEditOpen = 14;
constexpr int32_t effEditClose = 15;
constexpr int32_t effEditIdle = 19;
constexpr int32_t effGetEffectName = 45;
constexpr int32_t effGetVendorString = 47;
constexpr int32_t effGetProductString = 48;

constexpr int32_t effFlagsHasEditor = 1 << 0;
constexpr int32_t effFlagsCanReplacing = 1 << 4;

constexpr int32_t audioMasterAutomate = 0;
constexpr int32_t audioMasterVersion = 1;
constexpr int32_t audioMasterGetSampleRate = 16;
constexpr int32_t audioMasterGetBlockSize = 17;
constexpr int32_t audioMasterGetInputLatency = 18;
constexpr int32_t audioMasterGetOutputLatency = 19;
constexpr int32_t audioMasterCanDo = 37;
constexpr int32_t audioMasterGetLanguage = 38;
constexpr int32_t audioMasterCockosExtension = static_cast<int32_t>(0xDEADBEEF);
constexpr int32_t cockosGetApiFunction = static_cast<int32_t>(0xDEADF00D);
constexpr int32_t cockosGetHostContext = static_cast<int32_t>(0xDEADF00E);

struct AEffect;

using AudioMasterCallback = intptr_t(__cdecl*)(
    AEffect* effect,
    int32_t opcode,
    int32_t index,
    intptr_t value,
    void* ptr,
    float opt);

using AEffectDispatcherProc = intptr_t(__cdecl*)(
    AEffect* effect,
    int32_t opcode,
    int32_t index,
    intptr_t value,
    void* ptr,
    float opt);

using AEffectProcessProc = void(__cdecl*)(
    AEffect* effect,
    float** inputs,
    float** outputs,
    int32_t sampleFrames);

using AEffectSetParameterProc = void(__cdecl*)(
    AEffect* effect,
    int32_t index,
    float parameter);

using AEffectGetParameterProc = float(__cdecl*)(
    AEffect* effect,
    int32_t index);

struct AEffect
{
    int32_t magic;
    AEffectDispatcherProc dispatcher;
    AEffectProcessProc process;
    AEffectSetParameterProc setParameter;
    AEffectGetParameterProc getParameter;
    int32_t numPrograms;
    int32_t numParams;
    int32_t numInputs;
    int32_t numOutputs;
    int32_t flags;
    intptr_t reserved1;
    intptr_t reserved2;
    int32_t initialDelay;
    int32_t realQualities;
    int32_t offQualities;
    float ioRatio;
    void* object;
    void* user;
    int32_t uniqueID;
    int32_t version;
    AEffectProcessProc processReplacing;
    AEffectProcessProc processDoubleReplacing;
    char future[56];
};

struct ERect
{
    int16_t top;
    int16_t left;
    int16_t bottom;
    int16_t right;
};

using VstPluginMainProc = AEffect*(__cdecl*)(AudioMasterCallback audioMaster);

thread_local std::vector<int32_t> g_audioMasterOpcodes;
thread_local std::vector<std::wstring> g_cockosFunctionRequests;

std::wstring ToWide(const char* value);

int g_reaperConfigNumCpu = 1;
char g_reaperFxLoadStateContext = 0;
char g_emptyReaperIniPath[] = "";

double __cdecl CockosGetZeroTime()
{
    return 0.0;
}

int __cdecl CockosGetStoppedPlayState()
{
    return 0;
}

int __cdecl CockosGetStoppedPlayStateEx(void*)
{
    return 0;
}

void __cdecl CockosSetEditCursorPosition(double, bool, bool)
{
}

int __cdecl CockosGetSetRepeat(int)
{
    return 0;
}

void __cdecl CockosGetProjectPath(char* buffer, int bufferSize)
{
    if (buffer != nullptr && bufferSize > 0)
    {
        buffer[0] = '\0';
    }
}

void __cdecl CockosTransportNoOp()
{
}

int __cdecl CockosAudioIsNotRunning()
{
    return 0;
}

void* __cdecl CockosGetConfigVar(const char* name, int* sizeOut)
{
    if (sizeOut != nullptr)
    {
        *sizeOut = 0;
    }

    if (name == nullptr)
    {
        return nullptr;
    }

    const std::string_view configName(name);
    if (configName == "__numcpu")
    {
        if (sizeOut != nullptr)
        {
            *sizeOut = sizeof(g_reaperConfigNumCpu);
        }

        return &g_reaperConfigNumCpu;
    }

    if (configName == "__fx_loadstate_ctx")
    {
        if (sizeOut != nullptr)
        {
            *sizeOut = sizeof(g_reaperFxLoadStateContext);
        }

        return &g_reaperFxLoadStateContext;
    }

    return nullptr;
}

const char* __cdecl CockosGetIniFile()
{
    return g_emptyReaperIniPath;
}

int __cdecl CockosPluginRegister(const char*, void*)
{
    return 0;
}

intptr_t ResolveCockosHostFunction(const char* functionName)
{
    if (functionName == nullptr)
    {
        return 0;
    }

    const std::string_view name(functionName);
    if (name == "GetPlayPosition" || name == "GetPlayPosition2" || name == "GetCursorPosition")
    {
        return reinterpret_cast<intptr_t>(&CockosGetZeroTime);
    }

    if (name == "GetPlayState")
    {
        return reinterpret_cast<intptr_t>(&CockosGetStoppedPlayState);
    }

    if (name == "GetPlayStateEx")
    {
        return reinterpret_cast<intptr_t>(&CockosGetStoppedPlayStateEx);
    }

    if (name == "SetEditCurPos")
    {
        return reinterpret_cast<intptr_t>(&CockosSetEditCursorPosition);
    }

    if (name == "GetSetRepeat")
    {
        return reinterpret_cast<intptr_t>(&CockosGetSetRepeat);
    }

    if (name == "GetProjectPath")
    {
        return reinterpret_cast<intptr_t>(&CockosGetProjectPath);
    }

    if (name == "OnPlayButton" || name == "OnStopButton" || name == "OnPauseButton")
    {
        return reinterpret_cast<intptr_t>(&CockosTransportNoOp);
    }

    if (name == "IsInRealTimeAudio" || name == "Audio_IsRunning")
    {
        return reinterpret_cast<intptr_t>(&CockosAudioIsNotRunning);
    }

    if (name == "get_config_var")
    {
        return reinterpret_cast<intptr_t>(&CockosGetConfigVar);
    }

    if (name == "get_ini_file")
    {
        return reinterpret_cast<intptr_t>(&CockosGetIniFile);
    }

    if (name == "plugin_register")
    {
        return reinterpret_cast<intptr_t>(&CockosPluginRegister);
    }

    return 0;
}

intptr_t ResolveAudioMasterCanDo(const void* ptr)
{
    if (ptr == nullptr)
    {
        return 0;
    }

    const std::string_view capability(static_cast<const char*>(ptr));
    if (capability == "sendVstEvents" ||
        capability == "sendVstMidiEvent" ||
        capability == "receiveVstEvents" ||
        capability == "receiveVstMidiEvent" ||
        capability == "sizeWindow")
    {
        return 1;
    }

    return 0;
}

std::wstring FormatAudioMasterOpcodes()
{
    if (g_audioMasterOpcodes.empty())
    {
        return L"none";
    }

    std::wstring result;
    for (size_t index = 0; index < g_audioMasterOpcodes.size(); index++)
    {
        if (index > 0)
        {
            result += L",";
        }

        result += std::to_wstring(g_audioMasterOpcodes[index]);
    }

    return result;
}

std::wstring FormatCockosFunctionRequests()
{
    if (g_cockosFunctionRequests.empty())
    {
        return L"none";
    }

    std::wstring result;
    for (size_t index = 0; index < g_cockosFunctionRequests.size(); index++)
    {
        if (index > 0)
        {
            result += L",";
        }

        result += g_cockosFunctionRequests[index];
    }

    return result;
}

intptr_t __cdecl AudioMaster(
    AEffect*,
    int32_t opcode,
    int32_t index,
    intptr_t,
    void* ptr,
    float)
{
    g_audioMasterOpcodes.push_back(opcode);

    if (opcode == audioMasterCockosExtension)
    {
        if (index == cockosGetApiFunction && ptr != nullptr)
        {
            const auto* functionName = static_cast<const char*>(ptr);
            const intptr_t resolvedFunction = ResolveCockosHostFunction(functionName);
            g_cockosFunctionRequests.push_back(
                ToWide(functionName) + (resolvedFunction != 0 ? L":ok" : L":null"));
            return resolvedFunction;
        }

        if (index == cockosGetHostContext)
        {
            return 0;
        }

        return 0;
    }

    switch (opcode)
    {
    case audioMasterAutomate:
        return 0;
    case audioMasterVersion:
        return 2400;
    case audioMasterGetSampleRate:
        return 48000;
    case audioMasterGetBlockSize:
        return 512;
    case audioMasterGetInputLatency:
    case audioMasterGetOutputLatency:
        return 0;
    case audioMasterCanDo:
        return ResolveAudioMasterCanDo(ptr);
    case audioMasterGetLanguage:
        return 1;
    default:
        return 0;
    }
}

AEffect* CallEntryPointSafely(VstPluginMainProc entryPoint, bool& raisedException)
{
    raisedException = false;

    __try
    {
        g_cockosFunctionRequests.clear();
        return entryPoint(AudioMaster);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        raisedException = true;
        return nullptr;
    }
}

intptr_t CallDispatcherSafely(
    AEffect* effect,
    int32_t opcode,
    int32_t index,
    intptr_t value,
    void* ptr,
    float opt,
    bool& raisedException)
{
    raisedException = false;

    __try
    {
        return effect->dispatcher(effect, opcode, index, value, ptr, opt);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        raisedException = true;
        return 0;
    }
}

void CallProcessReplacingSafely(
    AEffect* effect,
    float** inputs,
    float** outputs,
    int32_t sampleFrames,
    bool& raisedException)
{
    raisedException = false;

    __try
    {
        effect->processReplacing(effect, inputs, outputs, sampleFrames);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        raisedException = true;
    }
}

std::wstring ToWide(const char* value)
{
    if (value == nullptr || value[0] == '\0')
    {
        return std::wstring();
    }

    const int length = MultiByteToWideChar(CP_UTF8, 0, value, -1, nullptr, 0);
    if (length <= 1)
    {
        return std::wstring();
    }

    std::wstring result(static_cast<size_t>(length - 1), L'\0');
    MultiByteToWideChar(CP_UTF8, 0, value, -1, result.data(), length);
    return result;
}

std::wstring GetWindowsErrorMessage(DWORD errorCode)
{
    wchar_t* buffer = nullptr;
    const DWORD length = FormatMessageW(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
        nullptr,
        errorCode,
        0,
        reinterpret_cast<wchar_t*>(&buffer),
        0,
        nullptr);

    if (length == 0 || buffer == nullptr)
    {
        return L"Windows error " + std::to_wstring(errorCode);
    }

    std::wstring message(buffer, length);
    LocalFree(buffer);

    while (!message.empty() && (message.back() == L'\r' || message.back() == L'\n' || message.back() == L' '))
    {
        message.pop_back();
    }

    return message;
}

struct SnjVst2Host
{
    HMODULE module = nullptr;
    AEffect* effect = nullptr;
    bool opened = false;
    bool processingConfigured = false;
    bool editorOpen = false;
    int inputChannels = 0;
    int outputChannels = 0;
    std::wstring lastError;
    std::wstring pluginName;

    ~SnjVst2Host()
    {
        ClosePlugin();
    }

    int Fail(int code, const std::wstring& message)
    {
        lastError = message;
        return code;
    }

    void SetOk()
    {
        lastError.clear();
    }

    void ClosePlugin()
    {
        if (effect != nullptr && opened && effect->dispatcher != nullptr)
        {
            if (editorOpen)
            {
                effect->dispatcher(effect, effEditClose, 0, 0, nullptr, 0.0f);
                editorOpen = false;
            }

            effect->dispatcher(effect, effMainsChanged, 0, 0, nullptr, 0.0f);
            effect->dispatcher(effect, effClose, 0, 0, nullptr, 0.0f);
        }

        effect = nullptr;
        opened = false;
        processingConfigured = false;
        inputChannels = 0;
        outputChannels = 0;
        pluginName.clear();

        if (module != nullptr)
        {
            FreeLibrary(module);
            module = nullptr;
        }
    }
};

SnjVst2Host* FromHandle(SnjVst2HostHandle host)
{
    return static_cast<SnjVst2Host*>(host);
}

void ReadPluginStrings(SnjVst2Host& host)
{
    if (host.effect == nullptr || host.effect->dispatcher == nullptr)
    {
        return;
    }

    char name[256] = {};
    host.effect->dispatcher(host.effect, effGetEffectName, 0, 0, name, 0.0f);
    host.pluginName = ToWide(name);

    char vendor[256] = {};
    host.effect->dispatcher(host.effect, effGetVendorString, 0, 0, vendor, 0.0f);

    char product[256] = {};
    host.effect->dispatcher(host.effect, effGetProductString, 0, 0, product, 0.0f);
}
}

extern "C" SNJVST2HOST_API int SnjVst2Host_GetApiVersion()
{
    return 1;
}

extern "C" SNJVST2HOST_API SnjVst2HostHandle SnjVst2Host_Create()
{
    auto host = std::make_unique<SnjVst2Host>();
    return host.release();
}

extern "C" SNJVST2HOST_API void SnjVst2Host_Destroy(SnjVst2HostHandle host)
{
    std::unique_ptr<SnjVst2Host> nativeHost(FromHandle(host));
}

extern "C" SNJVST2HOST_API int SnjVst2Host_LoadPlugin(
    SnjVst2HostHandle host,
    const wchar_t* pluginPath)
{
    SnjVst2Host* nativeHost = FromHandle(host);
    if (nativeHost == nullptr || pluginPath == nullptr || pluginPath[0] == L'\0')
    {
        return kErrorInvalidArgument;
    }

    nativeHost->ClosePlugin();

    const DWORD attributes = GetFileAttributesW(pluginPath);
    if (attributes == INVALID_FILE_ATTRIBUTES || (attributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
    {
        return nativeHost->Fail(kErrorFileNotFound, L"VST2 plugin DLL was not found.");
    }

    HMODULE module = LoadLibraryW(pluginPath);
    if (module == nullptr)
    {
        return nativeHost->Fail(
            kErrorLoadLibrary,
            L"Unable to load VST2 DLL: " + GetWindowsErrorMessage(GetLastError()));
    }

    auto entryPoint = reinterpret_cast<VstPluginMainProc>(GetProcAddress(module, "VSTPluginMain"));
    if (entryPoint == nullptr)
    {
        entryPoint = reinterpret_cast<VstPluginMainProc>(GetProcAddress(module, "main"));
    }

    if (entryPoint == nullptr)
    {
        FreeLibrary(module);
        return nativeHost->Fail(kErrorEntryPoint, L"VST2 DLL does not export VSTPluginMain or main.");
    }

    g_audioMasterOpcodes.clear();
    bool entryPointRaisedException = false;
    AEffect* effect = CallEntryPointSafely(entryPoint, entryPointRaisedException);
    if (entryPointRaisedException)
    {
        FreeLibrary(module);
        return nativeHost->Fail(kErrorPluginLoad, L"VST2 entrypoint raised a native exception.");
    }

    if (effect == nullptr)
    {
        FreeLibrary(module);
        return nativeHost->Fail(
            kErrorPluginLoad,
            L"VST2 entrypoint returned null AEffect. audioMaster opcodes: " +
                FormatAudioMasterOpcodes() +
                L"; Cockos requests: " +
                FormatCockosFunctionRequests());
    }

    if (effect->magic != kEffectMagic)
    {
        FreeLibrary(module);
        return nativeHost->Fail(kErrorPluginLoad, L"VST2 AEffect magic is invalid.");
    }

    if (effect->dispatcher == nullptr)
    {
        FreeLibrary(module);
        return nativeHost->Fail(kErrorPluginLoad, L"VST2 plugin does not provide a dispatcher.");
    }

    nativeHost->module = module;
    nativeHost->effect = effect;

    nativeHost->effect->dispatcher(nativeHost->effect, effOpen, 0, 0, nullptr, 0.0f);
    nativeHost->opened = true;
    ReadPluginStrings(*nativeHost);
    nativeHost->SetOk();

    return kOk;
}

extern "C" SNJVST2HOST_API int SnjVst2Host_SetupProcessing(
    SnjVst2HostHandle host,
    double sampleRate,
    int maxBlockSize,
    int inputChannels,
    int outputChannels)
{
    SnjVst2Host* nativeHost = FromHandle(host);
    if (nativeHost == nullptr)
    {
        return kErrorInvalidArgument;
    }

    if (nativeHost->effect == nullptr)
    {
        return nativeHost->Fail(kErrorNotConfigured, L"No VST2 plugin has been loaded.");
    }

    if (sampleRate <= 0 || maxBlockSize <= 0 || inputChannels <= 0 || outputChannels <= 0)
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Invalid VST2 processing setup.");
    }

    if (nativeHost->effect->processReplacing == nullptr)
    {
        return nativeHost->Fail(kErrorUnsupported, L"VST2 plugin does not provide processReplacing.");
    }

    if (nativeHost->effect->numInputs < inputChannels || nativeHost->effect->numOutputs < outputChannels)
    {
        return nativeHost->Fail(
            kErrorUnsupported,
            L"VST2 plugin channel layout is unsupported.");
    }

    nativeHost->inputChannels = inputChannels;
    nativeHost->outputChannels = outputChannels;
    nativeHost->processingConfigured = true;

    nativeHost->effect->dispatcher(nativeHost->effect, effSetSampleRate, 0, 0, nullptr, static_cast<float>(sampleRate));
    nativeHost->effect->dispatcher(nativeHost->effect, effSetBlockSize, 0, maxBlockSize, nullptr, 0.0f);
    nativeHost->effect->dispatcher(nativeHost->effect, effMainsChanged, 0, 1, nullptr, 0.0f);
    nativeHost->SetOk();

    return kOk;
}

extern "C" SNJVST2HOST_API int SnjVst2Host_ProcessFloat32(
    SnjVst2HostHandle host,
    const float* inputInterleaved,
    float* outputInterleaved,
    int frameCount)
{
    SnjVst2Host* nativeHost = FromHandle(host);
    if (nativeHost == nullptr || inputInterleaved == nullptr || outputInterleaved == nullptr || frameCount < 0)
    {
        return kErrorInvalidArgument;
    }

    if (!nativeHost->processingConfigured)
    {
        return nativeHost->Fail(kErrorNotConfigured, L"VST2 processing has not been configured.");
    }

    if (frameCount == 0)
    {
        nativeHost->SetOk();
        return kOk;
    }

    if (nativeHost->effect == nullptr || nativeHost->effect->processReplacing == nullptr)
    {
        return nativeHost->Fail(kErrorNotConfigured, L"VST2 processReplacing is not available.");
    }

    const int pluginInputChannels = nativeHost->inputChannels > nativeHost->effect->numInputs
        ? nativeHost->inputChannels
        : nativeHost->effect->numInputs;
    const int pluginOutputChannels = nativeHost->outputChannels > nativeHost->effect->numOutputs
        ? nativeHost->outputChannels
        : nativeHost->effect->numOutputs;
    std::vector<std::vector<float>> inputChannels(
        static_cast<size_t>(pluginInputChannels),
        std::vector<float>(static_cast<size_t>(frameCount), 0.0f));
    std::vector<std::vector<float>> outputChannels(
        static_cast<size_t>(pluginOutputChannels),
        std::vector<float>(static_cast<size_t>(frameCount), 0.0f));
    std::vector<float*> inputPointers(static_cast<size_t>(pluginInputChannels), nullptr);
    std::vector<float*> outputPointers(static_cast<size_t>(pluginOutputChannels), nullptr);

    for (int channel = 0; channel < pluginInputChannels; channel++)
    {
        inputPointers[static_cast<size_t>(channel)] = inputChannels[static_cast<size_t>(channel)].data();
    }

    for (int channel = 0; channel < pluginOutputChannels; channel++)
    {
        outputPointers[static_cast<size_t>(channel)] = outputChannels[static_cast<size_t>(channel)].data();
    }

    for (int frame = 0; frame < frameCount; frame++)
    {
        const int inputFrameOffset = frame * nativeHost->inputChannels;
        for (int channel = 0; channel < nativeHost->inputChannels; channel++)
        {
            inputChannels[static_cast<size_t>(channel)][static_cast<size_t>(frame)] =
                inputInterleaved[inputFrameOffset + channel];
        }
    }

    bool processRaisedException = false;
    CallProcessReplacingSafely(
        nativeHost->effect,
        inputPointers.data(),
        outputPointers.data(),
        frameCount,
        processRaisedException);

    if (processRaisedException)
    {
        return nativeHost->Fail(kErrorPluginLoad, L"VST2 processReplacing raised a native exception.");
    }

    for (int frame = 0; frame < frameCount; frame++)
    {
        const int outputFrameOffset = frame * nativeHost->outputChannels;
        for (int channel = 0; channel < nativeHost->outputChannels; channel++)
        {
            outputInterleaved[outputFrameOffset + channel] =
                outputChannels[static_cast<size_t>(channel)][static_cast<size_t>(frame)];
        }
    }

    nativeHost->SetOk();

    return kOk;
}

extern "C" SNJVST2HOST_API int SnjVst2Host_OpenEditor(
    SnjVst2HostHandle host,
    void* parentHwnd)
{
    SnjVst2Host* nativeHost = FromHandle(host);
    if (nativeHost == nullptr)
    {
        return kErrorInvalidArgument;
    }

    if (nativeHost->effect == nullptr)
    {
        return nativeHost->Fail(kErrorNotConfigured, L"No VST2 plugin has been loaded.");
    }

    if ((nativeHost->effect->flags & effFlagsHasEditor) == 0)
    {
        return nativeHost->Fail(kErrorUnsupported, L"VST2 plugin does not report an editor.");
    }

    if (parentHwnd == nullptr)
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"VST2 editor parent window handle is null.");
    }

    nativeHost->effect->dispatcher(nativeHost->effect, effEditOpen, 0, 0, parentHwnd, 0.0f);
    nativeHost->editorOpen = true;
    nativeHost->SetOk();

    return kOk;
}

extern "C" SNJVST2HOST_API int SnjVst2Host_GetEditorSize(
    SnjVst2HostHandle host,
    int* width,
    int* height)
{
    SnjVst2Host* nativeHost = FromHandle(host);
    if (nativeHost == nullptr || width == nullptr || height == nullptr)
    {
        return kErrorInvalidArgument;
    }

    *width = 0;
    *height = 0;

    if (nativeHost->effect == nullptr || nativeHost->effect->dispatcher == nullptr)
    {
        return nativeHost->Fail(kErrorNotConfigured, L"No VST2 plugin has been loaded.");
    }

    ERect* editorRect = nullptr;
    bool dispatcherRaisedException = false;
    CallDispatcherSafely(
        nativeHost->effect,
        effEditGetRect,
        0,
        0,
        &editorRect,
        0.0f,
        dispatcherRaisedException);

    if (dispatcherRaisedException)
    {
        return nativeHost->Fail(kErrorPluginLoad, L"VST2 effEditGetRect raised a native exception.");
    }

    if (editorRect == nullptr)
    {
        return nativeHost->Fail(kErrorUnsupported, L"VST2 editor did not provide a size.");
    }

    const int editorWidth = editorRect->right - editorRect->left;
    const int editorHeight = editorRect->bottom - editorRect->top;
    if (editorWidth <= 0 || editorHeight <= 0)
    {
        return nativeHost->Fail(kErrorUnsupported, L"VST2 editor size is invalid.");
    }

    *width = editorWidth;
    *height = editorHeight;
    nativeHost->SetOk();

    return kOk;
}

extern "C" SNJVST2HOST_API void SnjVst2Host_EditorIdle(SnjVst2HostHandle host)
{
    SnjVst2Host* nativeHost = FromHandle(host);
    if (nativeHost == nullptr ||
        nativeHost->effect == nullptr ||
        nativeHost->effect->dispatcher == nullptr ||
        !nativeHost->editorOpen)
    {
        return;
    }

    bool dispatcherRaisedException = false;
    CallDispatcherSafely(
        nativeHost->effect,
        effEditIdle,
        0,
        0,
        nullptr,
        0.0f,
        dispatcherRaisedException);
}

extern "C" SNJVST2HOST_API void SnjVst2Host_CloseEditor(SnjVst2HostHandle host)
{
    SnjVst2Host* nativeHost = FromHandle(host);
    if (nativeHost == nullptr || nativeHost->effect == nullptr || !nativeHost->editorOpen)
    {
        return;
    }

    nativeHost->effect->dispatcher(nativeHost->effect, effEditClose, 0, 0, nullptr, 0.0f);
    nativeHost->editorOpen = false;
}

extern "C" SNJVST2HOST_API const wchar_t* SnjVst2Host_GetLastError(SnjVst2HostHandle host)
{
    const SnjVst2Host* nativeHost = FromHandle(host);
    if (nativeHost == nullptr)
    {
        return L"Invalid VST2 host handle.";
    }

    return nativeHost->lastError.c_str();
}
