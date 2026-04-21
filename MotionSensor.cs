using System;
using System.Collections.Generic;

public class MotionSensor : ISubject
{
    private List<IObserver> observers = new List<IObserver>();

    public event Action<string> MotionDetected;

    public void Attach(IObserver observer)
    {
        observers.Add(observer);
    }

    public void Notify(string message)
    {
        foreach (var o in observers)
        {
            o.Update(message);
        }
    }

    public void DetectMotion()
    {
        string message = "Motion detected!";
        Notify(message);
        MotionDetected?.Invoke(message);
    }
}