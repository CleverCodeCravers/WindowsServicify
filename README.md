# Windows Servicify

![Build](https://github.com/CleverCodeCravers/WindowsServicify/actions/workflows/build.yml/badge.svg)

A .NET CLI tool that wraps any script or executable as a Windows Service. It configures, installs, and monitors processes as Windows services with automatic restart and logging.

## Installation

Download the latest release from [GitHub Releases](https://github.com/CleverCodeCravers/WindowsServicify/releases). Extract the ZIP file to a folder of your choice.

**Requirements:** Windows 10/11 or Windows Server 2016+. No .NET runtime installation needed (self-contained executable).

## Usage

### Setting up and starting a Windows service

Lets say we have a PowerShell script that we want to run continuously in the background.

After obtaining the exe file we copy it to a separate folder. There we run:

```powershell
WindowsServicify.exe --configure
```

Then the application will ask us several questions that we need to answer:

```
Enter the service name you'd like: _
Enter the display name you'd like: _
Enter the service description you'd like: _
Enter the command you'd like to execute: _
Working directory for the command: _
```

These values will be written into a config.json file in the same folder the exe is in.

After that we have the following commands available:

```powershell
WindowsServicify.exe --install
WindowsServicify.exe --uninstall
WindowsServicify.exe --testrun
```

- `--install` will install the Windows service (per default with local system as a user and as disabled, but you know you can change these settings in the service manager once the service is there...)
- `--uninstall` will remove our service from the services
- `--testrun` will start the application in console mode. Everything else is the same. This way you can have a look at all the outputs.

Everything else can be configured using the normal Windows mechanisms.

### Example Config File

```json
{
  "ServiceName": "TestService",
  "DisplayName": "TestService1",
  "Description": "",
  "Command": "powershell.exe",
  "WorkingDirectory": "C:\\Scripts",
  "Arguments": "-File HelloWorld.ps1"
}
```

## Logging

The application will automatically create log files. It will keep 7 days, older logs will automatically be deleted. Each log file is named by the date `yyyy-MM-dd.log`.
The log files contain all the script output with date/timestamps in front of each line like so:

```
  - [2023-01-01 10:35:23] Hello world
```

## About the execution

The Windows service will, when started,

- execute the named command line in the named working directory.
- It will continuously watch it.
- When the process crashes or stops, it will log the crash and automatically restart the process.
- All outputs of the process are written into the log files.

## Alternatives

- [WinSW](https://github.com/winsw/winsw) -- XML-based configuration, mature project
- [Shawl](https://github.com/mtkennerly/shawl) -- Rust-based, minimal
- [NSSM](https://nssm.cc/) -- GUI-based, long-established

## Development

**Prerequisites:** [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
# Build
dotnet build Source/WindowsServicify/WindowsServicify.sln

# Run tests
dotnet test Source/WindowsServicify/WindowsServicify.sln

# Run tests with coverage
dotnet test Source/WindowsServicify/WindowsServicify.sln --collect:"XPlat Code Coverage" --settings Source/WindowsServicify/WindowsServicify.Domain.Tests/coverlet.runsettings
```

## License

[MIT](LICENSE)
