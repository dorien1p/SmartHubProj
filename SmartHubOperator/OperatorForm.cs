using System.Net.Http.Json;
using System.Windows.Forms;
using System.Drawing;

public class OperatorForm : Form
{
    ////////////////////////////////////////////////////////////
    /// OPERATOR CLIENT - ONLY THIS APP MOVES THE PERSON
    ////////////////////////////////////////////////////////////

    private const int TileSize = 32;
    private const int MapCols = 20;
    private const int MapRows = 14;
    private readonly System.Windows.Forms.Timer timer = new();
    private readonly HashSet<Keys> keys = new();
    private readonly HttpClient http = new();
    private StateDto? state;

    private readonly string baseUrl = "http://192.168.4.26:5000";
    private const string operatorKey = "TEAMSMARTHUB";
    private readonly Rectangle floorPlanBounds;
    private readonly Rectangle sidePanelBounds;
    private bool isBusy;
    private bool isConnected;
    private string statusText = "Starting operator client...";
    private string moveStatus = "No movement sent yet.";

    public OperatorForm()
    {
        Text = "SmartHub Operator";
        ClientSize = new Size(1050, 560);
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;
        KeyPreview = true;
        BackColor = Color.FromArgb(18, 18, 22);
        floorPlanBounds = new Rectangle(20, 20, MapCols * TileSize, MapRows * TileSize);
        sidePanelBounds = new Rectangle(floorPlanBounds.Right + 25, 20, 340, 500);
        Shown += async (_, _) => { Focus(); await LoadStateAsync(); timer.Start(); };
        Activated += (_, _) => Focus();
        MouseDown += (_, _) => Focus();
        timer.Interval = 120;
        timer.Tick += async (_, _) =>
        {
            if (isBusy) return;
            isBusy = true;
            try { await SendMovementAsync(); await LoadStateAsync(); Invalidate(); }
            finally { isBusy = false; }
        };
    }

    protected override void OnKeyDown(KeyEventArgs e) { keys.Add(e.KeyCode); base.OnKeyDown(e); }
    protected override void OnKeyUp(KeyEventArgs e) { keys.Remove(e.KeyCode); base.OnKeyUp(e); }

    private async Task SendMovementAsync()
    {
        float dx = 0f, dy = 0f;
        const float moveAmount = 0.14f;
        if (keys.Contains(Keys.W)) dy -= moveAmount;
        if (keys.Contains(Keys.S)) dy += moveAmount;
        if (keys.Contains(Keys.A)) dx -= moveAmount;
        if (keys.Contains(Keys.D)) dx += moveAmount;
        if (dx == 0f && dy == 0f) { moveStatus = "Standing still."; return; }
        var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/operator/move");
        req.Headers.Add("X-Operator-Key", operatorKey);
        req.Content = JsonContent.Create(new MoveRequest(dx, dy));
        try
        {
            using HttpResponseMessage response = await http.SendAsync(req);
            if (response.IsSuccessStatusCode) { moveStatus = $"Move sent: dx={dx:0.00}, dy={dy:0.00}"; statusText = "Operator connected."; isConnected = true; }
            else { moveStatus = $"Move rejected: {(int)response.StatusCode} {response.ReasonPhrase}"; isConnected = false; }
        }
        catch (Exception ex) { moveStatus = $"Move failed: {ex.Message}"; statusText = "Unable to reach SmartHub server."; isConnected = false; }
    }

    private async Task LoadStateAsync()
    {
        try
        {
            state = await http.GetFromJsonAsync<StateDto>($"{baseUrl}/api/state");
            if (state is not null) { statusText = "Operator connected."; isConnected = true; }
            else { statusText = "Server returned empty state."; isConnected = false; }
        }
        catch (Exception ex) { statusText = $"Connection failed: {ex.Message}"; isConnected = false; }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(Color.FromArgb(18, 18, 22));
        DrawFloorPlan(g);
        DrawPanel(g);
    }

