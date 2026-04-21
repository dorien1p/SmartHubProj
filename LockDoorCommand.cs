public class LockDoorCommand : ICommand
{
    private SmartDoorLock doorLock;

    public LockDoorCommand(SmartDoorLock doorLock)
    {
        this.doorLock = doorLock;
    }

    public void Execute()
    {
        doorLock.Lock();
    }
}