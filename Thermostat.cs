using System.Collections.Generic;
using System;

public class Thermostat : SmartDevice, ISubject
{
    private List<IObserver> observers = new List<IObserver>();
    public int Temperature { get; set; }

    public Thermostat(string name, int temperature) : base(name)
    {
        Temperature = temperature;
    }

    public void SetTemperature(int temp)
    {
        Temperature = temp;
        Notify(Name + " temperature set to " + Temperature + "°F");
    }

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
}