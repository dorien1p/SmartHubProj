using System;
using System.Threading;

class Program
{
    static void Main()
    {
        SmartLight light = new SmartLight("Living Room Light");
        SmartDoorLock door = new SmartDoorLock("Front Door");
        Thermostat thermostat = new Thermostat("Main Thermostat", 70);

        ICommand lightOn = new TurnOnLightCommand(light);
        ICommand lockDoor = new LockDoorCommand(door);
        ICommand setTemp = new SetTemperatureCommand(thermostat, 72);

        MotionSensor sensor = new MotionSensor();
        HomeownerApp app = new HomeownerApp();
        EventLogger logger = new EventLogger();

        sensor.Attach(app);
        sensor.Attach(logger);
        thermostat.Attach(app);
        thermostat.Attach(logger);

        lightOn.Execute();
        lockDoor.Execute();
        setTemp.Execute();

        sensor.MotionDetected += message =>
        {
            Console.WriteLine("[EVENT] " + message);
        };

        Thread sensorThread = new Thread(() =>
        {
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(2000);
                sensor.DetectMotion();
            }
        });

        sensorThread.Start();
        sensorThread.Join();

        Console.WriteLine();
        Console.WriteLine("Final Device Status:");
        Console.WriteLine($"Light: {(light.Status ? "ON" : "OFF")}");
        Console.WriteLine($"Door: {(door.Status ? "LOCKED" : "UNLOCKED")}");
        Console.WriteLine($"Thermostat: {thermostat.Temperature}°F");
    }
}
