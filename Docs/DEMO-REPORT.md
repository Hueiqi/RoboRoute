# RoboRoute — Demo Report and Next Steps

**Date:** 10 September 2026  
**Platform:** Windows, Unity 6000.3.10f1, Universal Render Pipeline  
**Status:** Working prototype build; final gameplay acceptance and portfolio polish remain in progress.

## Project summary

RoboRoute is a 3D warehouse robot simulator designed to demonstrate C# software engineering. The player collects a package, transports it around warehouse racks, and delivers it to the assigned bay before its deadline. Autonomous couriers operate in the same warehouse.

The project was created from the installed Unity Universal 3D template and the existing RoboRoute starter. The original starter remains available separately.

## Demo materials

- [Watch the 33-second gameplay preview](../Evidence/RoboRoute-Demo.mp4)
- [Warehouse screenshot](../Evidence/warehouse.png)
- [Unity automated-check results](../Evidence/domain-checks.txt)
- [Independent C# check results](../Evidence/standalone-rules.txt)
- [Project README and architecture diagram](../README.md)
- [Windows executable](../Build/RoboRoute.exe)

The preview is a silent recording of scripted gameplay. It shows the warehouse and delivery activity; it is not a completed six-job acceptance test. Keep the executable with its accompanying Build files.

## What has been implemented

| Area | Current implementation | Engineering demonstrated |
|---|---|---|
| Warehouse | Procedural 3D floor, racks, obstacles, pickup pad and three delivery bays | Scene construction and reusable code |
| Player robot | WASD/arrow movement, collision handling, E interaction | C#, input handling and object responsibilities |
| Packages | One-package capacity; lower movement speed while loaded | Business rules and state management |
| Delivery jobs | Six sequential jobs; rotating destinations; 75-second deadlines | FIFO scheduling and validation |
| Scoring | 100 points per accepted delivery; no reward for wrong-bay or duplicate delivery | Rule invariants |
| Battery | Travel consumption, charging dock and recovery penalty | Resource management |
| Autonomous robots | Two couriers with pickup, delivery and charging states | State machines and grid pathfinding |
| Persistence | Versioned JSON save/load with validation and temporary-file replacement | Serialization and file I/O |
| Dashboard | Score, deadlines, battery, success rate and average delivery time | Metrics and UI |
| Presentation | Opening tutorial, generated notification sounds, restart confirmation | Onboarding and feedback |
| Documentation | README, architecture diagram, acceptance checklist and demo script | Technical communication |

## Evidence and testing

**Confirmed:**

- Unity successfully compiled and built the Windows player, exiting with code 0.
- The Unity domain-check runner passed **92 assertions** covering mission rules, scoring, deadline progression, JSON roundtripping and route clearance.
- Seven additional mission-rule assertions passed through a standalone C# compilation.
- Frames from the visible game run were inspected at 1280 × 800. The warehouse, dashboard, robots and station labels rendered correctly.
- Captured gameplay showed two accepted deliveries and a score of 200.
- Runtime testing exposed a stripped-shader problem. An explicit Resources material was added, and the subsequent visible run rendered correctly.

**Not yet confirmed:**

- A complete six-delivery standalone smoke run with a final success report.
- Manual keyboard controls and the complete acceptance checklist. Computer-use access to the game window was not approved.
- Save/load behavior across all real file failures and restart scenarios.
- Subjective sound quality, full accessibility checks and performance under a controlled benchmark.

The latest background capture produced black frames. Those frames are not valid visual evidence. The usable preview comes from the earlier visible run. A low frame-time value in the HUD is not sufficient to claim a performance target.

## Current limitations

- Autonomous robots route around fixed rack bounds and yield to the player; robot-to-robot traffic reservations are not implemented.
- Player saves restore mission data and return the player to the charger. They do not restore the complete autonomous fleet or exact world positions.
- The job schedule is a fixed six-job sequence, rather than a configurable dispatcher.
- The interface uses prototype IMGUI controls. A final portfolio interface and broader resolution testing remain.
- Automated checks are a custom assertion runner, not a Unity Test Runner test suite.
- GitHub publication has not been performed. The project should be presented honestly as an AI-assisted prototype.

## What I will do next

| Order | Work | Completion evidence |
|---|---|---|
| 1 | Resolve the background capture issue and rerun the complete scripted shift | Six deliveries, score 600, completion report and no runtime exceptions |
| 2 | Verify manual movement, pickup, wrong-bay rejection, pause, restart and charging | Completed acceptance checklist with observed results; manual testing needs game-window access or user testing |
| 3 | Exercise save/load and recovery edge cases | Valid restore, damaged-save rejection and unchanged current state after failed load |
| 4 | Improve robot traffic handling and interface presentation | Couriers avoid conflicts; readable UI at target resolutions |
| 5 | Run a controlled performance check | Hardware, resolution, frame timings and allocation measurements |
| 6 | Record the final demonstration and prepare the portfolio package | Short final video, current screenshots, build, source and accurate README |

I will fix failures found during these checks before describing the corresponding feature as verified. GitHub preparation will be local until a repository and publication request are provided.

## How to try the current build

Open `Build/RoboRoute.exe`, select **Start my shift**, and use **WASD / arrow keys** to drive. Press **E** on PICKUP, then drive to the assigned bay and press **E** again. Recharge on the teal pad.

To edit the project, add the `RoboRoute` folder in Unity Hub and open it with Unity 6000.3.10f1. Open `Assets/RoboRoute/Scenes/Warehouse.unity` and press Play.
