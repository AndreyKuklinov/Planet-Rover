# Andrey room source data

`reference.png` and `legend.png` are the supplied images. `rooms.json` is the reviewed
22-column, 12-row transcription: `#` is a wall, `.` is floor, and object coordinates
run left-to-right and top-to-bottom from zero. Rocket coordinates identify the body;
the symbols above it are requests, not separate objects. Red is the existing orange A.

- `ReadReference.ps1` samples the gray walls and writes a draft to
  `tmp/andrey-walls.json`, plus enlarged numbered images in `tmp/andrey-grids`.
- `PopulateReference.ps1` applies the manually reviewed object positions and requests
  to `rooms.json`. Wall data is retained.
- `RenderOverview.ps1` creates the annotated overview in the room folder. Its
  `OutputDirectory` parameter can target an isolated validation project.
- `Assets/Editor/BuildAndreyRooms.cs` generates the room prefabs from the manifest,
  checks overlap and bounds, and executes the Play Mode validation suite. It can be
  run with Unity's `-executeMethod BuildAndreyRooms.Build`. Validation alone uses
  `BuildAndreyRooms.Validate`. The batch driver exits itself; omit `-quit` while testing.

Use an isolated project or close the project's existing editor before starting a batch
editor. Rebuilding replaces authored geometry and objects, so do not rebuild over
manual geometry edits you want to keep. Existing Room options are preserved; newly
created rooms enable timing, reset and both reflections with a 60-second target.

The test driver selects real rover actions from current occupancy and inventory; it
does not add room scripts or alter gameplay mechanics. Four deterministic seeds
exercise the actual startup reflections. Extra tests deliver a bucket before gardening
and invoke the existing room rewind in rooms 09-12. Physical reset-input charging and
human completion times are outside this automated check.

The driver uses a fixed test timestep and samples after game updates to avoid missing
release cells during editor frame spikes. `ResumeValidation` retains successful runs
from the report and continues the remaining scenarios after a driver correction.
Reported action and rewind counts are example solutions, not optimality claims.
