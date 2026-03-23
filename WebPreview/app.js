(function () {
  const screens = document.querySelectorAll(".screen");
  let selectedMissionId = null;

  const SCREEN_LABELS = {
    "screen-splash":    "Splash",
    "screen-main-menu": "Menu",
    "screen-missions":  "Missions",
    "screen-hud":       "HUD",
    "screen-pause":     "Pause",
    "screen-map":       "Map",
    "screen-dialogue":  "Dialogue",
    "screen-character": "Character",
    "screen-wanted":    "Wanted",
    "screen-settings":  "Settings",
  };

  function showScreen(id) {
    screens.forEach((s) => {
      s.classList.toggle("screen--active", s.id === id);
    });
    updateNav(id);
  }

  function buildNav() {
    const nav = document.getElementById("screen-nav");
    if (!nav) return;
    Object.entries(SCREEN_LABELS).forEach(([id, label]) => {
      const a = document.createElement("a");
      a.textContent = label;
      a.dataset.target = id;
      a.setAttribute("role", "button");
      a.setAttribute("tabindex", "0");
      nav.appendChild(a);
    });
  }

  function updateNav(activeId) {
    const nav = document.getElementById("screen-nav");
    if (!nav) return;
    nav.querySelectorAll("a").forEach((a) => {
      a.classList.toggle("active", a.dataset.target === activeId);
    });
  }

  function initNavigation() {
    document.addEventListener("click", (event) => {
      const target = event.target.closest("[data-target]");
      if (!target) return;
      const screenId = target.getAttribute("data-target");

      if (screenId === "screen-hud" && !selectedMissionId) {
        // Require a mission to be selected before entering HUD
        return;
      }

      showScreen(screenId);
      if (screenId === "screen-hud") {
        updateHudForSelectedMission();
      }
    });

    // Pill toggle (character creator style / ride options)
    document.addEventListener("click", (event) => {
      const pill = event.target.closest(".pill");
      if (!pill) return;
      const group = pill.closest(".char-pills");
      if (!group) return;
      group.querySelectorAll(".pill").forEach((p) => p.classList.remove("pill--active"));
      pill.classList.add("pill--active");
    });
  }

  function renderMissions() {
    const list = document.getElementById("mission-list");
    if (!list || !window.NH_MISSIONS) return;

    const missions = window.NH_MISSIONS.main_story_missions || [];
    list.innerHTML = "";

    missions.forEach((mission, index) => {
      const li = document.createElement("li");
      li.className = "list-item";
      li.dataset.missionId = mission.missionID;
      if (index === 0) {
        li.classList.add("list-item--selected");
        selectedMissionId = mission.missionID;
      }

      li.innerHTML = `<div class="list-item__title">${mission.title}</div>
        <div class="list-item__subtitle">${mission.description}</div>`;

      li.addEventListener("click", () => {
        selectedMissionId = mission.missionID;
        document.querySelectorAll(".list-item").forEach((el) => {
          el.classList.toggle("list-item--selected", el === li);
        });
      });

      list.appendChild(li);
    });
  }

  function updateHudForSelectedMission() {
    const missions = window.NH_MISSIONS.main_story_missions || [];
    const mission = missions.find((m) => m.missionID === selectedMissionId);
    const objectivesList = document.getElementById("hud-objectives-list");
    if (!objectivesList) return;
    objectivesList.innerHTML = "";
    if (!mission) return;
    mission.objectives.forEach((obj) => {
      const li = document.createElement("li");
      li.className = "hud-objective";
      li.textContent = obj;
      objectivesList.appendChild(li);
    });
  }

  function init() {
    buildNav();
    initNavigation();
    renderMissions();
  }

  document.addEventListener("DOMContentLoaded", init);
})();

