using System;

public class SmartDoorLock : SmartDevice
{
    public SmartDoorLock(string name) : base(name)
    {
    }

    public void Lock()
    {
        IsOn = true;
        Console.WriteLine($"{Name} LOCKED");
    }

    public void Unlock()
    {
        IsOn = false;
        Console.WriteLine($"{Name} UNLOCKED");
    }

    public override void DisplayStatus()
    {
        Console.WriteLine($"{Name}: {(IsOn ? "LOCKED" : "UNLOCKED")}");
    }
}