    private void DrawFloorPlan(Graphics g)
    {
        using var bg = new SolidBrush(Color.FromArgb(28, 28, 34));
        using var border = new Pen(Color.FromArgb(120, 160, 160, 180), 2f);
        g.FillRectangle(bg, floorPlanBounds);
        g.DrawRectangle(border, floorPlanBounds);
        if (state is null) { g.DrawString("Connecting to SmartHub server...", Font, Brushes.White, floorPlanBounds.X + 20, floorPlanBounds.Y + 20); return; }
        foreach (var wall in state.walls) { using var wallBrush = new SolidBrush(Color.DimGray); g.FillRectangle(wallBrush, floorPlanBounds.X + wall.x * TileSize, floorPlanBounds.Y + wall.y * TileSize, TileSize, TileSize); }
        foreach (var doorTile in state.doorTiles)
        {
            bool locked = state.doors.Any(d => (Math.Floor(d.x) == doorTile.x && Math.Floor(d.y) == doorTile.y && d.isLocked) || (d.id == 0 && d.isLocked && ((doorTile.x == 9 && doorTile.y == 13) || (doorTile.x == 10 && doorTile.y == 13))));
            using var doorBrush = new SolidBrush(locked ? Color.Crimson : Color.SaddleBrown);
            g.FillRectangle(doorBrush, floorPlanBounds.X + doorTile.x * TileSize, floorPlanBounds.Y + doorTile.y * TileSize, TileSize, TileSize);
        }
        for (int y = 0; y < MapRows; y++) for (int x = 0; x < MapCols; x++) { using var gridPen = new Pen(Color.FromArgb(30, 255, 255, 255)); g.DrawRectangle(gridPen, floorPlanBounds.X + x * TileSize, floorPlanBounds.Y + y * TileSize, TileSize, TileSize); }
        foreach (var room in state.rooms) g.DrawString(room.name, Font, Brushes.WhiteSmoke, floorPlanBounds.X + room.x * TileSize, floorPlanBounds.Y + room.y * TileSize);
        foreach (var device in state.securityDevices)
        {
            float px = floorPlanBounds.X + device.x * TileSize, py = floorPlanBounds.Y + device.y * TileSize, rangePixels = device.range * TileSize;
            using var rangeBrush = new SolidBrush(device.triggered ? Color.FromArgb(55, 255, 80, 80) : Color.FromArgb(40, 80, 220, 120));
            g.FillEllipse(rangeBrush, px - rangePixels, py - rangePixels, rangePixels * 2, rangePixels * 2);
        }
        foreach (var light in state.lights) { float px = floorPlanBounds.X + light.x * TileSize, py = floorPlanBounds.Y + light.y * TileSize; using var b = new SolidBrush(light.isOn ? Color.Gold : Color.Gray); g.FillEllipse(b, px - 7, py - 7, 14, 14); g.DrawString("L", Font, Brushes.Black, px - 5, py - 8); }
        foreach (var door in state.doors) { float px = floorPlanBounds.X + door.x * TileSize, py = floorPlanBounds.Y + door.y * TileSize; using var b = new SolidBrush(door.isLocked ? Color.Crimson : Color.DeepSkyBlue); g.FillRectangle(b, px - 8, py - 8, 16, 16); g.DrawString("D", Font, Brushes.White, px - 6, py - 8); }
        foreach (var device in state.securityDevices) { float px = floorPlanBounds.X + device.x * TileSize, py = floorPlanBounds.Y + device.y * TileSize; using var b = new SolidBrush(device.triggered ? Color.Red : Color.LimeGreen); g.FillEllipse(b, px - 6, py - 6, 12, 12); g.DrawString(device.deviceType == "Camera" ? "C" : "S", Font, Brushes.Black, px - 6, py - 8); }
        float personPx = floorPlanBounds.X + state.person.x * TileSize, personPy = floorPlanBounds.Y + state.person.y * TileSize;
        g.FillEllipse(Brushes.DeepSkyBlue, personPx - 8, personPy - 8, 16, 16);
    }

