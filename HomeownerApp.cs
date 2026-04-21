using System.Windows.Forms;

public class HomeownerApp : IObserver
{
    private readonly Form _form;

    public HomeownerApp(Form form)
    {
        _form = form;
    }

    public void Update(string message)
    {
        MessageBox.Show(_form, message, "Smart Home Alert");
    }
}