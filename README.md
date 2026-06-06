# Snj Voice Changer v0

Snj Voice Changer is an experimental real-time voice changer for Windows. The long-term goal is to capture a microphone input, process it through a configurable VST plugin chain, and expose the processed audio as a virtual microphone that can be selected in apps like Google Meet, Discord, or Chrome.

## Current Status

This repository currently contains the first WinForms prototype built with .NET 9.

Implemented:

- Lists active Windows input audio devices.
- Shows full CoreAudio device friendly names.
- Displays a live input signal level meter for the selected microphone.
- Shows virtual microphone detection status for `Snj Voice Changer`.

Not implemented yet:

- Virtual microphone driver.
- Real-time audio routing.
- VST plugin loading.
- VST chain editing and processing.

## Virtual Microphone Note

A normal WinForms application cannot create a Windows recording endpoint by itself. For `Snj Voice Changer` to appear as a selectable microphone in Chrome or Google Meet, the project will need a virtual audio driver or integration with an existing virtual audio cable solution.

The current prototype only detects whether such an endpoint already exists.

## Requirements

- Windows
- Visual Studio 2022 or newer
- .NET 9 SDK

## Run

Open `SnjVoiceChanger.sln` in Visual Studio and run the `SnjVoiceChanger` project.

## Roadmap

1. Confirm the virtual microphone strategy.
2. Add audio capture and routing pipeline.
3. Add VST plugin discovery and loading.
4. Add editable VST chain UI.
5. Route processed audio into the virtual microphone endpoint.
