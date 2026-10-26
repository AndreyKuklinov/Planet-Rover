using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Image transcription is kept as data; the rooms have no dependency on this editor tool.
[InitializeOnLoad]
public static class BuildAndreyRooms
{
    const string Folder = "Assets/Prefabs/Levels/showcase 2026/andrey";
    const string Manifest = "Tools/AndreyRooms/rooms.json";
    const string Pending = "PlanetRover.AndreyValidation";
    [Serializable] class Pack { public RoomSpec[] rooms; }
    [Serializable] class RoomSpec { public int number, width, height; public string name; public string[] walls; public ObjectSpec[] objects; }
    [Serializable] class ObjectSpec { public string prefab; public int x,y; public string[] requests; }
    [Serializable] class SavedResults { public string[] rows; }
    static Pack pack;
    static readonly List<string> results = new List<string>();
    static readonly Direction[] directions = { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
    static Vector2Int Cell(int x,int y) => new Vector2Int(x-11,5-y);
    static string PathFor(int i) => Folder+"/"+pack.rooms[i].name+".prefab";
    static ItemData Data(string name) => AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Scriptable Objects/Items/"+name+".asset");
    static void Check(bool value,string message) { if(!value)throw new InvalidOperationException(message); }
    static void Read() { pack=JsonUtility.FromJson<Pack>(File.ReadAllText(Manifest));Check(pack.rooms.Length==12,"Expected twelve reference rooms"); }
    static BuildAndreyRooms()
    {
        if(!SessionState.GetBool(Pending,false))return;
        Read();
        roomIndex=SessionState.GetInt(Pending+".Room",0);reflection=SessionState.GetInt(Pending+".Reflection",0);recovery=SessionState.GetBool(Pending+".Recovery",false);
        var saved=SessionState.GetString(Pending+".Results","");if(!string.IsNullOrEmpty(saved))results.AddRange(JsonUtility.FromJson<SavedResults>(saved).rows);
        deadline=EditorApplication.timeSinceStartup+360;EditorApplication.update+=InstallTestLoop;
    }
    public static void Build()
    {
        try
        {
            Read();
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Prefabs/Levels/showcase 2026","andrey");
            for(int i=0;i<pack.rooms.Length;i++)BuildOne(i);
            AssetDatabase.SaveAssets();
            RoomBuildValidator.ValidateRoomAssets(Enumerable.Range(0,12).Select(PathFor));
            WriteReport(false);
            Debug.Log("ANDREY: Built all 12 exact 22x12 image transcriptions; overlap validation passed.");
            Validate();
        }
        catch(Exception ex) { Debug.LogException(ex);EditorApplication.Exit(1); }
    }
    static void BuildOne(int index)
    {
        var spec=pack.rooms[index];var path=PathFor(index);bool existing=File.Exists(path);
        var root=PrefabUtility.LoadPrefabContents(existing?path:"Assets/Prefabs/Levels/Empty Level.prefab");
        try
        {
            root.name=spec.name;root.transform.position=new Vector3(.5f,.5f,0);
            var room=root.GetComponent<Room>();var grid=room.Grid;
            if(!existing)
            {
                var options=new SerializedObject(room);
                foreach(string option in new[]{"isTimed","canUseReset","allowHorizontalReflection","allowVerticalReflection"})options.FindProperty(option).boolValue=true;
                options.FindProperty("<StandardTime>k__BackingField").floatValue=60;
                options.ApplyModifiedPropertiesWithoutUndo();
            }
            var markers=new SerializedObject(grid.Bounds);var unityGrid=grid.GetComponent<Grid>();
            ((Transform)markers.FindProperty("bottomLeftMarker").objectReferenceValue).position=unityGrid.CellToWorld(new Vector3Int(-11,-6,0));
            ((Transform)markers.FindProperty("topRightMarker").objectReferenceValue).position=unityGrid.CellToWorld(new Vector3Int(11,6,0));
            var parent=root.transform.Find("Objects");while(parent.childCount>0)UnityEngine.Object.DestroyImmediate(parent.GetChild(0).gameObject);
            var swaps=root.transform.Find("Swap Markers");if(swaps!=null)while(swaps.childCount>0)UnityEngine.Object.DestroyImmediate(swaps.GetChild(0).gameObject);
            var occupied=new HashSet<Vector2Int>();
            Action<string,int,int,ObjectSpec> place=(kind,x,y,settings)=>
            {
                Check(occupied.Add(Cell(x,y)),spec.name+" duplicate cell "+x+","+y);
                var source="Assets/Prefabs/"+(kind=="Rover"?"Rover/Rover":"Level Objects/"+kind)+".prefab";
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(source);Check(prefab!=null,"Missing prefab "+source);
                var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);obj.transform.position=grid.CellToWorld(Cell(x,y));
                if(kind=="Rocket")
                {
                    var so=new SerializedObject(obj.GetComponent<ItemCollector>());var items=so.FindProperty("<RequiredItems>k__BackingField");
                    items.arraySize=settings.requests.Length;
                    for(int j=0;j<items.arraySize;j++) {var data=Data(settings.requests[j]);Check(data!=null,"Unknown request "+settings.requests[j]);items.GetArrayElementAtIndex(j).objectReferenceValue=data;}
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(obj.transform);
            };
            Check(spec.width==22&&spec.height==12&&spec.walls.Length==12,"Invalid reference dimensions");
            for(int y=0;y<12;y++)for(int x=0;x<22;x++)if(spec.walls[y][x]=='#')place("Obstacle",x,y,null);
            foreach(var obj in spec.objects) {Check(spec.walls[obj.y][obj.x]!='#',"Reference object on wall");place(obj.prefab,obj.x,obj.y,obj);}
            Check(spec.objects.Count(o=>o.prefab=="Rover")==1,"Need one rover");
            Check(grid.IsWithinBounds(Cell(0,0))&&grid.IsWithinBounds(Cell(21,11))&&!grid.IsWithinBounds(Cell(-1,5))&&!grid.IsWithinBounds(Cell(22,5))&&!grid.IsWithinBounds(Cell(10,-1))&&!grid.IsWithinBounds(Cell(10,12)),"Wrong room bounds");
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally {PrefabUtility.UnloadPrefabContents(root);}
    }
    public static void Validate()
    {
        SessionState.EraseInt(Pending+".Room");SessionState.EraseInt(Pending+".Reflection");SessionState.EraseBool(Pending+".Recovery");SessionState.EraseString(Pending+".Results");
        Read();EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");SessionState.SetBool(Pending,true);EditorApplication.isPlaying=true;
    }
    public static void ResumeValidation()
    {
        Read();
        var rows=File.ReadAllLines(Folder+"/README.md").Where(line=>line.Contains(" | H=")||line.Contains(" | early bucket + rewind |")).ToArray();
        var cases=new List<(int room,int flip,bool recover)>();
        for(int i=0;i<12;i++) {for(int f=0;f<4;f++)cases.Add((i,f,false));if(i>=8)cases.Add((i,0,true));}
        Check(rows.Length<cases.Count,"Validation is already complete");var next=cases[rows.Length];
        SessionState.SetInt(Pending+".Room",next.room);SessionState.SetInt(Pending+".Reflection",next.flip);SessionState.SetBool(Pending+".Recovery",next.recover);
        SessionState.SetString(Pending+".Results",JsonUtility.ToJson(new SavedResults{rows=rows}));
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");SessionState.SetBool(Pending,true);EditorApplication.isPlaying=true;
        Debug.Log("ANDREY: Resuming after "+rows.Length+" successful complete runs.");
    }

    // A test driver, not a replacement rules engine: every selected operation executes
    // through the actual Rover and RoomResetController in real Play Mode frames.
    class Command { public string kind; public Vector2Int target; public Direction direction; public Command(string k,Vector2Int p,Direction d) {kind=k;target=p;direction=d;} }
    class Plan { public List<Command> commands;public float cost; }
    class Tree { public Vector2Int start; public Dictionary<Vector2Int,Vector2Int> parent=new Dictionary<Vector2Int,Vector2Int>(); public Dictionary<Vector2Int,int> depth=new Dictionary<Vector2Int,int>(); }
    static RoomLoader loader;static LevelManager manager;static Room room;static Rover rover;static GridObject roverObject;
    static int roomIndex,reflection,stage,actions,movements,resets;
    static bool recovery,earlyBucketSpent,reflectedH,reflectedV;
    static double deadline,commandDeadline;static float started;
    static Queue<Command> commands=new Queue<Command>();static Command running;static RoverHand hand;static bool released;
    static Tree reachable;
    static bool testLoopInstalled;
    static readonly Dictionary<Vector2Int,GridObject> planningObjects=new Dictionary<Vector2Int,GridObject>();
    static readonly HashSet<GridObject> planningPassables=new HashSet<GridObject>();
    static void InstallTestLoop()
    {
        if(testLoopInstalled||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        Application.runInBackground=true;
        if(Time.time<.1f)return;
        // Poll after every actual game update rather than at the independent
        // editor-update frequency, so a release cell cannot be skipped.
        var loop=UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
        for(int i=0;i<loop.subSystemList.Length;i++)
        {
            if(loop.subSystemList[i].type!=typeof(UnityEngine.PlayerLoop.PostLateUpdate))continue;
            var systems=loop.subSystemList[i].subSystemList.ToList();
            systems.Add(new UnityEngine.LowLevel.PlayerLoopSystem{type=typeof(BuildAndreyRooms),updateDelegate=Tick});
            loop.subSystemList[i].subSystemList=systems.ToArray();
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(loop);testLoopInstalled=true;
            Time.captureDeltaTime=1f/60f;
            foreach(var device in UnityEngine.InputSystem.InputSystem.devices.ToArray())
                if(device is UnityEngine.InputSystem.Keyboard||device is UnityEngine.InputSystem.Gamepad)UnityEngine.InputSystem.InputSystem.DisableDevice(device);
            Debug.Log("ANDREY: Isolated input driver attached after game updates with a fixed 1/60 test timestep.");
            return;
        }
        throw new InvalidOperationException("No post-update player loop for validation");
    }
    static IEnumerable<RoverHand> Hands => rover.GetComponentsInChildren<RoverHand>();
    static IEnumerable<GridObject> Ground => room.GetComponentsInChildren<GridObject>().Where(o=>room.Grid.Objects.Contains(o));
    static IEnumerable<ItemCollector> Collectors => room.GetComponentsInChildren<ItemCollector>().Where(c=>c.RequiredItems.Count>0);
    static int Demand(string item) {var data=Data(item);return Collectors.Sum(c=>c.RequiredItems.Count(i=>i==data));}
    static Vector2Int Position(GridObject obj) => room.Grid.WorldToCell(obj.transform.position);
    static bool Passable(GridObject obj) => !object.ReferenceEquals(obj,null)&&planningPassables.Contains(obj);
    static GridObject At(Vector2Int p) {planningObjects.TryGetValue(p,out var obj);return obj;}
    // Bounds and occupancy were checked against the actual RoomGrid first. Cache
    // the fixed reference rectangle for the search, avoiding millions of native
    // Grid/Transform calls. Actual rover actions still use the real room rules.
    static bool InBounds(Vector2Int p) => p.x>=-11&&p.x<=10&&p.y>=-6&&p.y<=5;
    static Tree Reach(Vector2Int start,Vector2Int? blocked=null)
    {
        var t=new Tree{start=start};t.depth[start]=0;var q=new Queue<Vector2Int>();q.Enqueue(start);
        while(q.Count>0)
        {
            var from=q.Dequeue();
            foreach(var d in directions)
            {
                var step=DirectionVector.GetVector2Int(d);
                for(var p=from+step;InBounds(p);p+=step)
                {
                    if(blocked.HasValue&&p==blocked.Value)break;
                    var obj=At(p);if(!object.ReferenceEquals(obj,null)&&!Passable(obj))break;
                    if(object.ReferenceEquals(obj,null)&&!t.depth.ContainsKey(p)) {t.parent[p]=from;t.depth[p]=t.depth[from]+1;q.Enqueue(p);}
                }
            }
        }
        return t;
    }
    static bool Line(Vector2Int from,Vector2Int target,Direction d,Vector2Int? blocked=null)
    {
        var delta=target-from;var step=DirectionVector.GetVector2Int(d);
        if(delta==Vector2Int.zero||delta.x*step.y!=delta.y*step.x||delta.x*step.x+delta.y*step.y<=0)return false;
        for(var p=from+step;p!=target;p+=step)
        {
            if(blocked.HasValue&&p==blocked.Value)return false;
            var obj=At(p);if(!object.ReferenceEquals(obj,null)&&!Passable(obj))return false;
        }
        return true;
    }
    static List<Command> Moves(Tree t,Vector2Int destination)
    {
        var cells=new List<Vector2Int>();for(var p=destination;p!=t.start;p=t.parent[p])cells.Add(p);cells.Reverse();
        return cells.Select(p=>new Command("move",p,Direction.Up)).ToList();
    }
    static Plan ActionPlan(IEnumerable<GridObject> targets,Func<RoverHand,GridObject,bool> valid)
    {
        Plan best=null;
        foreach(var obj in targets.ToArray())foreach(var h in Hands)
        {
            if(!valid(h,obj))continue;var target=Position(obj);
            foreach(var pair in reachable.depth)
            {
                if(!Line(pair.Key,target,h.Direction))continue;
                float cost=pair.Value*3+Vector2Int.Distance(pair.Key,target)*.08f;
                if(best!=null&&cost>=best.cost)continue;
                var list=Moves(reachable,pair.Key);list.Add(new Command("act",target,h.Direction));best=new Plan{cost=cost,commands=list};
            }
        }
        return best;
    }
    static Plan Transfer(IEnumerable<GridObject> targets,Func<IGrabbable,GridObject,bool> valid)
    {
        Plan best=null;var targetArray=targets.ToArray();
        foreach(var held in Hands.Where(h=>h.IsHoldingObject))foreach(var empty in Hands.Where(h=>!h.IsHoldingObject))
        {
            // First establish that the new arm can perform a currently blocked task.
            bool useful=targetArray.Any(obj=>valid(held.HeldObject,obj)&&reachable.depth.Keys.Any(p=>Line(p,Position(obj),empty.Direction)));
            if(!useful)continue;
            foreach(var dropper in reachable.depth)
            {
                if(best!=null&&dropper.Value*3>=best.cost)continue;
                var cell=dropper.Key+DirectionVector.GetVector2Int(held.Direction);
                if(!InBounds(cell)||!object.ReferenceEquals(At(cell),null))continue;
                var after=Reach(dropper.Key,cell);
                foreach(var pickup in after.depth)
                {
                    if(!Line(pickup.Key,cell,empty.Direction,cell))continue;
                    float cost=(dropper.Value+pickup.Value)*3+Vector2Int.Distance(pickup.Key,cell)*.08f;
                    if(best!=null&&cost>=best.cost)continue;
                    var list=Moves(reachable,dropper.Key);list.Add(new Command("drop",cell,held.Direction));list.AddRange(Moves(after,pickup.Key));list.Add(new Command("act",cell,empty.Direction));
                    best=new Plan{commands=list,cost=cost};
                }
            }
        }
        return best;
    }
    static bool CanUse(IGrabbable held,GridObject obj) => obj.TryGetComponent<IInteractable>(out var action)&&action.CanInteractWith(held);
    static bool BucketHeld(RoverHand h,bool filled) => h.HeldObject!=null&&h.HeldObject.GridObject.TryGetComponent<Bucket>(out var b)&&b.IsFilled==filled;
    static bool FlowerHeld(RoverHand h) => h.HeldObject!=null&&h.HeldObject.GridObject.TryGetComponent<Plant>(out var p)&&p.IsGrown;
    static Plan DeliverBuckets() => ActionPlan(Collectors.Select(c=>c.GetComponent<GridObject>()),(h,o)=>BucketHeld(h,false)&&CanUse(h.HeldObject,o));
    static Plan TakeBucket() => ActionPlan(Ground.Where(o=>o.GetComponent<Bucket>()!=null),(h,o)=>!h.IsHoldingObject);
    static void Queue(Plan plan) {foreach(var c in plan.commands)commands.Enqueue(c);}
    static void ChooseWork()
    {
        Check(actions<350&&resets<12,"Driver exceeded action/reset limit");
        planningObjects.Clear();planningPassables.Clear();
        foreach(var obj in Ground)
        {
            if(obj==roverObject)continue;
            planningObjects[Position(obj)]=obj;
            if(obj.TryGetComponent<IPassable>(out var pass)&&pass.CanHandPassThrough)planningPassables.Add(obj);
        }
        reachable=Reach(room.Grid.WorldToCell(rover.transform.position));
        Plan plan;
        if(recovery&&!earlyBucketSpent)
        {
            plan=DeliverBuckets();
            if(plan!=null) {Queue(plan);earlyBucketSpent=true;commands.Enqueue(new Command("reset",default,default));return;}
            plan=Transfer(Collectors.Select(c=>c.GetComponent<GridObject>()),(held,o)=>held.GridObject.GetComponent<Bucket>()?.IsFilled==false&&CanUse(held,o));
            if(plan!=null){Queue(plan);return;}
            plan=TakeBucket();Check(plan!=null,"Could not acquire bucket for early-spend test");Queue(plan);return;
        }
        var rockets=Collectors.Select(c=>c.GetComponent<GridObject>()).ToArray();
        plan=ActionPlan(rockets,(h,o)=>FlowerHeld(h)&&CanUse(h.HeldObject,o));if(plan!=null){Queue(plan);return;}
        plan=Transfer(rockets,(held,o)=>held.GridObject.GetComponent<Plant>()?.IsGrown==true&&CanUse(held,o));if(plan!=null){Queue(plan);return;}
        var grown=Ground.Where(o=>o.GetComponent<Plant>()?.IsGrown==true).ToArray();
        if(Demand("Flower A")>0)
        {
            // Harvesting with the watering arm exchanges the empty bucket onto
            // the flower's cell. This is necessary at single-sided alcoves.
            plan=ActionPlan(grown,(h,o)=>!h.IsHoldingObject||BucketHeld(h,false));if(plan!=null){Queue(plan);return;}
            var plants=Ground.Where(o=>o.GetComponent<Plant>()?.IsGrown==false).ToArray();
            plan=ActionPlan(plants,(h,o)=>BucketHeld(h,true)&&CanUse(h.HeldObject,o));if(plan!=null){Queue(plan);return;}
            if(plants.Length>0)
            {
                var liquids=Ground.Where(o=>o.GetComponent<Liquid>()!=null).ToArray();
                plan=ActionPlan(liquids,(h,o)=>BucketHeld(h,false)&&CanUse(h.HeldObject,o));if(plan!=null){Queue(plan);return;}
                if(!Hands.Any(h=>h.HeldObject?.GridObject.GetComponent<Bucket>()!=null)) {plan=TakeBucket();if(plan!=null){Queue(plan);return;}}
                plan=Transfer(plants,(held,o)=>held.GridObject.GetComponent<Bucket>()?.IsFilled==true&&CanUse(held,o));if(plan!=null){Queue(plan);return;}
                plan=Transfer(liquids,(held,o)=>held.GridObject.GetComponent<Bucket>()?.IsFilled==false&&CanUse(held,o));if(plan!=null){Queue(plan);return;}
            }
            plan=Transfer(rockets,(held,o)=>held.GridObject.GetComponent<Plant>()?.IsGrown==true&&CanUse(held,o));if(plan!=null){Queue(plan);return;}
        }
        // Before restoring an exhausted supply, bank useful empty buckets too.
        plan=DeliverBuckets();if(plan!=null){Queue(plan);return;}
        if(Demand("Flower A")==0)
        {
            plan=TakeBucket();if(plan!=null){Queue(plan);return;}
            plan=Transfer(rockets,(held,o)=>held.GridObject.GetComponent<Bucket>()?.IsFilled==false&&CanUse(held,o));if(plan!=null){Queue(plan);return;}
        }
        commands.Enqueue(new Command("reset",default,default));
    }
    static int SeedFor(int mode)
    {
        var old=UnityEngine.Random.state;
        try
        {
            for(int seed=1;seed<1000;seed++)
            {
                UnityEngine.Random.InitState(seed);bool h=UnityEngine.Random.value<.5f,v=UnityEngine.Random.value<.5f;
                if(h==((mode&1)!=0)&&v==((mode&2)!=0))return seed;
            }
            throw new Exception("No reflection seed");
        }
        finally {UnityEngine.Random.state=old;}
    }
    static void Refresh() {room=loader.CurrentRoom;rover=room.GetComponentInChildren<Rover>();roverObject=rover.GetComponent<GridObject>();}
    static void BeginRoom()
    {
        UnityEngine.Random.InitState(SeedFor(reflection));
        manager.StartLevel(LevelData.CreateRoomTest(AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(roomIndex)).GetComponent<Room>()));
        reflectedH=(reflection&1)!=0;reflectedV=(reflection&2)!=0;stage=1;commands.Clear();running=null;hand=null;
        actions=movements=resets=0;earlyBucketSpent=false;started=Time.time;deadline=EditorApplication.timeSinceStartup+360;
    }
    static Vector2Int Reflected(int x,int y) => Cell(reflectedH?21-x:x,reflectedV?11-y:y);
    static void VerifyLayout()
    {
        var s=pack.rooms[roomIndex];Check(room.IsTimed&&room.CanUseReset&&room.AllowHorizontalReflection&&room.AllowVerticalReflection&&room.StandardTime==60,"Room defaults differ");
        for(int y=0;y<12;y++)for(int x=0;x<22;x++)
        {
            var authored=s.objects.FirstOrDefault(o=>o.x==x&&o.y==y);
            string kind=s.walls[y][x]=='#'?"Obstacle":authored?.prefab;
            var obj=room.Grid.Objects.GetObject(Reflected(x,y));Check((obj==null)==(kind==null),"Occupancy mismatch "+x+","+y);
            if(obj==null)continue;Check(obj.name==kind,"Wrong reference object at "+x+","+y+": "+obj.name);
            if(kind=="Rocket")Check(obj.GetComponent<ItemCollector>().RequiredItems.SequenceEqual(authored.requests.Select(Data)),"Rocket request mismatch");
        }
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            Check(EditorApplication.timeSinceStartup<deadline,"Timed out, command "+running?.kind);
            if(stage==0)
            {
                if(Time.time<1)return;
                manager=UnityEngine.Object.FindObjectOfType<LevelManager>();loader=UnityEngine.Object.FindObjectOfType<RoomLoader>();
                if(manager==null||loader==null||loader.CurrentRoom==null)return;
                var lobby=UnityEngine.Object.FindObjectOfType<LobbyManager>();if(lobby!=null)lobby.IsRoomTesting=true;
                manager.BeginRoomTestSession();BeginRoom();return;
            }
            if(stage==1) {Refresh();if(room.GetComponentsInChildren<GridObject>().Any(o=>o.RoomGrid==null))return;VerifyLayout();stage=2;return;}
            if(stage==3) {Refresh();stage=2;return;}
            if(running!=null){TickCommand();return;}
            if(manager.IsRoomTestFinished)
            {
                Check(manager.CompletedRoomCount==1&&Demand("Bucket")==0&&Demand("Flower A")==0,"Incomplete objective registration");
                string row="| "+pack.rooms[roomIndex].name+" | "+(recovery?"early bucket + rewind":"H="+reflectedH+", V="+reflectedV)+" | "+actions+" | "+movements+" | "+resets+" |";
                results.Add(row);Debug.Log("ANDREY: PASS "+row);
                if(recovery) {recovery=false;roomIndex++;reflection=0;}
                else if(reflection<3)reflection++;
                else if(roomIndex>=8) {recovery=true;reflection=0;}
                else {roomIndex++;reflection=0;}
                if(roomIndex==12) {WriteReport(true);Debug.Log("ANDREY: PASS all 52 complete runs (four orientations per room plus four early-bucket recovery runs).");SessionState.EraseBool(Pending);EditorApplication.Exit(0);return;}
                BeginRoom();return;
            }
            if(commands.Count==0)ChooseWork();
            running=commands.Dequeue();StartCommand();
        }
        catch(Exception ex)
        {
            Debug.LogError("ANDREY: FAIL "+pack.rooms[roomIndex].name+" reflection="+reflection+" recovery="+recovery+" actions="+actions+" resets="+resets);
            Debug.LogException(ex);SessionState.EraseBool(Pending);WriteReport(false);EditorApplication.Exit(1);
        }
    }
    static Direction DirectionTo(Vector2Int d) => d.x>0?Direction.Right:d.x<0?Direction.Left:d.y>0?Direction.Up:Direction.Down;
    static void Finish(){running=null;hand=null;}
    static void StartCommand()
    {
        commandDeadline=EditorApplication.timeSinceStartup+15;
        if(running.kind=="reset")
        {
            var old=room;var rockets=Collectors.ToArray();var counts=rockets.Select(c=>c.RequiredItems.Count).ToArray();float time=manager.ElapsedTime;
            loader.ResetController.ResetRoom();Check(loader.CurrentRoom!=old,"Room rewind failed");Refresh();
            for(int i=0;i<rockets.Length;i++)Check(rockets[i]!=null&&rockets[i].transform.IsChildOf(room.transform)&&rockets[i].RequiredItems.Count==counts[i],"Rocket progress lost across reset");
            Check(manager.ElapsedTime>=time,"Reset refunded scoring time");Check(manager.ObjectiveTracker.AllObjectives.Except(manager.ObjectiveTracker.FulfilledObjectives).All(o=>o!=null),"An unfinished objective was lost");
            resets++;stage=3;Finish();return;
        }
        if(running.kind=="drop")
        {
            hand=Hands.Single(h=>h.Direction==running.direction);Check(hand.IsHoldingObject,"No transfer cargo");
            rover.OnDrop(new[]{hand.Direction});Check(!hand.IsHoldingObject,"Required drop blocked");Finish();return;
        }
        var dir=running.kind=="move"?DirectionTo(running.target-room.Grid.WorldToCell(rover.transform.position)):running.direction;
        hand=Hands.Single(h=>h.Direction==dir);
        if(running.kind=="move")
        {
            var from=room.Grid.WorldToCell(rover.transform.position);var stop=room.Grid.GetStoppingSquare(from,dir,hand.HeldObject);
            Check(Vector2Int.Distance(from,stop)+.01f>=Vector2Int.Distance(from,running.target),"Planner selected blocked movement: "+from+" -> "+running.target+", stops at "+stop);
            movements++;
        }
        else
        {
            var target=room.Grid.Objects.GetObject(running.target);Check(target!=null,"Interaction target vanished");
            Check(target.TryGetComponent<IGrabbable>(out var grabbable)&&grabbable.CanBeGrabbed||CanUse(hand.HeldObject,target),"Invalid chosen interaction");actions++;
        }
        released=false;rover.OnPress(dir);
    }
    static void TickCommand()
    {
        Check(EditorApplication.timeSinceStartup<commandDeadline,"Command stalled: "+running.kind+" target="+running.target+" hand="+hand.Direction+" state="+hand.HandState+" hand cell="+room.Grid.WorldToCell(hand.HandPosition)+" rover="+room.Grid.WorldToCell(rover.transform.position));
        if(!released) {if(room.Grid.WorldToCell(hand.HandPosition)!=running.target)return;rover.OnRelease(hand.Direction);released=true;return;}
        if(rover.IsMoving||hand.HandState!=HandState.None)return;
        if(running.kind=="move")Check(room.Grid.WorldToCell(rover.transform.position)==running.target,"Movement missed its target");
        Finish();
    }
    static void WriteReport(bool passed)
    {
        var text=new StringBuilder("# Andrey's image rooms\n\nTwelve faithful transcriptions of the supplied legend and numbered scheme, using the original 22 x 12 cell grid including walls. Red maps to the game's orange (A). Coordinates in rooms.json are zero-based, left-to-right and top-to-bottom; rocket coordinates refer to the rocket body, not the request icons above it.\n\nAll new rooms have StandardTime = 60 seconds, timing and room reset enabled, and both startup reflections enabled directly on the Room component. Existing room options are preserved on a rebuild. No shared config assets or gameplay-rule changes are introduced. These standalone rooms are not added to existing level pools.\n\n![Reference layouts](overview.png)\n\n| Room | Source family | Buckets | Liquid portions | Saplings | Rocket requests |\n|---|---|---:|---:|---:|---|\n");
        foreach(var s in pack.rooms)
        {
            string family=s.number<=4?"Ведро и ростки / Buckets and sprouts":s.number<=8?"Учимся ресетить / Learning to reset":"Случайная потеря ведра / Accidental bucket loss";
            string demand=string.Join("; ",s.objects.Where(o=>o.prefab=="Rocket").Select(o=>"("+o.x+","+o.y+"): "+string.Join(", ",o.requests.GroupBy(r=>r).Select(g=>g.Count()+" x "+g.Key))));
            text.Append("| "+s.name+" | "+family+" | "+s.objects.Count(o=>o.prefab=="Bucket")+" | "+s.objects.Count(o=>o.prefab=="Liquid A")+" | "+s.objects.Count(o=>o.prefab=="Sapling A")+" | "+demand+" |\n");
        }
        text.Append("\n## Validation\n\n"+(passed?"PASS: 52 full Unity Play Mode runs: all four horizontal/vertical reflection combinations for every room, plus an early bucket delivery followed by rewind in rooms 09-12. Each run checked all authored wall/object cells and request counts, performed real arm extension/release, bucket filling, watering, harvesting and delivery, and registered final room completion. Rewinds preserved outstanding rocket requests and did not restore the scoring clock.\n":"Validation is pending or incomplete; successful runs so far are recorded below.\n"));
        text.Append("\nThe driver invokes the existing RoomResetController.ResetRoom method to verify state persistence. It does not test physical player input charging or establish human completion times; 60 seconds is provisional. Counts describe executable test routes, not minimum actions or minimum rewinds. Reflections use deterministic random seeds and the actual room startup reflection implementation. Later recovery checks use a fixed test timestep and sample after each game update to avoid missed release cells; this affects only the isolated driver, not the room assets. No source geometry or resource counts were modified to make a solution work. The source image, legend, wall extraction utility and manually reviewed manifest are in Tools/AndreyRooms.\n\n| Room | Orientation / scenario | Interactions | Movement segments | Rewinds |\n|---|---|---:|---:|---:|\n");
        foreach(var row in results)text.Append(row+"\n");
        text.Append("\n## Resource notes\n\n05 needs a rewind because two buckets must satisfy three requests. 06 needs a rewind to harvest its single sapling twice. 07 and 08 require restocking both flower or bucket supplies; bank useful deliveries before restoring the room. In 09 and 10, delivering the only bucket before gardening requires recovery. 11 provides eight buckets for four bucket requests and three flowers. 12 provides four buckets, nine liquids, twelve saplings and nine flower requests; three saplings can remain unused. Rocket launches and fulfilled requests remain preserved across resets.\n");
        File.WriteAllText(Folder+"/README.md",text.ToString());
    }
}
