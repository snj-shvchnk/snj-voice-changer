# Snj Voice Changer

<p align="center">
  <img src="SnjVoiceChanger/Assets/app.png" alt="Snj Voice Changer" width="220">
</p>

Snj Voice Changer is a Windows desktop voice changer that routes a real microphone through a mixed VST2/VST3 plugin chain and into a virtual audio cable. It is built for calls, streams, experiments, and the happy little chaos of making your voice sound less ordinary.

```text
Real microphone -> Snj Voice Changer -> VST2/VST3 chain -> CABLE Input -> CABLE Output -> Meet / Discord / Chrome
```

## Download

The ready-to-install Windows build is in [`dist`](dist):

- [`dist/SnjVoiceChanger_v1.2.exe`](dist/SnjVoiceChanger_v1.2.exe)

Download and run the installer. It installs the app into `Program Files`, creates shortcuts, and can optionally launch the bundled VB-CABLE driver installer on the final setup screen.

## Requirements

- Windows 10/11 x64.
- VB-Audio Virtual Cable or a compatible virtual audio cable.
- 64-bit VST2 `.dll` plugins and/or VST3 `.vst3` plugins if you want effects in the chain.

VB-CABLE creates two important Windows audio endpoints:

- `CABLE Input` - playback/output device used by Snj Voice Changer.
- `CABLE Output` - recording/input device used by Meet, Discord, Chrome, OBS, etc.

Snj Voice Changer sends processed audio to `CABLE Input`. Your call or recording app should use `CABLE Output` as its microphone.

## Quick Start

1. Download [`SnjVoiceChanger_v1.2.exe`](dist/SnjVoiceChanger_v1.2.exe).
2. Run the installer.
3. Leave `Install VB-CABLE virtual audio driver` checked if VB-CABLE is not installed yet.
4. Launch Snj Voice Changer.
5. Select your real microphone in `InputDevice`.
6. Select `CABLE Input` in `OutputDevice`.
7. Press `Start`.
8. In Google Meet, Discord, Chrome, OBS, or another app, select `CABLE Output` as the microphone.
9. Add VST2 or VST3 plugins to the chain if you want voice effects.

For best results, set your microphone and VB-CABLE endpoints to the same sample rate in Windows sound settings, preferably `48000 Hz`.

## Features

- Real-time microphone routing.
- Input and output device selectors.
- VB-CABLE endpoint detection.
- Input and output level meters.
- Compact latency diagnostics.
- Recursive VST plugin folder scanning.
- 64-bit VST2 plugin support.
- VST3 plugin support.
- Mixed VST2/VST3 chain processing.
- Plugin editor windows.
- Plugin enable/disable checkboxes.
- Plugin reorder controls.
- Dark Windows desktop UI.
- Self-contained installer with desktop and Start Menu shortcuts.

## VST Plugins

The app scans the selected plugin folder recursively. The default bundled layout is:

```text
common/VST/vst2
common/VST/vst3
```

VST2 candidates are accepted only when they are 64-bit Windows DLLs and export `VSTPluginMain` or legacy `main`. VST3 candidates are loaded from `.vst3` modules.

Some host-specific plugins may not behave like normal portable VST plugins. In particular, Cockos/ReaPlugs VST2 editors can render incomplete slider controls in this lightweight host, even when audio processing works. Use standalone VST2 plugins or VST3 alternatives if a specific editor behaves oddly.

## Notes

Snj Voice Changer does not install its own virtual microphone driver. It relies on VB-CABLE or another signed virtual audio cable. The bundled VB-CABLE installer is launched separately so you can choose whether to install the driver.

If the virtual cable was just installed, Windows may need a moment, an audio-device refresh, or a reboot before the new endpoints appear.

## Changelog

### v1.2

- Added 64-bit VST2 plugin discovery, loading, editor hosting, and audio processing.
- Added mixed VST2/VST3 chains, so both plugin formats can be used together.
- Added recursive plugin scanning for the `common/VST/vst2` and `common/VST/vst3` layout.
- Improved installer/publish scripts to include both native VST host DLLs.

### v1.1

- Added the first installable self-contained release package.
- Added dark UI polish, app icon, fixed-size layout, and installer shortcuts.

### v1.0

- First working voice-routing release with VB-CABLE output, VST3 processing, plugin editors, reorder controls, and enable/disable checkboxes.

## Development

Open `SnjVoiceChanger.sln` in Visual Studio 2022 or newer.

Main projects:

- `SnjVoiceChanger` - C# WinForms application.
- `SnjVstHostNative` - native C++ VST3 host layer.
- `SnjVst2HostNative` - native C++ VST2 host layer.

Build/research notes and handoff documents live in [`docs`](docs). Installer scripts live in [`publish`](publish).

The native VST3 host expects the Steinberg VST3 SDK under `third_party/vst3sdk`.

## Status

The current release line routes audio, hosts mixed VST2/VST3 plugin chains, opens plugin editors, installs cleanly, and is ready for real-world testing.
