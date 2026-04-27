using System.Collections.Concurrent;

namespace SmartHubCore;

////////////////////////////////////////////////////////////
/// INTERFACES - REQUIRED BY PROJECT 8
////////////////////////////////////////////////////////////

public interface ICommand
{
    string Name { get; }
    void Execute();
    void Undo();
}

public interface IObserver
{
    void Update(string message);
}

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify(string message);
}

////////////////////////////////////////////////////////////
/// ABSTRACT CLASS - REQUIRED BY PROJECT 8
////////////////////////////////////////////////////////////

public abstract class SmartDevice
{
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }

    public event EventHandler<string>? StatusChanged;

    protected SmartDevice(string name, float x, float y)
    {
        Name = name;
        X = x;
        Y = y;
    }

    protected void RaiseStatusChanged(string message)
    {
        StatusChanged?.Invoke(this, message);
    }

    public abstract string GetStatus();
}

////////////////////////////////////////////////////////////
/// SMART DEVICE CLASSES - 8+ DEVICE TYPES
////////////////////////////////////////////////////////////

public class SmartLightDevice : SmartDevice
{
    public bool IsOn { get; set; }
    public SmartLightDevice(string name, float x, float y, bool isOn) : base(name, x, y) => IsOn = isOn;
    public void Toggle() { IsOn = !IsOn; RaiseStatusChanged($"{Name} is now {GetStatus()}."); }
    public override string GetStatus() => IsOn ? "ON" : "OFF";
}

public class SmartDoorLockDevice : SmartDevice
{
    public bool IsLocked { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
    public bool HasSecondTile { get; set; }
    public int SecondTileX { get; set; }
    public int SecondTileY { get; set; }

    public SmartDoorLockDevice(string name, float x, float y, bool isLocked, int tileX, int tileY,
        bool hasSecondTile = false, int secondTileX = 0, int secondTileY = 0) : base(name, x, y)
    {
        IsLocked = isLocked;
        TileX = tileX;
        TileY = tileY;
        HasSecondTile = hasSecondTile;
        SecondTileX = secondTileX;
        SecondTileY = secondTileY;
    }

    public void Toggle() { IsLocked = !IsLocked; RaiseStatusChanged($"{Name} is now {GetStatus()}."); }
    public override string GetStatus() => IsLocked ? "LOCKED" : "UNLOCKED";
}

public class SmartThermostatDevice : SmartDevice
{
    public int Temperature { get; set; }
    public SmartThermostatDevice(string name, float x, float y, int temperature) : base(name, x, y) => Temperature = temperature;
    public void Adjust(int delta) { Temperature += delta; RaiseStatusChanged($"{Name} set to {Temperature}°F."); }
    public override string GetStatus() => $"{Temperature}°F";
}

public abstract class SecurityDevice : SmartDevice, ISubject
{
    private readonly List<IObserver> observers = [];
    public string DeviceType { get; set; }
    public float Range { get; set; }
    public bool Triggered { get; private set; }

    public event EventHandler? TriggeredChanged;

    protected SecurityDevice(string name, float x, float y, string deviceType, float range) : base(name, x, y)
    {
        DeviceType = deviceType;
        Range = range;
    }

    public void SetTriggered(bool triggered)
    {
        if (Triggered == triggered) return;
        Triggered = triggered;
        string message = Triggered ? $"{Name} triggered." : $"{Name} cleared.";
        TriggeredChanged?.Invoke(this, EventArgs.Empty);
        RaiseStatusChanged(message);
        Notify(message);
    }

    public void Attach(IObserver observer)
    {
        if (!observers.Contains(observer)) observers.Add(observer);
    }

    public void Detach(IObserver observer) => observers.Remove(observer);

    public void Notify(string message)
    {
        foreach (IObserver observer in observers)
            observer.Update(message);
    }

