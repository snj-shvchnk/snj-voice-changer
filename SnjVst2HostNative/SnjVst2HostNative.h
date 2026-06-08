#pragma once

#ifdef SNJVST2HOSTNATIVE_EXPORTS
#define SNJVST2HOST_API __declspec(dllexport)
#else
#define SNJVST2HOST_API __declspec(dllimport)
#endif

typedef void* SnjVst2HostHandle;

extern "C"
{
SNJVST2HOST_API int SnjVst2Host_GetApiVersion();

SNJVST2HOST_API SnjVst2HostHandle SnjVst2Host_Create();

SNJVST2HOST_API void SnjVst2Host_Destroy(SnjVst2HostHandle host);

SNJVST2HOST_API int SnjVst2Host_LoadPlugin(
    SnjVst2HostHandle host,
    const wchar_t* pluginPath);

SNJVST2HOST_API int SnjVst2Host_SetupProcessing(
    SnjVst2HostHandle host,
    double sampleRate,
    int maxBlockSize,
    int inputChannels,
    int outputChannels);

SNJVST2HOST_API int SnjVst2Host_ProcessFloat32(
    SnjVst2HostHandle host,
    const float* inputInterleaved,
    float* outputInterleaved,
    int frameCount);

SNJVST2HOST_API int SnjVst2Host_OpenEditor(
    SnjVst2HostHandle host,
    void* parentHwnd);

SNJVST2HOST_API int SnjVst2Host_GetEditorSize(
    SnjVst2HostHandle host,
    int* width,
    int* height);

SNJVST2HOST_API void SnjVst2Host_EditorIdle(SnjVst2HostHandle host);

SNJVST2HOST_API void SnjVst2Host_CloseEditor(SnjVst2HostHandle host);

SNJVST2HOST_API const wchar_t* SnjVst2Host_GetLastError(SnjVst2HostHandle host);
}
