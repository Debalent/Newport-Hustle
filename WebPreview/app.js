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
      outfitColor: localStorage.getItem('nh_outfit_color') ?? '#FF7A00',
      trait:   localStorage.getItem('nh_trait') ?? 'Big Talker',
    }
  };

  /* -------------------------------------------------------
     DOM REFS
  ------------------------------------------------------- */
  const muteBtn  = document.getElementById('mute-btn');
  const bgMusic   = document.getElementById('bg-music');
  const bgMusic2  = document.getElementById('bg-music-2');

  /* -------------------------------------------------------
     WEB AUDIO FALLBACK SYSTEM
     Used only if the actual audio file fails to load.
  ------------------------------------------------------- */
  var audioCtx    = null;
  var masterGain  = null;
  var loopTimer   = null;
  var nextBarTime = 0;
  var barIndex    = 0;
  var BPM         = 88;
  var BEAT        = 60 / BPM;
  var BAR         = BEAT * 4;
  // Lo-fi minor chord progression [C2,Eb2,G2 | Bb1,Db2,F2 | B1,D2,F#2 repeated]
  var CHORDS = [
    [130.81, 155.56, 196.00],
    [116.54, 138.59, 174.61],
    [123.47, 146.83, 185.00],
    [116.54, 138.59, 174.61],
  ];

  function _initAudio() {
    if (audioCtx) return;
    audioCtx   = new (window.AudioContext || window.webkitAudioContext)();
    masterGain = audioCtx.createGain();
    masterGain.gain.value = state.musicMuted ? 0 : state.musicVolume * 0.5;
    masterGain.connect(audioCtx.destination);
  }

  function _note(freq, t, dur, type, vol) {
    var o = audioCtx.createOscillator(), g = audioCtx.createGain();
    o.type = type; o.frequency.value = freq;
    g.gain.setValueAtTime(0, t);
    g.gain.linearRampToValueAtTime(vol, t + 0.04);
    g.gain.setValueAtTime(vol, t + dur - 0.08);
    g.gain.linearRampToValueAtTime(0, t + dur);
    o.connect(g); g.connect(masterGain); o.start(t); o.stop(t + dur + 0.01);
  }

  function _noise(t, dur, hp, vol) {
    var len = Math.ceil(audioCtx.sampleRate * dur);
    var buf = audioCtx.createBuffer(1, len, audioCtx.sampleRate);
    var d = buf.getChannelData(0);
    for (var i = 0; i < len; i++) d[i] = Math.random() * 2 - 1;
    var src = audioCtx.createBufferSource(); src.buffer = buf;
    var f = audioCtx.createBiquadFilter(); f.type = 'highpass'; f.frequency.value = hp;
    var g = audioCtx.createGain();
    g.gain.setValueAtTime(vol, t); g.gain.exponentialRampToValueAtTime(0.0001, t + dur);
    src.connect(f); f.connect(g); g.connect(masterGain); src.start(t); src.stop(t + dur + 0.01);
  }

  function _kick(t) {
    var o = audioCtx.createOscillator(), g = audioCtx.createGain();
    o.frequency.setValueAtTime(80, t); o.frequency.exponentialRampToValueAtTime(20, t + 0.35);
    g.gain.setValueAtTime(0.7, t); g.gain.exponentialRampToValueAtTime(0.001, t + 0.4);
    o.connect(g); g.connect(masterGain); o.start(t); o.stop(t + 0.42);
  }

  function _bar(t, idx) {
    var ch = CHORDS[idx % CHORDS.length];
    ch.forEach(function(f) { _note(f, t, BAR, 'sine', 0.06); }); // pad
    _note(ch[0] * 0.5, t, BEAT * 0.75, 'triangle', 0.26);          // sub bass beat 1
    _note(ch[0] * 0.5, t + BEAT * 2, BEAT * 0.75, 'triangle', 0.22); // sub bass beat 3
    _kick(t); _kick(t + BEAT * 2);                                   // kick 1+3
    _noise(t + BEAT, 0.14, 1800, 0.35);                             // snare beat 2
    _noise(t + BEAT * 3, 0.14, 1800, 0.32);                        // snare beat 4
    for (var i = 0; i < 8; i++) {                                   // hi-hats
      _noise(t + i * BEAT * 0.5, 0.04, 8000, i % 2 === 0 ? 0.16 : 0.09);
    }
  }

  function _scheduler() {
    if (!audioCtx) return;
    while (nextBarTime < audioCtx.currentTime + 0.35) {
      _bar(nextBarTime, barIndex); nextBarTime += BAR; barIndex++;
    }
  }

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
      case 'screen-missions':          renderMissions(); break;
      case 'screen-character-creator': syncCreatorPreview(); break;
      case 'screen-hud':
        initHudSim();
        triggerMissionIntro('Welcome Back', 'Meet your mentor downtown');
        // Boot the Three.js world (defer 2 rAFs so CSS layout is paint-complete)
        if (window.NHWorld) {
          var threeCanvas = document.getElementById('three-canvas');
          var lockPrompt  = document.getElementById('fps-lock-prompt');
          if (threeCanvas && !window.NHWorld._inited) {
            window.NHWorld._inited = true;
            requestAnimationFrame(function() {
              requestAnimationFrame(function() {
                window.NHWorld.init(threeCanvas);
                window.NHWorld.onNPCInteract(function(npcId, name) {
                  showToast('Talking to ' + name + '...');
                  showScreen('screen-missions');
                });
                // Dismiss click-to-play prompt on first canvas touch
                if (lockPrompt) {
                  threeCanvas.addEventListener('pointerdown', function dismiss() {
                    lockPrompt.classList.add('is-hidden');
                    threeCanvas.removeEventListener('pointerdown', dismiss);
                  }, { once: true });
                }
              });
            });
          } else if (threeCanvas) {
            window.NHWorld.resume();
          }
        }
        break;
      case 'screen-map':               initMap(); break;
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
      const btn = e.target.closest('.btn, .gta-nav-btn, .top-bar__back');
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
    if (state.musicStarted) return;
    state.musicStarted = true;
    if (!bgMusic) { _startSynth(); return; }

    // Build playlist: track 1 then track 2, cycling forever
    var _playlist = [bgMusic, bgMusic2 || bgMusic];
    var _playIdx  = 0;

    function _applyVol(t) {
      t.volume = state.musicMuted ? 0 : state.musicVolume;
    }

    function _playNext() {
      _playIdx = (_playIdx + 1) % _playlist.length;
      var next = _playlist[_playIdx];
      next.currentTime = 0;
      _applyVol(next);
      next.play().catch(function() {});
    }

    _playlist.forEach(function(t) {
      _applyVol(t);
      t.addEventListener('ended', _playNext);
    });

    _playlist[0].play().catch(function() { _startSynth(); });
  }

  function toggleMute() {
    state.musicMuted = !state.musicMuted;
    [bgMusic, bgMusic2].forEach(function(t) {
      if (t) t.volume = state.musicMuted ? 0 : state.musicVolume;
    });
    if (masterGain && audioCtx) {
      masterGain.gain.setTargetAtTime(
        state.musicMuted ? 0 : state.musicVolume * 0.5,
        audioCtx.currentTime, 0.15
      );
    }
    localStorage.setItem('nh_muted', JSON.stringify(state.musicMuted));
    updateMuteBtn();
  }

  // Web Audio fallback (synth beat)
  function _startSynth() {
    _initAudio();
    if (audioCtx.state === 'suspended') audioCtx.resume();
    nextBarTime = audioCtx.currentTime + 0.05;
    barIndex    = 0;
    loopTimer   = setInterval(_scheduler, 100);
    _scheduler();
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
    'Validating sidewalk license... ✔',
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
        if (inputId === 'vol-music') {
          const vol = input.value / 100;
          state.musicVolume = vol;
          [bgMusic, bgMusic2].forEach(function(t) {
            if (t) t.volume = state.musicMuted ? 0 : vol;
          });
          localStorage.setItem('nh_vol', String(vol));
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
     TOAST NOTIFICATIONS
  ------------------------------------------------------- */
  function showToast(message, duration) {
    if (duration === undefined) duration = 2800;
    var container = document.getElementById('toast-container');
    if (!container) return;
    var toast = document.createElement('div');
    toast.className = 'toast';
    toast.textContent = message;
    container.appendChild(toast);
    requestAnimationFrame(function() {
      requestAnimationFrame(function() {
        toast.classList.add('toast--show');
      });
    });
    setTimeout(function() {
      toast.classList.remove('toast--show');
      toast.addEventListener('transitionend', function() { toast.remove(); }, { once: true });
    }, duration);
  }

  /* -------------------------------------------------------
     MISSION INTRO — cinematic bar slide-in
  ------------------------------------------------------- */
  function triggerMissionIntro(title, subtitle) {
    var overlay = document.getElementById('mission-intro');
    if (!overlay) return;
    var titleEl = document.getElementById('mission-intro-title');
    var subEl   = document.getElementById('mission-intro-subtitle');
    if (titleEl) titleEl.textContent = title    || 'Mission';
    if (subEl)   subEl.textContent   = subtitle || '';
    overlay.setAttribute('aria-hidden', 'false');
    overlay.style.opacity = '1';
    overlay.classList.add('is-active');
    setTimeout(function() {
      overlay.classList.remove('is-active');
      setTimeout(function() {
        overlay.style.opacity = '0';
        overlay.setAttribute('aria-hidden', 'true');
      }, 400);
    }, 3400);
  }

  /* -------------------------------------------------------
     HUD SIMULATION — GTA V Corner HUD
  ------------------------------------------------------- */
  function initHudSim() {
    const moneyEl  = document.getElementById('hud-money');
    const repFill  = document.getElementById('hud-rep-fill');
    const hpFill   = document.getElementById('hud-hp');
    const arFill   = document.getElementById('hud-ar');
    const objEl    = document.getElementById('hw-obj-text');
    if (!moneyEl) return;

    let cash = 325;
    let rep  = 30;
    let hp   = 80;
    let ar   = 55;

    if (objEl) objEl.style.transition = 'opacity 0.2s ease';

    const ticker = setInterval(() => {
      cash += Math.floor(Math.random() * 18) - 4;
      rep   = Math.min(100, rep + (Math.random() > 0.75 ? 1 : 0));
      hp    = Math.max(35, Math.min(100, hp + (Math.random() > 0.88 ? -2 : 1)));
      ar    = Math.max(0,  Math.min(100, ar + (Math.random() > 0.92 ? -3 : 0)));
      moneyEl.textContent = '$' + cash.toLocaleString();
      if (repFill) repFill.style.width = rep + '%';
      if (hpFill)  hpFill.style.width  = hp  + '%';
      if (arFill)  arFill.style.width  = ar  + '%';
    }, 2000);

    // Cycle active mission objective text
    const objectives = [
      'Meet your mentor downtown',
      "Head to Barber's Row",
      'Earn $50 in tips',
      'Talk to the Spa Owner',
      'Start a side hustle',
      'Avoid police heat',
      'Explore the Riverfront',
    ];
    let objIdx = 0;
    const objTimer = setInterval(function() {
      objIdx = (objIdx + 1) % objectives.length;
      if (objEl) {
        objEl.style.opacity = '0';
        setTimeout(() => {
          objEl.textContent   = objectives[objIdx];
          objEl.style.opacity = '1';
        }, 200);
      }
    }, 5000);

    // Stop timers when we leave the HUD screen
    const hudScreen = document.getElementById('screen-hud');
    const observer  = new MutationObserver(() => {
      if (!hudScreen.classList.contains('screen--active')) {
        clearInterval(ticker);
        clearInterval(objTimer);
        if (window.NHWorld) window.NHWorld.pause();
        observer.disconnect();
      }
    });
    observer.observe(hudScreen, { attributes: true, attributeFilter: ['class'] });
  }

  /* -------------------------------------------------------
     MAP — Zone tap interactions
  ------------------------------------------------------- */
  var _mapInited = false;
  function initMap() {
    if (_mapInited) return;
    _mapInited = true;

    var popup     = document.getElementById('map-popup');
    var popupName = document.getElementById('map-popup-name');
    var popupTag  = document.getElementById('map-popup-tag');
    var popupMissions = document.getElementById('map-popup-missions');
    var popupClose = document.getElementById('map-popup-close');
    var popupGo   = document.getElementById('map-popup-go');
    if (!popup) return;

    var zoneColors = {
      downtown:    '#FF7A00',
      riverfront:  '#4ecdc4',
      barbersrow:  '#FF7A00',
      strip:       '#ff6b9d',
      spa:         '#4ecdc4',
      residential: '#aaa',
      diaz:        '#f94144',
      airport:     '#f94144',
    };

    function openPopup(el) {
      var name     = el.getAttribute('data-name');
      var tag      = el.getAttribute('data-tag');
      var missions = parseInt(el.getAttribute('data-missions'), 10);
      var zone     = el.getAttribute('data-zone');
      var color    = zoneColors[zone] || '#fff';
      popupName.textContent = name;
      popupName.style.color = color;
      popupTag.textContent  = tag;
      popupMissions.textContent = missions > 0
        ? missions + ' active mission' + (missions === 1 ? '' : 's')
        : 'No missions available yet';
      popupMissions.style.color = missions > 0 ? '#FF7A00' : '#888';
      popupGo.style.display = missions > 0 ? 'inline-block' : 'none';
      popup.classList.add('is-visible');
    }

    document.querySelectorAll('.map-zone').forEach(function(zone) {
      zone.addEventListener('click', function(e) {
        e.stopPropagation();
        openPopup(this);
      });
    });

    if (popupClose) {
      popupClose.addEventListener('click', function() {
        popup.classList.remove('is-visible');
      });
    }

    // Tapping outside the popup closes it
    document.getElementById('screen-map').addEventListener('click', function() {
      popup.classList.remove('is-visible');
    });

    if (popupGo) {
      popupGo.addEventListener('click', function(e) {
        e.stopPropagation();
        popup.classList.remove('is-visible');
        showScreen('screen-missions');
      });
    }
  }

  /* -------------------------------------------------------
     HAMBURGER NAV DRAWER
  ------------------------------------------------------- */
  function initHamburger() {
    var hamburgerBtn = document.getElementById('hamburger-btn');
    var drawer       = document.getElementById('nav-drawer');
    var backdrop     = document.getElementById('nav-drawer-backdrop');
    var closeBtn     = document.getElementById('nav-drawer-close');
    if (!hamburgerBtn || !drawer) return;

    function openDrawer() {
      drawer.classList.add('nav-drawer--open');
      drawer.setAttribute('aria-hidden', 'false');
      hamburgerBtn.setAttribute('aria-expanded', 'true');
      hamburgerBtn.classList.add('is-open');
    }

    function closeDrawer() {
      drawer.classList.remove('nav-drawer--open');
      drawer.setAttribute('aria-hidden', 'true');
      hamburgerBtn.setAttribute('aria-expanded', 'false');
      hamburgerBtn.classList.remove('is-open');
    }

    hamburgerBtn.addEventListener('click', function (e) {
      e.stopPropagation();
      startMusic();
      openDrawer();
    });

    if (closeBtn)  closeBtn.addEventListener('click', closeDrawer);
    if (backdrop)  backdrop.addEventListener('click', closeDrawer);

    // Close drawer when a nav item inside it is tapped
    drawer.addEventListener('click', function (e) {
      if (e.target.closest('[data-target]')) closeDrawer();
    });
  }

  /* -------------------------------------------------------
     INIT
  ------------------------------------------------------- */
  function init() {
    // Mute button — respond to pointerdown for instant mobile response
    updateMuteBtn();
    let _mutePointerFired = false;
    muteBtn.addEventListener('pointerdown', (e) => {
      e.preventDefault();
      e.stopPropagation();
      _mutePointerFired = true;
      toggleMute();
    }, { passive: false });
    // click handles keyboard (Enter/Space) without double-firing after pointerdown
    muteBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      if (_mutePointerFired) { _mutePointerFired = false; return; }
      toggleMute();
    });

    // Boot splash animation
    animateBootText();

    // Wire up all subsystems
    initNavigation();
    initHamburger();
    initCharacterCreator();
    initStorySelect();
    initSettings();

    // Ensure splash is visible
    showScreen('screen-splash');

    // Expose toast for cross-module use (world.js, etc.)
    window.showToast = showToast;
  }

  document.addEventListener('DOMContentLoaded', init);
})();
