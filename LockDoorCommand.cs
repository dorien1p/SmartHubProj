public class LockDoorCommand : ICommand
{
    private SmartDoorLock door;

    public LockDoorCommand(SmartDoorLock door)
    {
        this.door = door;
    }

    public void Execute()
    {
        door.LockDoor();
    }
}