using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Authoring-only specification and real-input smoke-test harness for the 60-second room studies.
// Kept outside the room prefabs: none of the rooms depends on this class at runtime.
[InitializeOnLoad]
public static class BuildMinuteRooms
{
    const string Folder = "Assets/Prefabs/Levels/showcase 2026/60_rooms";
    const string Pending = "PlanetRover.MinuteValidation";
    class Obj
    {
        public string prefab; public int x, y; public string[] requests; public bool timed;
        public Obj(string p, int a, int b, params string[] r) { prefab=p; x=a; y=b; requests=r; }
    }
    class Cmd
    {
        public string op; public int x,y; public Direction dir; public float seconds;
        public Cmd(string o, int a=0, int b=0, Direction d=Direction.Right, float s=0) { op=o;x=a;y=b;dir=d;seconds=s; }
    }
    class Spec
    {
        public string name, challenge, geometry, idea, caveat="";
        public bool[,] floor = new bool[17,9]; public int sx=3,sy=4;
        public List<Obj> objects = new List<Obj>(); public List<Cmd> commands = new List<Cmd>();
        public Spec(string n,string c,string g,string text) { name=n;challenge=c;geometry=g;idea=text; Rect(1,1,15,7); }
        public Spec Rect(int x1,int y1,int x2,int y2) { for(int x=x1;x<=x2;x++) for(int y=y1;y<=y2;y++) floor[x,y]=true;return this; }
        public Spec Empty() { floor=new bool[17,9];return this; }
        public Spec Wall(int x1,int y1,int x2,int y2) { for(int x=x1;x<=x2;x++) for(int y=y1;y<=y2;y++) floor[x,y]=false;return this; }
        public Spec Start(int x,int y) { sx=x;sy=y;return this; }
        public Spec Add(string p,int x,int y,params string[] r) { objects.Add(new Obj(p,x,y,r));return this; }
        public Spec Timer(int x,int y,float open=5) { var o=new Obj("Timed Emitter",x,y,open.ToString(System.Globalization.CultureInfo.InvariantCulture));o.timed=true;objects.Add(o);return this; }
        public Spec Do(params Cmd[] c) { commands.AddRange(c);return this; }
    }
    static Cmd Act(int x,int y,Direction d=Direction.Right) => new Cmd("act",x,y,d);
    static Cmd Go(int x,int y) => new Cmd("go",x,y);
    static Cmd Drop(Direction d=Direction.Right) => new Cmd("drop",d:d);
    static Cmd Reset() => new Cmd("reset");
    static Cmd WaitDoor(int x,int y) => new Cmd("door",x,y);
    static Cmd WaitSaw(int x,int y) => new Cmd("saw",x,y);
    static Cmd Delay(float s) => new Cmd("delay",s:s);
    static Cmd Parallel(params Direction[] dirs) => new Cmd("parallel",a:dirs.Aggregate(0,(v,d)=>v|(1<<(int)d)));
    static readonly Direction U=Direction.Up,D=Direction.Down,L=Direction.Left,R=Direction.Right;
    static Vector2Int Cell(int x,int y) => new Vector2Int(x-8,4-y);
    static string PrefabPath(int i) => Folder+"/"+Specs()[i].name+".prefab";
    static List<Spec> specs;
    static List<Spec> Specs()
    {
        if(specs!=null)return specs;
        var a=new List<Spec>();
        a.Add(new Spec("01 Switchback Freight","Navigation, Carrying","Winding / Flexible / Singular","Batch six rocks on different arms; choose upper and lower routes around the freight island. Two docks demand three orange and three blue rocks.").Wall(7,2,9,6).Start(3,5)
            .Add("Rock A",2,2).Add("Rock B",3,4).Add("Rock A",4,6).Add("Rock B",5,2).Add("Rock B",2,6).Add("Rock A",5,6)
            .Add("Rocket",14,2,"Rock A","Rock A","Rock A").Add("Rocket",14,6,"Rock B","Rock B","Rock B")
            .Do(Act(2,2),Act(3,4,U),Act(14,2),Act(14,6,U),Act(4,6),Act(5,2,U),Act(14,2),Act(14,6,U),Act(5,6),Act(2,6,U),Act(14,2),Act(14,6,U)));
        a.Add(new Spec("02 Paired Foundries","Simultaneous action, Carrying, Order of operations","Winding / Flexible / Fractured","Two-arm workstations let the team pick up and process two rocks concurrently. Power orange, dispatch both cogs across the island, then switch to the blue station and ship its pair back.").Wall(8,2,8,6).Start(3,4)
            .Add("Rock A",3,2).Add("Rock A",5,4).Add("Button A",3,6).Add("Saw A",3,1).Add("Saw A",6,4)
            .Add("Rock B",13,2).Add("Rock B",11,4).Add("Button B",13,6).Add("Saw B",13,1).Add("Saw B",10,4)
            .Add("Rocket",12,2,"Cog A","Cog A").Add("Rocket",4,6,"Cog B","Cog B")
            .Do(Go(3,4),Parallel(U,R),Act(3,6,D),Parallel(U,R),Act(12,2),Act(12,2,U),
                Go(13,4),Parallel(U,L),Act(13,6,D),Parallel(U,L),Act(4,6,L),Act(4,6,U)));

        a.Add(new Spec("03 Greenhouse Circuit","Carrying, Order of operations, Navigation","Winding / Fixed / Fractured","One watering can serves three plants and a liquid order across a central gap. Harvest on a second arm; recolor the final orange flower blue before shipping.").Wall(8,1,8,7).Rect(8,4,8,4).Start(3,4)
            .Add("Bucket",5,4).Add("Liquid A",3,2).Add("Liquid B",3,6).Add("Liquid A",6,2).Add("Liquid B",6,6)
            .Add("Sapling A",11,2).Add("Sapling B",13,4).Add("Sapling A",11,6).Add("Cloud B",10,4)
            .Add("Rocket",14,2,"Flower A","Flower B","Flower B","Liquid B")
            .Do(Act(5,4),Act(3,2),Act(11,2),Act(11,2,U),Act(14,2,U),Act(3,6),Act(13,4),Act(13,4,U),Act(14,2,U),
                Act(6,2),Act(11,6),Act(11,6,U),Act(10,4,U),Act(14,2,U),Act(6,6),Act(14,2)));
        a.Add(new Spec("04 Color Customs","Order of operations, Carrying, Navigation","Winding / Flexible / Singular","Four mismatched cargo types must pass the correct one-use color stations. Choose the work order, keep the bucket useful, and route around staggered partitions.").Wall(7,2,7,4).Wall(10,4,10,6).Start(4,4)
            .Add("Rock A",3,2).Add("Cog B",3,6).Add("Flower A",6,6).Add("Bucket",5,4).Add("Liquid A",4,6)
            .Add("Cloud B",9,2).Add("Cloud A",9,6).Add("Cloud B",12,4).Add("Cloud B",12,6)
            .Add("Rocket",14,3,"Rock B","Cog A","Flower B","Liquid B")
            .Do(Act(3,2),Act(9,2),Act(14,3),Act(3,6),Act(9,6),Act(14,3),
                Act(6,6,U),Act(12,4,U),Act(14,3,U),Act(5,4),Act(4,6),Act(12,6),Act(14,3)));
        a.Add(new Spec("05 Bootstrap Workshop","Order of operations, Carrying","Winding / Fixed / Fractured","Spend the starter cog on the door, manufacture a cog to build the blue saw, and ship an orange cog before switching to the blue production phase. All blue inputs are inside the final wing.").Wall(8,1,8,7).Rect(8,4,8,4).Start(3,5)
            .Add("Cog A",3,4).Add("Rock A",4,2).Add("Rock A",5,6).Add("Button A",2,6).Add("Saw A",6,2).Add("Door Neutral",8,4)
            .Add("Cloud B",11,2).Add("Saw Neutral",11,4).Add("Rock B",12,2).Add("Rock B",13,4).Add("Button B",14,6)
            .Add("Rocket",14,2,"Cog A","Cog B","Cog B")
            .Do(Act(3,4),Act(8,4),Act(2,6),Act(4,2),Act(6,2),Act(11,2),Act(11,4),
                Act(5,6),Act(6,2),Act(14,2),Act(12,2),Act(13,4,U),Act(14,6,L),Act(11,4),Act(11,4,U),Act(14,2),Act(14,2,U)));
        a.Add(new Spec("06 Airlock Shuffle","Carrying, Order of operations, Navigation","Direct / Fixed / Fractured","Use the center as a shipping hub. Open orange to collect a three-arm load from the left, unload, then open blue for the right-hand shipment. Switching also shuts the other airlock.")
            .Wall(6,1,6,7).Wall(10,1,10,7).Rect(6,4,6,4).Rect(10,4,10,4).Start(8,4)
            .Add("Door A",6,4).Add("Door B",10,4).Add("Button A",8,6).Add("Button B",8,2)
            .Add("Rock A",3,2).Add("Rock A",4,5).Add("Rock A",2,4).Add("Rock B",13,2).Add("Rock B",14,5).Add("Rock B",12,4)
            .Add("Rocket",9,5,"Rock A","Rock A","Rock A").Add("Rocket",7,3,"Rock B","Rock B","Rock B")
            .Do(Act(8,6,D),Go(3,4),Act(3,2,U),Act(4,5),Act(2,4,D),Go(8,4),Act(9,5,U),Act(9,5),Act(9,5,D),
                Act(8,2,U),Go(12,3),Act(13,2,L),Act(14,5,D),Act(12,4,U),Go(8,4),Act(7,3,L),Act(7,3,D),Act(7,3,U)));
        a.Add(new Spec("07 Relay Warehouse","Rotation, Carrying, Navigation","Winding / Fixed / Fractured","Three right-facing loading spurs feed one left-facing dock. Stage each load in the transfer bay; fill the bucket before its handoff. Neither cargo pickup nor dock delivery can be approached from the opposite side.")
            .Empty().Rect(2,4,15,4).Rect(6,2,10,6).Rect(12,2,12,6).Rect(12,2,15,2).Rect(12,6,15,6).Start(8,4)
            .Add("Rock A",15,2).Add("Cog B",15,4).Add("Bucket",15,6).Add("Liquid A",10,2)
            .Add("Rocket",2,4,"Rock A","Cog B","Liquid A")
            .Do(Act(15,2),Go(8,4),Drop(),Act(9,4,L),Act(2,4,L),
                Act(15,4),Go(8,4),Drop(),Act(9,4,L),Act(2,4,L),
                Act(15,6),Act(10,2),Go(8,4),Drop(),Act(9,4,L),Act(2,4,L)));
        a.Add(new Spec("08 Clockwork Orchard","Timing, Carrying, Order of operations","Winding / Flexible / Fractured","Grow and ship two flowers, then park the bucket and process two rocks. The shared timer alternates power between the two saws; prepare the next color while the other is active.")
            .Wall(8,2,8,3).Wall(8,5,8,6).Start(4,4)
            .Add("Bucket",5,4).Add("Liquid A",3,2).Add("Liquid B",3,6).Add("Sapling A",10,2).Add("Sapling B",10,6)
            .Add("Rock A",6,2).Add("Rock B",6,6).Add("Saw A",12,2).Add("Saw B",12,6).Timer(8,4,6)
            .Add("Rocket",14,4,"Flower A","Flower B","Cog A","Cog B")
            .Do(Act(5,4),Act(3,2),Act(10,2),Act(10,2,U),Act(14,4,U),Act(3,6),Act(10,6),Act(10,6,U),Act(14,4,U),
                Go(5,5),Drop(),Act(6,2),WaitSaw(12,2),Act(12,2),Act(14,4),Act(6,6),WaitSaw(12,6),Act(12,6),Act(14,4)));
        a.Add(new Spec("09 Refill Contracts","Reset, Order of operations, Navigation","Winding / Flexible / Singular","Bank one rock and one blue-liquid delivery, then rewind. Reuse the restored rock as blue saw input on the second cycle while replenishing the liquid order. The cycles have different jobs, not just repeated deliveries.")
            .Wall(7,3,9,5).Start(4,4)
            .Add("Rock A",3,2).Add("Liquid A",3,6).Add("Bucket",5,4).Add("Cloud B",9,2).Add("Cloud B",9,6)
            .Add("Button B",11,6).Add("Saw B",11,4).Add("Rocket",13,2,"Rock A","Cog B").Add("Rocket",13,6,"Liquid B","Liquid B")
            .Do(Act(3,2),Act(13,2),Act(5,4),Act(3,6),Act(9,6),Act(13,6),Reset(),
                Act(3,2),Act(9,2),Act(11,6,U),Act(11,4),Act(13,2),Act(5,4),Act(3,6),Act(9,6),Act(13,6)));
        a.Add(new Spec("10 Crossdock Finale","Obstacles, Order of operations, Carrying","Winding / Fixed / Fractured","Manufacture an orange cog and blue liquid to launch the blocking rocket. Its departure opens the nursery wing; reuse the surviving bucket to grow both flower colors and load the final dock.")
            .Wall(8,1,8,7).Rect(8,4,8,4).Start(3,4)
            .Add("Rock A",3,2).Add("Bucket",5,4).Add("Liquid A",3,6).Add("Cloud B",6,6).Add("Saw A",5,2).Add("Button A",2,4)
            .Add("Rocket",8,4,"Cog A","Liquid B").Add("Liquid B",11,2).Add("Liquid A",11,6).Add("Sapling B",12,6).Add("Sapling A",12,2).Add("Rock A",13,2)
            .Add("Rocket",14,4,"Flower B","Flower A","Rock A")
            .Do(Act(3,2),Act(2,4,U),Act(5,2),Act(8,4),Act(5,4),Act(3,6),Act(6,6),Act(8,4),
                Act(11,2),Act(12,6),Act(12,6,U),Act(14,4,U),Act(11,6),Act(12,2),Act(12,2,U),Act(14,4,U),Act(13,2),Act(14,4)));
        specs=a;return specs;
    }

