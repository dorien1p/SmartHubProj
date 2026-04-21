using System;

public class EventLogger : IObserver
{
    public void Update(string message)
    {
        Console.WriteLine("[LOG] " + message);
    }
}