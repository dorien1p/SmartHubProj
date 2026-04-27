using Moq;
using SmartHubCore;
using Xunit;

public class PatternTests
{
    ////////////////////////////////////////////////////////////
    /// COMMAND PATTERN TEST - EXECUTE / UNDO / REDO
    ////////////////////////////////////////////////////////////

    [Fact]
    public void ToggleLightCommand_CanUndoAndRedo()
    {
        var light = new SmartLightDevice("Test Light", 0, 0, false);
        var invoker = new CommandInvoker(_ => { });

        invoker.ExecuteCommand(new ToggleLightCommand(light));
        Assert.True(light.IsOn);

        Assert.True(invoker.Undo());
        Assert.False(light.IsOn);

        Assert.True(invoker.Redo());
        Assert.True(light.IsOn);
    }

    ////////////////////////////////////////////////////////////
    /// OBSERVER PATTERN TEST - SUBJECT NOTIFIES OBSERVER
    ////////////////////////////////////////////////////////////

    [Fact]
    public void SecurityDevice_NotifiesObserver_WhenTriggered()
    {
        var camera = new SmartCameraDevice("Test Camera", 0, 0, 5);
        var observer = new Mock<IObserver>();
        camera.Attach(observer.Object);

        camera.SetTriggered(true);

        observer.Verify(o => o.Update(It.Is<string>(s => s.Contains("triggered"))), Times.Once);
    }

    ////////////////////////////////////////////////////////////
    /// AUTOMATION RULE TEST - HIGH TEMP TURNS FAN ON
    ////////////////////////////////////////////////////////////

    [Fact]
    public void BackgroundTick_TurnsFanOn_WhenTemperatureIsHot()
    {
        var state = new SmartHubState();
        state.Initialize();

        while (state.Thermostat.Temperature < 76)
            state.ExecuteCommand(new AdjustThermostatCommand(state.Thermostat, 1));

        state.RunBackgroundTick();

        Assert.True(state.Fans[0].IsOn);
    }

    ////////////////////////////////////////////////////////////
    /// LOCKED DOOR TEST - MOVEMENT BLOCKED BY LOCKED DOOR
    ////////////////////////////////////////////////////////////

    [Fact]
    public void MovePerson_DoesNotPassThroughLockedFrontDoor()
    {
        var state = new SmartHubState();
        state.Initialize();

        // Move downward many times toward the locked front door.
        for (int i = 0; i < 200; i++)
            state.MovePerson(0, 0.14f);

        Assert.True(state.Person.Y < 13);
    }
}
