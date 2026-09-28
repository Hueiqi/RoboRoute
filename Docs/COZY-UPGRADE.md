# Cozy village visual upgrade

This revision translates the supplied visual references into an original, procedural 3D village inside the existing Unity project. The images guide the palette, proportions and atmosphere; they are not embedded as a flat gameplay background.

## Implemented presentation

- Cream, timber and teal-roofed parcel cottages replace the industrial racks.
- A floating garden island has a wooden entry bridge, layered cliff stones, trees, planted flowers, fences and lanterns.
- Mint, coral and honey-yellow lanes identify the three delivery bays.
- Player and autonomous robots share a rounded cream shell, colored helmet/backpack, wheels, antenna and blinking LED eyes.
- Conveyors move parcels, the charging ring pulses, and accepted deliveries trigger a robot reaction and confetti.
- A warm cream HUD displays mission, destination, deadline, score, battery and fleet status. Tutorial, restart confirmation and completion panels use the same visual language.
- Keyboard movement follows the isometric camera orientation.

## Preserved engineering

The six-job timed schedule, scoring rules, one-parcel load limit, battery consumption, charging, JSON schema and fixed-grid route search remain. Buildings occupy the original rack collision footprints. Decorative elements have no collision, so they cannot silently obstruct previously valid paths.

Presentation lives in `WarehouseBuilder`, `CozyHud` and `CozyMotion`. `ShiftState` and `SaveService` retain the existing business rules and persistence format. The previous runtime sources are retained in `Docs/BeforeCozy` for comparison.

## Scope and validation

This is a stylized 3D interpretation, not a promise to exactly reproduce the supplied rendered concept art. Geometry is generated in Play mode; it is not a library of hand-authored model prefabs.

`CozyPreview` is an opt-in standalone capture tool (`-cozy-preview`). It briefly runs the scene, writes a screenshot and scene counts to `Build/Evidence`, and exits. It does not certify keyboard behavior or a full delivery shift. The existing rule checks are rerun during the Windows build.

The scripted six-delivery integration run passed with score 600, zero missed jobs, zero runtime errors, and a successful isolated save/load roundtrip. Manual keyboard controls and subjective audio still need acceptance testing. The existing limitations on fleet saves and robot-to-robot traffic reservations remain.

## Open the packaged project

Extract `RoboRoute-Cozy-Unity-Project.zip`, then add the extracted folder containing Assets, Packages and ProjectSettings in Unity Hub. Use Unity 6000.3.10f1. Open Assets/RoboRoute/Scenes/Warehouse.unity and press Play. The village is generated when Play begins. The separate Windows build can be played through Build/RoboRoute.exe.


## Save-timing regression fix

Repeated runtime checks exposed floating-point accumulation that could put total delivery time slightly above the shift clock and reject an otherwise valid save. Delivery totals are now capped to the authoritative elapsed clock when a delivery is accepted. Two regression assertions cover this case, bringing the domain checks to 94. The JSON schema and scoring rules are unchanged.
