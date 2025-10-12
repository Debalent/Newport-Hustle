# 🎮 Newport Hustle - Unity Setup Guide

## ✅ Project Files Created

Your Newport Hustle project now has the essential Unity files:

- ✅ **ProjectSettings/ProjectVersion.txt** - Unity 2021.3 LTS configured
- ✅ **ProjectSettings/ProjectSettings.asset** - Game settings (mobile optimized)
- ✅ **Assets/Scenes/MainScene.unity** - Starter scene with camera and lighting
- ✅ **All C# Scripts** - Complete game code ready to use

## 📋 Next Steps After Unity Hub Installs

### Step 1: Complete Unity Hub Installation
The installer should launch automatically when download completes.
- Follow the on-screen prompts
- Choose installation location (default is fine)
- Click "Finish" when done

### Step 2: Launch Unity Hub
1. Open **Unity Hub** from your Start Menu
2. **Sign in** or create a free Unity account
3. Go to **Settings** → **Licenses**
4. Click **"Add"** → **"Get a free personal license"**

### Step 3: Install Unity 2021.3 LTS
1. Click the **"Installs"** tab
2. Click **"Install Editor"**
3. Choose **"Unity 2021.3 LTS"** (Long Term Support)
4. Select these modules:
   - ☑️ **Android Build Support**
   - ☑️ **Documentation**
   - ☑️ **Microsoft Visual Studio Community** (if not installed)

5. Click **"Install"** (This will take 10-20 minutes)

### Step 4: Open Newport Hustle Project
1. Go to the **"Projects"** tab in Unity Hub
2. Click **"Add"** (or "Open")
3. Navigate to:
   ```
   C:\Users\Admin\OneDrive\Documents\Newport-Hustle\NewportHustleGame
   ```
4. Select the **NewportHustleGame** folder
5. Click **"Select Folder"**

Unity will now import your project (first time takes 5-10 minutes)

### Step 5: See Your UI! 🎨

Once Unity opens:

1. **Open the Main Scene:**
   - In the Project panel (bottom), navigate to `Assets/Scenes`
   - Double-click `MainScene.unity`

2. **Add UI Components:**
   - Right-click in the Hierarchy panel
   - Select: **UI → Canvas**
   - Right-click on Canvas
   - Add **UI → Button**, **UI → Image**, etc.

3. **Attach Your Scripts:**
   - Drag scripts from `Assets/Scripts/UI/` onto GameObjects
   - Example: Drag `MenuNavigation.cs` onto a GameObject

4. **Press Play (▶️):**
   - Click the Play button at the top
   - Your game will run in the Game view!

## 🎨 Creating the Newport Hustle UI

### To Create the Main Menu:

1. **Create Canvas:**
   - Hierarchy → Right-click → UI → Canvas
   - Canvas will auto-create EventSystem

2. **Add Logo:**
   - Right-click Canvas → UI → Image
   - Drag `Newport_Hustle_Logo.png` from Assets/UI/Branding to the Image component

3. **Add Menu Buttons:**
   - Right-click Canvas → UI → Button
   - Rename to "NewGameButton"
   - Duplicate for Continue, Settings, Quit buttons

4. **Attach MenuNavigation Script:**
   - Select Canvas
   - In Inspector, click "Add Component"
   - Search for "Menu Navigation"
   - Drag buttons from Hierarchy to script's button slots

5. **Press Play to Test!**

### To Create the Mobile HUD:

1. **Create HUD Canvas:**
   - Hierarchy → Right-click → UI → Canvas
   - Rename to "Mobile HUD"

2. **Add Joystick:**
   - Right-click Mobile HUD → UI → Image
   - Rename to "MovementJoystick"
   - Attach the `Joystick.cs` script

3. **Add Health Bar:**
   - Right-click Mobile HUD → UI → Slider
   - Position in top-left corner

4. **Add Money/Stats Text:**
   - Right-click Mobile HUD → UI → Text - TextMeshPro
   - Position for displaying player stats

5. **Attach HUDController Script:**
   - Select Mobile HUD
   - Add the `HUDController.cs` component
   - Link all UI elements in the Inspector

## 🚀 Running the Game

1. **Make sure a scene is open** (MainScene.unity)
2. **Click the Play button** (▶️) at the top center
3. **Game will run** in the Game view
4. **Click Play again** to stop

## 📱 Building for Mobile (After Setup)

### For Android:
1. **File → Build Settings**
2. Select **Android**
3. Click **"Switch Platform"**
4. Click **"Build"**
5. Choose save location
6. Install APK on Android device

## 🔧 Troubleshooting

**Q: Unity Hub installer didn't launch?**
- Check: `C:\Users\Admin\AppData\Local\Temp\UnityHubSetup.exe`
- Double-click to run manually

**Q: Project won't open?**
- Make sure you selected the **NewportHustleGame** folder (not the parent folder)
- Unity needs the folder with ProjectSettings inside

**Q: Scripts have errors?**
- Unity might need to download packages
- Let it finish "Importing..." at the bottom of the screen

**Q: Can't see UI elements?**
- Switch to **Game view** (tab next to Scene view)
- Make sure Canvas is in the scene
- Check Camera has "UI" layer in Culling Mask

## 📚 Learning Resources

- **Unity Learn:** [learn.unity.com](https://learn.unity.com)
- **UI Documentation:** [docs.unity3d.com/Manual/UISystem.html](https://docs.unity3d.com/Manual/UISystem.html)
- **Mobile Development:** [docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html](https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html)

## 🎯 Your Project Status

✅ **Complete C# Scripts** - All game systems coded
✅ **Unity Project Files** - Ready to open
✅ **Mobile Optimization** - Configured for Android/iOS
✅ **Documentation** - Comprehensive guides included

**You're ready to bring Newport Hustle to life!** 🚀

---

*Need help? Check the Unity forums or the Unity documentation linked above!*
