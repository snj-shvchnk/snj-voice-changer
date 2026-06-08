#include "pch.h"
#include "SnjVstHostNative.h"

#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/gui/iplugview.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivsteditcontroller.h"
#include "public.sdk/source/vst/hosting/hostclasses.h"
#include "public.sdk/source/vst/hosting/module.h"
#include "public.sdk/source/vst/hosting/plugprovider.h"

#include <algorithm>
#include <cmath>
#include <cstring>
#include <memory>
#include <stdexcept>
#include <string>

namespace
{
constexpr int kSuccess = 0;
constexpr int kErrorInvalidHost = -1;
constexpr int kErrorInvalidArgument = -2;
constexpr int kErrorFileNotFound = -3;
constexpr int kErrorUnsupported = -4;
constexpr int kErrorNotConfigured = -5;
constexpr int kErrorNotImplemented = -6;
constexpr int kErrorPluginLoad = -7;
constexpr int kErrorNoFactory = -8;
constexpr int kErrorNoAudioEffect = -9;
constexpr int kErrorNoEditor = -10;
constexpr int kErrorEditorAttach = -11;

const wchar_t kNullHostError[] = L"Invalid host handle.";

bool SameViewRect(const Steinberg::ViewRect& left, const Steinberg::ViewRect& right)
{
    return left.left == right.left &&
        left.top == right.top &&
        left.right == right.right &&
        left.bottom == right.bottom;
}

class NativePlugFrame final : public Steinberg::IPlugFrame
{
public:
    explicit NativePlugFrame(HWND parentHwnd)
        : parentHwnd_(parentHwnd)
    {
    }

    void SetPlugView(Steinberg::IPlugView* plugView)
    {
        plugView_ = plugView;
    }

    void ResizeParentToViewSize(const Steinberg::ViewRect& viewSize) const
    {
        if (!IsWindow(parentHwnd_))
        {
            return;
        }

        const int width = std::max<int>(1, viewSize.right - viewSize.left);
        const int height = std::max<int>(1, viewSize.bottom - viewSize.top);
        const DWORD style = static_cast<DWORD>(GetWindowLongPtrW(parentHwnd_, GWL_STYLE));

        if ((style & WS_CHILD) == WS_CHILD)
        {
            SetWindowPos(
                parentHwnd_,
                nullptr,
                0,
                0,
                width,
                height,
                SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE);
            return;
        }

        RECT windowRect{ 0, 0, width, height };
        const DWORD extendedStyle = static_cast<DWORD>(GetWindowLongPtrW(parentHwnd_, GWL_EXSTYLE));
        const BOOL hasMenu = GetMenu(parentHwnd_) != nullptr;

        if (!AdjustWindowRectEx(&windowRect, style, hasMenu, extendedStyle))
        {
            windowRect = RECT{ 0, 0, width, height };
        }

        SetWindowPos(
            parentHwnd_,
            nullptr,
            0,
            0,
            windowRect.right - windowRect.left,
            windowRect.bottom - windowRect.top,
            SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE);
    }

    Steinberg::tresult PLUGIN_API resizeView(
        Steinberg::IPlugView* view,
        Steinberg::ViewRect* newSize) override
    {
        if (view == nullptr || newSize == nullptr || view != plugView_)
        {
            return Steinberg::kInvalidArgument;
        }

        if (resizeViewInProgress_)
        {
            return Steinberg::kResultFalse;
        }

        Steinberg::ViewRect currentSize{};
        if (view->getSize(&currentSize) == Steinberg::kResultTrue && SameViewRect(currentSize, *newSize))
        {
            return Steinberg::kResultTrue;
        }

        resizeViewInProgress_ = true;
        ResizeParentToViewSize(*newSize);
        const Steinberg::tresult result = view->onSize(newSize);
        resizeViewInProgress_ = false;
        return result == Steinberg::kResultTrue ? Steinberg::kResultTrue : result;
    }