    public override string GetStatus() => Triggered ? "TRIGGERED" : "MONITORING";
}

public class SmartCameraDevice : SecurityDevice
{
    public SmartCameraDevice(string name, float x, float y, float range) : base(name, x, y, "Camera", range) { }
}

public class SmartMotionSensorDevice : SecurityDevice
{
    public SmartMotionSensorDevice(string name, float x, float y, float range) : base(name, x, y, "Sensor", range) { }
}

public class SmartFanDevice : SmartDevice
{
    public bool IsOn { get; set; }
    public SmartFanDevice(string name, float x, float y, bool isOn) : base(name, x, y) => IsOn = isOn;
    public void Toggle() { IsOn = !IsOn; RaiseStatusChanged($"{Name} is now {GetStatus()}."); }
    public void SetOn(bool on) { if (IsOn != on) { IsOn = on; RaiseStatusChanged($"{Name} is now {GetStatus()}."); } }
    public override string GetStatus() => IsOn ? "ON" : "OFF";
}

public class SmartAlarmDevice : SmartDevice
{
    public bool IsArmed { get; set; }
    public bool IsSirenOn { get; set; }
    public SmartAlarmDevice(string name, float x, float y, bool isArmed) : base(name, x, y) => IsArmed = isArmed;
    public void ToggleArmed() { IsArmed = !IsArmed; RaiseStatusChanged($"{Name} is now {GetStatus()}."); }
    public void SetSiren(bool on) { if (IsSirenOn != on) { IsSirenOn = on; RaiseStatusChanged($"{Name} siren {(on ? "ON" : "OFF")}."); } }
    public override string GetStatus() => IsArmed ? (IsSirenOn ? "ARMED / SIREN" : "ARMED") : "DISARMED";
}

public class SmartWindowBlindDevice : SmartDevice
{
    public bool IsOpen { get; set; }
    public SmartWindowBlindDevice(string name, float x, float y, bool isOpen) : base(name, x, y) => IsOpen = isOpen;
    public void Toggle() { IsOpen = !IsOpen; RaiseStatusChanged($"{Name} is now {GetStatus()}."); }
    public override string GetStatus() => IsOpen ? "OPEN" : "CLOSED";
}

////////////////////////////////////////////////////////////
/// OBSERVER IMPLEMENTATIONS
////////////////////////////////////////////////////////////

public class HomeownerAppObserver(ConcurrentQueue<string> alerts) : IObserver
{
    public void Update(string message) => alerts.Enqueue($"{DateTime.Now:HH:mm:ss} - Homeowner App: {message}");
}

public class EventLoggerObserver(ConcurrentQueue<string> alerts) : IObserver
{
    public void Update(string message) => alerts.Enqueue($"{DateTime.Now:HH:mm:ss} - Log: {message}");
}

////////////////////////////////////////////////////////////
/// COMMAND PATTERN + UNDO / REDO
////////////////////////////////////////////////////////////

public class CommandInvoker
{
    private readonly Stack<ICommand> undoStack = new();
    private readonly Stack<ICommand> redoStack = new();
    private readonly Action<string> log;

    public CommandInvoker(Action<string> log) => this.log = log;

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        undoStack.Push(command);
        redoStack.Clear();
        log($"Command executed: {command.Name}");
    }

    public bool Undo()
    {
        if (undoStack.Count == 0) return false;
        ICommand command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
        log($"Command undone: {command.Name}");
        return true;
    }

    public bool Redo()
    {
        if (redoStack.Count == 0) return false;
        ICommand command = redoStack.Pop();
        command.Execute();
        undoStack.Push(command);
        log($"Command redone: {command.Name}");
        return true;
    }
}

public class ToggleLightCommand(SmartLightDevice light) : ICommand
{
    public string Name => $"Toggle {light.Name}";
    public void Execute() => light.Toggle();
    public void Undo() => light.Toggle();
}

public class ToggleDoorLockCommand(SmartDoorLockDevice door) : ICommand
{
    public string Name => $"Toggle {door.Name}";
    public void Execute() => door.Toggle();
    public void Undo() => door.Toggle();
}

public class AdjustThermostatCommand(SmartThermostatDevice thermostat, int delta) : ICommand
{
    public string Name => $"Adjust {thermostat.Name} by {delta}";
    public void Execute() => thermostat.Adjust(delta);
    public void Undo() => thermostat.Adjust(-delta);
}

public class ToggleFanCommand(SmartFanDevice fan) : ICommand
{
    public string Name => $"Toggle {fan.Name}";
    public void Execute() => fan.Toggle();
    public void Undo() => fan.Toggle();
}

public class ToggleAlarmCommand(SmartAlarmDevice alarm) : ICommand
{
    public string Name => $"Toggle {alarm.Name}";
    public void Execute() => alarm.ToggleArmed();
    public void Undo() => alarm.ToggleArmed();
}

