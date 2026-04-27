# SmartHub Simulator

Branch: simulation
Repository: https://github.com/dorien1p/SmartHubProj.git

---

## Overview

SmartHub is a smart home simulation system that models a real residential environment with two distinct user roles:

1. Operator (resident inside the home)
2. Remote user (homeowner monitoring and controlling the home)

The system demonstrates real-time interaction between a moving resident and remotely controlled smart devices using a network-based architecture.

---

## Objective

This project satisfies the requirements of a smart home simulation system by implementing:

* Observer Pattern
* Command Pattern with Undo/Redo
* Interfaces
* Abstract Classes
* Events
* Multithreading
* Automation Rules
* Scheduling System
* Logging (Serilog)
* Unit Testing

---

## System Architecture

### Server (SmartHub Core)

The server manages the entire system state and provides API endpoints for control and monitoring.

Responsibilities include:

* Device state management
* Resident position tracking
* Security monitoring
* Automation rules execution
* Background processing
* Logging
* Network communication

---

### Operator Application

The operator represents the resident inside the home.

Capabilities:

* Move through the house using W, A, S, D
* Trigger cameras and sensors
* Interact with doors and environment

Movement is restricted by:

* Walls
* Locked doors

---

### Remote Web Application

The remote user accesses the system through a browser on any device within the same network.

Capabilities:

* Toggle lights
* Lock/unlock doors
* Control fan
* Activate/deactivate alarm
* Open/close blinds
* Adjust thermostat
* View alerts
* Use undo and redo

The remote user cannot control movement.

---

## Smart Devices

The system includes more than eight devices:

* Living Room Light
* Kitchen Light
* Bedroom Light
* Front Door Lock
* Bedroom Door Lock
* Thermostat
* Smart Fan
* Smart Alarm
* Window Blinds
* Motion Sensor
* Security Cameras

---

## Design Patterns

### Observer Pattern

Security devices act as subjects and notify observers when triggered.

Observers include:

* Homeowner interface
* Event logger

---

### Command Pattern

All device operations are implemented as commands.

Examples:

* ToggleLightCommand
* ToggleDoorLockCommand
* ToggleFanCommand
* ToggleAlarmCommand
* ToggleBlindCommand
* AdjustThermostatCommand

---

### Undo and Redo

Command history is maintained and supports undo and redo operations.

Endpoints:

* POST /api/undo
* POST /api/redo

---

### Interfaces

* ICommand
* IObserver
* ISubject

---

### Abstract Class

* SmartDevice

---

### Events

* TriggeredChanged
* LogEmitted

---

### Multithreading

A background loop continuously updates the simulation:

* Security checks
* Automation rules
* Scheduled logic

---

## Automation Rules

The system includes event-driven behavior:

* Motion detection triggers alarm
* Motion detection activates lights
* High temperature activates fan
* Low temperature disables fan
* Security triggers generate alerts

---

## Scheduling System

The system uses a background loop that runs continuously to process:

* Automation rules
* Security updates
* Timed system actions

---

## Logging

Serilog is used for logging.

Logs are written to:

logs/smarthub-.log

Logging includes:

* Device actions
* Security events
* Automation triggers
* System activity

---

## Network Access

The system runs on:

http://0.0.0.0:5000

Access from another device using:

http://<your-ip>:5000

Example:

http://192.168.4.26:5000

---

## Security

Operator movement is protected.

Endpoint:

POST /api/operator/move

Requires header:

X-Operator-Key: TEAMSMARTHUB

Only the operator application can move the resident.

---

## API Endpoints

### State

GET /api/state
GET /api/network

---

### Device Controls

POST /api/lights/{id}/toggle
POST /api/doors/{id}/toggle
POST /api/fans/{id}/toggle
POST /api/alarms/{id}/toggle
POST /api/blinds/{id}/toggle

---

### Thermostat

POST /api/thermostat/up
POST /api/thermostat/down

---

### Command History

POST /api/undo
POST /api/redo

---

### Operator Movement

POST /api/operator/move

---

## How to Run

### Start Server

```bash id="l7vhzd"
cd SmartHubServer
dotnet run
```

---

### Start Operator Application

```bash id="0y21yx"
cd SmartHubOperator
dotnet run
```

---

### Open Web Application

Open in browser:

http://<your-ip>:5000

---

## Unit Testing

Run tests:

```bash id="ylkh8v"
cd SmartHubTests
dotnet test
```

Tests include:

* Command execution
* Undo/redo functionality
* Observer notifications
* Automation behavior

---

## Demonstration Flow

1. Start the server
2. Start the operator application
3. Open the web interface on another device
4. Move the resident
5. Trigger sensors or cameras
6. Observe alerts
7. Control devices remotely
8. Lock doors and test movement restrictions
9. Use undo and redo
10. Review logs

---

## Requirement Coverage

* 8+ devices implemented
* Observer pattern implemented
* Command pattern implemented
* Undo/redo implemented
* Interfaces implemented
* Abstract class implemented
* Events implemented
* Multithreading implemented
* Automation rules implemented
* Scheduling system implemented
* Serilog logging implemented
* Unit tests implemented

---

## Conclusion

SmartHub is a complete smart home simulation system that demonstrates distributed control, real-time monitoring, automation, and software design patterns within a functional and interactive environment.
