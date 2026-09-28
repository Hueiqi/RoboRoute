# Acceptance checks

- Start tutorial, move in all directions, verify walls and racks block the player.
- Pick up at white pad; verify capacity one and slower loaded speed.
- Deliver at incorrect bay: no points, package retained. Correct bay: +100 once.
- Allow a deadline to expire: failed count increments, cargo clears, next bay dispatches.
- Pause: clock, robots, movement and charging stop. Resume: all restart.
- Exhaust battery, recover, recharge; recovery consumes 30 mission seconds.
- Save while loaded, alter progress, load: mission and cargo restore at charger.
- Corrupt JSON: load fails without replacing current state.
- Observe both autonomous robots navigate around racks and complete deliveries.
- Finish six jobs and restart. Confirm completed timer remains stopped.
- Check tutorial and all HUD controls at 1280x800 and 1920x1080.
- Profile a 60-second run in a standalone build. Record hardware, resolution, frame timings, allocations and errors; the HUD average is only an informal indicator.

Automated evidence is written only after Unity successfully executes BuildProject.Check.

## Verified on 2026-09-10

- Unity 6000.3.10f1 Windows player build completed successfully (exit code 0).
- Unity domain checks passed 92 assertions; see `Evidence/domain-checks.txt`.
- Independent C# mission checks passed seven assertions; see `Evidence/standalone-rules.txt`.
- Captured runtime frames were visually inspected at 1280x800: warehouse, labels, player, couriers and dashboard render correctly. Successful deliveries update the dashboard.
- A build-only stripped shader error was found during runtime checking and fixed by including an explicit Resources material.
- Computer-use access to the game window was not approved. Manual keyboard interaction, subjective sound quality and full acceptance testing are not certified by this session.
- Frame-time readings include capture overhead and are not a controlled performance benchmark.

## Cozy edition verification

The cozy Windows build compiles successfully and preserves all 92 passing domain assertions. The accelerated scripted integration run completed all six deliveries, scored 600, missed zero jobs, reported zero runtime exceptions/errors, and passed a real JSON save/load roundtrip. Test saves use `roboroute-smoke.json`, separate from normal progress. The script uses the normal carrying speed and battery rules, but drives programmatically instead of using keyboard input.

Current visual evidence and release notes are in `Evidence/cozy-preview.png`, `Evidence/cozy-demo.mp4`, `Evidence/cozy-integration-check.txt` and `Docs/COZY-UPGRADE.md`. The demo is silent and accelerated 3×; its frame cadence is not a performance benchmark. Earlier report sections describe the previous industrial prototype.

### Final cozy release check — 11 September 2026

The final build passed **94 domain assertions**, including two added float-accumulation regression checks. The tracked standalone run exited with code 0: six delivered, zero missed, 600 points, zero runtime errors/exceptions, and a successful real save/load roundtrip. See `Evidence/cozy-integration-check.txt`.

The final screenshot and 26.55-second silent, accelerated video were generated from this build. Unity and the game were closed after verification. Manual keyboard acceptance testing and a controlled performance benchmark remain separate from this scripted evidence.

## Achievement edition verification

The current result supersedes the older 94-assertion snapshot: 118 assertions passed. `Evidence/achievements-integration-check.txt` records 6/6 delivered, score 600, zero errors, shift and career persistence, and seven collected stickers. Screenshots of the HUD, tutorial, collection and results were visually inspected. The report and quick guide were rendered and all 15 pages inspected. See the project report for limitations and remaining manual acceptance work.