    private void DrawPanel(Graphics g)
    {
        using var panelBrush = new SolidBrush(Color.FromArgb(30, 32, 38));
        using var panelBorder = new Pen(Color.FromArgb(120, 160, 160, 180), 2f);
        g.FillRectangle(panelBrush, sidePanelBounds);
        g.DrawRectangle(panelBorder, sidePanelBounds);
        int x = sidePanelBounds.X + 16, y = sidePanelBounds.Y + 16;
        using var titleFont = new Font("Consolas", 18, FontStyle.Bold);
        using var sectionFont = new Font("Consolas", 11, FontStyle.Bold);
        using var normalFont = new Font("Consolas", 10, FontStyle.Regular);
        g.DrawString("Operator Console", titleFont, Brushes.White, x, y); y += 40;
        g.DrawString("Movement", sectionFont, Brushes.White, x, y); y += 24;
        g.DrawString("W A S D = Move resident", normalFont, Brushes.LightSteelBlue, x, y); y += 20;
        g.DrawString("Click this window first", normalFont, Brushes.Khaki, x, y); y += 30;
        Brush statusBrush = isConnected ? Brushes.LightGreen : Brushes.OrangeRed;
        g.DrawString("Connection", sectionFont, Brushes.White, x, y); y += 24;
        g.DrawString(statusText, normalFont, statusBrush, x, y); y += 18;
        g.DrawString($"Server: {baseUrl}", normalFont, Brushes.LightSteelBlue, x, y); y += 18;
        g.DrawString(moveStatus, normalFont, Brushes.LightSteelBlue, x, y); y += 28;
        if (state is null) { g.DrawString("Waiting for server...", normalFont, Brushes.Khaki, x, y); return; }
        g.DrawString("Resident Position", sectionFont, Brushes.White, x, y); y += 24;
        g.DrawString($"X: {state.person.x:0.00}", normalFont, Brushes.DeepSkyBlue, x, y); y += 18;
        g.DrawString($"Y: {state.person.y:0.00}", normalFont, Brushes.DeepSkyBlue, x, y); y += 28;
        g.DrawString("Remote users control:", sectionFont, Brushes.White, x, y); y += 24;
        g.DrawString("Lights / Doors / Thermostat", normalFont, Brushes.LightSteelBlue, x, y); y += 30;
        g.DrawString("Latest Alerts", sectionFont, Brushes.White, x, y); y += 24;
        foreach (var alert in state.alerts.Take(10)) { g.DrawString(alert, normalFont, Brushes.Khaki, x, y); y += 18; }
    }
}

public record MoveRequest(float Dx, float Dy);
public class StateDto { public int mapCols { get; set; } public int mapRows { get; set; } public int tileSize { get; set; } public PersonDto person { get; set; } = new(); public List<LightDto> lights { get; set; } = []; public List<DoorDto> doors { get; set; } = []; public ThermostatDto thermostat { get; set; } = new(); public List<SecurityDto> securityDevices { get; set; } = []; public List<string> alerts { get; set; } = []; public List<RoomDto> rooms { get; set; } = []; public List<TileDto> walls { get; set; } = []; public List<TileDto> doorTiles { get; set; } = []; }
public class PersonDto { public string name { get; set; } = ""; public float x { get; set; } public float y { get; set; } }
public class LightDto { public int id { get; set; } public string name { get; set; } = ""; public float x { get; set; } public float y { get; set; } public bool isOn { get; set; } public string status { get; set; } = ""; }
public class DoorDto { public int id { get; set; } public string name { get; set; } = ""; public float x { get; set; } public float y { get; set; } public bool isLocked { get; set; } public string status { get; set; } = ""; }
public class ThermostatDto { public string name { get; set; } = ""; public float x { get; set; } public float y { get; set; } public int temperature { get; set; } public string status { get; set; } = ""; }
public class SecurityDto { public int id { get; set; } public string name { get; set; } = ""; public string deviceType { get; set; } = ""; public float x { get; set; } public float y { get; set; } public float range { get; set; } public string dStatus { get; set; } = ""; public bool triggered { get; set; } }
public class RoomDto { public string name { get; set; } = ""; public int x { get; set; } public int y { get; set; } }
public class TileDto { public int x { get; set; } public int y { get; set; } }
