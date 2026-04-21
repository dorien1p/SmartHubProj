using System;

public abstract class SmartDevice
{
    public string Name { get; set; }
    public bool IsOn { get; protected set; }

    public SmartDevice(string name)
    {
        Name = name;
        IsOn = false;
    }

    public virtual void TurnOn()
    {
        IsOn = true;
        Console.WriteLine($"{Name} ON");
    }

    public virtual void TurnOff()
    {
        IsOn = false;
        Console.WriteLine($"{Name} OFF");
    }

    public virtual void Toggle()
    {
        if (IsOn)
            TurnOff();
        else
            TurnOn();
    }

    public virtual void DisplayStatus()
    {
        Console.WriteLine($"{Name}: {(IsOn ? "ON" : "OFF")}");
    }
}