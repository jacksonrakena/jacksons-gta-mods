# Jackson's GTA 5 Mods
This repository contains all of the source code for my publicly available GTA 5 mods, because I am lazy and it's convenient.

## Projects

### ModCommon
`ModCommon` provides utility classes and the reference DLL for `ScriptHookVDotNet` to all projects.  
ModCommon controls the SHVDN version.  
Utility classes available:  
- `ModConfig`
    - Provides utilities for interacting with INI files and reading config values
    - Will provide support for writing values in future

### NoDamageMod
Abyssal's famous [One-Hit Knock-Out mod](https://www.gta5-mods.com/scripts/abyssal-s-one-hit-knock-out-1-4).  
Uses ModCommon from version 2.0 onwards.  

### InputDetect
A work-in-progress mod to detect, record, and count all inputs.  
Uses Windows Forms for UI.