    Steinberg::tresult PLUGIN_API queryInterface(const Steinberg::TUID iid, void** obj) override
    {
        if (obj == nullptr)
        {
            return Steinberg::kInvalidArgument;
        }

        *obj = nullptr;
        if (Steinberg::FUnknownPrivate::iidEqual(iid, Steinberg::IPlugFrame::iid) ||
            Steinberg::FUnknownPrivate::iidEqual(iid, Steinberg::FUnknown::iid))
        {
            *obj = static_cast<Steinberg::IPlugFrame*>(this);
            addRef();
            return Steinberg::kResultTrue;
        }

        return Steinberg::kNoInterface;
    }

    Steinberg::uint32 PLUGIN_API addRef() override
    {
        return 1000;
    }

    Steinberg::uint32 PLUGIN_API release() override
    {
        return 1000;
    }

private:
    HWND parentHwnd_ = nullptr;
    Steinberg::IPlugView* plugView_ = nullptr;
    bool resizeViewInProgress_ = false;
};

struct EditorState
{
    Steinberg::IPtr<Steinberg::Vst::PlugProvider> plugProvider;
    Steinberg::IPtr<Steinberg::Vst::IEditController> controller;
    Steinberg::IPtr<Steinberg::IPlugView> plugView;
    std::unique_ptr<NativePlugFrame> plugFrame;
    bool attached = false;
};

void CloseEditorState(EditorState& editor)
{
    if (editor.plugView)
    {
        editor.plugView->setFrame(nullptr);

        if (editor.attached)
        {
            editor.plugView->removed();
            editor.attached = false;
        }
    }

    editor.plugView = nullptr;
    editor.plugFrame.reset();
    editor.controller = nullptr;
    editor.plugProvider = nullptr;
}

void CloseEditorState(std::unique_ptr<EditorState>& editor)
{
    if (editor)
    {
        CloseEditorState(*editor);
        editor.reset();
    }
}

struct SnjVstHost
{
    std::wstring pluginPath;
    std::wstring lastError;
    double sampleRate = 0.0;
    int maxBlockSize = 0;
    int inputChannels = 0;
    int outputChannels = 0;
    bool processingConfigured = false;
    VST3::Hosting::Module::Ptr module;
    VST3::Hosting::ClassInfo selectedEffectClass;
    bool hasSelectedEffectClass = false;
    std::string selectedEffectClassId;
    std::string selectedEffectClassName;
    Steinberg::Vst::HostApplication pluginContext;
    std::unique_ptr<EditorState> editor;

    ~SnjVstHost()
    {
        CloseEditorState(editor);
        ClearPluginContextIfOwned();
    }

    void ClearError()
    {
        lastError.clear();
    }

    int Fail(int code, const wchar_t* message)
    {
        lastError = message != nullptr ? message : L"Unknown error.";
        return code;
    }

    int Fail(int code, const std::wstring& message)
    {
        lastError = message.empty() ? L"Unknown error." : message;
        return code;
    }

    void ResetPlugin()
    {
        CloseEditorState(editor);
        ClearPluginContextIfOwned();
        hasSelectedEffectClass = false;
        selectedEffectClassId.clear();
        selectedEffectClassName.clear();
        selectedEffectClass = VST3::Hosting::ClassInfo();
        module.reset();
        pluginPath.clear();
    }

