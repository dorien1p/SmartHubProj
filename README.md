# Project 8 - Team SmartHub Complete Build

This complete version includes:

- 8+ smart devices
- Observer pattern
- Command pattern with undo/redo
- Interfaces
- Abstract classes
- Events
- Multithreading
- Automation rules
- Scheduling system
- Serilog logging
- xUnit + Moq unit tests
- LAN/IP web control panel
- Operator desktop client for resident movement

## Run the server

```bash
cd SmartHubServer
dotnet run --project SmartHomeSimulator.csproj
```

Open from phone/laptop on same Wi-Fi:

```text
http://192.168.4.26:5000
```

## Run the operator app

```bash
cd SmartHubOperator
dotnet run --project SmartHubOperator.csproj
```

## Run tests

```bash
cd SmartHubTests
dotnet test
```

## Change IP if needed

In `SmartHubOperator/OperatorForm.cs`, update:

```csharp
private readonly string baseUrl = "http://192.168.4.26:5000";
```
