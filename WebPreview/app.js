(function () {
  const screens = document.querySelectorAll(".screen");
  let selectedMissionId = null;

  function showScreen(id) {
    screens.forEach((s) => {
      if (s.id === id) {
        s.classList.add("screen--active");
      } else {
        s.classList.remove("screen--active");
      }
    });
  }

  function initNavigation() {
    document.body.addEventListener("click", (event) => {
      const target = event.target.closest("[data-target]");
      if (!target) return;
      const screenId = target.getAttribute("data-target");

      if (screenId === "screen-hud" && !selectedMissionId) {
        // If no mission is selected, ignore starting HUD
        return;
      }

      showScreen(screenId);
      if (screenId === "screen-hud") {
        updateHudForSelectedMission();
      }
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

      li.innerHTML = `
        <div class="list-item__title">${mission.title}</div>
        <div class="list-item__subtitle">${mission.description}</div>
      `;

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
    initNavigation();
    renderMissions();
  }

  document.addEventListener("DOMContentLoaded", init);
})();