    void ClearPluginContextIfOwned()
    {
        auto& contextFactory = Steinberg::Vst::PluginContextFactory::instance();
        if (contextFactory.getPluginContext() == &pluginContext)
        {
            contextFactory.setPluginContext(nullptr);
        }
    }
};

SnjVstHost* FromHandle(SnjVstHostHandle host)
{
    return static_cast<SnjVstHost*>(host);
}

bool EndsWithVst3(const std::wstring& path)
{
    constexpr wchar_t kExtension[] = L".vst3";
    constexpr size_t kExtensionLength = 5;

    if (path.length() < kExtensionLength)
    {
        return false;
    }

    const wchar_t* suffix = path.c_str() + path.length() - kExtensionLength;
    return _wcsicmp(suffix, kExtension) == 0;
}

bool PathExists(const wchar_t* path)
{
    const DWORD attributes = GetFileAttributesW(path);
    return attributes != INVALID_FILE_ATTRIBUTES;
}

std::string WideToUtf8(const std::wstring& value)
{
    if (value.empty())
    {
        return {};
    }

    const int byteCount = WideCharToMultiByte(
        CP_UTF8,
        WC_ERR_INVALID_CHARS,
        value.c_str(),
        static_cast<int>(value.length()),
        nullptr,
        0,
        nullptr,
        nullptr);

    if (byteCount <= 0)
    {
        throw std::runtime_error("Plugin path is not valid UTF-16.");
    }

    std::string result(static_cast<size_t>(byteCount), '\0');
    const int convertedByteCount = WideCharToMultiByte(
        CP_UTF8,
        WC_ERR_INVALID_CHARS,
        value.c_str(),
        static_cast<int>(value.length()),
        result.data(),
        byteCount,
        nullptr,
        nullptr);

    if (convertedByteCount != byteCount)
    {
        throw std::runtime_error("Plugin path conversion failed.");
    }

    return result;
}

std::wstring Utf8ToWide(const std::string& value)
{
    if (value.empty())
    {
        return {};
    }

    const int characterCount = MultiByteToWideChar(
        CP_UTF8,
        MB_ERR_INVALID_CHARS,
        value.c_str(),
        static_cast<int>(value.length()),
        nullptr,
        0);

    if (characterCount <= 0)
    {
        return std::wstring(value.begin(), value.end());
    }

    std::wstring result(static_cast<size_t>(characterCount), L'\0');
    const int convertedCharacterCount = MultiByteToWideChar(
        CP_UTF8,
        MB_ERR_INVALID_CHARS,
        value.c_str(),
        static_cast<int>(value.length()),
        result.data(),
        characterCount);

    if (convertedCharacterCount != characterCount)
    {
        return std::wstring(value.begin(), value.end());
    }

    return result;
}

std::wstring BuildError(const wchar_t* prefix, const std::string& detail)
{
    std::wstring result(prefix != nullptr ? prefix : L"");
    if (!detail.empty())
    {
        result += Utf8ToWide(detail);
    }
    return result;
}

bool IsSupportedChannelCount(int channelCount)
{
    return channelCount == 1 || channelCount == 2;
}
}

extern "C" SNJVSTHOST_API int SnjVstHost_GetApiVersion()
{
    return 1;
}

extern "C" SNJVSTHOST_API SnjVstHostHandle SnjVstHost_Create()
{
    auto host = std::make_unique<SnjVstHost>();
    return host.release();
}

extern "C" SNJVSTHOST_API void SnjVstHost_Destroy(SnjVstHostHandle host)
{
    delete FromHandle(host);
}

extern "C" SNJVSTHOST_API int SnjVstHost_LoadPlugin(
    SnjVstHostHandle host,
    const wchar_t* pluginPath)
{
    SnjVstHost* nativeHost = FromHandle(host);
    if (nativeHost == nullptr)
    {
        return kErrorInvalidHost;
    }

    if (pluginPath == nullptr || pluginPath[0] == L'\0')
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Plugin path is required.");
    }

    const std::wstring path(pluginPath);
    if (!EndsWithVst3(path))
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Plugin path must end with .vst3.");
    }

    if (!PathExists(pluginPath))
    {
        return nativeHost->Fail(kErrorFileNotFound, L"Plugin path does not exist.");
    }

    nativeHost->ResetPlugin();

    std::string utf8Path;
    try
    {
        utf8Path = WideToUtf8(path);
    }
    catch (const std::exception& ex)
    {
        return nativeHost->Fail(kErrorInvalidArgument, BuildError(L"Invalid plugin path: ", ex.what()));
    }

    std::string sdkError;
    VST3::Hosting::Module::Ptr module;
    try
    {
        module = VST3::Hosting::Module::create(utf8Path, sdkError);
    }
    catch (const std::exception& ex)
    {
        return nativeHost->Fail(
            kErrorPluginLoad,
            BuildError(L"VST3 SDK failed to load the module: ", ex.what()));
    }

    if (!module)
    {
        if (sdkError.empty())
        {
            sdkError = "unknown SDK error";
        }

        return nativeHost->Fail(
            kErrorPluginLoad,
            BuildError(L"VST3 SDK failed to load the module: ", sdkError));
    }

    const VST3::Hosting::PluginFactory& factory = module->getFactory();
    if (!factory.get())
    {
        return nativeHost->Fail(kErrorNoFactory, L"VST3 module did not provide a plugin factory.");
    }

    VST3::Hosting::ClassInfo selectedClass;
    bool foundAudioEffect = false;
    try
    {
        for (const VST3::Hosting::ClassInfo& classInfo : factory.classInfos())
        {
            if (classInfo.category() == kVstAudioEffectClass)
            {
                selectedClass = classInfo;
                foundAudioEffect = true;
                break;
            }
        }
    }
    catch (const std::exception& ex)
    {
        return nativeHost->Fail(
            kErrorPluginLoad,
            BuildError(L"VST3 SDK failed while reading plugin classes: ", ex.what()));
    }

    if (!foundAudioEffect)
    {
        return nativeHost->Fail(kErrorNoAudioEffect, L"VST3 module does not contain an audio effect class.");
    }

    nativeHost->pluginPath = path;
    nativeHost->module = std::move(module);
    nativeHost->selectedEffectClass = selectedClass;
    nativeHost->hasSelectedEffectClass = true;
    nativeHost->selectedEffectClassId = selectedClass.ID().toString();
    nativeHost->selectedEffectClassName = selectedClass.name();
    nativeHost->ClearError();
    return kSuccess;
}

