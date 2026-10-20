# STEAM AR - Physical Science Storybook Module
### *Interactive Science Education through Augmented Reality & Narrative Storytelling*

<br/>

![Unity](https://img.shields.io/badge/UNITY-2022.3.62F3%20LTS-black?style=for-the-badge&logo=unity&logoColor=white)
![Vuforia Engine](https://img.shields.io/badge/VUFORIA%20ENGINE-11.4.4-0080C6?style=for-the-badge)
![C#](https://img.shields.io/badge/C%23-.NET%20STANDARD%202.1-239120?style=for-the-badge)  
![Platform](https://img.shields.io/badge/PLATFORM-ANDROID%20%7C%20IOS%20(MOBILE%20AR)-3DDC84?style=for-the-badge)
![Firebase](https://img.shields.io/badge/FIREBASE-REALTIME%20DATABASE-FFA611?style=for-the-badge&logo=firebase&logoColor=white)  
![Google Sheets](https://img.shields.io/badge/GOOGLE%20SHEETS-APPS%20SCRIPT%20API-34A853?style=for-the-badge&logo=googlesheets&logoColor=white)
![Git LFS](https://img.shields.io/badge/GIT%20LFS-ENABLED-F05032?style=for-the-badge&logo=git-lfs&logoColor=white)
![Figma](https://img.shields.io/badge/FIGMA-UI%2FUX%20DESIGN-F24E1E?style=for-the-badge&logo=figma&logoColor=white)

---

**Research Component:** IT22177100 | **Target Platform:** Android (Mobile / Tablet)  
**Target Audience:** Grade 4 & Grade 5 Students (Sri Lankan Primary Curriculum / *Parisaraya* Syllabus)

---

## Project Overview

**STEAM AR - Physical Science Storybook Module** is an interactive, narrative-driven Augmented Reality (AR) mobile learning application designed for Grade 4 and Grade 5 students in Sri Lanka, directly aligned with the national *Parisaraya* (Environmental Studies / Primary Science) syllabus. Rather than presenting abstract physical science principles as flat 2D textbook diagrams, the application brings learning to life by anchoring gamified 3D scientific experiments onto a physical printed storybook titled *"Dasun and the Moonstone"*. Students follow the adventures of Dasun, using real-world physical markers to manipulate electric circuits, experiment with mechanical forces and pulleys, and investigate magnetism and electromagnets. Built with a fully native bilingual localization engine (English and Sinhala) and integrated empirical research analytics, the system bridges the gap between digital interactivity and foundational primary STEM education.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Key Features & Pedagogical Modules](#key-features--pedagogical-modules)
   - [Lesson 1: Electricity & Circuits](#lesson-1-electricity--circuits)
   - [Lesson 2: Forces & Simple Machines](#lesson-2-forces--simple-machines)
   - [Lesson 3: Magnetism & Electromagnets](#lesson-3-magnetism--electromagnets)
3. [Technology Stack & Architectural Specifications](#technology-stack--architectural-specifications)
4. [Bilingual Localization Engine](#bilingual-localization-engine)
5. [Scene Breakdown & Navigation Hierarchy](#scene-breakdown--navigation-hierarchy)
6. [Scripts & Engineering Architecture](#scripts--engineering-architecture)
7. [Installation & Build Guide](#installation--build-guide)

---

## Key Features & Pedagogical Modules

### Lesson 1: Electricity & Circuits
* **Dynamic Procedural Circuit Building**: Students tap the positive and negative terminals of a 3D dry cell and the contact points of a 3D lightbulb. Using Unity's `LineRenderer`, the application dynamically generates smooth, physics-curved 3D wires in real time.
* **Immediate Visual Feedback**: Once a closed circuit is formed, the 3D bulb immediately illuminates, accompanied by tactile haptic feedback and celebration audio.
* **In-AR Conceptual Assessment**: A state-driven quiz challenges students on circuit theory (e.g., adding cells vs. adding wires, series vs. parallel configurations).
* **Series Stacking Experiment**: In the second AR phase, students stack up to 3 batteries in series. With each additional dry cell, the bulb's light intensity and illumination radius scale dynamically, visually reinforcing the concept of additive voltage.

### Lesson 2: Forces & Simple Machines
* **Push & Pull Dynamics**: Students assist Dasun in moving a heavy wooden crate. Interactive touch buttons trigger directional forces—pushing moves the object away with realistic crate tilting and sliding audio, while pulling draws the crate closer.
* **Rope Catenary Physics**: Pull actions dynamically compute realistic rope sag between Dasun's hands and the crate tie-ring using multi-segment curved rendering.
* **Mechanical Advantage & Ramp Assessment**: Students evaluate inclined planes (100cm vs. 150cm ramps) to discover that gentler slopes require less applied force.
* **Interactive Pulley Lifting Simulation**: Students execute downward finger swipes on the screen to pull a rope. Dasun's character arms procedurally animate downwards, redirecting the force through a 4-point pulley system to hoist the heavy load effortlessly.

### Lesson 3: Magnetism & Electromagnets
* **Pole Interactions (Attraction & Repulsion)**: Students manipulate 3D bar magnets in AR, tapping to flip poles ($N$ and $S$). Opposite poles ($N-S$) accelerate and snap together with metallic audio and haptic feedback. Like poles ($N-N$ or $S-S$) produce an active physical repulsion vector, realistic mechanical jitter, low magnetic humming, and continuous haptic resistance.
* **Electromagnet Blueprint Assembly**: A 2D engineering blueprint requires students to drag and drop core components (iron nail, copper wire coil, dry cell battery) into place. Trick items (yarn, plastic, permanent magnets) trigger educational voice explanations of electrical conductivity.
* **3D AR Ferromagnetic Testing**: Once built, the energized electromagnet appears in 3D AR alongside everyday items (steel paperclips, safety pins, wooden blocks, plastic toys, copper coins). Students drag items to the nail tip; ferromagnetic items snap and adhere to the magnetic field, while non-ferromagnetic items rebound with informative scientific voiceovers.

---

## Technology Stack & Architectural Specifications

| Layer | Technology | Purpose |
| :--- | :--- | :--- |
| **Game Engine** | Unity 3D (2022.3 LTS) | Core 3D real-time simulation, scene graph, rendering pipeline, and physics engine. |
| **AR Subsystem** | Vuforia Engine (11.4.4) | Image target detection, spatial marker tracking, plane tracking, and device pose estimation. |
| **Programming Language** | C# (.NET Standard 2.1) | Object-oriented game architecture, state machines, math/physics routines, and event handling. |
| **UI Framework** | Unity UI (uGUI) & TextMeshPro | Responsive UI layout, vector-sharp typography, and bilingual font asset rendering (FM-Abhaya / LiberationSans). |
| **Visual Design** | Figma | UI/UX layout, wireframing, color palette design, and custom iconography. |
| **Physics & Geometry** | Unity `LineRenderer` & Procedural Math | Real-time procedural 3D wire synthesis, bezier desk arcs, and dynamic rope catenary curves. |
| **Cloud & Analytics** | Firebase Realtime Database & REST API | Quantitative research telemetry (AR interaction time, quiz error rate, session duration, and retry counts). |
| **Offline Fail-Safe** | Local CSV & `PlayerPrefs` Cache | High-reliability persistent file storage on mobile devices for rural classroom trials without active Wi-Fi. |
| **Hardware Immersion** | Android Java JNI (Vibrator Service) | Fine-tuned haptic feedback (30ms–50ms pulses) for magnetic snaps, wire connections, and error cues. |

---

## Bilingual Localization Engine

Primary education in Sri Lanka requires localized instructional delivery to ensure reading comprehension does not hinder scientific discovery. The module features an ultra-lightweight, zero-overhead bilingual localization architecture:

1. **Dual-Language Font Pipeline**: Supports standard Latin characters alongside Sinhala Unicode and FM-Abhaya font glyphs rendered cleanly via TextMeshPro.
2. **Dynamic UI Dispatching (`LocalizedText.cs`)**: Rather than relying on heavy external runtime translation libraries, UI panels maintain synchronized language pairs that activate/deactivate instantaneously when the language is toggled mid-game.
3. **Synchronized Voiceover & Subtitles (`StoryUIManager.cs`)**: Every narrative beat, scientific instruction, and quiz explanation features studio-recorded dual voice tracks (English and Sinhala) synchronized with animated letter-by-letter typewriter subtitles.

---

## Scene Breakdown & Navigation Hierarchy

The application employs a robust single-scene state-machine architecture (`Assets/Scenes/SampleScene.unity`) to eliminate heavy scene loading delays on mobile devices, organizing modules into dedicated view containers:

| View / Module | Identifier | Description |
| :--- | :--- | :--- |
| **Language Selection** | `languagePanel` | Initial splash screen where students select English or Sinhala instruction. |
| **Student Profile Login** | `loginPanel` | Student name entry and grade selection (Grade 4 vs. Grade 5) for research cohort tagging. |
| **Main Menu Hub** | `homePanel` | Central navigation gateway to start AR book scanning, view lesson modules, or adjust settings. |
| **Interactive Tutorial** | `tutorialPanel` | Multi-slide onboarding carousel introducing physical marker scanning and touch controls. |
| **AR Scanner View** | `arScannerPanel` | Full-screen live Vuforia camera feed actively searching for physical storybook markers. |
| **Electricity Module** | `AR_Circuits (Lesson 1)` | 3D circuit breadboard, procedural wire connecting, series stacking, and core quiz panels. |
| **Forces & Simple Machines** | `AR_Pulleys (Lesson 2)` | 3D crate push/pull mechanics, ramp angle assessments, and swipe-driven pulley hoisting. |
| **Magnetism & Electromagnets** | `AR_Magnets (Lesson 3)` | 3D bar magnet polarity testing, 2D blueprint drag-and-drop, and AR magnetic sorting. |
| **Story Reveal & Rewards** | `storyRevealPanel` | Cinematic storybook rewards with Dasun unlocking the Moonstone's powers after mastering lessons. |

---

## Scripts & Engineering Architecture

The codebase strictly follows separation of concerns, defensive programming, and mobile optimization patterns:

```
Assets/Scripts/
├── Core UI & Localization
│   ├── UIManager.cs                   # Master view router, panel switcher, and Vuforia tracking governor
│   ├── StoryUIManager.cs              # Typewriter subtitle coroutines and bilingual voiceover dispatch
│   ├── LocalizedText.cs               # Instantaneous on-the-fly English/Sinhala UI label toggle
│   └── TutorialManager.cs             # Onboarding carousel slide controller
├── Lesson 1: Circuits & Electricity
│   ├── CircuitQuizManager.cs          # Terminal raycast detection, LineRenderer wire generation, circuit solver
│   ├── CircuitCoreQuiz.cs             # 2-stage conceptual quiz on battery power and series vs. parallel
│   └── BrightnessExperimentManager.cs # Dynamic dry cell stacking with real-time light intensity & range scaling
├── Lesson 2: Forces & Pulleys
│   ├── PushPullManager.cs             # Crate slide translations, scrape SFX, and catenary rope sag rendering
│   ├── Lesson2QuizManager.cs          # Enum-driven quiz on inclined plane angles and mechanical advantage
│   ├── PulleyManager.cs               # 4-point continuous pulley rope physics and procedural arm rotation IK
│   └── PulleySwipeManager.cs          # Vertical drag swipe gesture parser
├── Lesson 3: Magnetism & Electromagnets
│   ├── MagnetTouchManager.cs          # 3D magnet drag, 180° pole flip, magnetic snap, and repulsion jitter
│   ├── ElectromagnetQuizManager.cs    # 2D blueprint assembly logic with distractor item feedback
│   ├── ElectromagnetDragItem.cs       # Drag-and-drop handler on Canvas using IBeginDrag / IDrag / IEndDrag
│   ├── ElectromagnetARManager.cs      # AR testing state machine and Vuforia marker tracking listener
│   ├── ElectromagnetTestManager.cs    # ScreenPointToRay drag-and-drop for ferromagnetic vs. non-ferrous testing
│   └── MagneticTestItem.cs            # Sinusoidal mathematical hover bobbing (y = sin(t) * h)
├── Research & Telemetry
│   └── CircuitAnalyticsManager.cs     # Singleton metrics engine (AR time, quiz time, error rate, Firebase/CSV)
└── Mobile & AR Utilities
    ├── CameraPauseManager.cs          # Disables Vuforia background tracking on 2D menus to prevent overheating
    ├── ARMemoryReset.cs               # Flushes image target spatial memory between lesson transitions
    ├── ARPinchToScale.cs              # Clamped two-finger pinch-to-scale gesture (0.8x to 1.4x bounds)
    └── HapticFeedbackManager.cs       # Android Java JNI bridge for tactile sensory feedback
```

### Architectural Highlights:
* **Raycast Terminal Snapping**: [CircuitQuizManager.cs](Assets/Scripts/CircuitQuizManager.cs) and [ElectromagnetTestManager.cs](Assets/Scripts/ElectromagnetTestManager.cs) translate 2D screen touches into 3D world rays (`Camera.main.ScreenPointToRay`), evaluating Euclidean proximity to terminal nodes before establishing electrical or magnetic bonds.
* **State-Machine Lifecycle Governor**: [UIManager.cs](Assets/Scripts/UIManager.cs) governs all panel transitions and shuts off camera tracking during story reveals (`shouldTrack = (panelToShow == arScannerPanel || panelToShow == arQuizUIPanel)`) to prevent premature marker triggers.
* **Thermal Management**: [CameraPauseManager.cs](Assets/Scripts/CameraPauseManager.cs) toggles `VuforiaBehaviour.Instance.enabled` on and off during non-AR phases, dramatically reducing GPU load and extending mobile battery life.

---

## Installation & Build Guide

### Prerequisites
* **Unity Editor**: `2022.3.62f3` (LTS) or compatible 2022.3 release.
* **Build Support**: Android Build Support (OpenJDK, Android SDK & NDK Tools).
* **Git LFS**: Git Large File Storage must be installed (`git lfs install`) to pull packaged Vuforia tarball dependencies.

### Cloning & Setup
```bash
# 1. Clone the repository
git clone https://github.com/Uvindu-Induwara-Mendis/IT22177100_Basic_Physical_Science_STEAM_AR.git

# 2. Enter project directory
cd IT22177100_Basic_Physical_Science_STEAM_AR

# 3. Pull Large File Storage dependencies
git lfs pull
```

### Building for Android
1. Open the project in **Unity 2022.3.62f3**.
2. Navigate to **File > Build Settings**.
3. Select **Android** and click **Switch Platform**.
4. Ensure `Assets/Scenes/SampleScene.unity` is checked in the **Scenes In Build** list.
5. Click **Build** to produce the standalone `.apk` or **Build and Run** to install directly to a connected Android device via USB debugging.
