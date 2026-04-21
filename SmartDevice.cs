public abstract class SmartDevice
{
    public string Name { get; set; }
    public bool Status { get; set; }

    public SmartDevice(string name)
    {
        Name = name;
    }
}