extern "C" SNJVSTHOST_API int SnjVstHost_SetupProcessing(
    SnjVstHostHandle host,
    double sampleRate,
    int maxBlockSize,
    int inputChannels,
    int outputChannels)
{
    SnjVstHost* nativeHost = FromHandle(host);
    if (nativeHost == nullptr)
    {
        return kErrorInvalidHost;
    }

    if (!std::isfinite(sampleRate) || sampleRate <= 0.0)
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Sample rate must be greater than zero.");
    }

    if (maxBlockSize <= 0)
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Maximum block size must be greater than zero.");
    }

    if (!IsSupportedChannelCount(inputChannels) || !IsSupportedChannelCount(outputChannels))
    {
        return nativeHost->Fail(kErrorUnsupported, L"Only mono and stereo channel counts are supported.");
    }

    if (inputChannels != outputChannels && !(inputChannels == 1 && outputChannels == 2))
    {
        return nativeHost->Fail(kErrorUnsupported, L"Only matching channel counts or mono input to stereo output are supported.");
    }

    nativeHost->sampleRate = sampleRate;
    nativeHost->maxBlockSize = maxBlockSize;
    nativeHost->inputChannels = inputChannels;
    nativeHost->outputChannels = outputChannels;
    nativeHost->processingConfigured = true;
    nativeHost->ClearError();
    return kSuccess;
}

extern "C" SNJVSTHOST_API int SnjVstHost_ProcessFloat32(
    SnjVstHostHandle host,
    const float* inputInterleaved,
    float* outputInterleaved,
    int frameCount)
{
    SnjVstHost* nativeHost = FromHandle(host);
    if (nativeHost == nullptr)
    {
        return kErrorInvalidHost;
    }

    if (!nativeHost->processingConfigured)
    {
        return nativeHost->Fail(kErrorNotConfigured, L"Processing has not been configured.");
    }

    if (frameCount < 0)
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Frame count must not be negative.");
    }

    if (frameCount > nativeHost->maxBlockSize)
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Frame count exceeds configured maximum block size.");
    }

    if (frameCount == 0)
    {
        nativeHost->ClearError();
        return kSuccess;
    }

    if (inputInterleaved == nullptr || outputInterleaved == nullptr)
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Input and output buffers are required.");
    }

    if (nativeHost->inputChannels == nativeHost->outputChannels)
    {
        const size_t sampleCount = static_cast<size_t>(frameCount) *
            static_cast<size_t>(nativeHost->inputChannels);
        std::memcpy(outputInterleaved, inputInterleaved, sampleCount * sizeof(float));
    }
    else
    {
        for (int frame = 0; frame < frameCount; ++frame)
        {
            const float sample = inputInterleaved[frame];
            outputInterleaved[frame * 2] = sample;
            outputInterleaved[frame * 2 + 1] = sample;
        }
    }

    nativeHost->ClearError();
    return kSuccess;
}

