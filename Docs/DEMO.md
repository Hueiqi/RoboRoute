# Demonstration script

Target length: 45–60 seconds.

1. Show the warehouse and tutorial. Introduce the player, pickup pad, three bays and two autonomous couriers.
2. Drive to pickup, press E, explain capacity one and reduced loaded speed.
3. Deliver at the correct station; show score, deadline and success rate.
4. Show robots routing around storage racks. Explain the FIFO BFS frontier and explicit pickup/delivery/charging states.
5. Show Save/Load and explain validated JSON and charger reset.
6. Show domain-check evidence and architecture diagram. State what was automated and what was manually tested.

The `-roboroute-demo` standalone argument runs a scripted route through all six jobs and exports screenshots plus a runtime report into `Build/Evidence`. It bypasses keyboard movement and is a visual smoke test, not proof of manual input behavior or a benchmark. It creates delivery autosaves under the game's usual persistent data directory.
