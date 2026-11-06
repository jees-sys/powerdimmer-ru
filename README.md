# PowerDimmer RU

A distraction dimmer for Windows: dims inactive windows so that only the one you are working in stays bright. A fork of [shayne/PowerDimmer](https://github.com/shayne/PowerDimmer) (MIT) with reliability fixes and a Russian tray UI.

![Concept](https://github.com/shayne/PowerDimmer/raw/main/.github/powerdimmer.png)

## Why this fork

The original is great, but on some Windows setups its settings silently stop being saved (the `Config.Net` JSON store uses `File.Replace`, which can fail and, in the worst case, crash the app). This fork replaces that with a simple, robust settings implementation and adds quality-of-life features.

**Improvements over the original:**

- **Russian tray menu** with one-click presets: `Off (0)`, `Soft (30)`, `Medium (60)`, `Dark (100)`.
- **Dim slider 0–100** in the tray — the value is the *dim strength*: 0 = no dimming, 100 = full blackout around the active window.
- **Reliable settings**: plain JSON at `%APPDATA%\PowerDimmer\settings.json`, written via a temp file + copy with retries — no `File.Replace`, no lost changes, no crashes.
- **Debounced slider saving** — the settings file is no longer rewritten on every tick.
- **Single-instance guard** (named mutex).
- Every settings write is fault-tolerant; the app never crashes because of the settings file.

## Features (inherited)

- Dims everything except the currently focused window.
- Pin/unpin a window so it stays bright: `Win + Shift + D`.
- Window shade for that one blinding window without dark mode: `Win + Alt + S`.
- Shade a rectangular area of a window: `Win + Alt + A`.
- Optional taskbar dimming, multi-monitor support.

## Usage

1. Download a build or [build it yourself](#building).
2. Run `PowerDimmer.exe` — the app has **no window**, it lives in the system tray.
3. Right-click the tray icon: toggles, presets, the dim slider, and exit.

> **Emergency switch:** `Win + Ctrl + Alt + D` toggles dimming on/off instantly. If you set 100% and the screen goes black around you, press it. 😄

## Building

Requires the [.NET SDK](https://dotnet.microsoft.com/en-us/download) (6.0 or newer).

```
dotnet build
```

## License

MIT. Based on [PowerDimmer](https://github.com/shayne/PowerDimmer) by [shayne](https://github.com/shayne).
