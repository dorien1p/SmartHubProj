public class SmartDoorLock : SmartDevice
{
    public SmartDoorLock(string name) : base(name)
    {
    }

    public void Lock()
    {
        IsOn = true;
    }

    public void Unlock()
    {
        IsOn = false;
    }

    public override string GetStatus()
    {
        return IsOn ? "LOCKED" : "UNLOCKED";
    }
}