public class ToggleBlindCommand(SmartWindowBlindDevice blind) : ICommand
{
    public string Name => $"Toggle {blind.Name}";
    public void Execute() => blind.Toggle();
    public void Undo() => blind.Toggle();
}

////////////////////////////////////////////////////////////
/// AUTOMATION RULES + SCHEDULING SYSTEM
////////////////////////////////////////////////////////////

public class AutomationRule
{
    public string Name { get; }
    private readonly Func<SmartHubState, bool> condition;
    private readonly Action<SmartHubState> action;
    private readonly TimeSpan cooldown;
    private DateTime lastRun = DateTime.MinValue;

    public AutomationRule(string name, Func<SmartHubState, bool> condition, Action<SmartHubState> action, TimeSpan cooldown)
    {
        Name = name;
        this.condition = condition;
        this.action = action;
        this.cooldown = cooldown;
    }

    public void Evaluate(SmartHubState state)
    {
        if (!condition(state)) return;
        if (DateTime.Now - lastRun < cooldown) return;
        action(state);
        lastRun = DateTime.Now;
        state.AddAlert($"Automation rule fired: {Name}");
    }
}

public class AutomationEngine
{
    private readonly List<AutomationRule> rules = [];
    public IReadOnlyList<AutomationRule> Rules => rules;
    public void AddRule(AutomationRule rule) => rules.Add(rule);
    public void Run(SmartHubState state)
    {
        foreach (AutomationRule rule in rules)
            rule.Evaluate(state);
    }
}

public class ScheduledAction
{
    public string Name { get; }
    public TimeSpan Interval { get; }
    public DateTime NextRun { get; private set; }
    private readonly Action<SmartHubState> action;

    public ScheduledAction(string name, TimeSpan interval, Action<SmartHubState> action)
    {
        Name = name;
        Interval = interval;
        this.action = action;
        NextRun = DateTime.Now + interval;
    }

    public void RunIfDue(SmartHubState state)
    {
        if (DateTime.Now < NextRun) return;
        action(state);
        state.AddAlert($"Scheduled action ran: {Name}");
        NextRun = DateTime.Now + Interval;
    }
}

public class SmartScheduler
{
    private readonly List<ScheduledAction> actions = [];
    public IReadOnlyList<ScheduledAction> Actions => actions;
    public void Add(ScheduledAction action) => actions.Add(action);
    public void RunDue(SmartHubState state)
    {
        foreach (ScheduledAction action in actions)
            action.RunIfDue(state);
    }
}

////////////////////////////////////////////////////////////
/// SHARED STATE + SIMULATION
////////////////////////////////////////////////////////////

public sealed class SmartHubState
{
    public const string OperatorKey = "TEAMSMARTHUB";

    private readonly object sync = new();
    private readonly int[,] floorMap = new int[20, 14];
    private readonly CommandInvoker commandInvoker;
    private readonly AutomationEngine automationEngine = new();
    private readonly SmartScheduler scheduler = new();

    public PersonMarker Person { get; } = new("Resident", 4.5f, 3.5f);
    public List<SmartLightDevice> Lights { get; } = [];
    public List<SmartDoorLockDevice> Doors { get; } = [];
    public SmartThermostatDevice Thermostat { get; private set; } = null!;
    public List<SecurityDevice> SecurityDevices { get; } = [];
    public List<SmartFanDevice> Fans { get; } = [];
    public List<SmartAlarmDevice> Alarms { get; } = [];
    public List<SmartWindowBlindDevice> Blinds { get; } = [];
    public ConcurrentQueue<string> Alerts { get; } = new();
    public event Action<string>? LogEmitted;

    public SmartHubState()
    {
        commandInvoker = new CommandInvoker(AddAlert);
    }

