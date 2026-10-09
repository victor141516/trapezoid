# Trapezoid

Trapezoid is a lightweight Windows-native window manager inspired by Rectangle.

It intentionally implements the keyboard shortcut side of Rectangle: moving,
resizing, centering, restoring, and sending windows between displays. Drag-to-snap
is not implemented because Windows already provides that behavior.

Repeated shortcut executions cycle through the same kinds of follow-up positions
Rectangle users expect, such as left/right half cycling through one half, two
thirds, and one third.

## Run

```powershell
dotnet run
```

## Build

```powershell
dotnet build
```

Settings are stored in `%AppData%\Trapezoid\settings.json`.

## Background startup and portable releases

Run `Trapezoid.exe --background` to enable shortcuts and the tray icon without
opening the settings window. Only one instance runs per session. A second
interactive launch opens the existing settings window; a second background
launch exits silently.

Release packages include the runtime inside the executable. Build ZIP packages,
checksums, and the `winkit.json` release manifest with:

```powershell
./scripts/publish-portable.ps1 -AppName Trapezoid -ProjectPath Trapezoid.csproj -Version v1.3.0
```

When launched with `WINKIT_MANAGED=1`, WinKit controls startup and the independent
launch-at-login checkbox is disabled.

Launch at login is implemented with a `Trapezoid.lnk` shortcut in the current
user's Startup folder, the same folder opened by `shell:startup`.
