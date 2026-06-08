# Snj Voice Changer

<p align="center">
  <img src="SnjVoiceChanger/Assets/app.png" alt="Snj Voice Changer" width="220">
</p>

Snj Voice Changer is a Windows desktop voice changer for routing a real microphone through a VST plugin chain and into a virtual audio cable. The app is built as a C# WinForms tool with a native VST3 host layer.

The current practical workflow is:

```text
Real microphone -> Snj Voice Changer -> VST3 chain -> CABLE Input -> CABLE Output -> Google Meet / Discord / Chrome
```

## What Works

- Select a Windows input device.
- Select an output device, usually `CABLE Input` from VB-CABLE.
- Detect the paired VB-CABLE endpoints.
- Route live microphone audio to the selected output device.
- Scan a VST3 plugin folder.
- Add VST3 plugins to a processing chain.
- Open plugin editors in separate windows.
- Reorder, remove, enable, and bypass chain plugins.
- Show input/output level meters and compact latency diagnostics.

## Ready Builds

Prebuilt archives are stored in [`dist`](dist):

- `SnjVoiceChanger_v1.0.7z` - current recommended build.

Older prototype archives may also be kept there for history.

Unpack the archive and run `SnjVoiceChanger.exe`.

## Requirements

- Windows
- VB-Audio Virtual Cable or a compatible virtual audio cable
- A VST3 plugin folder if you want effects

VB-CABLE creates two important endpoints:

- Playback endpoint: `CABLE Input`
- Recording endpoint: `CABLE Output`

Snj Voice Changer sends processed audio to `CABLE Input`; apps like Google Meet should use `CABLE Output` as their microphone.

## Quick Start

1. Install VB-CABLE if it is not installed yet.
2. Unpack the latest archive from [`dist`](dist).
3. Run `SnjVoiceChanger.exe`.
4. Select your real microphone in `InputDevice`.
5. Select `CABLE Input` in `OutputDevice`.
6. Press `Start`.
7. In Google Meet, Discord, Chrome, or another app, select `CABLE Output` as the microphone.
8. Optional: scan a VST3 folder, add plugins to the chain, and open their editors.

For the cleanest routing, set your microphone and VB-CABLE endpoints to the same sample rate in Windows sound settings, preferably `48000 Hz`.

## Development

Open `SnjVoiceChanger.sln` in Visual Studio 2022 or newer.

Main projects:

- `SnjVoiceChanger` - C# WinForms application.
- `SnjVstHostNative` - native C++ VST3 host layer.

Developer handoff notes and task specs live in [`docs`](docs).

## Notes

Snj Voice Changer does not install its own virtual microphone driver. It currently relies on an existing signed virtual cable such as VB-CABLE. Native VST2 support is planned as a future stage; the current chain is VST3-focused.
