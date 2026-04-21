public class SetTemperatureCommand : ICommand
{
    private Thermostat thermostat;
    private int temperature;

    public SetTemperatureCommand(Thermostat thermostat, int temperature)
    {
        this.thermostat = thermostat;
        this.temperature = temperature;
    }

    public void Execute()
    {
        thermostat.SetTemperature(temperature);
    }
}