/**
 * Newport Hustle — UI Engine
 * -------------------------------------------------------
 * Responsibilities:
 *   1. Screen navigation (transition system)
 *   2. Background music (autoplay + localStorage prefs)
 *   3. Ripple tap feedback
 *   4. Character Creator (live preview)
 *   5. Story Mode selection
 *   6. Mission list rendering
 *   7. Settings (sliders, toggles)
 *   8. HUD simulation
 */
(function () {
  'use strict';

  /* -------------------------------------------------------
     STATE
  ------------------------------------------------------- */
  const state = {
    currentScreen:   'screen-splash',
    selectedMission: null,
    selectedStory:   'hustler',
    musicStarted:    false,
    musicMuted:      JSON.parse(localStorage.getItem('nh_muted') ?? 'false'),
    musicVolume:     parseFloat(localStorage.getItem('nh_vol')  ?? '0.5'),
    character: {
      name:    localStorage.getItem('nh_name')  ?? '',
      skin:    localStorage.getItem('nh_skin')  ?? '#c68642',
      outfit:  localStorage.getItem('nh_outfit')?? 'hustler',
      outfitColor: localStorage.getItem('nh_outfit_color') ?? '#e8ff42',
      trait:   localStorage.getItem('nh_trait') ?? 'Big Talker',
    }
  };

  /* -------------------------------------------------------
     DOM REFS
  ------------------------------------------------------- */
  const music   = document.getElementById('bg-music');
  const muteBtn = document.getElementById('mute-btn');

  /* -------------------------------------------------------
     UTILITY
  ------------------------------------------------------- */
  function qs(selector, scope = document) { return scope.querySelector(selector); }
  function qsa(selector, scope = document) { return scope.querySelectorAll(selector); }

  /* -------------------------------------------------------
     SCREEN TRANSITIONS
  ------------------------------------------------------- */
  function showScreen(id) {
    const incoming = document.getElementById(id);
    if (!incoming || id === state.currentScreen) return;

    const outgoing = document.getElementById(state.currentScreen);

    if (outgoing) {
      outgoing.classList.add('screen--exit');
      outgoing.classList.remove('screen--active');
      // Clean up exit class after animation finishes
      outgoing.addEventListener('transitionend', () => {
        outgoing.classList.remove('screen--exit');
      }, { once: true });
    }

    incoming.classList.add('screen--active');
    state.currentScreen = id;

    // Run any screen-specific initialization
    onScreenEnter(id);
  }

  /* Called once every time a screen becomes active */
  function onScreenEnter(id) {
    switch (id) {
      case 'screen-missions':        renderMissions(); break;
      case 'screen-character-creator': syncCreatorPreview(); break;
      case 'screen-hud':             initHudSim(); break;
      default: break;
    }
  }

  /* -------------------------------------------------------
     NAVIGATION (event delegation)
  ------------------------------------------------------- */
  function initNavigation() {
    document.body.addEventListener('click', (e) => {
      // Start music on first human interaction
      startMusic();

      // Ripple
      const btn = e.target.closest('.btn, .hud-btn, .top-bar__back');
      if (btn) spawnRipple(btn, e);

      // Screen navigation
      const navTarget = e.target.closest('[data-target]');
      if (navTarget) {
        const targetId = navTarget.getAttribute('data-target');
        showScreen(targetId);
      }
    });

    // Keyboard navigation: Enter/Space trigger click on focused data-target
    document.body.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' || e.key === ' ') {
        const focused = document.activeElement;
        if (focused && focused.hasAttribute('data-target')) {
          e.preventDefault();
          focused.click();
        }
      }
    });
  }

  /* -------------------------------------------------------
     RIPPLE FEEDBACK
  ------------------------------------------------------- */
  function spawnRipple(el, e) {
    const ripple = document.createElement('span');
    ripple.className = 'ripple';
    const rect = el.getBoundingClientRect();
    ripple.style.left = `${e.clientX - rect.left}px`;
    ripple.style.top  = `${e.clientY - rect.top}px`;
    el.appendChild(ripple);
    ripple.addEventListener('animationend', () => ripple.remove(), { once: true });
  }

  /* -------------------------------------------------------
     MUSIC SYSTEM
  ------------------------------------------------------- */
  function startMusic() {
    if (state.musicStarted || !music) return;
    music.volume = state.musicVolume;
    music.muted  = state.musicMuted;
    music.play().then(() => {
      state.musicStarted = true;
    }).catch(() => {
      // Autoplay blocked; will retry on next interaction (already handled)
    });
  }

  function toggleMute() {
    if (!music) return;
    state.musicMuted = !state.musicMuted;
    music.muted = state.musicMuted;
    localStorage.setItem('nh_muted', JSON.stringify(state.musicMuted));
    updateMuteBtn();
  }

  function updateMuteBtn() {
    muteBtn.textContent = state.musicMuted ? '🔇' : '🔊';
    muteBtn.setAttribute('aria-label', state.musicMuted ? 'Unmute Music' : 'Mute Music');
  }

  /* -------------------------------------------------------
     SPLASH BOOT TEXT
  ------------------------------------------------------- */
  const BOOT_LINES = [
    'HustleOS™ v4.20 — Initializing...',
    'Loading street credentials...',
    'Validating sidewalk license... ✓',
    'Checking respect levels: ZERO',
    'Installing swagger patches...',
    'WARNING: Side-hustle detected.',
    'System ready. Let\'s get it.'
  ];

  function animateBootText() {
    const el = document.getElementById('boot-text');
    if (!el) return;
    let i = 0;
    const interval = setInterval(() => {
      el.textContent = BOOT_LINES[i % BOOT_LINES.length];
      i++;
      if (i >= BOOT_LINES.length) clearInterval(interval);
    }, 600);
  }

  /* -------------------------------------------------------
     MISSIONS
  ------------------------------------------------------- */
  function renderMissions() {
    const list = document.getElementById('mission-list');
    if (!list || !window.NH_MISSIONS) return;

    const missions = window.NH_MISSIONS.main_story_missions || [];

    // Set context text based on story
    const ctxEl = document.getElementById('mission-context-text');
    if (ctxEl) {
      const storyMap = {
        hustler:    'Your story is just getting started, fam.',
        corporate:  'Calendar blocked. Hustle anyway.',
        influencer: 'Content doesn\'t make itself. Or does it?',
        chaos:      'Why have a plan when chaos is free?'
      };
      ctxEl.textContent = storyMap[state.selectedStory] ?? 'Choose your next move, boss.';
    }

    // Only re-render if needed
    if (list.children.length === missions.length) return;

    list.innerHTML = '';
    missions.forEach((mission, index) => {
      const li = document.createElement('li');
      li.className = 'list-item';
      li.setAttribute('role', 'option');
      li.setAttribute('aria-selected', index === 0 ? 'true' : 'false');
      li.dataset.missionId = mission.missionID;

      if (index === 0 && !state.selectedMission) {
        li.classList.add('list-item--selected');
        state.selectedMission = mission.missionID;
      }

      li.innerHTML = `<div class="list-item__title">${mission.title}</div>
                      <div class="list-item__subtitle">${mission.description}</div>`;

      li.addEventListener('click', () => {
        state.selectedMission = mission.missionID;
        qsa('.list-item').forEach(el => {
          const isThis = el === li;
          el.classList.toggle('list-item--selected', isThis);
          el.setAttribute('aria-selected', String(isThis));
        });
      });

      list.appendChild(li);
    });
  }

  /* -------------------------------------------------------
     STORY SELECTION
  ------------------------------------------------------- */
  function initStorySelect() {
    const grid = document.getElementById('story-grid');
    if (!grid) return;

    grid.addEventListener('click', (e) => {
      const card = e.target.closest('.story-card');
      if (!card) return;

      state.selectedStory = card.dataset.story;
      qsa('.story-card', grid).forEach(c => {
        c.classList.toggle('story-card--active', c === card);
      });
    });

    // Keyboard: Enter/Space on story card
    grid.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' || e.key === ' ') {
        const card = e.target.closest('.story-card');
        if (card) { e.preventDefault(); card.click(); }
      }
    });
  }

  /* -------------------------------------------------------
     CHARACTER CREATOR
  ------------------------------------------------------- */
  function initCharacterCreator() {
    const nameInput  = document.getElementById('char-name');
    const skinGroup  = document.getElementById('skin-swatches');
    const outfitGroup= document.getElementById('outfit-swatches');
    const traitGroup = document.getElementById('trait-swatches');
    const saveBtn    = document.getElementById('btn-save-character');

    if (!nameInput) return;

    // Restore saved values
    nameInput.value = state.character.name;

    // Activate saved selections
    activateSwatch(skinGroup,   '[data-skin="'   + state.character.skin   + '"]');
    activateSwatch(outfitGroup, '[data-outfit="' + state.character.outfit + '"]');
    activateTrait(traitGroup, state.character.trait);

    // Name input
    nameInput.addEventListener('input', () => {
      state.character.name = nameInput.value.trim();
      syncCreatorPreview();
    });

    // Skin
    skinGroup.addEventListener('click', (e) => {
      const sw = e.target.closest('.swatch--skin');
      if (!sw) return;
      state.character.skin = sw.dataset.skin;
      qsa('.swatch--skin', skinGroup).forEach(s => s.classList.remove('swatch--active'));
      sw.classList.add('swatch--active');
      syncCreatorPreview();
    });

    // Outfit
    outfitGroup.addEventListener('click', (e) => {
      const card = e.target.closest('.outfit-card');
      if (!card) return;
      state.character.outfit      = card.dataset.outfit;
      state.character.outfitColor = card.dataset.color;
      qsa('.outfit-card', outfitGroup).forEach(c => c.classList.remove('outfit-card--active'));
      card.classList.add('outfit-card--active');
      syncCreatorPreview();
    });

    // Trait
    traitGroup.addEventListener('click', (e) => {
      const pill = e.target.closest('.trait-pill');
      if (!pill) return;
      state.character.trait = pill.dataset.trait;
      qsa('.trait-pill', traitGroup).forEach(p => p.classList.remove('trait-pill--active'));
      pill.classList.add('trait-pill--active');
      syncCreatorPreview();
    });

    // Save
    if (saveBtn) {
      saveBtn.addEventListener('click', () => {
        saveCharacter();
        showScreen('screen-main-menu');
      });
    }

    syncCreatorPreview();
  }

  function activateSwatch(group, selector) {
    if (!group) return;
    const el = qs(selector, group);
    if (el) {
      qsa('.swatch--skin, .outfit-card, .trait-pill', group).forEach(s => s.classList.remove('swatch--active', 'outfit-card--active', 'trait-pill--active'));
      el.classList.add('swatch--active');
    }
  }

  function activateTrait(group, trait) {
    if (!group) return;
    qsa('.trait-pill', group).forEach(p => {
      p.classList.toggle('trait-pill--active', p.dataset.trait === trait);
    });
  }

  function syncCreatorPreview() {
    const head    = document.getElementById('avatar-head');
    const outfit  = document.getElementById('avatar-outfit');
    const nameEl  = document.getElementById('preview-name');
    const traitEl = document.getElementById('preview-trait');

    if (head)   head.style.background = state.character.skin;
    if (outfit) {
      outfit.style.background = state.character.outfitColor;
      const icons = { hustler: '🔥', corporate: '💼', influencer: '📱', chaos: '💥' };
      outfit.textContent = icons[state.character.outfit] ?? '🔥';
    }
    if (nameEl)  nameEl.textContent  = state.character.name  || 'Your Name';
    if (traitEl) traitEl.textContent = state.character.trait || 'Pick Your Vibe';
  }

  function saveCharacter() {
    localStorage.setItem('nh_name',         state.character.name);
    localStorage.setItem('nh_skin',         state.character.skin);
    localStorage.setItem('nh_outfit',       state.character.outfit);
    localStorage.setItem('nh_outfit_color', state.character.outfitColor);
    localStorage.setItem('nh_trait',        state.character.trait);
  }

  /* -------------------------------------------------------
     SETTINGS
  ------------------------------------------------------- */
  function initSettings() {
    // Range sliders — live value display
    const sliders = [
      { inputId: 'vol-master', valueId: 'val-master' },
      { inputId: 'vol-music',  valueId: 'val-music' },
      { inputId: 'vol-sfx',    valueId: 'val-sfx' }
    ];

    sliders.forEach(({ inputId, valueId }) => {
      const input = document.getElementById(inputId);
      const label = document.getElementById(valueId);
      if (!input || !label) return;
      input.addEventListener('input', () => {
        label.textContent = input.value;
        if (inputId === 'vol-music' && music) {
          music.volume = input.value / 100;
          state.musicVolume = music.volume;
          localStorage.setItem('nh_vol', String(music.volume));
        }
      });
    });

    // Toggle buttons
    const toggles = ['toggle-vibration', 'toggle-hints', 'toggle-satire'];
    toggles.forEach(id => {
      const btn = document.getElementById(id);
      if (!btn) return;
      btn.addEventListener('click', () => {
        const isOn = btn.getAttribute('aria-checked') === 'true';
        btn.setAttribute('aria-checked', String(!isOn));
        btn.textContent = !isOn ? 'ON' : 'OFF';
        btn.classList.toggle('toggle-btn--off', isOn);
      });
    });

    // Reset
    const resetBtn = document.getElementById('btn-reset-game');
    if (resetBtn) {
      resetBtn.addEventListener('click', () => {
        if (window.confirm('Reset all game data? This is irreversible.')) {
          localStorage.clear();
          window.location.reload();
        }
      });
    }
  }

  /* -------------------------------------------------------
     HUD SIMULATION
  ------------------------------------------------------- */
  function initHudSim() {
    // Simple animated money counter to feel alive
    const moneyEl   = document.getElementById('hud-money');
    const respectEl = document.getElementById('hud-respect');
    if (!moneyEl || !respectEl) return;

    let cash = 325;
    let rep  = 10;

    const ticker = setInterval(() => {
      cash += Math.floor(Math.random() * 15) - 3;
      rep   = Math.min(rep + (Math.random() > 0.8 ? 1 : 0), 999);
      moneyEl.textContent   = `$${cash.toLocaleString()}`;
      respectEl.textContent = `Rep: ${rep}`;
    }, 2000);

    // Stop when we leave the HUD screen
    const observer = new MutationObserver(() => {
      const hudScreen = document.getElementById('screen-hud');
      if (!hudScreen.classList.contains('screen--active')) {
        clearInterval(ticker);
        observer.disconnect();
      }
    });
    observer.observe(document.getElementById('screen-hud'), { attributes: true, attributeFilter: ['class'] });
  }

  /* -------------------------------------------------------
     INIT
  ------------------------------------------------------- */
  function init() {
    // Mute button
    updateMuteBtn();
    muteBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      toggleMute();
    });

    // Attempt autoplay immediately (may be blocked; first click will unblock)
    if (music) {
      music.volume = state.musicVolume;
      music.muted  = state.musicMuted;
      music.play().then(() => { state.musicStarted = true; }).catch(() => {});
    }

    // Boot splash animation
    animateBootText();

    // Wire up all subsystems
    initNavigation();
    initCharacterCreator();
    initStorySelect();
    initSettings();

    // Ensure splash is visible
    showScreen('screen-splash');
  }

  document.addEventListener('DOMContentLoaded', init);
})();
