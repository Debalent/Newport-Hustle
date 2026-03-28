/**
 * Newport Hustle — Three.js GTA-Style World Engine
 * --------------------------------------------------
 * Third-person open-world camera · Newport, AR city
 *
 * Controls (Desktop)
 *   WASD / Arrow keys  — Move player
 *   Mouse drag         — Orbit camera (GTA right-stick)
 *   Scroll wheel       — Zoom in / out
 *   Shift              — Sprint
 *   E                  — Interact with NPC / enter vehicle
 *
 * Controls (Mobile)
 *   Left joystick      — Move
 *   Right joystick     — Camera orbit
 *   E button           — Interact / enter vehicle
 */
(function () {
  'use strict';

  /* ═══════════════════════════════════════════
     PUBLIC API
  ═══════════════════════════════════════════ */
  window.NHWorld = {
    init:          initWorld,
    pause:         pauseWorld,
    resume:        resumeWorld,
    dispose:       disposeWorld,
    onNPCInteract: function (fn) { _cbNPCInteract = fn; },
    _inVehicle:    false,
  };

  /* ═══════════════════════════════════════════
     STATE
  ═══════════════════════════════════════════ */
  var THREE;
  var renderer, scene, camera, clock;
  var playerGroup, playerBodyMesh;
  var animFrameId, isRunning = false;
  var _cbNPCInteract = null;
  var _canvas;

  // Camera
  var camYaw   = 0;
  var camPitch = 0.35;
  var camDist  = 9;
  var CAM_MIN_DIST  = 3;
  var CAM_MAX_DIST  = 18;
  var CAM_MIN_PITCH = 0.12;
  var CAM_MAX_PITCH = 1.1;

  // Mouse drag
  var mouseDown = false, lastMX = 0, lastMY = 0;

  // Keyboard
  var keys = {};

  // Mobile joysticks
  var joyMove = { active: false, id: null, ox: 0, oy: 0, x: 0, y: 0 };
  var joyLook = { active: false, id: null, ox: 0, oy: 0, x: 0, y: 0 };

  // World objects
  var colliders     = [];  // {minX, maxX, minZ, maxZ}
  var npcObjects    = [];
  var vehicleObjects = [];
  var _rotLights    = [];

  // Interaction
  var INTERACT_RANGE = 3.8;
  var nearbyNPC     = null;
  var nearbyVehicle = null;
  var inVehicle     = null;

  // Animation
  var _bobT   = 0;
  var _waveT  = 0;
  var _alertT = 0;

  /* ═══════════════════════════════════════════
     NPC & VEHICLE DEFINITIONS
  ═══════════════════════════════════════════ */
  var NPC_DEFS = [
    { id: 'mentor',   name: 'Big Earl',          action: 'Talk to your Mentor',  pos: { x: 12,  z: -8  }, color: 0x4ecdc4, skin: 0x8d5524 },
    { id: 'rival',    name: 'Darius Cole',        action: 'Talk to your Rival',   pos: { x: -16, z:  4  }, color: 0xf94144, skin: 0x4a2c0a },
    { id: 'barber',   name: "Tre's Cuts",         action: "Enter Barber's Row",   pos: { x:  22, z: 20  }, color: 0xFF7A00, skin: 0xc68642 },
    { id: 'spa',      name: 'Jade Spa',           action: 'Enter Jade Spa',       pos: { x: -20, z: -22 }, color: 0xff6b9d, skin: 0xf5cba7 },
    { id: 'side',     name: 'Side Hustle Guy',    action: 'Start a Side Hustle',  pos: { x:   6, z:  24 }, color: 0xffd700, skin: 0xe8a87c },
  ];

  var VEHICLE_DEFS = [
    { id: 'sedan',  pos: { x:  5, z: -1 }, rot: 0,            color: 0x1a3a88, label: 'Enter Vehicle' },
    { id: 'suv',    pos: { x: -8, z: 14 }, rot: Math.PI/2,    color: 0x882222, label: 'Enter Vehicle' },
    { id: 'police', pos: { x: 20, z: -6 }, rot: -Math.PI/4,   color: 0x111144, label: null, isPolice: true },
  ];

  /* ═══════════════════════════════════════════
     INIT / DISPOSE
  ═══════════════════════════════════════════ */
  // Get canvas dimensions, falling back to parent or window when canvas hasn't laid out yet
  function _canvasW(c) { return c.clientWidth  || (c.parentElement && c.parentElement.clientWidth)  || window.innerWidth; }
  function _canvasH(c) { return c.clientHeight || (c.parentElement && c.parentElement.clientHeight) || window.innerHeight; }

  function initWorld(canvas) {
    if (!window.THREE) {
      console.error('[NHWorld] Three.js not loaded.');
      return;
    }
    THREE = window.THREE;
    if (renderer) disposeWorld();

    _canvas = canvas;
    clock   = new THREE.Clock();

    var W = _canvasW(canvas);
    var H = _canvasH(canvas);

    // Renderer
    renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true, powerPreference: 'high-performance' });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.setSize(W, H);
    renderer.shadowMap.enabled  = true;
    renderer.shadowMap.type     = THREE.PCFSoftShadowMap;
    renderer.toneMapping        = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 0.85;

    // Scene
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x0d1520);
    scene.fog = new THREE.FogExp2(0x0d1520, 0.022);

    // Camera
    camera = new THREE.PerspectiveCamera(68, W / H, 0.1, 350);

    buildLighting();
    buildCity();
    buildPlayer();
    buildNPCs();
    buildVehicles();
    bindInput();

    // Dismiss lock prompt
    var prompt = document.getElementById('fps-lock-prompt');
    if (prompt) {
      // On mobile / touch devices no pointer-lock is needed — hide immediately
      if (window.matchMedia('(pointer: coarse)').matches) {
        prompt.classList.add('is-hidden');
      } else {
        // Desktop: dismiss on first canvas click
        canvas.addEventListener('pointerdown', function dismiss() {
          prompt.classList.add('is-hidden');
          canvas.removeEventListener('pointerdown', dismiss);
        }, { once: true });
        // Also dismiss if the prompt itself is tapped (it sits on top of canvas)
        prompt.addEventListener('pointerdown', function () {
          prompt.classList.add('is-hidden');
        }, { once: true });
      }
    }

    // Resize handler
    window._nhResize = function () {
      if (!renderer) return;
      var W = _canvasW(canvas);
      var H = _canvasH(canvas);
      renderer.setSize(W, H);
      camera.aspect = W / H;
      camera.updateProjectionMatrix();
    };
    window.addEventListener('resize', window._nhResize);
    // One-shot resize after first frame to catch any late layout
    requestAnimationFrame(function() { if (window._nhResize) window._nhResize(); });

    isRunning = true;
    animate();
  }

  function pauseWorld()  { isRunning = false; }
  function resumeWorld() { isRunning = true;  clock.getDelta(); /* flush dt */ }

  function disposeWorld() {
    isRunning = false;
    cancelAnimationFrame(animFrameId);
    unbindInput();
    window.removeEventListener('resize', window._nhResize);
    if (renderer) { renderer.dispose(); renderer = null; }
    colliders     = [];
    npcObjects    = [];
    vehicleObjects = [];
    _rotLights    = [];
    inVehicle     = null;
    scene = null;
    camera = null;
  }

  /* ═══════════════════════════════════════════
     LIGHTING
  ═══════════════════════════════════════════ */
  function buildLighting() {
    scene.add(new THREE.AmbientLight(0x2a3a5a, 0.55));

    var moon = new THREE.DirectionalLight(0x8ab4d4, 0.75);
    moon.position.set(-40, 70, -30);
    moon.castShadow = true;
    moon.shadow.mapSize.width  = 2048;
    moon.shadow.mapSize.height = 2048;
    moon.shadow.camera.near    = 1;
    moon.shadow.camera.far     = 200;
    moon.shadow.camera.left    = moon.shadow.camera.bottom = -90;
    moon.shadow.camera.right   = moon.shadow.camera.top   =  90;
    scene.add(moon);

    var fill = new THREE.DirectionalLight(0xFF8040, 0.22);
    fill.position.set(40, 25, 40);
    scene.add(fill);

    scene.add(new THREE.HemisphereLight(0x1a2a4a, 0x2a1808, 0.38));
  }

  /* ═══════════════════════════════════════════
     CITY GEOMETRY
  ═══════════════════════════════════════════ */
  function buildCity() {
    // Ground — asphalt
    addPlane(0, 0, 280, 280, 0x1e1e26);

    // Sidewalk grid
    addPlane( 12, 0, 4, 80,  0x2a2a34); // east  NS
    addPlane(-12, 0, 4, 80,  0x2a2a34); // west  NS
    addPlane(  0, 12, 80, 4, 0x2a2a34); // north EW
    addPlane(  0,-12, 80, 4, 0x2a2a34); // south EW

    // Road centre lines
    addRoadLines(0,   80, true);   // N–S
    addRoadLines(0,   80, false);  // E–W

    // === DOWNTOWN CORE ===
    addBuilding( 26,  26, 14, 30, 14, 0x1b3a5c, 'NEWPORT TOWER');
    addBuilding(-26,  26, 10, 22, 10, 0x2a1a18, 'CITY HALL');
    addBuilding( 26, -26, 12, 25, 12, 0x3a2010, 'MARLOWE BLDG');
    addBuilding(-26, -26, 10, 18,  9, 0x1a2a18, 'GREEN PLAZA');

    // === BARBER'S ROW (south) ===
    addBuilding( 24,  44,  6,  9,  8, 0x2a1808, "TRE'S CUTS");
    addBuilding( 14,  44,  5,  8,  7, 0x181820, 'SALON');
    addBuilding(  4,  44,  6,  8,  7, 0x202818, 'DELI');
    addBuilding(-6,   44,  5,  7,  6, 0x1a1a22, 'PAWN');

    // === SPA DISTRICT (west) ===
    addBuilding(-38, -14,  8, 13,  8, 0x2a1830, 'JADE SPA');
    addBuilding(-38,   5,  6,  9,  6, 0x1a2030, 'WELLNESS CTR');

    // === RESIDENTIAL (north-west) ===
    addBuilding(-32,  36,  5, 10,  5, 0x201820, 'APTS A');
    addBuilding(-40,  36,  5, 10,  5, 0x202018, 'APTS B');
    addBuilding(-24,  36,  4,  9,  5, 0x181818, 'APTS C');

    // === POLICE STATION ===
    addBuilding( 30, -16, 10, 11, 10, 0x151530, 'NEWPORT PD');
    addRotLight( 30, 12, -16); // police roof light

    // === RIVERFRONT (east) ===
    addBuilding( 48,   8,  6, 14,  8, 0x0d2030, 'RIVERFRONT');
    addBuilding( 48,  22,  5, 10,  6, 0x0e1828, 'MARINA');

    // === DEEP SKYLINE ===
    addBuilding( 60,  60, 18, 50, 18, 0x0d1a2a, null);
    addBuilding(-60,  60, 14, 38, 15, 0x0e1208, null);
    addBuilding( 60, -60, 15, 42, 15, 0x1a0808, null);
    addBuilding(-60, -60, 20, 55, 20, 0x081008, null);
    addBuilding(  0,  80, 12, 35, 14, 0x0d1520, null);
    addBuilding( 80,   0, 14, 44, 16, 0x100d1a, null);
    addBuilding(-80,   0, 12, 40, 14, 0x0a0d10, null);

    // === STREETLIGHTS ===
    [ [8,8], [-8,8], [8,-8], [-8,-8],
      [8,22], [-8,22], [8,-22], [-8,-22],
      [22,0], [-22,0] ].forEach(function(p) { addStreetlight(p[0], p[1]); });
  }

  function addPlane(x, z, w, d, color) {
    var geo  = new THREE.PlaneGeometry(w, d);
    var mat  = new THREE.MeshLambertMaterial({ color: color });
    var mesh = new THREE.Mesh(geo, mat);
    mesh.rotation.x  = -Math.PI / 2;
    mesh.position.set(x, 0.01, z);
    mesh.receiveShadow = true;
    scene.add(mesh);
  }

  function addRoadLines(offset, len, isNS) {
    var step = 4;
    for (var i = -len / 2; i < len / 2; i += step) {
      var geo = new THREE.PlaneGeometry(isNS ? 0.28 : 2.4, isNS ? 2.4 : 0.28);
      var mat = new THREE.MeshBasicMaterial({ color: 0x555566 });
      var m   = new THREE.Mesh(geo, mat);
      m.rotation.x = -Math.PI / 2;
      m.position.set(isNS ? offset : i + 1.2, 0.02, isNS ? i + 1.2 : offset);
      scene.add(m);
    }
  }

  function addBuilding(x, z, hw, height, hd, color, label) {
    var geo  = new THREE.BoxGeometry(hw * 2, height, hd * 2);
    var mat  = new THREE.MeshLambertMaterial({ color: color });
    var mesh = new THREE.Mesh(geo, mat);
    mesh.position.set(x, height / 2, z);
    mesh.castShadow    = true;
    mesh.receiveShadow = true;
    scene.add(mesh);

    // Roof edge stripe (orange accent)
    var roofGeo = new THREE.BoxGeometry(hw * 2 + 0.3, 0.35, hd * 2 + 0.3);
    var roofMat = new THREE.MeshLambertMaterial({ color: 0xFF7A00, emissive: 0xFF7A00, emissiveIntensity: 0.18 });
    var roof    = new THREE.Mesh(roofGeo, roofMat);
    roof.position.set(x, height + 0.18, z);
    scene.add(roof);

    // Windows
    if (height > 8) addWindowGrid(x, z, hw, height, hd);

    // Floating name label
    if (label) addFloatLabel(x, height + 1.5, z, label, color, null);

    // Collider
    colliders.push({ minX: x - hw, maxX: x + hw, minZ: z - hd, maxZ: z + hd });
  }

  function addWindowGrid(bx, bz, hw, height, hd) {
    var cols = Math.max(1, Math.floor((hw * 2) / 2.8));
    var rows = Math.max(1, Math.floor(height / 2.8));
    var mat  = new THREE.MeshBasicMaterial({ color: 0xffe8a0, transparent: true });

    for (var r = 0; r < rows; r++) {
      for (var c = 0; c < cols; c++) {
        if (Math.random() < 0.28) continue;
        var geo   = new THREE.PlaneGeometry(0.9, 1.2);
        var wmesh = new THREE.Mesh(geo, mat.clone());
        wmesh.material.opacity = 0.2 + Math.random() * 0.65;
        // Place on +Z face of building
        wmesh.position.set(
          bx - hw + 1.4 + c * 2.8,
          1.6 + r * 2.8,
          bz + hd + 0.05
        );
        scene.add(wmesh);
      }
    }
  }

  function addStreetlight(x, z) {
    var poleMat = new THREE.MeshLambertMaterial({ color: 0x44445a });
    var pole    = new THREE.Mesh(new THREE.CylinderGeometry(0.08, 0.11, 5.2, 7), poleMat);
    pole.position.set(x, 2.6, z);
    scene.add(pole);

    var arm = new THREE.Mesh(new THREE.CylinderGeometry(0.05, 0.05, 1.6, 5), poleMat.clone());
    arm.rotation.z = Math.PI / 2;
    arm.position.set(x + 0.8, 5.3, z);
    scene.add(arm);

    var lampMat = new THREE.MeshBasicMaterial({ color: 0xFFE090 });
    var lamp    = new THREE.Mesh(new THREE.BoxGeometry(0.5, 0.24, 0.5), lampMat);
    lamp.position.set(x + 1.3, 5.3, z);
    scene.add(lamp);

    var pLight = new THREE.PointLight(0xffe8a0, 1.8, 20, 2);
    pLight.position.set(x + 1.3, 5.3, z);
    scene.add(pLight);
  }

  function addRotLight(x, y, z) {
    var lR = new THREE.PointLight(0xff2222, 2.2, 22, 2);
    var lB = new THREE.PointLight(0x2222ff, 2.2, 22, 2);
    lR.position.set(x - 0.8, y, z);
    lB.position.set(x + 0.8, y, z);
    scene.add(lR, lB);
    _rotLights.push({ lights: [lR, lB], phase: 0 });
  }

  /* ═══════════════════════════════════════════
     PLAYER
  ═══════════════════════════════════════════ */
  function buildPlayer() {
    playerGroup = new THREE.Group();
    scene.add(playerGroup);

    // Legs
    var legMat = new THREE.MeshLambertMaterial({ color: 0x1a1a2e });
    [-0.16, 0.16].forEach(function (ox) {
      var leg = new THREE.Mesh(new THREE.CylinderGeometry(0.1, 0.1, 0.7, 8), legMat);
      leg.position.set(ox, 0.35, 0);
      leg.castShadow = true;
      playerGroup.add(leg);
    });

    // Body (hoodie — orange)
    var bodyMat = new THREE.MeshLambertMaterial({ color: 0xFF7A00 });
    playerBodyMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.3, 0.28, 1.1, 10), bodyMat);
    playerBodyMesh.position.y = 1.05;
    playerBodyMesh.castShadow = true;
    playerGroup.add(playerBodyMesh);

    // Head
    var headMat = new THREE.MeshLambertMaterial({ color: 0xc68642 });
    var head    = new THREE.Mesh(new THREE.SphereGeometry(0.26, 10, 8), headMat);
    head.position.y = 1.85;
    head.castShadow = true;
    playerGroup.add(head);

    // Cap
    var capMat = new THREE.MeshLambertMaterial({ color: 0x111111 });
    var cap    = new THREE.Mesh(new THREE.CylinderGeometry(0.28, 0.28, 0.12, 10), capMat);
    var brim   = new THREE.Mesh(new THREE.CylinderGeometry(0.32, 0.32, 0.06, 10), capMat);
    cap.position.set(0, 2.1, 0);
    brim.position.set(0.1, 2.04, 0);
    brim.rotation.z = 0.12;
    playerGroup.add(cap, brim);

    // Ground shadow disc
    var shadowMat  = new THREE.MeshBasicMaterial({ color: 0x000000, transparent: true, opacity: 0.35 });
    var shadowDisc = new THREE.Mesh(new THREE.CircleGeometry(0.4, 14), shadowMat);
    shadowDisc.rotation.x = -Math.PI / 2;
    shadowDisc.position.y = 0.015;
    playerGroup.add(shadowDisc);

    playerGroup.position.set(0, 0, 0);
  }

  /* ═══════════════════════════════════════════
     NPCs
  ═══════════════════════════════════════════ */
  function buildNPCs() {
    NPC_DEFS.forEach(function (def) {
      var group = new THREE.Group();

      // Body
      var bodyMat = new THREE.MeshLambertMaterial({ color: def.color });
      var body    = new THREE.Mesh(new THREE.CylinderGeometry(0.28, 0.28, 1.1, 9), bodyMat);
      body.position.y = 0.85;
      body.castShadow = true;
      group.add(body);

      // Head
      var headMat = new THREE.MeshLambertMaterial({ color: def.skin });
      var head    = new THREE.Mesh(new THREE.SphereGeometry(0.24, 9, 7), headMat);
      head.position.y = 1.72;
      group.add(head);

      // Glow ring on ground
      var ringGeo = new THREE.RingGeometry(0.48, 0.68, 22);
      var ringMat = new THREE.MeshBasicMaterial({ color: def.color, transparent: true, opacity: 0.55, side: THREE.DoubleSide });
      var ring    = new THREE.Mesh(ringGeo, ringMat);
      ring.rotation.x  = -Math.PI / 2;
      ring.position.y  = 0.02;
      group.add(ring);

      // Floating name label
      addFloatLabel(0, 2.35, 0, def.name, def.color, group);

      group.position.set(def.pos.x, 0, def.pos.z);
      group.userData = { npcId: def.id, action: def.action, ring: ring, def: def };
      scene.add(group);
      npcObjects.push(group);
    });
  }

  /* ═══════════════════════════════════════════
     VEHICLES
  ═══════════════════════════════════════════ */
  function buildVehicles() {
    VEHICLE_DEFS.forEach(function (def) {
      var group = new THREE.Group();

      // Main body
      var bodyMat = new THREE.MeshLambertMaterial({ color: def.color });
      var bodyGeo = new THREE.BoxGeometry(2.15, 0.78, 4.5);
      var body    = new THREE.Mesh(bodyGeo, bodyMat);
      body.position.y = 0.62;
      body.castShadow = true;
      group.add(body);

      // Cabin / roof
      var roofCol = blendHex(def.color, 0x000000, 0.4);
      var roofMat = new THREE.MeshLambertMaterial({ color: roofCol });
      var roof    = new THREE.Mesh(new THREE.BoxGeometry(1.75, 0.7, 2.5), roofMat);
      roof.position.set(0, 1.35, -0.15);
      group.add(roof);

      // Windshield
      var windMat  = new THREE.MeshBasicMaterial({ color: 0x4488bb, transparent: true, opacity: 0.45, side: THREE.DoubleSide });
      var windMesh = new THREE.Mesh(new THREE.PlaneGeometry(1.65, 0.62), windMat);
      windMesh.position.set(0, 1.42, 1.1);
      windMesh.rotation.x = 0.18;
      group.add(windMesh);

      // 4 Wheels
      var wheelMat = new THREE.MeshLambertMaterial({ color: 0x111114 });
      var rimMat   = new THREE.MeshLambertMaterial({ color: 0x888890 });
      [ [-0.95, -1.45], [0.95, -1.45], [-0.95, 1.45], [0.95, 1.45] ].forEach(function (wh) {
        var wGeo = new THREE.CylinderGeometry(0.33, 0.33, 0.22, 14);
        var w    = new THREE.Mesh(wGeo, wheelMat);
        w.rotation.z = Math.PI / 2;
        w.position.set(wh[0], 0.33, wh[1]);
        group.add(w);
        var rim = new THREE.Mesh(new THREE.CylinderGeometry(0.22, 0.22, 0.23, 8), rimMat);
        rim.rotation.z = Math.PI / 2;
        rim.position.copy(w.position);
        group.add(rim);
      });

      // Headlights
      var hlColor = def.isPolice ? 0x4444ff : 0xFFFFB0;
      var hlMat   = new THREE.MeshBasicMaterial({ color: hlColor });
      [ [-0.62, 2.18], [0.62, 2.18] ].forEach(function (hl) {
        var hlM = new THREE.Mesh(new THREE.PlaneGeometry(0.26, 0.2), hlMat);
        hlM.position.set(hl[0], 0.72, hl[1]);
        group.add(hlM);
        var hLight = new THREE.SpotLight(hlColor, 2.5, 28, 0.3, 0.5);
        hLight.position.set(hl[0], 0.72, hl[1]);
        hLight.target.position.set(hl[0], 0.3, hl[1] + 12);
        group.add(hLight, hLight.target);
      });

      // Police light bar
      if (def.isPolice) {
        var barMat = new THREE.MeshLambertMaterial({ color: 0x222244 });
        var bar    = new THREE.Mesh(new THREE.BoxGeometry(1.5, 0.18, 0.35), barMat);
        bar.position.set(0, 1.75, -0.15);
        group.add(bar);
        var lR = new THREE.PointLight(0xff2222, 2, 14, 2);
        var lB = new THREE.PointLight(0x2222ff, 2, 14, 2);
        lR.position.set(-0.45, 1.95, -0.15);
        lB.position.set( 0.45, 1.95, -0.15);
        group.add(lR, lB);
        _rotLights.push({ lights: [lR, lB], phase: Math.PI * 0.5 });
      }

      // Interact label
      if (def.label) addFloatLabel(0, 2.5, 0, def.label, 0xFF7A00, group);

      group.position.set(def.pos.x, 0, def.pos.z);
      group.rotation.y = def.rot;
      group.userData   = { vehicleId: def.id, def: def };
      scene.add(group);
      vehicleObjects.push(group);
    });
  }

  function blendHex(a, b, t) {
    var ar = (a >> 16) & 0xff, ag = (a >> 8) & 0xff, ab = a & 0xff;
    var br = (b >> 16) & 0xff, bg = (b >> 8) & 0xff, bb = b & 0xff;
    var r  = Math.round(ar + (br - ar) * t);
    var g  = Math.round(ag + (bg - ag) * t);
    var bl = Math.round(ab + (bb - ab) * t);
    return (r << 16) | (g << 8) | bl;
  }

  /* ═══════════════════════════════════════════
     CANVAS TEXT LABELS (sprites)
  ═══════════════════════════════════════════ */
  function addFloatLabel(x, y, z, text, color, parent) {
    var c   = document.createElement('canvas');
    c.width = 256; c.height = 48;
    var ctx = c.getContext('2d');

    ctx.clearRect(0, 0, 256, 48);
    ctx.fillStyle = 'rgba(0,0,0,0.60)';
    rrect(ctx, 2, 2, 252, 44, 8); ctx.fill();

    var hex = '#' + ((color >>> 0).toString(16).padStart(6, '0'));
    ctx.fillStyle = hex;
    ctx.font = 'bold 17px Lato, sans-serif';
    ctx.textAlign    = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(text.toUpperCase(), 128, 24);

    var tex     = new THREE.CanvasTexture(c);
    var spriteMat = new THREE.SpriteMaterial({ map: tex, transparent: true, depthTest: false });
    var sprite  = new THREE.Sprite(spriteMat);
    sprite.scale.set(3.4, 0.62, 1);
    sprite.position.set(x, y, z);

    if (parent) parent.add(sprite);
    else        scene.add(sprite);
    return sprite;
  }

  function rrect(ctx, x, y, w, h, r) {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.lineTo(x + w - r, y);    ctx.arcTo(x + w, y,     x + w, y + r,     r);
    ctx.lineTo(x + w, y + h - r); ctx.arcTo(x + w, y + h, x + w - r, y + h, r);
    ctx.lineTo(x + r, y + h);     ctx.arcTo(x,     y + h, x,       y + h-r, r);
    ctx.lineTo(x,     y + r);     ctx.arcTo(x,     y,     x + r,   y,       r);
    ctx.closePath();
  }

  /* ═══════════════════════════════════════════
     INPUT BINDING
  ═══════════════════════════════════════════ */
  function bindInput() {
    document.addEventListener('keydown', onKeyDown);
    document.addEventListener('keyup',   onKeyUp);
    _canvas.addEventListener('mousedown', onMouseDown);
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup',   onMouseUp);
    _canvas.addEventListener('wheel',     onWheel,      { passive: false });
    _canvas.addEventListener('touchstart', onTouchStart, { passive: false });
    _canvas.addEventListener('touchmove',  onTouchMove,  { passive: false });
    _canvas.addEventListener('touchend',   onTouchEnd,   false);
    document.addEventListener('keydown', function (e) {
      if ((e.key === 'e' || e.key === 'E') && !e.repeat) doInteract();
    });
    var interactBtn = document.getElementById('fps-interact');
    if (interactBtn) interactBtn.addEventListener('pointerdown', doInteract);
  }

  function unbindInput() {
    document.removeEventListener('keydown',   onKeyDown);
    document.removeEventListener('keyup',     onKeyUp);
    document.removeEventListener('mousemove', onMouseMove);
    document.removeEventListener('mouseup',   onMouseUp);
    if (_canvas) {
      _canvas.removeEventListener('mousedown',  onMouseDown);
      _canvas.removeEventListener('wheel',      onWheel);
      _canvas.removeEventListener('touchstart', onTouchStart);
      _canvas.removeEventListener('touchmove',  onTouchMove);
      _canvas.removeEventListener('touchend',   onTouchEnd);
    }
  }

  function onKeyDown(e) { keys[e.code] = true; }
  function onKeyUp(e)   { keys[e.code] = false; }

  function onMouseDown(e) {
    if (e.button === 0) { mouseDown = true; lastMX = e.clientX; lastMY = e.clientY; e.preventDefault(); }
  }
  function onMouseMove(e) {
    if (!mouseDown) return;
    var dx = e.clientX - lastMX;
    var dy = e.clientY - lastMY;
    lastMX = e.clientX; lastMY = e.clientY;
    camYaw   -= dx * 0.0038;
    camPitch  = clamp(camPitch + dy * 0.0038, CAM_MIN_PITCH, CAM_MAX_PITCH);
  }
  function onMouseUp() { mouseDown = false; }
  function onWheel(e) {
    e.preventDefault();
    camDist = clamp(camDist + e.deltaY * 0.018, CAM_MIN_DIST, CAM_MAX_DIST);
  }

  // Touch — dual joystick zones
  function onTouchStart(e) {
    e.preventDefault();
    var rect = _canvas.getBoundingClientRect();
    var midX = rect.width / 2;
    Array.from(e.changedTouches).forEach(function (t) {
      var tx = t.clientX - rect.left, ty = t.clientY - rect.top;
      if (tx < midX && !joyMove.active) {
        joyMove.active = true; joyMove.id = t.identifier; joyMove.ox = tx; joyMove.oy = ty;
      } else if (tx >= midX && !joyLook.active) {
        joyLook.active = true; joyLook.id = t.identifier; joyLook.ox = tx; joyLook.oy = ty;
      }
    });
  }
  function onTouchMove(e) {
    e.preventDefault();
    var rect = _canvas.getBoundingClientRect();
    Array.from(e.changedTouches).forEach(function (t) {
      var tx = t.clientX - rect.left, ty = t.clientY - rect.top;
      if (t.identifier === joyMove.id) {
        var dx = tx - joyMove.ox, dy = ty - joyMove.oy;
        var m  = Math.sqrt(dx * dx + dy * dy);
        if (m > 50) { dx = dx / m * 50; dy = dy / m * 50; }
        joyMove.x = dx / 50; joyMove.y = dy / 50;
        moveKnob('joy-move-knob', dx, dy);
      } else if (t.identifier === joyLook.id) {
        var ddx = tx - joyLook.ox, ddy = ty - joyLook.oy;
        var mm  = Math.sqrt(ddx * ddx + ddy * ddy);
        if (mm > 50) { ddx = ddx / mm * 50; ddy = ddy / mm * 50; }
        joyLook.x = ddx / 50; joyLook.y = ddy / 50;
        moveKnob('joy-look-knob', ddx, ddy);
      }
    });
  }
  function onTouchEnd(e) {
    Array.from(e.changedTouches).forEach(function (t) {
      if (t.identifier === joyMove.id) { joyMove.active = false; joyMove.x = 0; joyMove.y = 0; moveKnob('joy-move-knob', 0, 0); }
      if (t.identifier === joyLook.id) { joyLook.active = false; joyLook.x = 0; joyLook.y = 0; moveKnob('joy-look-knob', 0, 0); }
    });
  }
  function moveKnob(id, dx, dy) {
    var el = document.getElementById(id);
    if (el) el.style.transform = 'translate(calc(-50% + ' + dx + 'px), calc(-50% + ' + dy + 'px))';
  }

  /* ═══════════════════════════════════════════
     INTERACTION
  ═══════════════════════════════════════════ */
  function doInteract() {
    if (nearbyNPC) {
      var def = nearbyNPC.userData.def;
      if (_cbNPCInteract) _cbNPCInteract(def.id, def.name, def.action);
    } else if (nearbyVehicle && !inVehicle) {
      inVehicle = nearbyVehicle;
      playerBodyMesh.visible   = false;
      window.NHWorld._inVehicle = true;
      toast('Hold W to drive · E to exit');
    } else if (inVehicle) {
      playerGroup.position.x   = inVehicle.position.x + 3.2;
      playerGroup.position.z   = inVehicle.position.z;
      inVehicle                = null;
      playerBodyMesh.visible   = true;
      window.NHWorld._inVehicle = false;
    }
  }

  function toast(msg) {
    if (typeof window.showToast === 'function') window.showToast(msg);
  }

  /* ═══════════════════════════════════════════
     ANIMATION LOOP
  ═══════════════════════════════════════════ */
  var _legsPhase = 0;

  function animate() {
    animFrameId = requestAnimationFrame(animate);
    if (!isRunning || !renderer) return;
    var dt = Math.min(clock.getDelta(), 0.05);
    _bobT      += dt * 7;
    _waveT     += dt * 1.2;
    _alertT    += dt;

    updateMovement(dt);
    updateNPCIdle(dt);
    updatePoliceAlert();
    updateRotLights(dt);
    updateProximityUI();
    updateCameraPosition();

    renderer.render(scene, camera);
  }

  /* ═══════════════════════════════════════════
     MOVEMENT
  ═══════════════════════════════════════════ */
  function updateMovement(dt) {
    if (!playerGroup) return;

    // Camera rotation from keyboard Q/Z (alternative to mouse drag)
    if (keys['KeyQ']) camYaw += dt * 2;
    if (keys['KeyZ']) camYaw -= dt * 2;

    // Camera rotation from right joystick
    camYaw   -= joyLook.x * dt * 2.8;
    camPitch  = clamp(camPitch + joyLook.y * dt * 1.6, CAM_MIN_PITCH, CAM_MAX_PITCH);

    var fwd   = keys['KeyW'] || keys['ArrowUp']    ? 1 : 0;
    var back  = keys['KeyS'] || keys['ArrowDown']  ? 1 : 0;
    var left  = keys['KeyA'] || keys['ArrowLeft']  ? 1 : 0;
    var right = keys['KeyD'] || keys['ArrowRight'] ? 1 : 0;
    var sprint= keys['ShiftLeft'] || keys['ShiftRight'] ? 1 : 0;

    // Merge keyboard + left joystick
    var mx = (right - left) + joyMove.x;
    var mz = (back  - fwd)  + joyMove.y;
    var mag = Math.sqrt(mx * mx + mz * mz);

    if (inVehicle) {
      // === VEHICLE DRIVING ===
      var vSpeed = (fwd || joyMove.y < -0.12)  ? (sprint ? 18 : 12)
                 : (back || joyMove.y > 0.12)  ? -5 : 0;
      var steer  = (right - left + joyMove.x);
      if (vSpeed !== 0) inVehicle.rotation.y -= steer * dt * 1.6;

      var vdir = inVehicle.rotation.y;
      var nvx  = inVehicle.position.x - Math.sin(vdir) * vSpeed * dt;
      var nvz  = inVehicle.position.z - Math.cos(vdir) * vSpeed * dt;
      if (!hitting(nvx, nvz, 1.5)) {
        inVehicle.position.x = nvx;
        inVehicle.position.z = nvz;
      }
      playerGroup.position.copy(inVehicle.position);
      // Smooth camera behind vehicle
      var targetYaw = inVehicle.rotation.y + Math.PI;
      var d = targetYaw - camYaw;
      while (d >  Math.PI) d -= Math.PI * 2;
      while (d < -Math.PI) d += Math.PI * 2;
      camYaw += d * dt * 1.2;

    } else if (mag > 0.06) {
      // === ON FOOT ===
      var spd   = (sprint || mag > 0.75) ? 11 : 5.5;
      var nx    = mx / Math.max(mag, 1);
      var nz    = mz / Math.max(mag, 1);

      // Camera-relative direction
      var wx = nx * Math.cos(camYaw) + nz * Math.sin(camYaw);
      var wz = nz * Math.cos(camYaw) - nx * Math.sin(camYaw);

      var newX = playerGroup.position.x + wx * spd * dt;
      var newZ = playerGroup.position.z + wz * spd * dt;
      if (!hitting(newX, playerGroup.position.z, 0.38)) playerGroup.position.x = newX;
      if (!hitting(playerGroup.position.x, newZ, 0.38)) playerGroup.position.z = newZ;

      // Rotate player to face direction
      var td = Math.atan2(wx, wz);
      var cd = playerGroup.rotation.y;
      var dd = td - cd;
      while (dd >  Math.PI) dd -= Math.PI * 2;
      while (dd < -Math.PI) dd += Math.PI * 2;
      playerGroup.rotation.y += dd * 12 * dt;

      // Leg walking bob
      _legsPhase += dt * (spd > 8 ? 14 : 9);
      if (playerBodyMesh) playerBodyMesh.position.y = 1.05 + Math.sin(_legsPhase) * 0.045;
    }
  }

  function hitting(x, z, r) {
    for (var i = 0; i < colliders.length; i++) {
      var c = colliders[i];
      if (x + r > c.minX && x - r < c.maxX && z + r > c.minZ && z - r < c.maxZ) return true;
    }
    return false;
  }

  /* ═══════════════════════════════════════════
     NPC IDLE ANIMATION
  ═══════════════════════════════════════════ */
  function updateNPCIdle() {
    npcObjects.forEach(function (npc, i) {
      var phase = _waveT + i * 1.3;
      npc.rotation.y = Math.sin(phase * 0.5) * 0.35;
      if (npc.userData.ring) {
        npc.userData.ring.material.opacity = 0.3 + Math.sin(phase * 2.2) * 0.22;
      }
    });
  }

  /* ═══════════════════════════════════════════
     POLICE ALERT
  ═══════════════════════════════════════════ */
  function updatePoliceAlert() {
    if (_alertT < 9) return;
    _alertT = 0;
    var cop = vehicleObjects.find ? vehicleObjects.find(function (v) { return v.userData.def.isPolice; })
                                  : null;
    if (!cop || !playerGroup) return;
    var dx = playerGroup.position.x - cop.position.x;
    var dz = playerGroup.position.z - cop.position.z;
    if (Math.sqrt(dx * dx + dz * dz) < 14) toast('⚠️ Police nearby — stay cool');
  }

  /* ═══════════════════════════════════════════
     ROTATING LIGHTS (police / building)
  ═══════════════════════════════════════════ */
  function updateRotLights(dt) {
    _rotLights.forEach(function (rl) {
      rl.phase += dt * 5.5;
      var a = Math.sin(rl.phase) > 0;
      if (rl.lights[0]) rl.lights[0].intensity = a ? 2.5 : 0.1;
      if (rl.lights[1]) rl.lights[1].intensity = a ? 0.1 : 2.5;
    });
  }

  /* ═══════════════════════════════════════════
     PROXIMITY UI (interact prompt)
  ═══════════════════════════════════════════ */
  function updateProximityUI() {
    if (!playerGroup) return;
    var px = playerGroup.position.x;
    var pz = playerGroup.position.z;

    nearbyNPC = null;
    var best = INTERACT_RANGE;
    npcObjects.forEach(function (n) {
      var d = dist2d(px, pz, n.position.x, n.position.z);
      if (d < best) { best = d; nearbyNPC = n; }
    });

    nearbyVehicle = null;
    if (!inVehicle) {
      var bestV = INTERACT_RANGE * 1.5;
      vehicleObjects.forEach(function (v) {
        if (v.userData.def.isPolice) return;
        var d = dist2d(px, pz, v.position.x, v.position.z);
        if (d < bestV) { bestV = d; nearbyVehicle = v; }
      });
    }

    var prompt = document.getElementById('fps-interact');
    var label  = document.getElementById('fps-interact-label');
    if (!prompt) return;

    if (nearbyNPC) {
      prompt.hidden = false;
      if (label) label.textContent = nearbyNPC.userData.action;
    } else if (nearbyVehicle) {
      prompt.hidden = false;
      if (label) label.textContent = 'Enter Vehicle';
    } else if (inVehicle) {
      prompt.hidden = false;
      if (label) label.textContent = 'Exit Vehicle  (E)';
    } else {
      prompt.hidden = true;
    }
  }

  /* ═══════════════════════════════════════════
     CAMERA UPDATE (third-person orbit)
  ═══════════════════════════════════════════ */
  function updateCameraPosition() {
    if (!playerGroup || !camera) return;
    var px = playerGroup.position.x;
    var py = playerGroup.position.y;
    var pz = playerGroup.position.z;
    var targetY = py + 1.55;

    var cx = px + camDist * Math.sin(camYaw) * Math.cos(camPitch);
    var cy = targetY   + camDist * Math.sin(camPitch);
    var cz = pz + camDist * Math.cos(camYaw) * Math.cos(camPitch);

    // Prevent camera from going underground
    if (cy < 0.5) cy = 0.5;

    camera.position.set(cx, cy, cz);
    camera.lookAt(px, targetY, pz);
  }

  /* ═══════════════════════════════════════════
     HELPERS
  ═══════════════════════════════════════════ */
  function clamp(v, lo, hi) { return v < lo ? lo : v > hi ? hi : v; }
  function dist2d(ax, az, bx, bz) { var dx = ax - bx, dz = az - bz; return Math.sqrt(dx * dx + dz * dz); }

})();
