# SteamWorkshopUpdater
A RimWorld mod updater build on Steamworks.NET

# How to use
Clone the repo.
Compile both the lib and cli projects.
Make sure you're logged in to the steam client, and not running any steam apps.
Call `SteamWorkshopUpdater.exe` with the full path to a mod folder as the argument, e.g.;

```
.\SteamWorkshopUpdater.exe "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\TestMod"
```

# Notes
Most of the code is in the `lib` project, so you can use that as a reference.
I am absolutely new to asynchronous programming in C#, so my choices may be dubious at best.
Descriptions are currently not handled, as I want to parse the 'html' that Unity uses into the 'bbcode' that Steam uses.
