# NLOverlay [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

**Coming soon**

# Prerequisites

- Ensure NetLimiter is installed and running
- Ensure [.NET 4.6.2 developer pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462) is installed (if you're going to build the solution locally)
- Set `RequireElevationLocal` to `false` in NetLimiter configuration file. Without this, the program will likely misbehave since it's API access is very limited. More info [here](https://github.com/tzoug/NLOverlay/releases).
    1. Stop the NetLimiter service `net stop nlsvc`
    2. Edit the configuration `nl_setting.xml`
        - Usually located in `C:\ProgramData\Locktime\NetLimiter\5`
        - Ensure the elevated permissions. `<RequireElevationLocal>false</RequireElevationLocal>`
    3. Start the NetLimiter service `net start nlsvc`

# Usage

There are **2** ways to run the program:

- Downloading the latest `.exe` file from the [releases page](https://github.com/tzoug/NLOverlay/releases)

**OR**

- [Build the solution](#building-the-solution) and run the output `.exe` file

# Building The Solution

- Launch `NLOverlay.sln`
- Build/rebuild the solution
    - The `.exe` file should appear in `NLOverlay/bin/Debug` or `NLOverlay/bin/Release`

# Ressources

- [Fody](https://github.com/Fody/Fody)
- [HandyControl](https://github.com/HandyOrg/HandyControl)
- ...