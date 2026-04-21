using System;

public class SmartDoorLock : SmartDevice
{
    public SmartDoorLock(string name) : base(name) { }

    public void LockDoor()
    {
        Status = true;
        Console.WriteLine(Name + " LOCKED");
    }

    public void UnlockDoor()
    {
        Status = false;
        Console.WriteLine(Name + " UNLOCKED");
    }
}