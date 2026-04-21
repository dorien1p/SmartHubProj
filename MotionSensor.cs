using System.Collections.Generic;

public class MotionSensor : SmartDevice, ISubject
{
    private List<IObserver> observers = new List<IObserver>();

    public MotionSensor(string name) : base(name) { }

    public void DetectMotion()
    {
        NotifyObservers("Motion detected!");
    }

    public void RegisterObserver(IObserver observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void NotifyObservers(string message)
    {
        foreach (var observer in observers)
        {
            observer.Update(message);
        }
    }

    public override string GetStatus()
    {
        return IsOn ? "ARMED" : "IDLE";
    }
}