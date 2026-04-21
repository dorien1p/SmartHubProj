using System.Collections.Generic;

public class MotionSensor : ISubject
{
    private List<IObserver> observers = new List<IObserver>();

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

    public void DetectMotion()
    {
        Console.WriteLine("[SENSOR] Motion detected!");
        NotifyObservers("Motion detected in the house!");
    }
}