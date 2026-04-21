public class HomeownerApp : IObserver
{
    public void Update(string message)
    {
        Console.WriteLine($"[ALERT TO HOMEOWNER] {message}");
    }
}