    public void Initialize()
    {
        BuildFloorPlan();

        Lights.Add(new SmartLightDevice("Living Room Light", 4.5f, 7.5f, true));
        Lights.Add(new SmartLightDevice("Kitchen Light", 11.5f, 3.5f, false));
        Lights.Add(new SmartLightDevice("Bedroom Light", 16.5f, 9.5f, false));
        Doors.Add(new SmartDoorLockDevice("Front Door", 9.5f, 13.0f, true, 9, 13, true, 10, 13));
        Doors.Add(new SmartDoorLockDevice("Bedroom Door", 13.5f, 10.5f, false, 13, 10));
        Thermostat = new SmartThermostatDevice("Main Thermostat", 9.5f, 6.5f, 75);
        SecurityDevices.Add(new SmartCameraDevice("Living Room Camera", 1.5f, 1.5f, 5.5f));
        SecurityDevices.Add(new SmartMotionSensorDevice("Kitchen Motion Sensor", 11.5f, 2.5f, 3.0f));
        SecurityDevices.Add(new SmartCameraDevice("Bedroom Camera", 18.0f, 5.5f, 5.0f));
        Fans.Add(new SmartFanDevice("Smart Fan", 10.5f, 6.5f, false));
        Alarms.Add(new SmartAlarmDevice("Smart Alarm", 7.5f, 11.5f, true));
        Blinds.Add(new SmartWindowBlindDevice("Living Room Blinds", 2.5f, 9.5f, true));

        IObserver homeowner = new HomeownerAppObserver(Alerts);
        IObserver logger = new EventLoggerObserver(Alerts);
        foreach (SecurityDevice device in SecurityDevices)
        {
            device.Attach(homeowner);
            device.Attach(logger);
        }

        ConfigureAutomationRules();
        ConfigureSchedule();
        AddAlert("System ready.");
    }

    private void ConfigureAutomationRules()
    {
        automationEngine.AddRule(new AutomationRule(
            "Motion detected turns on living room light and alarm siren",
            s => s.SecurityDevices.Any(d => d.Triggered),
            s => { s.Lights[0].IsOn = true; s.Alarms[0].SetSiren(true); },
            TimeSpan.FromSeconds(5)));

        automationEngine.AddRule(new AutomationRule(
            "Temperature too hot turns fan on",
            s => s.Thermostat.Temperature >= 76,
            s => s.Fans[0].SetOn(true),
            TimeSpan.FromSeconds(5)));

        automationEngine.AddRule(new AutomationRule(
            "Temperature comfortable turns fan off",
            s => s.Thermostat.Temperature <= 72,
            s => s.Fans[0].SetOn(false),
            TimeSpan.FromSeconds(5)));
    }

    private void ConfigureSchedule()
    {
        scheduler.Add(new ScheduledAction("Close blinds check", TimeSpan.FromSeconds(20), s => s.Blinds[0].IsOpen = false));
        scheduler.Add(new ScheduledAction("Trim old alerts", TimeSpan.FromSeconds(10), s => s.TrimAlerts()));
    }

    public void ExecuteCommand(ICommand command)
    {
        lock (sync) commandInvoker.ExecuteCommand(command);
    }

    public bool Undo()
    {
        lock (sync) return commandInvoker.Undo();
    }

    public bool Redo()
    {
        lock (sync) return commandInvoker.Redo();
    }

    public void MovePerson(float dx, float dy)
    {
        lock (sync)
        {
            float nextX = Person.X + dx;
            float nextY = Person.Y + dy;
            bool blockedX = !IsWalkable(nextX, Person.Y);
            bool blockedY = !IsWalkable(Person.X, nextY);
            if (!blockedX) Person.X = nextX;
            if (!blockedY) Person.Y = nextY;
            if (blockedX || blockedY) AddAlert("Access denied: locked door or wall blocking movement.");
        }
    }

    public void RunBackgroundTick()
    {
        lock (sync)
        {
            foreach (SecurityDevice device in SecurityDevices)
                device.SetTriggered(HasLineOfSight(device.X, device.Y, Person.X, Person.Y, device.Range));
            automationEngine.Run(this);
            scheduler.RunDue(this);
            TrimAlerts();
        }
    }

    public void AddAlert(string message)
    {
        string entry = $"{DateTime.Now:HH:mm:ss} - {message}";
        Alerts.Enqueue(entry);
        LogEmitted?.Invoke(entry);
        TrimAlerts();
    }

    private void TrimAlerts()
    {
        while (Alerts.Count > 30) Alerts.TryDequeue(out _);
    }

    private bool IsWalkable(float gridX, float gridY)
    {
        int x = (int)MathF.Floor(gridX);
        int y = (int)MathF.Floor(gridY);
        if (x < 0 || y < 0 || x >= 20 || y >= 14) return false;
        if (floorMap[x, y] == 1) return false;
        if (IsLockedDoorTile(x, y)) return false;
        return true;
    }

