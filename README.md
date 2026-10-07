#  Top-Down 2D Racing Game

A fast-paced, 2D top-down arcade racing game built in Unity. This project features custom drift physics, dynamic particle effects, and a complete lap-timing system designed for responsive and engaging gameplay.

##  Features

* **Custom Drift Physics:** Hand-tuned 2D vector physics that simulate realistic car handling, sliding, and drifting.
* **Dynamic Visual Effects:** Custom tire smoke implemented via Unity Particle Systems that react to the car's drifting angle and speed.
* **Lap Timing System:** Trigger-based checkpoint and lap timer logic to track player performance.
* **Smooth Camera Tracking:** A custom 2D camera follow script that smoothly interpolates target position while maintaining the player in focus.
* **Fully Functional UI:** Complete user interface including main menus, settings, and seamless scene transitions.

##  Core Scripts

The game's logic is driven by several key C# scripts:
* `CarScript.cs` - Handles the core input, acceleration, steering, and 2D vector drift physics.
* `CameraFollow2D.cs` - Manages smooth camera tracking behind the vehicle.
* `Trigger.cs` - Detects when the car passes through finish lines or checkpoints.
* `Logic.cs` - The core game manager handling the lap timer, score tracking, and scene transitions.

##  Getting Started

### Prerequisites
* **Unity Editor:** Make sure you have a recent version of Unity installed (2022.3 LTS or newer recommended).
* **Git:** To clone the repository.

### Installation
1. Clone this repository to your local machine:
   ```bash
   git clone [https://github.com/DenisCrc/topdown-racing-game.git](https://github.com/DenisCrc/topdown-racing-game.git)
