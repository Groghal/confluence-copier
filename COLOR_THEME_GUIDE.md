# 🎨 Cursor Dark Theme - Color System Guide

## Overview
Your Confluence Copier app now uses **Cursor's professional dark theme** - the same color palette used by the popular VSCode-based Cursor editor. All colors are defined in one central location (`App.axaml`), making it incredibly easy to customize the entire app's appearance instantly!

## 🔧 How to Change Colors

### 1. Open `App.axaml` 
All colors are defined in the `<Application.Resources>` section at the top of the file.

### 2. Modify Color Values
Change any of these core colors to instantly update the entire app:

```xml
<!-- BACKGROUND COLORS (Editor-Inspired) -->
<Color x:Key="WindowBackgroundColor">#FF1E1E1E</Color>        <!-- Main editor background (VSCode primary) -->
<Color x:Key="ControlBackgroundColor">#FF252526</Color>       <!-- Sidebar/panel background (VSCode secondary) -->
<Color x:Key="ButtonBackgroundColor">#FF2D2D30</Color>        <!-- Button default state (darker grey) -->
<Color x:Key="TitleBarStartColor">#FF3E3E42</Color>           <!-- Title bar gradient start (professional grey) -->
<Color x:Key="TitleBarEndColor">#FF2D2D30</Color>             <!-- Title bar gradient end (seamless blend) -->

<!-- TEXT COLORS (High Readability) -->
<Color x:Key="PrimaryTextColor">#FFCCCCCC</Color>             <!-- Main text (optimal contrast on dark) -->
<Color x:Key="SubtitleTextColor">#FF9DA5B4</Color>            <!-- Secondary text (subtle blue-grey) -->

<!-- BORDER COLORS (Subtle Definition) -->
<Color x:Key="BorderColor">#FF464647</Color>                  <!-- Default control borders (subtle separation) -->
<Color x:Key="FocusBorderColor">#FF007ACC</Color>             <!-- VSCode's signature blue accent -->
<Color x:Key="HoverBorderColor">#FF6C6C6C</Color>             <!-- Lighter borders on hover -->

<!-- INTERACTIVE STATES (Smooth Transitions) -->
<Color x:Key="ButtonHoverColor">#FF37373D</Color>             <!-- Subtle button hover (barely noticeable) -->
<Color x:Key="ButtonPressedColor">#FF262629</Color>           <!-- Clear button pressed feedback -->
<Color x:Key="TitleBarHoverColor">#FF474748</Color>           <!-- Title bar button hover -->

<!-- SPECIAL COLORS (Professional Accents) -->
<Color x:Key="CloseButtonHoverColor">#FFE81123</Color>        <!-- Standard close button red -->
<Color x:Key="WindowBorderColor">#FF454545</Color>            <!-- Window border (minimal but visible) -->
<Color x:Key="DialogBorderColor">#FF565656</Color>            <!-- Dialog border (slightly more prominent) -->

<!-- STATUS COLORS (Code Editor Style) -->
<Color x:Key="StatusReadyColor">#FF007ACC</Color>             <!-- VSCode's signature blue -->
<Color x:Key="StatusSuccessColor">#FF4EC9B0</Color>           <!-- Cyan-green (success/string color) -->
<Color x:Key="StatusWarningColor">#FFDCDCAA</Color>           <!-- Light yellow (warning/keyword color) -->
<Color x:Key="StatusErrorColor">#FFF44747</Color>             <!-- Bright red (error/invalid color) -->
```

## 🎯 Example Theme Variations

### Lighter Cursor Theme
```xml
<Color x:Key="WindowBackgroundColor">#FF2D2D30</Color>
<Color x:Key="ControlBackgroundColor">#FF3E3E42</Color>
<Color x:Key="PrimaryTextColor">#FFE1E1E1</Color>
<Color x:Key="FocusBorderColor">#FF1177BB</Color>
```

### Warmer Dark Theme
```xml
<Color x:Key="WindowBackgroundColor">#FF2B2B2B</Color>
<Color x:Key="ControlBackgroundColor">#FF3A3A3A</Color>
<Color x:Key="PrimaryTextColor">#FFDDDDDD</Color>
<Color x:Key="StatusReadyColor">#FF569CD6</Color>
```