extern "C" SNJVSTHOST_API int SnjVstHost_OpenEditor(
    SnjVstHostHandle host,
    void* parentHwnd)
{
    SnjVstHost* nativeHost = FromHandle(host);
    if (nativeHost == nullptr)
    {
        return kErrorInvalidHost;
    }

    if (parentHwnd == nullptr)
    {
        return nativeHost->Fail(kErrorInvalidArgument, L"Parent window handle is required.");
    }

    if (!nativeHost->module || !nativeHost->hasSelectedEffectClass)
    {
        return nativeHost->Fail(kErrorNotConfigured, L"No VST3 plugin has been loaded.");
    }

    CloseEditorState(nativeHost->editor);
    Steinberg::Vst::PluginContextFactory::instance().setPluginContext(&nativeHost->pluginContext);

    const VST3::Hosting::PluginFactory& factory = nativeHost->module->getFactory();
    if (!factory.get())
    {
        return nativeHost->Fail(kErrorNoFactory, L"VST3 module did not provide a plugin factory.");
    }

    auto editor = std::make_unique<EditorState>();
    editor->plugProvider = Steinberg::owned(new Steinberg::Vst::PlugProvider(
        factory,
        nativeHost->selectedEffectClass,
        true));

    if (!editor->plugProvider)
    {
        CloseEditorState(editor);
        return nativeHost->Fail(kErrorPluginLoad, L"VST3 plugin component/controller initialization failed.");
    }

    const bool providerInitialized = editor->plugProvider->initialize();
    editor->controller = editor->plugProvider->getControllerPtr();
    if (!editor->controller)
    {
        CloseEditorState(editor);
        return nativeHost->Fail(kErrorNoEditor, L"VST3 plugin does not provide an edit controller.");
    }

    if (!providerInitialized)
    {
        CloseEditorState(editor);
        return nativeHost->Fail(kErrorPluginLoad, L"VST3 plugin component/controller initialization failed.");
    }

    editor->plugView = Steinberg::owned(editor->controller->createView(Steinberg::Vst::ViewType::kEditor));
    if (!editor->plugView)
    {
        CloseEditorState(editor);
        return nativeHost->Fail(kErrorNoEditor, L"VST3 edit controller does not provide an editor view.");
    }

    if (editor->plugView->isPlatformTypeSupported(Steinberg::kPlatformTypeHWND) != Steinberg::kResultTrue)
    {
        CloseEditorState(editor);
        return nativeHost->Fail(kErrorUnsupported, L"VST3 editor view does not support HWND hosting.");
    }

    editor->plugFrame = std::make_unique<NativePlugFrame>(static_cast<HWND>(parentHwnd));
    editor->plugFrame->SetPlugView(editor->plugView);

    if (editor->plugView->setFrame(editor->plugFrame.get()) != Steinberg::kResultTrue)
    {
        CloseEditorState(editor);
        return nativeHost->Fail(kErrorEditorAttach, L"VST3 editor setFrame failed.");
    }

    if (editor->plugView->attached(parentHwnd, Steinberg::kPlatformTypeHWND) != Steinberg::kResultTrue)
    {
        CloseEditorState(editor);
        return nativeHost->Fail(kErrorEditorAttach, L"VST3 editor attached(HWND) failed.");
    }

    editor->attached = true;

    Steinberg::ViewRect viewSize{};
    if (editor->plugView->getSize(&viewSize) == Steinberg::kResultTrue)
    {
        editor->plugFrame->ResizeParentToViewSize(viewSize);
        editor->plugView->onSize(&viewSize);
    }

    nativeHost->editor = std::move(editor);
    nativeHost->ClearError();
    return kSuccess;
}

extern "C" SNJVSTHOST_API void SnjVstHost_CloseEditor(SnjVstHostHandle host)
{
    SnjVstHost* nativeHost = FromHandle(host);
    if (nativeHost != nullptr)
    {
        CloseEditorState(nativeHost->editor);
        nativeHost->ClearPluginContextIfOwned();
        nativeHost->ClearError();
    }
}

extern "C" SNJVSTHOST_API const wchar_t* SnjVstHost_GetLastError(SnjVstHostHandle host)
{
    const SnjVstHost* nativeHost = FromHandle(host);
    if (nativeHost == nullptr)
    {
        return kNullHostError;
    }

    return nativeHost->lastError.c_str();
}
