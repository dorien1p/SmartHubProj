using System;

public class HomeownerApp : IObserver
{
    public void Update(string message)
    {
        Console.WriteLine("[ALERT] " + message);
    }
}