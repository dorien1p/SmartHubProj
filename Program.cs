using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        SmartLight livingRoomLight = new SmartLight("Living Room Light");
        SmartLight kitchenLight = new SmartLight("Kitchen Light");
        Thermostat thermostat = new Thermostat("Main Thermostat");
        SmartDoorLock frontDoor = new SmartDoorLock("Front Door Lock");

        MotionSensor motionSensor = new MotionSensor();
        HomeownerApp homeowner = new HomeownerApp();
        motionSensor.RegisterObserver(homeowner);

        List<SmartDevice> devices = new List<SmartDevice>
        {
            livingRoomLight,
            kitchenLight,
            thermostat,
            frontDoor
        };

        bool running = true;

        while (running)
        {
            Console.WriteLine("\n===== SMART HOME SIMULATOR =====");
            Console.WriteLine("1. Toggle Living Room Light");
            Console.WriteLine("2. Toggle Kitchen Light");
            Console.WriteLine("3. Set Thermostat Temperature");
            Console.WriteLine("4. Lock Front Door");
            Console.WriteLine("5. Unlock Front Door");
            Console.WriteLine("6. Show Device Status");
            Console.WriteLine("7. Trigger Motion Sensor");
            Console.WriteLine("8. Exit");
            Console.Write("Choose an option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    new ToggleDeviceCommand(livingRoomLight).Execute();
                    break;

                case "2":
                    new ToggleDeviceCommand(kitchenLight).Execute();
                    break;

                case "3":
                    Console.Write("Enter new temperature: ");
                    string? tempInput = Console.ReadLine();

                    if (int.TryParse(tempInput, out int temp))
                    {
                        new SetTemperatureCommand(thermostat, temp).Execute();
                    }
                    else
                    {
                        Console.WriteLine("Invalid temperature.");
                    }
                    break;

                case "4":
                    new LockDoorCommand(frontDoor).Execute();
                    break;

                case "5":
                    frontDoor.Unlock();
                    break;

                case "6":
                    Console.WriteLine("\n--- DEVICE STATUS ---");
                    foreach (var device in devices)
                    {
                        device.DisplayStatus();
                    }
                    break;

                case "7":
                    motionSensor.DetectMotion();
                    break;

                case "8":
                    running = false;
                    Console.WriteLine("Exiting Smart Home Simulator...");
                    break;

                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
    }
}