# App Store Assets Guide for Newport Hustle

## Logo Integration for Mobile Platforms

### Source Logo

- **Location**: `Assets/UI/Branding/Newport_Hustle_Logo.png`
- **Format**: High-resolution PNG with transparency
- **Source File**: Original logo provided by user
- **Usage**: All loading screens, splash screens, and app store assets

### iOS App Store

1. **App Icon**
   - Size: 1024x1024 pixels
   - Format: PNG (no transparency)
   - Requirements: Square, no rounded corners (iOS handles this)
   - Location: App Store Connect submission

2. **Screenshots**
   - iPhone: 1290x2796 pixels (6.7" display)
   - iPad: 2048x2732 pixels (12.9" display)
   - Include Newport Hustle logo prominently in game UI
   - Show authentic Newport businesses (Jordan's Gas Station, Lackey Tamale Shop, The Yella Store)

3. **Privacy Policy**
   - Link to privacy policy required
   - Data collection practices disclosure
   - Newport community consent documentation

### Google Play Store

1. **App Icon**
   - Size: 512x512 pixels
   - Format: PNG (32-bit with transparency support)
   - Requirements: Follow Material Design guidelines
   - Adaptive icon support recommended

2. **Feature Graphic**
   - Size: 1024x500 pixels
   - Format: JPEG or PNG
   - Include Newport Hustle logo and key game elements
   - Showcase GTA-style gameplay and authentic Newport setting

3. **Screenshots**
   - Phone: 1080x1920 pixels minimum
   - Tablet: 1200x1920 pixels minimum
   - Show vehicle gameplay, police system, and Newport landmarks

## Logo Variations Required

### 1. Primary Logo

- **Usage**: Main game logo, splash screens, loading screens
- **Background**: Transparent PNG for overlay on game backgrounds
- **Size**: Scalable vector format preferred, minimum 2048px wide
- **Colors**: Full color version with Newport-appropriate color scheme

### 2. App Icon Version

- **Size**: 1024x1024 pixels
- **Background**: Solid color background (no transparency for iOS)
- **Design**: Simplified version that remains readable at small sizes
- **Text**: Minimal text, focus on iconic visual elements

### 3. Horizontal Banner

- **Aspect Ratio**: 16:9 or 5:2
- **Usage**: Feature graphics, promotional banners
- **Include**: Game title text alongside logo
- **Background**: Newport-themed imagery or solid color

### 4. Square Version

- **Aspect Ratio**: 1:1
- **Usage**: Social media, square promotional spaces
- **Layout**: Centered logo with balanced text placement
- **Background**: Consistent with brand colors

### 5. Vertical/Portrait

- **Aspect Ratio**: 9:16
- **Usage**: Mobile-optimized promotional materials
- **Layout**: Stacked logo and text for vertical spaces
- **Background**: Newport skyline or community imagery

## Asset Creation Checklist

### Required Assets to Create from Newport Hustle Logo

#### iOS App Store Package

- [ ] App Icon: 1024x1024 PNG (no transparency)
- [ ] iPhone Screenshots: 1290x2796 pixels
- [ ] iPad Screenshots: 2048x2732 pixels
- [ ] Apple Watch Icon: 1024x1024 pixels (if applicable)

#### Google Play Store Package

- [ ] App Icon: 512x512 PNG (with transparency)
- [ ] Feature Graphic: 1024x500 pixels
- [ ] Phone Screenshots: 1080x1920 pixels minimum
- [ ] Tablet Screenshots: 1200x1920 pixels minimum
- [ ] TV Banner: 1280x720 pixels (if applicable)

#### In-Game Assets

- [ ] Splash Screen Logo: 2048x2048 PNG (transparent)
- [ ] Loading Screen Logo: 1920x1080 PNG
- [ ] Menu Background Logo: Various sizes
- [ ] HUD Logo Element: 256x256 PNG (small version)

#### Marketing Assets

- [ ] Horizontal Banner: 1920x1080 PNG
- [ ] Square Social Media: 1080x1080 PNG
- [ ] Vertical Mobile Banner: 1080x1920 PNG
- [ ] Website Header: 1920x600 PNG
- [ ] Press Kit Logo: 2048x2048 PNG (transparent)

## Logo Usage Guidelines

### Logo Usage Rules

1. **Minimum Size**: Logo should not be smaller than 32x32 pixels
2. **Clear Space**: Maintain clear space around logo equal to height of logo text
3. **Color Contrast**: Ensure sufficient contrast against backgrounds
4. **Aspect Ratio**: Never stretch or distort the logo proportions
5. **Readability**: Text must remain legible at all sizes

### Brand Colors (from BrandingManager)

- **Primary Blue**: RGB(51, 102, 204) / #3366CC
- **Secondary Green**: RGB(76, 153, 76) / #4C994C
- **Accent Gold**: RGB(255, 193, 7) / #FFC107
- **Background White**: RGB(248, 249, 250) / #F8F9FA
- **Text Dark**: RGB(33, 37, 41) / #212529

### Typography

- **Title Font**: Bold, modern sans-serif
- **Body Text**: Clean, readable sans-serif
- **Game UI**: Consistent with mobile game standards
- **Accessibility**: WCAG 2.1 AA compliant contrast ratios

## Implementation Process

### Step 1: Asset Preparation

1. Open original logo in image editor (Photoshop, GIMP, etc.)
2. Create master artboard at highest resolution needed
3. Prepare transparent background versions
4. Create solid background versions for specific requirements
5. Export in required formats and sizes

### Step 2: Unity Integration

1. Import assets to `Assets/UI/Branding/` folder
2. Configure texture import settings for mobile optimization
3. Set up sprite atlases for efficient memory usage
4. Test logo display across different screen resolutions
5. Implement in LoadingScreenController and SplashScreenController

### Step 3: Mobile Optimization

1. Test logo visibility on various screen sizes
2. Optimize file sizes for mobile download
3. Ensure crisp display on high-DPI screens
4. Verify loading performance impact
5. Test on actual devices (iOS and Android)

### Step 4: Store Submission Preparation

1. Create store listing screenshots featuring the logo
2. Prepare app icons in all required sizes
3. Generate feature graphics and promotional materials
4. Review platform-specific requirements
5. Submit for app store review process

## Quality Assurance Checklist

### Visual Testing

- [ ] Logo displays correctly on splash screen
- [ ] Loading screen shows logo without distortion
- [ ] Menu system incorporates logo appropriately
- [ ] App icon appears crisp on device home screens
- [ ] Screenshots showcase logo and game effectively

### Technical Testing

- [ ] Logo files load quickly on mobile devices
- [ ] Transparent backgrounds work correctly
- [ ] Color accuracy maintained across platforms
- [ ] File sizes optimized for mobile distribution
- [ ] Memory usage impact acceptable

### Platform Compliance

- [ ] iOS App Store guidelines met
- [ ] Google Play Store requirements satisfied
- [ ] Content rating appropriate for family-friendly game
- [ ] Privacy policy includes Newport community consent
- [ ] Trademark considerations addressed

## File Organization

### Directory Structure

```text
Assets/UI/Branding/
├── Newport_Hustle_Logo.png (original)
├── App_Icons/
│   ├── iOS_1024x1024.png
│   └── Android_512x512.png
├── Screenshots/
│   ├── iPhone/
│   ├── iPad/
│   ├── Android_Phone/
│   └── Android_Tablet/
├── Marketing/
│   ├── Feature_Graphic_1024x500.png
│   ├── Horizontal_Banner_1920x1080.png
│   ├── Square_Social_1080x1080.png
│   └── Vertical_Mobile_1080x1920.png
└── In_Game/
    ├── Splash_Logo_2048x2048.png
    ├── Loading_Logo_1920x1080.png
    └── HUD_Logo_256x256.png
```

### Naming Convention

- Use descriptive names with dimensions
- Include platform-specific identifiers
- Maintain consistent naming across assets
- Version control for iterative updates
- Backup original source files

## Newport Community Integration

### Authentic Representation

- Showcase Jordan's Gas Station in screenshots
- Feature Lackey Tamale Shop as community gathering point
- Highlight The Yella Store as distinctive yellow landmark
- Include authentic Newport street scenes
- Demonstrate respect for community participation

### Community Consent Documentation

- Include community outreach information in store listings
- Reference authentic business partnerships
- Highlight positive community representation
- Maintain transparency about character development process
- Provide community contact information for questions

## Conclusion

This App Store Assets Guide ensures that the Newport Hustle logo is properly implemented across all mobile platforms while maintaining brand consistency and meeting technical requirements. The logo serves as a visual bridge between the authentic Newport community and the mobile gaming experience, representing the collaborative spirit of the project.

By following these guidelines, the Newport Hustle game will present a professional and cohesive brand image that honors the Newport community while attracting players interested in authentic, community-driven gaming experiences.
