const base = "";

let soundEnabled = false;
let wasTriggered = false;

////////////////////////////////////////////////////////////
// SOUND SYSTEM
////////////////////////////////////////////////////////////

function enableSound() {
  soundEnabled = true;

  const alarm = document.getElementById("alarmSound");
  const button = document.getElementById("enableSoundBtn");

  if (!alarm) {
    alert("Alarm audio element was not found.");
    return;
  }

  alarm.volume = 0.85;

  alarm.play()
    .then(() => {
      alarm.pause();
      alarm.currentTime = 0;
      button.textContent = "Sound Enabled";
      button.disabled = true;
    })
    .catch(() => {
      alert("Sound was blocked. Click Enable Sound again or check browser audio permissions.");
    });
}

function playAlarmSound() {
  if (!soundEnabled) return;

  const alarm = document.getElementById("alarmSound");
  if (!alarm) return;

  alarm.currentTime = 0;
  alarm.play().catch(() => {});
}

////////////////////////////////////////////////////////////
// API CALLS
////////////////////////////////////////////////////////////

async function getState() {
  const res = await fetch(`${base}/api/state`);
  return await res.json();
}

async function toggleLight(id) {
  await fetch(`${base}/api/lights/${id}/toggle`, { method: "POST" });
  load();
}

async function toggleDoor(id) {
  await fetch(`${base}/api/doors/${id}/toggle`, { method: "POST" });
  load();
}

async function toggleFan(id) {
  await fetch(`${base}/api/fans/${id}/toggle`, { method: "POST" });
  load();
}

async function toggleAlarm(id) {
  await fetch(`${base}/api/alarms/${id}/toggle`, { method: "POST" });
  load();
}

async function toggleBlind(id) {
  await fetch(`${base}/api/blinds/${id}/toggle`, { method: "POST" });
  load();
}

async function thermoUp() {
  await fetch(`${base}/api/thermostat/up`, { method: "POST" });
  load();
}

async function thermoDown() {
  await fetch(`${base}/api/thermostat/down`, { method: "POST" });
  load();
}

async function undo() {
  await fetch(`${base}/api/undo`, { method: "POST" });
  load();
}

async function redo() {
  await fetch(`${base}/api/redo`, { method: "POST" });
  load();
}

////////////////////////////////////////////////////////////
// UI HELPERS
////////////////////////////////////////////////////////////

function badge(text, cls) {
  return `<span class="badge ${cls}">${text}</span>`;
}

function statusClass(value) {
  const text = String(value || "").toLowerCase();

  if (text.includes("on")) return "on";
  if (text.includes("off")) return "off";
  if (text.includes("locked")) return "locked";
  if (text.includes("unlocked")) return "unlocked";
  if (text.includes("triggered")) return "triggered";
  if (text.includes("monitoring")) return "monitoring";
  if (text.includes("active")) return "active";
  if (text.includes("inactive")) return "inactive";
  if (text.includes("open")) return "open";
  if (text.includes("closed")) return "closed";

  return "off";
}

////////////////////////////////////////////////////////////
// MAIN LOAD LOOP
////////////////////////////////////////////////////////////

async function load() {
  let state;

  try {
    state = await getState();
  } catch {
    document.getElementById("liveAlert").className = "live-alert danger";
    document.getElementById("liveAlert").textContent = "Unable to connect to SmartHub server";
    return;
  }

  const triggered = state.securityDevices.filter(s => s.triggered);

  if (triggered.length > 0 && !wasTriggered) {
    playAlarmSound();
  }

  wasTriggered = triggered.length > 0;

  const liveAlert = document.getElementById("liveAlert");

  if (triggered.length > 0) {
    liveAlert.className = "live-alert danger";
    liveAlert.textContent = `MOTION DETECTED - ${triggered.map(t => t.name).join(", ")}`;
  } else {
    liveAlert.className = "live-alert safe";
    liveAlert.textContent = "System normal";
  }

  document.getElementById("residentPosition").innerHTML = `
    <div class="item">
      <div>${state.person.name}</div>
      <div>X: ${state.person.x.toFixed(2)} | Y: ${state.person.y.toFixed(2)}</div>
    </div>
  `;

  document.getElementById("lights").innerHTML = state.lights.map(l => `
    <div class="item">
      <div>${l.name} ${badge(l.status, l.isOn ? "on" : "off")}</div>
      <button onclick="toggleLight(${l.id})">Toggle</button>
    </div>
  `).join("");

  document.getElementById("doors").innerHTML = state.doors.map(d => `
    <div class="item">
      <div>${d.name} ${badge(d.status, d.isLocked ? "locked" : "unlocked")}</div>
      <button onclick="toggleDoor(${d.id})">Toggle</button>
    </div>
  `).join("");

  const fans = state.fans || [];
  document.getElementById("fans").innerHTML = fans.map(f => `
    <div class="item">
      <div>${f.name} ${badge(f.status, statusClass(f.status))}</div>
      <button onclick="toggleFan(${f.id})">Toggle</button>
    </div>
  `).join("") || `<div class="item"><div>No fans found</div></div>`;

  const alarms = state.alarms || [];
  document.getElementById("alarms").innerHTML = alarms.map(a => `
    <div class="item">
      <div>${a.name} ${badge(a.status, statusClass(a.status))}</div>
      <button onclick="toggleAlarm(${a.id})">Toggle</button>
    </div>
  `).join("") || `<div class="item"><div>No alarms found</div></div>`;

  const blinds = state.blinds || [];
  document.getElementById("blinds").innerHTML = blinds.map(b => `
    <div class="item">
      <div>${b.name} ${badge(b.status, statusClass(b.status))}</div>
      <button onclick="toggleBlind(${b.id})">Toggle</button>
    </div>
  `).join("") || `<div class="item"><div>No blinds found</div></div>`;

  document.getElementById("thermostat").innerHTML = `
    <div class="item">
      <div>${state.thermostat.name}</div>
      <div>${state.thermostat.temperature}°F</div>
    </div>
  `;

  document.getElementById("security").innerHTML = state.securityDevices.map(s => `
    <div class="item">
      <div>${s.name}</div>
      <div>${badge(s.dStatus || s.status, s.triggered ? "triggered" : "monitoring")}</div>
    </div>
  `).join("");

  document.getElementById("alerts").innerHTML = state.alerts.map(a => `
    <div class="item"><div>${a}</div></div>
  `).join("");
}

load();
setInterval(load, 1000);