### High Contrast Theme
```xml
<Color x:Key="WindowBackgroundColor">#FF000000</Color>
<Color x:Key="ControlBackgroundColor">#FF1C1C1C</Color>
<Color x:Key="PrimaryTextColor">#FFFFFFFF</Color>
<Color x:Key="BorderColor">#FF808080</Color>
```

### GitHub Dark Theme
```xml
<Color x:Key="WindowBackgroundColor">#FF0D1117</Color>
<Color x:Key="ControlBackgroundColor">#FF21262D</Color>
<Color x:Key="PrimaryTextColor">#FFC9D1D9</Color>
<Color x:Key="FocusBorderColor">#FF58A6FF</Color>
```

## 🌟 Theme Features

### ✅ **Professional Color Palette**
- Based on VSCode's Dark+ theme (which Cursor uses)
- Optimized for long coding sessions
- Excellent contrast ratios for readability

### ✅ **Unified System**
- **20 core colors** control the entire application
- Change one color → entire app updates instantly
- No need to hunt through multiple files

### ✅ **Smart Color Hierarchy**
- **Background colors**: From darkest (#1E1E1E) to lighter shades
- **Text colors**: High contrast white and subtle grey
- **Interactive states**: Smooth hover/pressed transitions
- **Status colors**: Color-coded feedback system

### ✅ **Window Borders & Effects**
- Professional window borders with rounded corners
- Subtle drop shadows for depth
- Different border weights for window hierarchy

## 🔍 Status Color System

### **Status Messages Use Unified Colors:**
- **Ready State**: VSCode blue (#007ACC) - "Status: Ready"
- **Success Messages**: Cyan-green (#4EC9B0) - successful operations
- **Warning Messages**: Light yellow (#DCDCAA) - caution states  
- **Error Messages**: Bright red (#F44747) - error conditions

### **Path Display Colors:**
- **Valid Paths**: Success green when page is found
- **Invalid Paths**: Error red when page not found
- **Default State**: Subtitle grey for helper text

## 🏷️ Label Color Unification

**All Labels Now Use Unified Colors:**
- **Main Window Labels**: "Copy From:", "Copy To:", "Status:" → `PrimaryTextBrush`
- **Settings Labels**: "Confluence URL:", "Username/Email:", "Authentication:", "Password:", "API Key:" → `PrimaryTextBrush`
- **Subtitle Text**: Page path displays, hints → `SubtitleTextBrush`

**Complete Text Hierarchy:**
- **Primary Labels**: Bold section headers use `PrimaryTextColor` (#CCCCCC)
- **Subtitle Text**: Helper text, paths use `SubtitleTextColor` (#9DA5B4)
- **Status Messages**: Dynamic colors based on message type
- **All Text Elements**: Every piece of text references the unified color system

## 🎨 Customization Tips

### **Quick Theme Changes:**
1. **Lighter**: Increase all color values by #20-#30
2. **Darker**: Decrease all color values by #10-#20  
3. **Colorful**: Replace greys with blue/purple tones
4. **High Contrast**: Use pure black/white combinations

### **Safe Color Ranges:**
- **Backgrounds**: #000000 to #404040 (darker is better)
- **Text**: #C0C0C0 to #FFFFFF (lighter is better)
- **Borders**: #404040 to #808080 (subtle but visible)

### **Professional Color Inspiration:**
- **VSCode Dark+**: Current theme (done!)
- **GitHub Dark**: #0D1117, #21262D, #C9D1D9
- **JetBrains Darcula**: #2B2B2B, #3C3F41, #BBBBBB
- **Atom One Dark**: #282C34, #21252B, #ABB2BF

---

## 🚀 Result

Your color system is now **completely unified** with Cursor's professional aesthetic! Every pixel is controlled from one central location, and the theme matches modern code editors that developers love. 🎉

**Pro Tip**: Save different color combinations as comments in your `App.axaml` file so you can quickly switch between themes!