    private bool IsLockedDoorTile(int tileX, int tileY)
    {
        foreach (SmartDoorLockDevice door in Doors)
        {
            if (!door.IsLocked) continue;
            if (door.TileX == tileX && door.TileY == tileY) return true;
            if (door.HasSecondTile && door.SecondTileX == tileX && door.SecondTileY == tileY) return true;
        }
        return false;
    }

    private bool HasLineOfSight(float fromX, float fromY, float toX, float toY, float maxRange)
    {
        float dx = toX - fromX;
        float dy = toY - fromY;
        float dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist > maxRange) return false;
        int steps = Math.Max(8, (int)(dist * 16f));
        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            int mx = (int)MathF.Floor(fromX + dx * t);
            int my = (int)MathF.Floor(fromY + dy * t);
            if (mx < 0 || my < 0 || mx >= 20 || my >= 14) return false;
            if (floorMap[mx, my] == 1) return false;
        }
        return true;
    }

    private void BuildFloorPlan()
    {
        for (int y = 0; y < 14; y++) for (int x = 0; x < 20; x++) floorMap[x, y] = 0;
        for (int x = 0; x < 20; x++) { floorMap[x, 0] = 1; floorMap[x, 13] = 1; }
        for (int y = 0; y < 14; y++) { floorMap[0, y] = 1; floorMap[19, y] = 1; }
        for (int y = 1; y < 10; y++) floorMap[8, y] = 1; floorMap[8, 5] = 2;
        for (int x = 8; x < 19; x++) floorMap[x, 4] = 1; floorMap[12, 4] = 2;
        for (int x = 3; x < 17; x++) floorMap[x, 10] = 1; floorMap[6, 10] = 2; floorMap[13, 10] = 2;
        for (int y = 4; y < 10; y++) floorMap[14, y] = 1; floorMap[14, 7] = 2;
        floorMap[9, 13] = 2; floorMap[10, 13] = 2;
    }

    public object ToDto()
    {
        lock (sync)
        {
            return new
            {
                mapCols = 20, mapRows = 14, tileSize = 32,
                person = new { name = Person.Name, x = Person.X, y = Person.Y },
                lights = Lights.Select((l, i) => new { id = i, name = l.Name, x = l.X, y = l.Y, isOn = l.IsOn, status = l.GetStatus() }),
                doors = Doors.Select((d, i) => new { id = i, name = d.Name, x = d.X, y = d.Y, isLocked = d.IsLocked, status = d.GetStatus() }),
                thermostat = new { name = Thermostat.Name, x = Thermostat.X, y = Thermostat.Y, temperature = Thermostat.Temperature, status = Thermostat.GetStatus() },
                securityDevices = SecurityDevices.Select((d, i) => new { id = i, name = d.Name, deviceType = d.DeviceType, x = d.X, y = d.Y, range = d.Range, dStatus = d.GetStatus(), triggered = d.Triggered }),
                fans = Fans.Select((f, i) => new { id = i, name = f.Name, x = f.X, y = f.Y, isOn = f.IsOn, status = f.GetStatus() }),
                alarms = Alarms.Select((a, i) => new { id = i, name = a.Name, x = a.X, y = a.Y, isArmed = a.IsArmed, isSirenOn = a.IsSirenOn, status = a.GetStatus() }),
                blinds = Blinds.Select((b, i) => new { id = i, name = b.Name, x = b.X, y = b.Y, isOpen = b.IsOpen, status = b.GetStatus() }),
                alerts = Alerts.Reverse().Take(12).ToArray(),
                rooms = new[] { new { name = "Living Room", x = 2, y = 2 }, new { name = "Kitchen", x = 10, y = 2 }, new { name = "Hallway", x = 8, y = 6 }, new { name = "Bedroom", x = 15, y = 7 }, new { name = "Bathroom", x = 15, y = 5 } },
                walls = GetTilesByValue(1), doorTiles = GetTilesByValue(2)
            };
        }
    }

    private object[] GetTilesByValue(int value)
    {
        var list = new List<object>();
        for (int y = 0; y < 14; y++) for (int x = 0; x < 20; x++) if (floorMap[x, y] == value) list.Add(new { x, y });
        return list.ToArray();
    }
}

public class PersonMarker(string name, float x, float y)
{
    public string Name { get; set; } = name;
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
}
