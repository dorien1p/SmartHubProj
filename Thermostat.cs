using System;

public class Thermostat : SmartDevice
{
    public int Temperature { get; private set; }

    public Thermostat(string name, int startingTemperature = 72) : base(name)
    {
        Temperature = startingTemperature;
    }

    public void SetTemperature(int newTemperature)
    {
        Temperature = newTemperature;
        Console.WriteLine($"{Name} set to {Temperature}°F");
    }

    public override void DisplayStatus()
    {
        Console.WriteLine($"{Name}: {(IsOn ? "ON" : "OFF")} | Temperature: {Temperature}°F");
    }
}