public class ToggleDeviceCommand : ICommand
{
    private readonly SmartDevice _device;

    public ToggleDeviceCommand(SmartDevice device)
    {
        _device = device;
    }

    public void Execute()
    {
        _device.Toggle();
    }
}