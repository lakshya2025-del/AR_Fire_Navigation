# AR Fire Navigation - ARCore & AR Foundation Evacuation System

A complete, production-ready Unity AR emergency evacuation and navigation prototype built with **Unity 6 (6000.5.2f1)**, **AR Foundation 6.5.1**, and **ARCore XR Plugin 6.5.1** for Android smartphones.

This application provides real-time augmented reality visual wayfinding to guide evacuees toward designated emergency fire exits using realistic, visible blue 3D directional arrows floating over the live camera view.

---

## 1. Project Overview & Features

- **Live ARCore Camera Stream**: Integrates device camera feed with AR Foundation background rendering.
- **Defensive Tracking State Machine**: Monitors ARCore session states (`WaitingForTracking`, `ReadyToStart`, `Navigating`, `Completed`, `TrackingLost`, `Error`).
- **Dynamic 3D Blue Directional Arrows**:
  - Positioned at floor level (default `0.05m` height offset).
  - Object pooled to prevent garbage collection spikes during navigation.
  - Automatically re-oriented along the path toward upcoming waypoints.
  - Subtle breathing/pulsing animation to enhance visibility against any background.
- **Configurable Multi-Waypoint Route**: Demonstrates a complete evacuation route including straight corridors, multiple turns, and a safe exit point.
- **High-Precision Direction & Turn Guidance**: Calculates relative yaw angles, distances, progress percentages, and provides human-readable instructions ("Continue Straight", "Turn Right", "Approaching Waypoint").
- **Hysteresis Waypoint Arrival Confirmation**: Requires multi-frame arrival confirmation (`arrivalConfirmationFrames = 3`) to prevent accidental waypoint skipping caused by noisy sensor readings.
- **Mobile Emergency UI**: High-contrast, responsive interface supporting various screen sizes and aspect ratios, safe area compensation, status badges, distance metrics, and interactive buttons.
- **Tracking Loss Recovery**: Gracefully pauses navigation visual cues when tracking is interrupted, displays instructions to point camera at textured surfaces, and resumes automatically upon relocalization.
- **Runtime Camera Permission**: Handles Android 6.0+ runtime camera permissions defensively.

---

## 2. Technical Specifications & Environment

- **Unity Editor Version**: `6000.5.2f1` (Unity 6.5).
- **Target Platform**: Android.
- **Target Architecture**: ARM64 (IL2CPP scripting backend).
- **Minimum Android API Level**: API Level 26 (Android 8.0 Oreo - required for Google ARCore).
- **Target Android API Level**: API Level 34 (Android 14).
- **Input System**: Both Input System and Legacy active (`activeInputHandler: 2`).
- **Package Manifest Dependencies**:
  - `com.unity.xr.arfoundation`: `6.5.1`
  - `com.unity.xr.arcore`: `6.5.1`
  - `com.unity.xr.core-utils`: `2.6.0`
  - `com.unity.xr.management`: `4.5.4`
  - `com.unity.inputsystem`: `1.20.0`
  - `com.unity.ugui`: `2.0.0`
  - `com.unity.test-framework`: `1.4.5`

---

## 3. Project Architecture

```
AR_Fire_Navigation/
├── Assets/
│   ├── Materials/
│   │   ├── ArrowBlue.mat              # Primary luminous blue arrow body
│   │   ├── ArrowGlow.mat              # Subtle cyan highlight glow
│   │   ├── UIBackground.mat           # High-contrast translucent dark backdrop
│   │   ├── WaypointExit.mat           # Emergency green exit door indicator
│   │   └── WaypointNode.mat           # Transit beacon orange node
│   ├── Prefabs/
│   │   ├── NavigationArrow.prefab     # 3D directional arrow with shaft & head
│   │   ├── NavigationWaypoint.prefab  # Ground transit pillar ring
│   │   └── NavigationCanvas.prefab    # Complete mobile emergency HUD
│   ├── Scenes/
│   │   └── MainARScene.unity          # Fully wired ARCore navigation scene
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── ARNavigationBootstrapper.cs # Validates components & permissions
│   │   │   ├── ARTrackingManager.cs        # Monitors ARSession states
│   │   │   └── NavigationManager.cs        # State machine & route coordination
│   │   ├── Navigation/
│   │   │   ├── WaypointRoute.cs            # Route data, progress, arrival logic
│   │   │   ├── DirectionCalculator.cs      # Vector math, angles, turn guidance
│   │   │   ├── ArrowController.cs          # 3D arrow pooling, placement, & pulse
│   │   │   └── NavigationUI.cs             # Mobile UI event & display controller
│   │   ├── Utilities/
│   │   │   ├── NavigationConfiguration.cs  # ScriptableObject configuration asset
│   │   │   └── ArrowMeshGenerator.cs       # Procedural 3D arrow mesh builder
│   │   ├── Editor/
│   │   │   └── ARProjectConfigurator.cs    # Automated build & settings validator
│   │   └── AppAssembly.asmdef
│   ├── Settings/
│   │   └── NavigationConfig.asset     # Default parameters instance
│   └── Tests/
│       ├── EditMode/
│       │   ├── EditModeTests.asmdef
│       │   ├── WaypointRouteTests.cs
│       │   ├── DirectionCalculatorTests.cs
│       │   └── NavigationStateMachineTests.cs
│       └── PlayMode/
│           ├── PlayModeTests.asmdef
│           └── NavigationFlowPlayTests.cs
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
├── ProjectSettings/
│   ├── ProjectVersion.txt
│   ├── PlayerSettings.asset
│   ├── EditorBuildSettings.asset
│   ├── TagManager.asset
│   ├── QualitySettings.asset
│   ├── GraphicsSettings.asset
│   ├── TimeManager.asset
│   └── InputManager.asset
├── README.md
└── BUILD_AND_TEST_REPORT.md
```

