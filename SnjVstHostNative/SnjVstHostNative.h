#pragma once

#ifdef SNJVSTHOSTNATIVE_EXPORTS
#define SNJVSTHOST_API __declspec(dllexport)
#else
#define SNJVSTHOST_API __declspec(dllimport)
#endif

typedef void* SnjVstHostHandle;

extern "C"
{
SNJVSTHOST_API int SnjVstHost_GetApiVersion();

SNJVSTHOST_API SnjVstHostHandle SnjVstHost_Create();

SNJVSTHOST_API void SnjVstHost_Destroy(SnjVstHostHandle host);

SNJVSTHOST_API int SnjVstHost_LoadPlugin(
    SnjVstHostHandle host,
    const wchar_t* pluginPath);

SNJVSTHOST_API int SnjVstHost_SetupProcessing(
    SnjVstHostHandle host,
    double sampleRate,
    int maxBlockSize,
    int inputChannels,
    int outputChannels);

SNJVSTHOST_API int SnjVstHost_ProcessFloat32(
    SnjVstHostHandle host,
    const float* inputInterleaved,
    float* outputInterleaved,
    int frameCount);

SNJVSTHOST_API int SnjVstHost_OpenEditor(
    SnjVstHostHandle host,
    void* parentHwnd);

SNJVSTHOST_API void SnjVstHost_CloseEditor(SnjVstHostHandle host);

SNJVSTHOST_API const wchar_t* SnjVstHost_GetLastError(SnjVstHostHandle host);
}
