# ProceduralAnimations

Procedural animation system in Unity demonstrating skeleton-based and mesh-based animation techniques using mathematical functions.

## Demos

### Fish Procedural Animation (Skeleton-based)
https://github.com/user-attachments/assets/eedc9cc0-a996-46f6-b060-748daab07291

Skeleton-based procedural animation with three behavioral states controlled via UI buttons.

### Blender Mesh-based Geometry Animation
https://github.com/user-attachments/assets/52242280-63a9-4d06-bf68-6bc047bde34e

Geometry-based procedural animations using mesh deformation techniques.

## Features

### Animation States
The system implements three distinct behavioral states:
- **Idle**: Gentle, natural resting motion
- **Flee**: Rapid escape movement pattern
- **Track Target**: Following and pursuit behavior

### Implementation
- **Sine-based Animation**: Uses mathematical sine waves to animate bone transforms, creating smooth, organic movement
- **Skeleton Animation**: Direct bone manipulation for procedural character animation
- **UI Controls**: Runtime switching between animation states via button interface

## Building and Running

### Prerequisites
- Unity 2020.3 or later
- Git

### Setup
1. Clone the repository:
```bash
   git clone https://github.com/yourusername/ProceduralAnimations.git
   cd ProceduralAnimations
```

2. Open in Unity:
   - Launch Unity Hub
   - Click "Add" and select the project folder
   - Open the project

### Running the Project
1. Open the main scene (usually `Assets/Scenes/MainScene.unity`)
2. Press the **Play** button in Unity Editor
3. Use the UI buttons to switch between animation states:
   - **Idle**: Watch the natural resting animation
   - **Flee**: Trigger escape behavior
   - **Track**: Enable target tracking mode

### Building
1. Go to `File > Build Settings`
2. Select your target platform (Windows/Mac/Linux)
3. Click `Build` and choose output location
4. Run the generated executable

## Code Structure

The project uses sine wave functions to procedurally animate skeletal bones:
- Each animation state has unique sine wave parameters (frequency, amplitude, phase)
- Bones are transformed in real-time based on mathematical calculations
- No pre-baked animation clips - all movement is generated at runtime