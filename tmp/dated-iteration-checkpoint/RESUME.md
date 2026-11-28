# Dated room iteration 2 checkpoint

Paused at the designer's request. The 20 original dated rooms were played through in Unity 2022.3.36f1 using Rover.OnPress, OnRelease and OnDrop. Baseline results are below. Input steps count a simultaneous group as one step. Runs used the authored orientation and the room-test stopwatch. These are automated routes, not human timing targets.

Ten proposed revisions (004, 007, 008, 009, 010, 014, 015, 017, 018, 019) and ten additions (021–030) are staged in Library/DatedGardenPlaytest/Assets/Prefabs/Levels/dated. They have not yet been promoted to the live Assets folder. Ten strong or useful reference rooms are left intact. Original room 006 supports all four hands grabbing together and then planting together. Room 013's forced regrip earns its longer route. Room 017 had 45 steps, 27 of them moves, with late cleanup. 007, 008, 010, 014, 015 contain repeated deliveries. 009 and 018 have recognizable geometry but relatively generic play. 002, 003 and 020 are good examples where the first pickup changes access. Keep unrelated lobby edits intact.

New rooms test parallel carry, forced handoff, cargo swapping, crossed targets, recoloring a seed with a cloud, a lever and door, a timed gate with a bypass, two handoffs on different axes, a three-hand opening and choosing which spare seed to dye.

Resume by inspecting tmp/dated-iteration-test.log for all 30 ROUTE PASS entries or the first ROUTE FAIL. Unity may have continued after the checkpoint. Finish or fix the playthrough before promotion. In the isolated project, run Unity -batchmode -nographics -projectPath <Library/DatedGardenPlaytest> -executeMethod GardenRouteRun.Run -logFile <absolute log path>. The project contains routes.txt. If needed, regenerate routes with the bundled Python and Library/garden_routes.py 30. Working Python sources, route lists and the temporary Unity harness are backed up beside this note, outside Library. To rebuild an isolated project, copy Assets, Packages, ProjectSettings; place GardenRouteRun.cs under its Assets/Editor and the Python files under the real Library, then use build_dated_iteration.py on the isolated dated folder. The builder checks against the original layouts and preserves old metadata.

After all final rooms pass, use Library/build_dated_iteration.py against Assets/Prefabs/Levels/dated, confirm the live room prefab and meta bytes match the tested copies and only intended files changed. Finish the requested design evaluation and give the user the room numbers, changes, playtest results and limitations. A visual before/after overview or report would help with selection.

## Baseline
ROUTE PASS 26_11_27_001 1.62s; 6 gestures
ROUTE PASS 26_11_27_002 3.70s; 8 gestures
ROUTE PASS 26_11_27_003 3.23s; 19 gestures
ROUTE PASS 26_11_27_004 5.08s; 13 gestures
ROUTE PASS 26_11_27_005 4.25s; 15 gestures
ROUTE PASS 26_11_27_006 0.55s; 2 gestures
ROUTE PASS 26_11_27_007 6.93s; 18 gestures
ROUTE PASS 26_11_27_008 8.93s; 24 gestures
ROUTE PASS 26_11_27_009 7.73s; 21 gestures
ROUTE PASS 26_11_27_010 3.75s; 15 gestures
ROUTE PASS 26_11_27_011 2.45s; 9 gestures
ROUTE PASS 26_11_27_012 5.15s; 19 gestures
ROUTE PASS 26_11_27_013 6.83s; 35 gestures
ROUTE PASS 26_11_27_014 9.40s; 20 gestures
ROUTE PASS 26_11_27_015 7.28s; 26 gestures
ROUTE PASS 26_11_27_016 3.00s; 13 gestures
ROUTE PASS 26_11_27_017 8.13s; 45 gestures
ROUTE PASS 26_11_27_018 7.80s; 18 gestures
ROUTE PASS 26_11_27_019 7.10s; 23 gestures
ROUTE PASS 26_11_27_020 7.25s; 25 gestures

## Iteration results at checkpoint
ROUTE PASS 26_11_27_001 1.62s; 6 gestures
ROUTE PASS 26_11_27_002 3.70s; 8 gestures
ROUTE PASS 26_11_27_003 3.23s; 19 gestures
ROUTE PASS 26_11_27_004 3.50s; 11 gestures
ROUTE PASS 26_11_27_005 4.25s; 15 gestures
ROUTE PASS 26_11_27_006 0.55s; 2 gestures
ROUTE PASS 26_11_27_007 4.60s; 14 gestures
ROUTE PASS 26_11_27_008 3.58s; 15 gestures
ROUTE PASS 26_11_27_009 3.30s; 11 gestures
ROUTE PASS 26_11_27_010 3.13s; 13 gestures
ROUTE PASS 26_11_27_011 2.45s; 9 gestures
ROUTE PASS 26_11_27_012 5.15s; 19 gestures
ROUTE PASS 26_11_27_013 6.83s; 35 gestures
ROUTE PASS 26_11_27_014 5.50s; 17 gestures
ROUTE PASS 26_11_27_015 3.83s; 18 gestures
ROUTE PASS 26_11_27_016 3.00s; 13 gestures
ROUTE PASS 26_11_27_017 2.75s; 17 gestures
ROUTE PASS 26_11_27_018 3.13s; 11 gestures
ROUTE PASS 26_11_27_019 2.43s; 15 gestures
ROUTE PASS 26_11_27_020 7.25s; 25 gestures
ROUTE PASS 26_11_27_021 0.87s; 3 gestures
ROUTE PASS 26_11_27_022 2.33s; 11 gestures
ROUTE PASS 26_11_27_023 2.38s; 11 gestures
ROUTE PASS 26_11_27_024 2.28s; 8 gestures
ROUTE PASS 26_11_27_025 1.17s; 3 gestures
ROUTE PASS 26_11_27_026 3.30s; 15 gestures
ROUTE PASS 26_11_27_027 4.08s; 10 gestures
ROUTE PASS 26_11_27_028 5.13s; 21 gestures
ROUTE FAIL index=28 step=9 command=grab,1,1,2 System.Exception: timed out

## Completed after resuming

All 30 final routes passed in tmp/dated-iteration-final.log. Room 029 solved the three-hand opening by moving down one cell before dropping teal, circling below the seed, and regripping with the upward hand. Room 030 also passed. All final live prefabs and their metadata matched the playtested copies byte for byte. The review is in output/Dated room playtest review.html.
