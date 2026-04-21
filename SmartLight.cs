using System;

public class SmartLight : SmartDevice
{
    public SmartLight(string name) : base(name) { }

    public void TurnOn()
    {
        Status = true;
        Console.WriteLine(Name + " ON");
    }

    public void TurnOff()
    {
        Status = false;
        Console.WriteLine(Name + " OFF");
    }
}