using Serilog;
using SmartHubCore;

var builder = WebApplication.CreateBuilder(args);

////////////////////////////////////////////////////////////
/// SERILOG LOGGING SETUP
////////////////////////////////////////////////////////////

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/smarthub-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

////////////////////////////////////////////////////////////
/// SERVICE SETUP
////////////////////////////////////////////////////////////

builder.Services.AddSingleton<SmartHubState>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});

var app = builder.Build();

////////////////////////////////////////////////////////////
/// MIDDLEWARE
////////////////////////////////////////////////////////////

app.UseSerilogRequestLogging();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

////////////////////////////////////////////////////////////
/// INITIALIZE SHARED SMARTHUB STATE
////////////////////////////////////////////////////////////

var state = app.Services.GetRequiredService<SmartHubState>();
state.Initialize();

state.LogEmitted += message =>
{
    Log.Information("{Message}", message);
};

////////////////////////////////////////////////////////////
/// MULTITHREADING - BACKGROUND SIMULATION LOOP
////////////////////////////////////////////////////////////

_ = Task.Run(async () =>
{
    while (true)
    {
        state.RunBackgroundTick();
        await Task.Delay(250);
    }
});

////////////////////////////////////////////////////////////
/// PUBLIC STATE + NETWORK INFO
////////////////////////////////////////////////////////////

app.MapGet("/api/state", (SmartHubState s) =>
{
    return Results.Ok(s.ToDto());
});

app.MapGet("/api/network", () =>
{
    var host = System.Net.Dns.GetHostName();

    var addresses = System.Net.Dns.GetHostAddresses(host)
        .Where(a =>
            a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
            !System.Net.IPAddress.IsLoopback(a))
        .Select(a => $"http://{a}:5000")
        .ToArray();

    return Results.Ok(new { urls = addresses });
});

////////////////////////////////////////////////////////////
/// REMOTE USER CONTROLS - PHONE / LAPTOP
////////////////////////////////////////////////////////////

app.MapPost("/api/lights/{id:int}/toggle", (int id, SmartHubState s) =>
{
    if (id < 0 || id >= s.Lights.Count)
        return Results.NotFound();

    s.ExecuteCommand(new ToggleLightCommand(s.Lights[id]));

    return Results.Ok(s.ToDto());
});

app.MapPost("/api/doors/{id:int}/toggle", (int id, SmartHubState s) =>
{
    if (id < 0 || id >= s.Doors.Count)
        return Results.NotFound();

    s.ExecuteCommand(new ToggleDoorLockCommand(s.Doors[id]));

    return Results.Ok(s.ToDto());
});

app.MapPost("/api/fans/{id:int}/toggle", (int id, SmartHubState s) =>
{
    if (id < 0 || id >= s.Fans.Count)
        return Results.NotFound();

    s.ExecuteCommand(new ToggleFanCommand(s.Fans[id]));

    return Results.Ok(s.ToDto());
});

app.MapPost("/api/alarms/{id:int}/toggle", (int id, SmartHubState s) =>
{
    if (id < 0 || id >= s.Alarms.Count)
        return Results.NotFound();

    s.ExecuteCommand(new ToggleAlarmCommand(s.Alarms[id]));

    return Results.Ok(s.ToDto());
});

app.MapPost("/api/blinds/{id:int}/toggle", (int id, SmartHubState s) =>
{
    if (id < 0 || id >= s.Blinds.Count)
        return Results.NotFound();

    s.ExecuteCommand(new ToggleBlindCommand(s.Blinds[id]));

    return Results.Ok(s.ToDto());
});

////////////////////////////////////////////////////////////
/// THERMOSTAT CONTROLS
////////////////////////////////////////////////////////////

app.MapPost("/api/thermostat/up", (SmartHubState s) =>
{
    s.ExecuteCommand(new AdjustThermostatCommand(s.Thermostat, 1));

    return Results.Ok(s.ToDto());
});

app.MapPost("/api/thermostat/down", (SmartHubState s) =>
{
    s.ExecuteCommand(new AdjustThermostatCommand(s.Thermostat, -1));

    return Results.Ok(s.ToDto());
});

////////////////////////////////////////////////////////////
/// COMMAND PATTERN - UNDO / REDO
////////////////////////////////////////////////////////////

app.MapPost("/api/undo", (SmartHubState s) =>
{
    bool success = s.Undo();

    return Results.Ok(new
    {
        success,
        state = s.ToDto()
    });
});

app.MapPost("/api/redo", (SmartHubState s) =>
{
    bool success = s.Redo();

    return Results.Ok(new
    {
        success,
        state = s.ToDto()
    });
});

////////////////////////////////////////////////////////////
/// OPERATOR-ONLY MOVEMENT
////////////////////////////////////////////////////////////

app.MapPost("/api/operator/move", async (HttpContext ctx, SmartHubState s) =>
{
    if (!ctx.Request.Headers.TryGetValue("X-Operator-Key", out var key) ||
        key != SmartHubState.OperatorKey)
    {
        return Results.Unauthorized();
    }

    var move = await ctx.Request.ReadFromJsonAsync<MoveRequest>();

    if (move is null)
        return Results.BadRequest();

    s.MovePerson(move.Dx, move.Dy);

    return Results.Ok(s.ToDto());
});

////////////////////////////////////////////////////////////
/// SERVER BINDING
////////////////////////////////////////////////////////////

app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");

////////////////////////////////////////////////////////////
/// RUN SERVER
////////////////////////////////////////////////////////////

app.Run();

////////////////////////////////////////////////////////////
/// REQUEST DTOs
////////////////////////////////////////////////////////////

record MoveRequest(float Dx, float Dy);