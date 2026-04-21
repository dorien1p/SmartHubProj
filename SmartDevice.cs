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
    }

    public virtual void TurnOff()
    {
        IsOn = false;
    }

    public virtual void Toggle()
    {
        if (IsOn)
            TurnOff();
        else
            TurnOn();
    }

    public virtual string GetStatus()
    {
        return IsOn ? "ON" : "OFF";
    }
}