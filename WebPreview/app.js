(function () {
  const screens = document.querySelectorAll(".screen");
  const music = document.getElementById("bg-music");
  const sfxClick = document.getElementById("sfx-click");
  const sfxWhoosh = document.getElementById("sfx-whoosh");
  const muteBtn = document.getElementById("mute-btn");
  let selectedMissionId = null;
  let isMusicPlaying = false;

  const ICONS = {
    volume: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M13.0001 3.00024L7.00006 8.00024H3.00006V16.0002H7.00006L13.0001 21.0002V3.00024ZM15.0001 12.0002C15.0001 10.1669 14.3039 8.49221 13.1717 7.24721L14.5859 5.833C16.0755 7.56403 17.0001 9.70053 17.0001 12.0002C17.0001 14.3 16.0755 16.4357 14.5859 18.1667L13.1717 16.7517C14.3039 15.5067 15.0001 13.832 15.0001 12.0002Z"></path></svg>`,
    muted: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M13.0001 3.00024L7.00006 8.00024H3.00006V16.0002H7.00006L13.0001 21.0002V3.00024ZM18.2427 12.0002L20.364 9.87891L18.9498 8.46469L16.8285 10.586L14.7072 8.46469L13.293 9.87891L15.4143 12.0002L13.293 14.1215L14.7072 15.5358L16.8285 13.4144L18.9498 15.5358L20.364 14.1215L18.2427 12.0002Z"></path></svg>`,
  };

  function playSfx(sfx) {
    if (sfx && !music.muted) {
      sfx.currentTime = 0;
      sfx.play();
    }
  }

  function showScreen(id) {
    let currentScreen = document.querySelector(".screen--active");
    if (currentScreen) {
      currentScreen.classList.remove("screen--active");
    }

    const newScreen = document.getElementById(id);
    if (newScreen) {
      newScreen.classList.add("screen--active");
      playSfx(sfxWhoosh);
    }
  }

  function startMusic() {
    if (music && !isMusicPlaying) {
      music.play().then(() => {
        isMusicPlaying = true;
      }).catch(error => {
        console.error("Audio playback failed:", error);
      });
    }
  }

  function toggleMute() {
    if (!music) return;
    music.muted = !music.muted;
    muteBtn.innerHTML = music.muted ? ICONS.muted : ICONS.volume;
    muteBtn.setAttribute("aria-label", music.muted ? "Unmute Audio" : "Mute Audio");
  }

  function initNavigation() {
    document.body.addEventListener("click", (event) => {
      startMusic();

      const target = event.target.closest("[data-target]");
      if (target) {
        playSfx(sfxClick);
        const screenId = target.getAttribute("data-target");
        if (screenId === "screen-hud" && !selectedMissionId) {
          return;
        }
        showScreen(screenId);
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

      li.innerHTML = `<div class="list-item__title">${mission.title}</div>
        <div class="list-item__subtitle">${mission.description}</div>`;

      li.addEventListener("click", () => {
        playSfx(sfxClick);
        selectedMissionId = mission.missionID;
        document.querySelectorAll(".list-item").forEach((el) => {
          el.classList.toggle("list-item--selected", el === li);
        });
      });

      list.appendChild(li);
    });
  }

  function init() {
    muteBtn.innerHTML = music.muted ? ICONS.muted : ICONS.volume;
    muteBtn.addEventListener("click", (e) => {
        e.stopPropagation(); // prevent navigation
        playSfx(sfxClick);
        toggleMute();
    });
    
    initNavigation();
    renderMissions();
    showScreen("screen-splash");
  }

  document.addEventListener("DOMContentLoaded", init);
})();