---

## 4. How to Open and Run the Project

### Requirements
1. Install **Unity 6000.5.2f1** via Unity Hub.
2. Ensure the **Android Build Support** module is installed:
   - Android SDK & NDK Tools
   - OpenJDK

### Opening the Project
1. Open **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Select the extracted `AR_Fire_Navigation` folder.
4. Open the project with Unity Editor `6000.5.2f1`.
5. Allow Unity to resolve packages and compile assets.
6. Open `Assets/Scenes/MainARScene.unity`.

---

## 5. Testing on an Android Device

### Preparing the Phone
1. Ensure your device is on the official [Google ARCore Supported Devices](https://developers.google.com/ar/devices) list.
2. Enable **Developer Options** and **USB Debugging** on the Android device.
3. Install **Google Play Services for AR** from the Google Play Store.
4. Connect the phone to your PC via USB.

### Building and Installing the APK
- **From Unity Editor**:
  1. Go to **File** -> **Build Profiles** (or **Build Settings**).
  2. Ensure platform is switched to **Android**.
  3. Verify `Assets/Scenes/MainARScene.unity` is checked.
  4. Click **Build and Run**.
- **Automated Batch Command**:
  ```powershell
  & "C:\Program Files\Unity\Hub\Editor\6000.5.2f1-x86_64\Editor\Unity.exe" -batchmode -quit -projectPath "path\to\AR_Fire_Navigation" -executeMethod ARFireNavigation.Editor.ARProjectConfigurator.BuildAndroidAPK -logFile "build.log"
  ```
  The resulting APK is written to `Builds/Android/AR_Fire_Navigation.apk`.

---

## 6. Navigation Customization & Configuration

### Adjusting Arrow Height, Scale, and Offset
Open `Assets/Settings/NavigationConfig.asset` in the Inspector:
- `Arrow Height Offset`: Default `0.05` meters (5 cm above floor plane).
- `Arrow Spacing`: Default `1.0` meter between successive arrows.
- `Arrow Scale`: Default `(0.45, 0.45, 0.45)`.
- `Model Forward Rotation Offset`: Adjust Euler offset if using a 3D arrow model that does not point along `+Z`.
- `Rotation Smoothing Speed`: Default `8.0` (higher is snappier, lower is smoother).
- `Arrival Radius`: Default `1.2` meters.
- `Arrival Confirmation Frames`: Default `3` consecutive frames.

### Modifying Waypoints and Routes
Select the `AppNavigationController` in `MainARScene.unity` and inspect the `WaypointRoute` component:
- Add or modify waypoints in the `Waypoints` list.
- Each waypoint has:
  - `Position`: Coordinates in meters `(X, Y, Z)`.
  - `Instruction`: Custom text prompt (e.g., `"Turn Left at Stairwell B"`).
  - `Custom Arrival Radius`: Specific threshold for this node (or `0` to inherit default).
  - `Is Exit`: Check this for the final emergency safe zone.

---

## 7. Real-World Building Calibration vs. Demonstration Route

### Critical Safety Distinction
> [!IMPORTANT]
> This project is an academic prototype demonstration. The sample waypoints operate in local coordinate space relative to the AR session origin. In a physical building, local AR tracking drifts over long distances and does not automatically align with building blueprints without physical localization anchors.

### Production Physical Building Calibration Steps
To deploy this system in an actual building:
1. **World Alignment Anchors (Physical Markers / QR Codes)**:
   Place high-contrast QR codes or AprilTags at designated corridor start points. When scanned by the AR camera, reset the coordinate origin and orientation to match the building CAD/BIM model.
2. **ARCore Cloud Anchors / Geospatial API**:
   Use Google ARCore Cloud Anchors or VPS (Visual Positioning System) to anchor waypoints to persistent physical features of the facility.
3. **Floor-Clamped Raycasting**:
   Integrate AR Foundation plane raycasting (`ARRaycastManager.Raycast`) to project waypoints and arrows dynamically onto the detected physical floor surface.
4. **Offline Resilience**:
   Pre-download all architectural waypoint graphs to local device storage to ensure zero reliance on active network connectivity during an emergency blackout.

---

## 8. Automated Testing & Verification

The project includes an automated test suite verifying route validation, direction calculations, turn instructions, state transitions, and integration:

- **EditMode Tests (18 Passed / 0 Failed)**:
  - `WaypointRouteTests`: Validation rejection of insufficient waypoints, coincident nodes, total route distance math, progress percentage calculation, and arrival radius detection.
  - `DirectionCalculatorTests`: Forward, right, left, backward cardinal angle detection, safe coincident point handling (no NaN/divide-by-zero), horizontal distance math, turn instruction generation, rotation smoothing, and model offset rotation.
  - `NavigationStateMachineTests`: Legal state transitions, tracking loss and recovery cycle, state change event firing, and clean navigation reset.
- **PlayMode Tests (2 Passed / 0 Failed)**:
  - `NavigationFlow_AdvancesWaypointsAndCompletesAtExit`: Simulates an evacuee traversing all waypoints, advancing sequentially, and reaching the destination exit.
  - `NavigationFlow_RestartResetsProgress`: Verifies navigation progress reset after starting.
