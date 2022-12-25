# Windows Servicify

The tool is a hybrid application. The usage is as follows:

## Setting up and starting a windows service

Lets say we have a powershell script that we want to run continuously in the background.

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

- `--install` will install the windows service (per default with local system as a user and as disabled, but you know you can change these settings in the service manager once the service is there...)
- `--uninstall` will remove our service from the services
- `--testrun` will start the application in console mode. Everything else is the same. This way you can have a look at all the outputs.

Everything else can be configured using the normal Windows mechanisms.

## Logging

The application will automatically create log files. It will keep 7 days, older logs will automatically be deleted. Each log file is named by the date `yyyy-MM-dd.log`. 
The log files contain all the script output with date/timestamps in front of each line like so:

```
  - [2023-01-01 10:35:23] Hello world
```

## About the execution

The windows service will, when started, 
- execute the named command line in the named working directory. 
- It will continously watch it.
- When the process crashes or stops, it will log the crash and automatically restart the process.



