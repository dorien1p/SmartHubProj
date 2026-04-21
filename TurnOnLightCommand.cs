public class TurnOnLightCommand : ICommand
{
    private SmartLight light;

    public TurnOnLightCommand(SmartLight light)
    {
        this.light = light;
    }

    public void Execute()
    {
        light.TurnOn();
    }
}