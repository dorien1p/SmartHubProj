public class ToggleDeviceCommand : ICommand
{
    private SmartDevice device;

    public ToggleDeviceCommand(SmartDevice device)
    {
        this.device = device;
    }

    public void Execute()
    {
        device.Toggle();
    }
}