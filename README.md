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

Launch at login is implemented with a `Trapezoid.lnk` shortcut in the current
user's Startup folder, the same folder opened by `shell:startup`.
