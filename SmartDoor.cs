public class SmartDoor : SmartDevice
{
    public SmartDoor(string name) : base(name) { }

    public override string GetStatus()
    {
        return IsOn ? "OPEN" : "CLOSED";
    }

    public override void TurnOn()
    {
        IsOn = true;
    }

    public override void TurnOff()
    {
        IsOn = false;
    }
}