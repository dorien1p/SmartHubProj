using System;
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SmartHomeForm());
    }
}

public class SmartHomeForm : Form
{
    private SmartLight livingRoomLight = new SmartLight("Living Room Light");
    private SmartCamera hallwayCamera = new SmartCamera("Hallway Camera");
    private MotionSensor motionSensor = new MotionSensor("Motion Sensor");
    private SmartDoor frontDoor = new SmartDoor("Front Door");
    private Thermostat thermostat = new Thermostat("Thermostat");

    private Label lblLightStatus = new Label();
    private Label lblCameraStatus = new Label();
    private Label lblSensorStatus = new Label();
    private Label lblDoorStatus = new Label();
    private Label lblThermostatStatus = new Label();

    private Panel roomPanel = new Panel();

    public SmartHomeForm()
    {
        Text = "Smart Home Simulator";
        Size = new Size(950, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.WhiteSmoke;

        var homeowner = new HomeownerApp(this);
        motionSensor.RegisterObserver(homeowner);

        BuildUI();
        RefreshStatuses();
    }

    private void BuildUI()
    {
        Label title = new Label
        {
            Text = "Smart Home Control Panel",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(300, 20)
        };
        Controls.Add(title);

        roomPanel = new Panel
        {
            Location = new Point(30, 80),
            Size = new Size(560, 500),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Beige
        };
        Controls.Add(roomPanel);

        Label roomTitle = new Label
        {
            Text = "Room Display",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 10)
        };
        roomPanel.Controls.Add(roomTitle);

        // Light visual
        Label lightBox = CreateDeviceBox("LIGHT", 60, 60);
        lblLightStatus = CreateStatusLabel(85, 130);
        roomPanel.Controls.Add(lightBox);
        roomPanel.Controls.Add(lblLightStatus);

        // Camera visual
        Label cameraBox = CreateDeviceBox("CAMERA", 240, 60);
        lblCameraStatus = CreateStatusLabel(255, 130);
        roomPanel.Controls.Add(cameraBox);
        roomPanel.Controls.Add(lblCameraStatus);

        // Sensor visual
        Label sensorBox = CreateDeviceBox("SENSOR", 400, 60);
        lblSensorStatus = CreateStatusLabel(415, 130);
        roomPanel.Controls.Add(sensorBox);
        roomPanel.Controls.Add(lblSensorStatus);

        // Door visual
        Label doorBox = CreateDeviceBox("DOOR", 120, 260);
        lblDoorStatus = CreateStatusLabel(140, 330);
        roomPanel.Controls.Add(doorBox);
        roomPanel.Controls.Add(lblDoorStatus);

        // Thermostat visual
        Label thermostatBox = CreateDeviceBox("THERMOSTAT", 330, 260);
        lblThermostatStatus = CreateStatusLabel(345, 330);
        roomPanel.Controls.Add(thermostatBox);
        roomPanel.Controls.Add(lblThermostatStatus);

        int buttonX = 650;
        int buttonY = 100;

        Button btnToggleLight = CreateButton("Toggle Light", buttonX, buttonY, (s, e) =>
        {
            new ToggleDeviceCommand(livingRoomLight).Execute();
            RefreshStatuses();
        });

        Button btnToggleCamera = CreateButton("Toggle Camera", buttonX, buttonY + 60, (s, e) =>
        {
            new ToggleDeviceCommand(hallwayCamera).Execute();
            RefreshStatuses();
        });

        Button btnToggleSensor = CreateButton("Arm / Disarm Sensor", buttonX, buttonY + 120, (s, e) =>
        {
            new ToggleDeviceCommand(motionSensor).Execute();
            RefreshStatuses();
        });

        Button btnTriggerSensor = CreateButton("Trigger Sensor", buttonX, buttonY + 180, (s, e) =>
        {
            motionSensor.DetectMotion();
            RefreshStatuses();
        });

        Button btnToggleDoor = CreateButton("Open / Close Door", buttonX, buttonY + 240, (s, e) =>
        {
            new ToggleDeviceCommand(frontDoor).Execute();
            RefreshStatuses();
        });

        Button btnTempUp = CreateButton("Temp +", buttonX, buttonY + 300, (s, e) =>
        {
            thermostat.IncreaseTemperature();
            RefreshStatuses();
        });

        Button btnTempDown = CreateButton("Temp -", buttonX, buttonY + 360, (s, e) =>
        {
            thermostat.DecreaseTemperature();
            RefreshStatuses();
        });

        Controls.Add(btnToggleLight);
        Controls.Add(btnToggleCamera);
        Controls.Add(btnToggleSensor);
        Controls.Add(btnTriggerSensor);
        Controls.Add(btnToggleDoor);
        Controls.Add(btnTempUp);
        Controls.Add(btnTempDown);
    }

    private Label CreateDeviceBox(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            TextAlign = ContentAlignment.MiddleCenter,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.LightGray,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 60),
            Location = new Point(x, y)
        };
    }

    private Label CreateStatusLabel(int x, int y)
    {
        return new Label
        {
            Text = "Status",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(x, y)
        };
    }

    private Button CreateButton(string text, int x, int y, EventHandler onClick)
    {
        Button btn = new Button
        {
            Text = text,
            Size = new Size(200, 40),
            Location = new Point(x, y),
            Font = new Font("Segoe UI", 10, FontStyle.Regular)
        };

        btn.Click += onClick;
        return btn;
    }

    private void RefreshStatuses()
    {
        lblLightStatus.Text = $"Status: {livingRoomLight.GetStatus()}";
        lblCameraStatus.Text = $"Status: {hallwayCamera.GetStatus()}";
        lblSensorStatus.Text = $"Status: {motionSensor.GetStatus()}";
        lblDoorStatus.Text = $"Status: {frontDoor.GetStatus()}";
        lblThermostatStatus.Text = $"Status: {thermostat.GetStatus()}";
    }
}