    static Spec Gate(string n,string c,string idea) => new Spec(n,c,"Direct / Fixed / Fractured",idea).Wall(8,1,8,7).Rect(8,4,8,4).Start(3,4);
    static ItemData ItemDataFor(string name) => AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Scriptable Objects/Items/"+name+".asset");
    static ItemColorData ColorFor(string name) => AssetDatabase.LoadAssetAtPath<ItemColorData>("Assets/Scriptable Objects/Items/Colors/Color "+name+".asset");
    static void Check(bool condition,string message) { if(!condition)throw new Exception(message); }
    static void SetRef(UnityEngine.Object obj,string property,UnityEngine.Object value)
    {
        var so=new SerializedObject(obj);so.FindProperty(property).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();
    }
    public static void Build()
    {
        try
        {
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Prefabs/Levels/showcase 2026","60_rooms");
            for(int i=0;i<Specs().Count;i++) BuildOne(i);
            AssetDatabase.SaveAssets();
            WriteCatalogue(false);
            Debug.Log("MINUTE: Generated "+Specs().Count+" distinct room prefabs with default room options.");
            Validate();
        }
        catch(Exception ex) { Debug.LogException(ex);EditorApplication.Exit(1); }
    }
    static void BuildOne(int index)
    {
        var s=Specs()[index];
        var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Levels/Empty Level.prefab");
        try
        {
            root.name=s.name;root.transform.position=new Vector3(-.5f,-.5f,0);
            var options=new SerializedObject(root.GetComponent<Room>());foreach(var option in new[]{"isTimed","canUseReset","allowHorizontalReflection","allowVerticalReflection"})options.FindProperty(option).boolValue=true;options.ApplyModifiedPropertiesWithoutUndo();
            var roomSettings=new SerializedObject(root.GetComponent<Room>());roomSettings.FindProperty("<StandardTime>k__BackingField").floatValue=60f;roomSettings.ApplyModifiedPropertiesWithoutUndo();
            var b=root.transform.Find("Bounds");
            b.Find("BottomLeft").localPosition=new Vector3(-8,-4,0);
            b.Find("TopRight").localPosition=new Vector3(9,5,0);
            var parent=root.transform.Find("Objects");
            while(parent.childCount>0)UnityEngine.Object.DestroyImmediate(parent.GetChild(0).gameObject);
            var grid=root.GetComponent<RoomGrid>();
            var placed=new HashSet<Vector2Int>();
            Action<string,int,int,Obj> put=(prefab,x,y,settings)=>
            {
                Check(x>=0&&x<17&&y>=0&&y<9,"Object outside room");
                Check(placed.Add(Cell(x,y)),s.name+" overlapping object at "+x+","+y);
                var path="Assets/Prefabs/"+(prefab=="Rover"?"Rover/Rover":"Level Objects/"+prefab)+".prefab";
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);Check(asset!=null,"Missing "+path);
                var obj=(GameObject)PrefabUtility.InstantiatePrefab(asset,parent);
                obj.transform.position=grid.CellToWorld(Cell(x,y));
                if(prefab=="Rocket")
                {
                    var so=new SerializedObject(obj.GetComponent<ItemCollector>());
                    var items=so.FindProperty("<RequiredItems>k__BackingField");items.arraySize=settings.requests.Length;
                    for(int j=0;j<items.arraySize;j++) {var data=ItemDataFor(settings.requests[j]);Check(data!=null,"Missing request "+settings.requests[j]);items.GetArrayElementAtIndex(j).objectReferenceValue=data;}
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                if(settings!=null&&settings.timed)
                {
                    var so=new SerializedObject(obj.GetComponent<TimedEmitter>());
                    var intervals=so.FindProperty("signalIntervals");intervals.arraySize=2;
                    intervals.GetArrayElementAtIndex(0).FindPropertyRelative("Signal").objectReferenceValue=ColorFor("A");
                    intervals.GetArrayElementAtIndex(0).FindPropertyRelative("TimeBeforeActivation").floatValue=6f;
                    intervals.GetArrayElementAtIndex(1).FindPropertyRelative("Signal").objectReferenceValue=ColorFor("B");
                    intervals.GetArrayElementAtIndex(1).FindPropertyRelative("TimeBeforeActivation").floatValue=float.Parse(settings.requests[0],System.Globalization.CultureInfo.InvariantCulture);
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(obj.transform);
            };
            for(int y=0;y<9;y++)for(int x=0;x<17;x++)if(!s.floor[x,y])put("Obstacle",x,y,null);
            Check(s.floor[s.sx,s.sy],"Spawn on wall");put("Rover",s.sx,s.sy,null);
            foreach(var o in s.objects) {Check(s.floor[o.x,o.y],s.name+" object on wall "+o.prefab);put(o.prefab,o.x,o.y,o);}
            Check(root.GetComponentsInChildren<GridObject>(true).Length==placed.Count,"Unexpected nested grid objects");
            Check(grid.IsWithinBounds(Cell(0,0))&&grid.IsWithinBounds(Cell(16,8))&&!grid.IsWithinBounds(Cell(17,4))&&!grid.IsWithinBounds(Cell(-1,4))&&!grid.IsWithinBounds(Cell(8,-1))&&!grid.IsWithinBounds(Cell(8,9)),"Bounds mismatch");
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath(index));
        }
        finally {PrefabUtility.UnloadPrefabContents(root);}
    }
    public static void Validate()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        SessionState.SetBool(Pending,true);
        EditorApplication.isPlaying=true;
    }
    static void WriteCatalogue(bool passed)
    {
        var text=new StringBuilder("# 60-second room studies\n\nTen standalone 17 x 9 rooms combining the basic vocabulary into multi-stage cooperative work. All use the default options on the Room component and have StandardTime = 60 seconds. This is a scoring target, not a forced duration: coordinated teams can finish faster. No idle waiting was added to pad the duration; the timer serves actual saw-power windows.\n\n");
        text.Append("Target audience: teams familiar with the basic blocks, primarily four players. Aim for roughly 45-90 seconds on early informed attempts, centered near 60; actual human timings still need measurement. Longer work comes from carrying decisions, return trips, production dependencies, signal transitions and handoffs. Reset remains available in every room; only Refill Contracts deliberately requires it. These are standalone prototypes, not additions to an existing lobby or repeated level pool.\n\n");
        text.Append("![Room layouts and delivery orders](overview.png)\n\n| Room | Challenge emphasis | Geometry | Work rhythm |\n|---|---|---|---|\n");
        foreach(var s in Specs())text.Append("| "+s.name+" | "+s.challenge+" | "+s.geometry+" | "+s.idea+" |\n");
        text.Append("\n## Validation and duration\n\n"+(passed?"PASS: all ten completed through actual Rover arm extension/release, carrying, interaction and movement in Unity Play Mode. Authored cells, requests, shared config, conversions, signals, mandatory cargo handoffs, blocking-rocket removal and reset objective persistence were exercised. The final room completion registered for every room.\n":"Play Mode validation pending.\n"));
        text.Append("\nAutomated runs know the route, choose stances instantly, and do not simulate human communication. Their durations are execution references, not four-player playtest estimates. The reset controller's hold is invoked directly for the persistence test; add its configured two-second hold to that run. Human error recovery, optional batching, parallel actions, route choice and task allocation can substantially change completion time. No claim of a unique or optimal solution is made.\n\n");
        if(passed)
        {
            text.Append("| Room | Scripted game seconds | Pickup and use interactions (excludes drops) | Movement segments |\n|---|---:|---:|---:|\n");
            foreach(var r in results)text.Append(r+"\n");
        }
        text.Append("\n## Executable reference routes\n\nCoordinates: (column,row), zero-based from the top left; A = orange, B = blue. `act` means align and release the specified arm on that object. `go` moves to a free cell. Navigation chooses valid long straight segments around current occupancy. Both held-item states and the remaining resource budget matter; suggested batching is optional unless geometry forces it.\n\n");
        foreach(var s in Specs())text.Append("- **"+s.name+"**: "+string.Join(" -> ",s.commands.Select(c=>c.op=="act"?c.dir+" ("+c.x+","+c.y+")":c.op=="reset"?"RESET":c.op=="parallel"?"PARALLEL "+string.Join("/",Enum.GetValues(typeof(Direction)).Cast<Direction>().Where(d=>(c.x&(1<<(int)d))!=0)):c.op=="drop"?"DROP "+c.dir:c.op+" ("+c.x+","+c.y+")"))+".\n");
        text.Append("\n## Tuning after human play\n\nRecord first informed completion and repeat completion for each team size. If a room is too short, first check whether its intended batching, transfer or dependency is bypassed. If it is too long, reduce one shipment or shorten a return route before increasing the score budget. Keep production layouts exclusive to one level and never request more room completions than unique prefabs in its pool.\n");
        File.WriteAllText(Folder+"/README.md",text.ToString());
    }

    static readonly List<string> results=new List<string>();
    static float roomStartTime; static int armActions,moveSegments;
    static int roomIndex,commandIndex,stage;
    static double deadline,delayUntil;
    static RoomLoader loader;static LevelManager manager;static Room room;static Rover rover;
    static readonly Queue<Cmd> queue=new Queue<Cmd>();
    static Cmd running;static RoverHand hand;static bool released;static Vector2Int origin;
    static List<RoverHand> parallelHands;static HashSet<RoverHand> parallelReleased;
    
    static BuildMinuteRooms()
    {
        if(!SessionState.GetBool(Pending,false))return;
        deadline=EditorApplication.timeSinceStartup+180;
        EditorApplication.update+=Tick;
    }
    static bool Passable(GridObject o) => o!=null&&o.TryGetComponent<IPassable>(out var p)&&p.CanHandPassThrough;
    static bool ClearLine(Vector2Int from,Vector2Int target,Direction d)
    {
        var delta=target-from;var step=DirectionVector.GetVector2Int(d);
        if(delta==Vector2Int.zero||delta.x*step.y!=delta.y*step.x||(delta.x*step.x+delta.y*step.y)<=0)return false;
        for(var p=from+step;p!=target;p+=step) {var o=room.Grid.Objects.GetObject(p);if(o!=null&&!Passable(o))return false;}
        return true;
    }
    static List<Cmd> Navigate(Cmd cmd)
    {
        var start=room.Grid.WorldToCell(rover.transform.position);var target=Cell(cmd.x,cmd.y);
        Func<Vector2Int,bool> goal=p=>cmd.op=="go"?p==target:ClearLine(p,target,cmd.dir);
        if(goal(start))return new List<Cmd>();
        var q=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>{start};var prev=new Dictionary<Vector2Int,Vector2Int>();
        q.Enqueue(start);Vector2Int? found=null;
        while(q.Count>0&&!found.HasValue)
        {
            var p=q.Dequeue();
            foreach(Direction d in Enum.GetValues(typeof(Direction)))
            {
                var step=DirectionVector.GetVector2Int(d);var to=p+step;
                while(room.Grid.IsWithinBounds(to))
                {
                    var obj=room.Grid.Objects.GetObject(to);
                    if(obj!=null&&!Passable(obj))break;
                    if(obj==null&&seen.Add(to))
                    {
                        prev[to]=p;
                        if(goal(to)){found=to;break;}
                        q.Enqueue(to);
                    }
                    to+=step;
                }
                if(found.HasValue)break;
            }
        }
        Check(found.HasValue,"No stance for "+cmd.op+" "+cmd.dir+" "+cmd.x+","+cmd.y);
        var cells=new List<Vector2Int>();var at=found.Value;
        while(at!=start){cells.Add(at);at=prev[at];}cells.Reverse();
        return cells.Select(p=>new Cmd("move",p.x+8,4-p.y)).ToList();
    }
    static Direction DirectionTo(Vector2Int delta) => delta.x>0?R:delta.x<0?L:delta.y>0?U:D;
    static void RefreshRoom() {room=loader.CurrentRoom;rover=room.GetComponentInChildren<Rover>();}
    static void BeginRoom()
    {
        manager.StartLevel(LevelData.CreateRoomTest(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(roomIndex)).GetComponent<Room>()));
        // Scripted solutions use the authored orientation; real room startups remain randomized.
        var options=new SerializedObject(loader.CurrentRoom);options.FindProperty("allowHorizontalReflection").boolValue=false;options.FindProperty("allowVerticalReflection").boolValue=false;options.ApplyModifiedPropertiesWithoutUndo();
        stage=1;commandIndex=0;queue.Clear();running=null;parallelHands=null;hand=null;
        roomStartTime=Time.time;armActions=moveSegments=0;
        deadline=EditorApplication.timeSinceStartup+240;
    }
    static void VerifyLayout()
    {
        var s=Specs()[roomIndex];Check(room.IsTimed&&room.CanUseReset,"Wrong room options");
        for(int y=0;y<9;y++)for(int x=0;x<17;x++)
        {
            var obj=room.Grid.Objects.GetObject(Cell(x,y));var spec=s.objects.FirstOrDefault(o=>o.x==x&&o.y==y);
            string expected=!s.floor[x,y]?"Obstacle":x==s.sx&&y==s.sy?"Rover":spec?.prefab;
            Check((obj==null)==(expected==null),"Occupancy mismatch "+x+","+y);
            if(obj==null)continue;
            
            Check(obj.gameObject.name==expected,"Wrong source prefab "+x+","+y+" expected "+expected+" actual "+obj.name);
            if(expected=="Rocket")Check(obj.GetComponent<ItemCollector>().RequiredItems.SequenceEqual(spec.requests.Select(ItemDataFor)),"Request mismatch");
        }
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            Check(EditorApplication.timeSinceStartup<deadline,"Timeout at stage "+stage+", command "+commandIndex+", "+running?.op);
            if(stage==0)
            {
                manager=UnityEngine.Object.FindObjectOfType<LevelManager>();loader=UnityEngine.Object.FindObjectOfType<RoomLoader>();
                if(manager==null||loader==null||loader.CurrentRoom==null)return;
                var lobby=UnityEngine.Object.FindObjectOfType<LobbyManager>();if(lobby!=null)lobby.IsRoomTesting=true;
                manager.BeginRoomTestSession();BeginRoom();return;
            }
            if(stage==1)
            {
                RefreshRoom();if(room.GetComponentsInChildren<GridObject>().Any(o=>o.RoomGrid==null))return;
                VerifyLayout();stage=2;return;
            }
            if(stage==3) {RefreshRoom();stage=2;return;}
            if(running!=null) {TickCommand();return;}
            if(queue.Count>0) {running=queue.Dequeue();StartCommand();return;}
            var commands=Specs()[roomIndex].commands;
            if(commandIndex==commands.Count)
            {
                if(!manager.IsRoomTestFinished)return;
                Check(manager.CompletedRoomCount==1,"Missing registered room completion");
                var seconds=Time.time-roomStartTime;
                results.Add("| "+Specs()[roomIndex].name+" | "+seconds.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+" | "+armActions+" | "+moveSegments+" |");
                Debug.Log("MINUTE: PASS "+Specs()[roomIndex].name+" seconds="+seconds+" interactions="+armActions+" movements="+moveSegments);
                roomIndex++;
                if(roomIndex==Specs().Count)
                {
                    WriteCatalogue(true);Debug.Log("MINUTE: PASS all "+Specs().Count+" rooms.");
                    SessionState.EraseBool(Pending);EditorApplication.Exit(0);return;
                }
                BeginRoom();return;
            }
            var cmd=commands[commandIndex++];
            if(cmd.op=="act"||cmd.op=="go"||cmd.op=="postpone")
            {
                foreach(var c in Navigate(cmd))queue.Enqueue(c);
                if(cmd.op!="go")queue.Enqueue(cmd);
            }
            else queue.Enqueue(cmd);
        }
        catch(Exception ex)
        {
            Debug.LogError("MINUTE: FAIL "+Specs()[roomIndex].name+", command "+commandIndex+", running "+running?.op);
            Debug.LogException(ex);SessionState.EraseBool(Pending);EditorApplication.Exit(1);
        }
    }
    static void Finish() {running=null;hand=null;parallelHands=null;}
    static void StartCommand()
    {
        if(running.op=="delay") {delayUntil=EditorApplication.timeSinceStartup+running.seconds;return;}
        if(running.op=="door"||running.op=="saw")return;
        if(running.op=="reset")
        {
            var before=loader.CurrentRoom;var clock=manager.ElapsedTime;
            var rockets=before.GetComponentsInChildren<ItemCollector>();var counts=rockets.Select(r=>r.RequiredItems.Count).ToArray();
            Check(counts.Sum()>0&&!manager.IsRoomTestFinished,"Reset must occur before final completion");
            loader.ResetController.ResetRoom();
            Check(loader.CurrentRoom!=before,"Reset did not restore room");RefreshRoom();
            for(int i=0;i<rockets.Length;i++)Check(rockets[i]!=null&&rockets[i].RequiredItems.Count==counts[i]&&rockets[i].transform.IsChildOf(room.transform),"Rocket progress not preserved");
            Check(manager.ElapsedTime>=clock,"Reset refunded score time");
            Check(manager.ObjectiveTracker.AllObjectives.All(o=>o!=null),"Stale objective after reset");
            Debug.Log("MINUTE: Reset persistence passed "+Specs()[roomIndex].name);stage=3;Finish();return;
        }
        if(running.op=="drop")
        {
            hand=rover.GetComponentsInChildren<RoverHand>().Single(h=>h.Direction==running.dir);
            Check(hand.IsHoldingObject,"Drop without cargo");rover.OnDrop(new[]{running.dir});Check(!hand.IsHoldingObject,"Blocked required drop");Finish();return;
        }
        if(running.op=="parallel")
        {
            parallelHands=rover.GetComponentsInChildren<RoverHand>().Where(h=>(running.x&(1<<(int)h.Direction))!=0).ToList();
            parallelReleased=new HashSet<RoverHand>();armActions+=parallelHands.Count;foreach(var h in parallelHands)rover.OnPress(h.Direction);return;
        }
        if(running.op=="move")moveSegments++;else armActions++;
        origin=room.Grid.WorldToCell(rover.transform.position);
        var dir=running.op=="move"?DirectionTo(Cell(running.x,running.y)-origin):running.dir;
        hand=rover.GetComponentsInChildren<RoverHand>().Single(h=>h.Direction==dir);
        if(running.op!="move")
        {
            var obj=room.Grid.Objects.GetObject(Cell(running.x,running.y));Check(obj!=null,"Missing target");
            Check((obj.TryGetComponent<IGrabbable>(out var gr)&&gr.CanBeGrabbed)||(obj.TryGetComponent<IInteractable>(out var inter)&&inter.CanInteractWith(hand.HeldObject)),"Invalid interaction with "+obj.name);
        }
        
        released=false;rover.OnPress(dir);
    }
    static void TickCommand()
    {
        if(running.op=="delay") {if(EditorApplication.timeSinceStartup>=delayUntil)Finish();return;}
        if(running.op=="door") {if(room.Grid.Objects.GetObject(Cell(running.x,running.y)).GetComponent<Door>().IsOpen)Finish();return;}
        if(running.op=="saw") {if(room.Grid.Objects.GetObject(Cell(running.x,running.y)).GetComponentInChildren<SignalItemConverter>().IsPowered)Finish();return;}
        if(running.op=="parallel")
        {
            foreach(var h in parallelHands)
                if(!parallelReleased.Contains(h))
                {
                    var current=room.Grid.WorldToCell(h.HandPosition);var from=room.Grid.WorldToCell(rover.transform.position);
                    if(current!=from&&current==room.Grid.GetStoppingSquare(from,h.Direction,h.HeldObject)) {rover.OnRelease(h.Direction);parallelReleased.Add(h);}
                }
            if(parallelReleased.Count==parallelHands.Count&&parallelHands.All(h=>h.HandState==HandState.None))Finish();return;
        }
        if(!released)
        {
            if(room.Grid.WorldToCell(hand.HandPosition)!=Cell(running.x,running.y))return;
            rover.OnRelease(hand.Direction);released=true;return;
        }
        if(rover.IsMoving||hand.HandState!=HandState.None)return;
        if(running.op=="move")Check(room.Grid.WorldToCell(rover.transform.position)==Cell(running.x,running.y),"Wrong movement endpoint");
        if(running.op=="postpone")
        {
            var emitter=room.GetComponentInChildren<TimedEmitter>();var so=new SerializedObject(emitter);
            var bar=(Image)so.FindProperty("progressBar").objectReferenceValue;
            Check(bar.color==ColorFor("B").Color&&bar.fillAmount<0.5f,"Lever did not postpone upcoming blue signal");
            Debug.Log("MINUTE: Timer postponement verified.");
        }
        Finish();
    }
}
