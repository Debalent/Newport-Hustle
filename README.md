# Newport Hustle

> *From barber's chair to city hall — hustle your way up through five bosses and reclaim Newport.*

**[► Play the Live Demo ◄](https://debalent.github.io/Newport-Hustle/WebPreview/)**  
Mobile-ready · Works in any browser · No install required

---

## What Is Newport Hustle?

Newport Hustle is a GTA-style open-world mobile game set in the real city of Newport, Arkansas. You play as **Ace** — a prodigal hustler returning home — navigating eight explorable districts, clashing with five power factions, and ultimately deciding the fate of your hometown.

It's character-driven drama with humor, heart, and escalating stakes across four acts. Think small-town politics, street-level economics, a crypto bro with a belt-buckle obsession, a self-help cult on the Spa District side of town, and a Mayor who's been pulling strings the whole time.

---

## The Story

### Act 1 — The Come Up
Ace rolls back into Newport and reconnects with **Mentor** at Ace's Barbershop on Barbers Row. He earns the community's respect, proves his skills, and gets swept into the orbit of **Marcus DeVille** — a charismatic hustler running a street brand out of his namesake district. The act ends with a DeVille Block Party that puts Ace on the map.

### Act 2 — The Grind
Marcus's empire cracks under a crypto collapse. Meanwhile Ace is drawn into conflicts with three other factions operating across Newport: a social-media influencer building a digital empire downtown, a tow-truck kingpin who runs the Diaz District, and a wellness guru whose Enlightenment Center is more cult than clinic. A leak exposes that the Mayor has been funding all of it.

### Act 3 — The War
Boss confrontations across every district. The DeVille Flagship. LexxCon. Rico's impound compound. The Enlightenment Center. Each fight settles a faction — and each defeated boss can become an unlikely ally if Ace showed them mercy.

### Act 4 — The Reckoning
Ace rallies Newport against **Mayor Derek Shonn**, the man behind every power play in town. Storm City Hall. Face the final boss on Election Day. Three possible endings determine whether Newport gets a new kind of leader — or falls back into the same hands.

---

## Characters

### Ace *(Player Character)*
Returned to Newport after time away. Street-smart, community-rooted, and standing at a crossroads between who he was and who Newport needs him to be. Fully customizable — complexion, outfit, personality trait, and street name.

### Mentor
The old-guard barber who kept Barbers Row standing while everyone else left or sold out. Ace's moral compass and Act 1 anchor. His slow nod of approval is worth more than any paycheck.

### Marcus DeVille
Charismatic, gold-trimmed, Cadillac-driving entrepreneur who built the DeVille brand on Newport street culture. Flashy but fragile — his empire is built on hype and crypto that's about to collapse. Ally in Act 1, unstable rival in Act 2, boss fight in Act 3.

### Lexi "The Algorithm" Vance
Influencer, content strategist, and digital mercenary operating out of Downtown. She filmed Ace without consent and turned it into content. Her weapons are drones, bot armies, and T-shirt cannons. Boss fight happens at LexxCon.

### Big Rico "The Repo King"
Runs Rico's Fleet out of the Diaz District. His tow trucks are everywhere, his impound compound is a fortress, and his pet monster truck "Deborah" is the Act 3 boss vehicle. More blunt instrument than chess player, but loyal to whoever last crossed him.

### Dr. Fable
Wellness influencer and self-appointed spiritual leader of the Spa District's Enlightenment Center. The flyers around town look friendly. The supplement line is less friendly. The acolyte swarms and incense traps during the raid are definitely not friendly.

### Mayor Derek Shonn
The true final boss. Old-money Newport establishment, publicly neutral, privately funding every faction to keep the city divided and controllable. The evidence trail leads to City Hall. The Election Day showdown determines whether Newport changes or doesn't.

---

## The City — Newport, AR

Eight explorable districts built on the real geography of Newport, Arkansas:

| District | Description |
|---|---|
| **Barbers Row** | Ace's home turf. Community anchor. Mentor's barbershop. |
| **DeVille District** | Marcus's territory. Flagship store, print shop, DeVille Coin kiosk. |
| **Downtown Newport** | City Hall, commercial strip, Lexi's content studio. |
| **Spa District** | Dr. Fable's Enlightenment Center and wellness empire. |
| **Diaz District** | Big Rico's tow yards and impound compound. |
| **Jacksonport Area** | Ace's safehouse. Quieter side of the map. |
| **Residential** | Where everybody actually lives. Mrs. Henderson watches from her porch. |
| **Riverfront** | The White River. History. The final convergence of everything. |

The in-game map uses real Newport geography — White River to the east, Rocky Bayou to the northeast, Malcolm Avenue as the north-south spine, and the C&N Railroad running diagonally through the middle.

---

## Gameplay

- **Open-world movement** — third-person camera, WASD on desktop, dual joystick on mobile
- **22 story missions** across 4 acts with branching boss outcomes
- **4 story paths** — Street Hustler, Corporate Snake, Influencer Gone Wrong, Chaos Mode — each with its own tone
- **Character creator** — complexion, outfit, personality trait ("Big Talker," "Too Smooth," "Chaotic Good," etc.)
- **5-star wanted system** — Newport PD escalates from a friendly chat to a county-wide incident
- **Faction respect meters** — how you treat each district affects what help you can call in during Act 4
- **Three endings** — determined by choices made across the full playthrough

---

## Try the Demo

**[► Live Web Demo ◄](https://debalent.github.io/Newport-Hustle/WebPreview/)**

The web demo is a fully interactive preview running in the browser — no Unity, no install. It includes:

- Animated splash screen and main menu
- Character creator with live preview
- Story mode and mission select (real mission data)
- Playable Three.js 3D world with NPC interaction, dual joystick mobile controls, and vehicle entry
- Full GTA-style HUD — minimap, cash counter, wanted stars, health/armor/rep bars
- Newport AR mission map with real geography
- Pause menu, settings, and back-to-menu navigation from anywhere

Works on iOS, Android, and desktop browsers.

---

## Project Status

| Component | Status |
|---|---|
| Web demo (browser playable) | ✅ Complete |
| Story & mission design (22 missions, 4 acts) | ✅ Complete |
| Character roster & boss arcs | ✅ Complete |
| Newport AR map & district design | ✅ Complete |
| Game configuration (JSON data layer) | ✅ Complete |
| Unity mobile build | 🔧 In development |
| NPC dialogue trees | 🔧 In development |
| Mission gameplay implementation | 🔧 In development |
| Community outreach & permissions | 📋 Planned pre-release |

---

## Technical Overview

- **Web Demo:** Pure HTML/CSS/JS + Three.js — no framework, no bundler
- **Game Engine:** Unity 2021.3 LTS
- **Platform targets:** iOS, Android
- **Language:** C# (game scripts) + JSON (mission/character/vehicle configuration)
- **Architecture:** Data-driven — missions, characters, vehicles, and story arcs all live in JSON config files separate from code

**Key scripts:**
- [GameManager.cs](NewportHustleGame/Scripts/Core/GameManager.cs) — core game loop
- [VehicleController.cs](NewportHustleGame/Scripts/Vehicles/VehicleController.cs) — vehicle physics
- [PoliceSystem.cs](NewportHustleGame/Scripts/World/PoliceSystem.cs) — wanted level AI
- [DialogueSystem.cs](NewportHustleGame/Scripts/Characters/DialogueSystem.cs) — NPC conversation engine
- [world.js](WebPreview/world.js) — Three.js open-world engine (web demo)
- [app.js](WebPreview/app.js) — UI navigation and screen system (web demo)

---

## Community & Respect

Newport Hustle is built out of genuine appreciation for Newport, Arkansas — its people, its geography, its culture, and its particular brand of small-town life. The game celebrates the city; it doesn't mock it.

Before any public release, all business references and community representations will go through proper consent and permission processes. Until then, this is a development demo and portfolio showcase.

---

## Run It Locally

**Web demo:**
```
# No server needed — just open WebPreview/index.html in any browser
# Or serve it:
cd WebPreview
npx serve .
```

**Unity project:**
1. Install [Unity Hub](https://unity.com/download) + Unity 2021.3 LTS
2. Open `NewportHustleGame/` as a Unity project
3. Press Play

Full setup guide: [UNITY_SETUP_GUIDE.md](UNITY_SETUP_GUIDE.md)

---

*Made with genuine love for Newport, Arkansas*  
*© 2025 Newport Hustle — All story characters are fictional*


### 🤣 What is Newport Hustle?

Ever wondered what Grand Theft Auto would look like if it took place in Newport, Arkansas? Wonder no more! Newport Hustle is the utterly ridiculous mobile game that brings big-city crime drama to small-town charm. It's like GTA, but instead of stealing supercars, you're "borrowing" Box Chevys, and instead of running from SWAT teams, you're being chased by Officer Johnson in his Crown Vic who probably went to high school with your cousin.

### 🏪 **Featuring REAL Newport Businesses!**

- **Jordan's Gas Station** - Where you fuel up and catch up on town gossip
- **Lackey Tamale Shop** - Best tamales this side of the White River (health restoration included!)
- **The Yella Store** - That bright yellow tobacco shop you can see from space

*If you live in Newport and don't laugh at this game, we'll give you your money back!*

---

## 🚗 Vehicle Collection (With Hilarious Knock-Off Names)

Because copyright lawyers are scarier than the Newport PD:

### Classic Rides

- **Riverside Classic** *(Box Chevy Caprice)* - Floats like a boat, turns like a battleship
- **Newport Supreme** *(1984 Cutlass Supreme)* - The car your dad wishes he still had
- **Arkansas Explorer** *(Chevy Tahoe)* - Soccer mom special with room for the whole family

### Luxury Options

- **Presidential SUV** *(Lincoln Navigator)* - For when you need to feel important in a town of 7,000
- **Luxury Crossover** *(Lexus RX)* - Because even small towns have people with taste

### Performance Cars

- **Arkansas Thunder** - Loud enough to wake up the whole neighborhood
- **Newport Lightning** - Faster than gossip spreading at the Dollar General

### Two-Wheelers

- **Newport Cruiser** *(Harley-style)* - Middle-age crisis on two wheels
- **Street Demon** *(Sport Bike)* - For when you need to outrun your responsibilities

---

## 👥 **Street Characters That Make Newport REAL**

### The Holy Trinity of Small-Town Authenticity

#### 🕵️ **Snitches** - "Neighborhood Informants"

- Rides: Beat-up BMX bikes
- Superpower: Somehow knowing everyone's business
- Dialogue: *"I seen everything that happened over there"*
- **Warning:** They WILL call the cops on you

#### 🍷 **Winos** - "Neighborhood Philosophers"

- Rides: Rusty beach cruiser bikes
- Superpower: Deep life wisdom after 2 PM
- Dialogue: *"Life's like riding a bike, sometimes you wobble"*
- **Bonus:** Actually gives good advice (sometimes)

#### 🚲 **Bums** - "Traveling Entrepreneurs"

- Rides: Mountain bikes with baskets full of "treasures"
- Superpower: Knows every shortcut in town
- Dialogue: *"One man's trash is another man's treasure"*
- **Pro Tip:** They'll trade you useful stuff for pocket change

---

## 🚔 **Police System**

The Newport PD operates on a sophisticated 4-star wanted system:

⭐ **Level 1:** Officer Davis pulls you over for a friendly chat  
⭐⭐ **Level 2:** Now you got TWO Crown Vics following you  
⭐⭐⭐ **Level 3:** Sheriff's department joins the party  
⭐⭐⭐⭐ **Level 4:** The whole county's involved (all 6 patrol cars)

Police Vehicles: "Authority Sedan" (Crown Vic) and "Patrol Classic" (Grand Marquis) — because they bought them at auction and they're LOUD

---

## 🎯 **Game Features**

### ✅ **What We Got:**

- **Authentic Newport Streets** - Drive down the REAL Main Street
- **Mobile-Optimized Controls** - Touch steering that actually works
- **Community-Driven Characters** - Based on real (consenting) Newport folks
- **Cultural Respect** - We love this town and it shows
- **Small-Town Humor** - Inside jokes only locals will get
- **GTA-Style Gameplay** - But with more politeness and sweet tea

### 🚧 **Coming Soon:**

- Fishing mini-games at the White River
- Annual Tractor Pull tournament mode
- Walmart parking lot social events
- Tornado warning survival challenges

---

## 💼 **Portfolio Project - Source Code Available**

### 🔍 **This is a Development Showcase**

This repository demonstrates Unity game development skills through a complete GTA-style mobile game implementation. All source code is available for review by potential employers or collaborators.

### 🛠️ **Technical Implementation**

- **Engine**: Unity 2021.3 LTS
- **Platform**: Mobile (Android/iOS)
- **Languages**: C# (2000+ lines of game code)
- **Architecture**: Modular system design with JSON configuration

### 📂 **Key Components to Review**

- **[`Scripts/Vehicles/VehicleController.cs`](NewportHustleGame/Scripts/Vehicles/VehicleController.cs)** - Complete vehicle physics system
- **[`Scripts/World/PoliceSystem.cs`](NewportHustleGame/Scripts/World/PoliceSystem.cs)** - AI behavior and wanted system
- **[`Scripts/World/ZoneManager.cs`](NewportHustleGame/Scripts/World/ZoneManager.cs)** - Open-world city management
- **[`Config/VehicleDatabase.json`](NewportHustleGame/Config/VehicleDatabase.json)** - Data-driven game configuration

### 🎨 **View the UI/UX System**

Want to see the game interface? Check out the complete UI implementation:

- **[View UI Scripts →](NewportHustleGame/Scripts/UI/)** - All UI components (HUD, Menus, Mobile Controls)
- **[MenuNavigation.cs](NewportHustleGame/Scripts/UI/MenuNavigation.cs)** - Complete menu system with settings
- **[HUDController.cs](NewportHustleGame/Scripts/UI/HUDController.cs)** - Mobile HUD with stats, missions, and location
- **[Joystick.cs](NewportHustleGame/Scripts/UI/Joystick.cs)** - Touch-based mobile controls
- **[BrandingManager.cs](NewportHustleGame/Scripts/UI/BrandingManager.cs)** - App store assets and branding

#### 🌐 Web UI Preview — Live on GitHub Pages

A fully interactive, mobile-style preview of the Newport Hustle UI is available in the repository and hosted via GitHub Pages:

**👉 [View Live UI Preview →](https://debalent.github.io/Newport-Hustle/WebPreview/)**

The preview includes **10 navigable screens**:

| Screen | Description |
| --- | --- |
| Splash | Animated logo entry |
| Main Menu | Continue / New Game / Settings |
| Mission Select | All 4 story missions (real data from MissionData.json) |
| HUD | In-game display with wanted stars, objectives, map button |
| Pause Menu | Resume / Map / Character / Quit |
| Map | Zones: Downtown, Barber's Row, Spa District, Residential, Diaz District, Riverfront |
| Dialogue | NPC conversation with branching choices (Mentor: Marcus) |
| Character Creator | Stats bars, style and ride selection |
| Wanted Level | 4-star police alert with escape tips |
| Settings | Volume sliders and graphics quality |

To re-deploy or fork on GitHub Pages: Settings → Pages → Source: `main` branch, root folder → Save.

**🎮 Want to Run It Yourself?**

1. **Clone this repository**
2. **Install Unity Hub** and Unity 2021.3 LTS
3. **Open the project** in Unity
4. **Press Play** to see the game in action!

📖 **[Full Setup Instructions →](UNITY_SETUP_GUIDE.md)**

### 🛠️🎯 **Development Skills Demonstrated**

- Mobile game architecture and optimization
- Complex physics and AI systems
- JSON-based configuration management
- Professional code organization and documentation
- Complete UI/UX implementation with mobile-first design

---

## 🤝 **Community Involvement**

This game is a **concept project** and **development portfolio piece** created with love and respect for Newport, Arkansas.

### **Current Status:**

- ✅ **Concept Development**: Complete game design and technical implementation
- 🚧 **Community Outreach**: Planning to approach local businesses and community
- 🎯 **Free Game**: Will be offered completely free to the community
- 💼 **Portfolio Project**: Demonstrates game development capabilities

### **How We Plan to Keep It Real:**

- 📧 Will contact all businesses for permission before any release
- 🤝 Character development only with community consent
- 🆓 **100% Free Game** - no monetization, pure community gift
- ❤️ **Respectful Representation** - celebrating Newport, not mocking it
- 📈 **Developer Portfolio** - showcasing technical and creative skills

### **Want to Get Involved?**

This is a **concept demonstration**. If you're a Newport business owner or community member interested in this becoming real:

Email: **[community@newporthustle.game](mailto:community@newporthustle.game)**  
*Currently seeking community input and permission for actual development*

---

## 🛠️ **Technical Stuff** *(For the Nerds)*

- **Engine:** Unity 2021.3 LTS
- **Platforms:** iOS, Android, Windows, Mac, Linux, Web
- **Language:** C# (over 2000 lines of pure Newport chaos)
- **Graphics:** Mobile-optimized 3D with authentic Arkansas scenery
- **Physics:** Realistic vehicle handling (Box Chevys really DO handle like boats)

---

## 📜 **Legal Disclaimer**

*Newport Hustle is a **concept/portfolio project** and parody game created as a demonstration of game development skills. This is currently a **technical proof-of-concept** and **not an official release**.*

*Any reference to real businesses (Jordan's Gas Station, Lackey Tamale Shop, The Yella Store) is done respectfully as part of a development concept. **No actual permissions have been obtained yet** - this would be required before any public release.*

*The game is designed as a **free community gift** with **no monetization**. The developer's goal is to use this project to demonstrate technical capabilities and potentially build a game development business.*

*If this concept becomes a real game, all business owners and community members would be contacted for permission and consent before any release.*

*The Newport, Arkansas depicted in this concept is enhanced for comedic effect but based on genuine appreciation for small-town community values.*

---

## 📞 **Contact & Support**

### 🎮 **Game Questions:**

- **Discord:** [Newport Hustle Community](https://discord.gg/newport-hustle)
- **Email:** [support@newporthustle.game](mailto:support@newporthustle.game)
- **Website:** [www.newporthustle.game](https://www.newporthustle.game)

### 🏪 **Local Business Inquiries:**

- **Email:** [business@newporthustle.game](mailto:business@newporthustle.game)
- **Phone:** (870) 555-GAME

### 🤝 **Community Relations:**

- **Email:** [community@newporthustle.game](mailto:community@newporthustle.game)
- **Office:** Newport Community Center (when we remember to show up)

---

## ⭐ **Concept Feedback** *(What People Might Say)*

*"I can't believe someone made a video game concept about our town!"*  
**- Potential Newport Resident**

*"If this was real, I'd love to drive to work the same way I do in real life!"*  
**- Future Jordan's Gas Station Customer**

*"My business could sell the best digital tamales in Arkansas!"*  
**- Potential Lackey Tamale Shop Partnership**

*"You could see my store from three blocks away, just like in real life!"*  
**- Potential The Yella Store Feature**

*"This developer really knows how to make small-town life fun!"*  
**- Game Development Portfolio Reviewer**

---

## 🎉 **Special Thanks**

**To the amazing people of Newport, Arkansas** - for inspiring this concept project. We hope to eventually get your blessing to make this real!

**To Jordan's Gas Station, Lackey Tamale Shop, and The Yella Store** - for being such iconic parts of Newport that they inspired this game concept. We'd love to feature you officially someday!

**To small-town communities everywhere** - for proving that the best stories come from real places with real heart.

**To the game development community** - for encouraging creative, community-focused projects that celebrate local culture.

---

### 🚗💨 Ready to Become Newport's Most Wanted?

**[► VIEW LIVE UI PREVIEW ◄](https://debalent.github.io/Newport-Hustle/WebPreview/)**

*Warning: May cause excessive laughter, nostalgia for small-town life, and an irresistible urge to visit Newport, Arkansas.*

---

*Made with ❤️, 🤣, and probably too much coffee*  
*© 2025 Newport Hustle Development Team*  
*Proudly made in partnership with Newport, Arkansas*
