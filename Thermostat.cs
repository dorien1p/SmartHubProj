public class Thermostat : SmartDevice
{
    public int Temperature { get; private set; }

    public Thermostat(string name, int startingTemp = 72) : base(name)
    {
        Temperature = startingTemp;
        IsOn = true;
    }

    public void IncreaseTemperature()
    {
        Temperature++;
    }

    public void DecreaseTemperature()
    {
        Temperature--;
    }

    public override string GetStatus()
    {
        return $"{Temperature}°F";
    }
}