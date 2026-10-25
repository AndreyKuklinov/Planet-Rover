using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Authoring-only specification and real-input smoke-test harness for the blocks library.
// Kept outside the room prefabs: none of the rooms depends on this class at runtime.
[InitializeOnLoad]
public static class BuildBlockRooms
{
    const string Folder = "Assets/Prefabs/Levels/showcase 2026/blocks";
    const string Pending = "PlanetRover.BlockValidation";
    const string ConfigPath = "Assets/Scriptable Objects/Configs/Room configs/Default Room Config.asset";
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
        a.Add(new Spec("01 Alignment - Open Floor","Navigation","Direct / Flexible / Singular","Find the row and approach side for one delivery.").Start(5,5)
            .Add("Rock A",11,3).Add("Rocket",3,2,"Rock A").Do(Act(11,3),Act(3,2)));
        a.Add(new Spec("02 Long Haul - Straight Corridor","Navigation","Mostly direct / Fixed / Fractured","Carry down one long corridor into a delivery bay, contrasting with turn-heavy travel.").Empty().Rect(1,4,15,4).Rect(12,3,15,5).Start(2,4)
            .Add("Rock A",6,4).Add("Rocket",14,5,"Rock A").Do(Act(6,4),Act(14,5)));
        a.Add(new Spec("03 Detour - Island Routes","Navigation","Winding / Flexible / Singular","Choose the upper or lower route around a central island.").Wall(7,2,9,6).Start(3,4)
            .Add("Rock A",12,4).Add("Rocket",3,6,"Rock A").Do(Act(12,4),Act(3,6)));
        a.Add(new Spec("04 Delivery - Linked Bays","Navigation","Winding / Fixed / Fractured","Work in two offset bays joined by one narrow connection.").Empty().Rect(1,1,5,7).Rect(11,1,15,7).Rect(6,4,10,4).Start(3,3)
            .Add("Rock A",13,5).Add("Rocket",3,6,"Rock A").Do(Act(13,5),Act(3,6)));
        a.Add(new Spec("05 Clearance - Rock in the Gap","Obstacles","Winding / Fixed / Fractured","The required rock blocks the only connection; collecting it opens passage.").Empty().Rect(2,2,2,6).Rect(2,6,13,6).Rect(13,2,13,6).Rect(11,1,14,3).Start(2,2)
            .Add("Rock A",7,6).Add("Rocket",13,2,"Rock A").Do(Act(7,6),Act(13,2)));
        a.Add(new Spec("06 Clearance - Launch the Blockade","Obstacles, Order of operations","Direct / Fixed / Fractured","Launch the first rocket to access a second pickup and unfinished rocket.").Empty().Rect(1,3,5,5).Rect(6,4,10,4).Rect(11,3,15,5).Start(2,4)
            .Add("Rock A",4,4).Add("Rocket",8,4,"Rock A").Add("Rock B",12,4).Add("Rocket",14,4,"Rock B").Do(Act(4,4),Act(8,4),Act(12,4),Act(14,4)));
        a.Add(new Spec("07 Rotation - Transfer Alcove","Rotation","Direct / Fixed / Singular","Pickup is right-facing and delivery left-facing; the alcove permits a handoff.").Empty().Rect(2,4,14,4).Rect(7,3,10,5).Start(8,4)
            .Add("Rock A",14,4).Add("Rocket",2,4,"Rock A").Do(Act(14,4),Drop(),Act(9,4,L),Act(2,4,L)));
        a.Add(new Spec("08 Rotation - Transfer Between Chambers","Rotation","Direct / Fixed / Fractured","The same handoff task occupies a central bay between separate workspaces.").Empty().Rect(1,2,5,6).Rect(11,2,15,6).Rect(6,4,10,4).Rect(8,3,10,5).Start(8,4)
            .Add("Rock A",14,4).Add("Rocket",2,4,"Rock A").Do(Act(14,4),Drop(),Act(9,4,L),Act(2,4,L)));
        var exchange=new Spec("09 Carrying - Exchange Cargo","Carrying","Direct / Flexible / Singular","An occupied arm exchanges rocks; recover the deposited rock for the other request.").Start(3,4)
            .Add("Rock A",6,4).Add("Rock B",10,4).Add("Rocket",13,4,"Rock B","Rock A").Do(Act(6,4),Act(10,4),Act(13,4),Act(10,4),Act(13,4));
        exchange.caveat="Exchange is demonstrated as an efficient option, not forced; separate deliveries remain possible.";a.Add(exchange);
        a.Add(new Spec("10 Carrying - Loaded Lever","Carrying","Direct / Fixed / Fractured","Operate the lever without discarding held cargo, then deliver both resources across the gate.").Wall(12,1,12,7).Rect(12,4,12,4).Start(5,4)
            .Add("Rock A",7,4).Add("Rock B",5,2).Add("Button A",10,4).Add("Door A",12,4)
            .Add("Rocket",14,2,"Rock A").Add("Rocket",14,6,"Rock B").Do(Act(7,4),Act(5,2,U),Act(10,4),Go(13,4),Act(14,2),Act(14,6,U)));
        var twin=new Spec("11 Parallel - Twin Dispatch","Simultaneous action, Carrying","Direct / Stationary workstation / Singular","Two arms can collect, retract and deliver in parallel from one workstation.").Start(8,4)
            .Add("Rock A",8,2).Add("Rock B",11,4).Add("Rocket",8,1,"Rock A").Add("Rocket",13,4,"Rock B").Do(Parallel(U,R),Parallel(U,R));
        twin.caveat="Parallel action improves throughput; it is not a strict simultaneity lock.";a.Add(twin);
        var four=new Spec("12 Parallel - Four Arm Dispatch","Simultaneous action, Carrying","Direct / Stationary workstation / Singular","One shared position supports four useful pickup and delivery lines.").Start(8,4)
            .Add("Rock A",8,2).Add("Rock A",10,4).Add("Rock A",8,6).Add("Rock A",6,4)
            .Add("Rocket",8,1,"Rock A").Add("Rocket",13,4,"Rock A").Add("Rocket",8,7,"Rock A").Add("Rocket",3,4,"Rock A").Do(Parallel(U,R,D,L),Parallel(U,R,D,L));
        four.caveat=twin.caveat;a.Add(four);
        a.Add(new Spec("13 Liquid - Fill and Deliver","Baseline interaction","Direct / Flexible / Singular","Collect one portion in a bucket; delivery returns the empty container.").Start(3,4)
            .Add("Bucket",5,4).Add("Liquid A",9,4).Add("Rocket",13,4,"Liquid A").Do(Act(5,4),Act(9,4),Act(13,4)));
        a.Add(new Spec("14 Liquid - Two Route Relay","Navigation","Winding / Flexible / Fractured","Keep the fill-and-deliver task, but choose between two connections joining the work bays.").Empty().Rect(1,1,6,7).Rect(10,1,15,7).Rect(7,2,9,2).Rect(7,6,9,6).Start(3,5)
            .Add("Bucket",4,4).Add("Liquid A",3,2).Add("Rocket",13,5,"Liquid A").Do(Act(4,4),Act(3,2),Act(13,5)));
        var swap=new Spec("15 Liquid - Exchange Contents","Carrying","Direct / Flexible / Singular","Swap a filled bucket with a source; the displaced liquid remains available for a later delivery.").Start(2,4)
            .Add("Bucket",4,4).Add("Liquid A",7,3).Add("Liquid B",10,3).Add("Rocket",13,4,"Liquid B","Liquid A")
            .Do(Act(4,4),Act(7,3),Act(10,3),Act(13,4),Act(10,3),Act(13,4));
        swap.caveat="The tested route demonstrates swapping; delivering each color before refilling is an alternative.";a.Add(swap);
        a.Add(new Spec("16 Cloud - Paint a Rock","Order of operations","Direct / Flexible / Singular","A wrong-color rock needs one cloud before delivery.").Start(3,4)
            .Add("Rock A",5,4).Add("Cloud B",9,4).Add("Rocket",13,4,"Rock B").Do(Act(5,4),Act(9,4),Act(13,4)));
        a.Add(new Spec("17 Cloud - Paint a Cog","Order of operations","Winding / Flexible / Singular","Keep the recoloring task but offset its stations; cogs retain type when recolored.").Start(3,5)
            .Add("Cog A",5,5).Add("Cloud B",9,3).Add("Rocket",13,3,"Cog B").Do(Act(5,5),Act(9,3),Act(13,3)));
        a.Add(new Spec("18 Cloud - Paint the Water","Order of operations","Direct / Flexible / Singular","Fill first: a cloud recolors liquid inside a bucket but cannot fill an empty one.").Start(2,4)
            .Add("Bucket",4,4).Add("Liquid A",7,4).Add("Cloud B",10,4).Add("Rocket",13,4,"Liquid B").Do(Act(4,4),Act(7,4),Act(10,4),Act(13,4)));
        a.Add(new Spec("19 Flower - Grow and Harvest","Order of operations","Winding / Flexible / Singular","Water a matching plant; harvest is a separate action while the bucket remains held.").Start(2,4)
            .Add("Bucket",4,4).Add("Liquid A",6,2).Add("Sapling A",10,4).Add("Rocket",14,4,"Flower A").Do(Act(4,4),Act(6,2),Act(10,4),Act(10,4,U),Act(14,4,U)));
        a.Add(new Spec("20 Flower - Shared Watering Can","Carrying, Order of operations","Winding / Flexible / Singular","Reuse one bucket for two colors and two plants; preserve the container while harvesting.").Start(2,4)
            .Add("Bucket",3,4).Add("Liquid A",5,2).Add("Liquid B",5,6).Add("Sapling A",10,2).Add("Sapling B",10,6).Add("Rocket",14,4,"Flower A","Flower B")
            .Do(Act(3,4),Act(5,2),Act(10,2),Act(10,2,U),Act(14,4,U),Act(5,6),Act(10,6),Act(10,6,U),Act(14,4,U)));
        a.Add(new Spec("21 Cloud - Paint a Flower","Order of operations, Navigation","Winding / Fixed / Fractured","Carry a pre-grown flower through three bays; recolor in the middle bay before delivering in the last.").Empty().Rect(1,1,5,7).Rect(7,1,10,7).Rect(12,1,15,7).Rect(6,2,6,2).Rect(11,6,11,6).Start(2,5)
            .Add("Flower A",3,4).Add("Cloud B",8,3).Add("Rocket",14,4,"Flower B").Do(Act(3,4),Act(8,3),Act(14,4)));
        a.Add(new Spec("22 Saw - Power and Process","Order of operations","Direct / Flexible / Singular","Select the saw's signal, then convert a matching rock into a cog.").Start(3,4)
            .Add("Rock A",5,4).Add("Button A",8,2).Add("Saw A",9,4).Add("Rocket",13,4,"Cog A").Do(Act(5,4),Act(8,2,U),Act(9,4),Act(13,4)));
        a.Add(new Spec("23 Saw - Configure Then Power","Order of operations","Winding / Flexible / Singular","Spend a starter cog on a neutral saw; configuration and powering are separate steps.").Start(3,4)
            .Add("Rock A",5,4).Add("Cog A",5,2).Add("Button A",8,2).Add("Saw Neutral",9,4).Add("Rocket",13,4,"Cog A")
            .Do(Act(5,4),Act(5,2,U),Act(9,4,U),Act(8,2,U),Act(9,4),Act(13,4)));
        a.Add(Gate("24 Door - Open the Passage","Navigation","A lever opens the only passage to the delivery bay.")
            .Add("Rock A",4,2).Add("Button A",4,6).Add("Door A",8,4).Add("Rocket",13,4,"Rock A").Do(Act(4,2),Act(4,6),Go(12,4),Act(13,4)));
        a.Add(Gate("25 Door - Configure the Passage","Order of operations","Use a cog to assign the door, then emit its matching signal.")
            .Add("Cog A",5,4).Add("Rock A",4,2).Add("Button A",4,6).Add("Door Neutral",8,4).Add("Rocket",13,4,"Rock A")
            .Do(Act(5,4),Act(8,4),Act(4,6),Act(4,2),Go(12,4),Act(13,4)));
        a.Add(Gate("26 Lever - Assign Its Color","Order of operations","A cog colors the neutral lever; a second interaction emits the signal.")
            .Add("Cog A",5,4).Add("Rock A",4,2).Add("Button Neutral",4,6).Add("Door A",8,4).Add("Rocket",13,4,"Rock A")
            .Do(Act(5,4),Act(4,6),Act(4,6),Act(4,2),Go(12,4),Act(13,4)));
        a.Add(new Spec("27 Signals - Alternate Gates","Order of operations, Navigation","Direct / Fixed / Fractured","Switch orange to blue in the middle bay; the second signal closes the route behind you.").Wall(6,1,6,7).Wall(10,1,10,7).Rect(6,4,6,4).Rect(10,4,10,4).Start(2,4)
            .Add("Rock A",3,2).Add("Button A",3,6).Add("Button B",8,6).Add("Door A",6,4).Add("Door B",10,4).Add("Rocket",14,4,"Rock A")
            .Do(Act(3,2),Act(3,6),Go(8,4),Act(8,6,D),Go(12,4),Act(14,4)));
        a.Add(Gate("28 Timer - Door Window","Timing","Wait for orange, then carry the rock through the timed opening.")
            .Add("Rock A",4,2).Add("Door A",8,4).Timer(3,6).Add("Rocket",13,4,"Rock A").Do(Act(4,2),WaitDoor(8,4),Go(12,4),Act(13,4)));
        a.Add(new Spec("29 Timer - Saw Window","Timing","Direct / Flexible / Singular","Prepare the rock, then process it during the orange power interval.").Start(3,4)
            .Add("Rock A",5,4).Add("Saw A",9,4).Timer(8,2).Add("Rocket",13,4,"Cog A").Do(Act(5,4),WaitSaw(9,4),Act(9,4),Act(13,4)));
        var postpone=Gate("30 Timer - Postpone the Switch","Timing, Carrying","The orange lever restarts the countdown to blue; use it to extend an open-door window.")
            .Add("Rock A",4,4).Add("Door A",8,4).Add("Button A",5,6).Timer(3,2,3).Add("Rocket",13,4,"Rock A")
            .Do(Act(4,4),WaitDoor(8,4),Delay(1),new Cmd("postpone",5,6),Go(12,4),Act(13,4));
        postpone.caveat="Lever use demonstrates postponement; fast teams can cross without it. No strict concurrency claim.";a.Add(postpone);
        a.Add(new Spec("31 Reset - Restore the Rock","Reset","Direct / Flexible / Singular","Two identical requests and one rock force a supply rewind after banking the first delivery.").Start(4,4)
            .Add("Rock A",7,4).Add("Rocket",12,4,"Rock A","Rock A").Do(Act(7,4),Act(12,4),Reset(),Act(7,4),Act(12,4)));
        a.Add(new Spec("32 Reset - Restore the Water","Reset","Direct / Flexible / Singular","One liquid portion serves two requests across a rewind; the bucket and source reset while delivery persists.").Start(2,4)
            .Add("Bucket",4,4).Add("Liquid A",8,4).Add("Rocket",13,4,"Liquid A","Liquid A")
            .Do(Act(4,4),Act(8,4),Act(13,4),Reset(),Act(4,4),Act(8,4),Act(13,4)));
        a.Add(new Spec("33 Sequence - Paint Before Cutting","Order of operations","Winding / Flexible / Singular","The blue saw cannot process the orange rock until the cloud recolors it.").Start(3,4)
            .Add("Rock A",5,4).Add("Cloud B",7,3).Add("Saw B",10,4).Add("Button B",10,2).Add("Rocket",14,4,"Cog B")
            .Do(Act(5,4),Act(7,3),Act(10,2,U),Act(10,4),Act(14,4)));
        a.Add(new Spec("34 Clearance - Flower in the Gap","Obstacles, Order of operations","Direct / Fixed / Fractured","Grow the blocking plant, park the bucket, then harvest the flower to clear the passage.").Empty().Rect(1,2,6,6).Rect(7,4,10,4).Rect(11,2,15,6).Start(2,4)
            .Add("Bucket",3,4).Add("Liquid A",5,2).Add("Sapling A",9,4).Add("Rocket",13,4,"Flower A")
            .Do(Act(3,4),Act(5,2),Act(9,4),Go(5,5),Drop(),Act(9,4),Go(12,4),Act(13,4)));
        a.Add(new Spec("35 Rotation - Bucket Handoff","Rotation, Carrying","Direct / Fixed / Singular","Fill on the right, transfer the whole filled bucket, and deliver its liquid on the left.").Empty().Rect(2,4,15,4).Rect(7,3,10,5).Start(8,4)
            .Add("Bucket",14,4).Add("Liquid A",15,4).Add("Rocket",2,4,"Liquid A").Do(Act(14,4),Act(15,4),Drop(),Act(9,4,L),Act(2,4,L)));
        specs=a;return specs;
    }
    static Spec Gate(string n,string c,string idea) => new Spec(n,c,"Direct / Fixed / Fractured",idea).Wall(8,1,8,7).Rect(8,4,8,4).Start(3,4);
    static RoomConfig DefaultConfig => AssetDatabase.LoadAssetAtPath<RoomConfig>(ConfigPath);
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
            Check(DefaultConfig!=null,"Default Room Config missing");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Prefabs/Levels/showcase 2026","blocks");
            for(int i=0;i<Specs().Count;i++) BuildOne(i);
            AssetDatabase.SaveAssets();
            WriteCatalogue(false);
            Debug.Log("BLOCKS: Generated "+Specs().Count+" distinct room prefabs with Default Room Config.");
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
            SetRef(root.GetComponent<Room>(),"config",DefaultConfig);
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
                    intervals.GetArrayElementAtIndex(0).FindPropertyRelative("TimeBeforeActivation").floatValue=1.5f;
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
        var text=new StringBuilder("# Basic room blocks\n\n35 standalone 17 x 9 room prefabs. Use Assets > Test Room to run any prefab. These are vocabulary studies, not a production level pool; none was added to an existing level. All use Default Room Config; the inherited 30-second StandardTime is provisional, not a human-calibrated score target.\n\n");
        text.Append("The framework's earlier one-rock experiment and all-four-directions requirement are not imposed on these full-mechanic studies. No Danger or Visibility mechanics are introduced. Rotation means changing arms, never rotating the rover. Baseline interactions are not artificially labeled Carrying or Timing. Geometry labels refer to the route used for the task; stationary workstations do not exercise the Flexible/Fixed path-choice axis.\n\n");
        text.Append("![Authored room layouts](overview.png)\n\n## Vocabulary and geometry comparisons\n\n01-04 compare alignment, long straight carry, alternate island routes, and linked bays. 07-08 hold the transfer task constant while changing spatial division; 13-14 compare open liquid delivery with a two-connection relay. 05/06/34 compare rock, rocket, and plant clearance. 16/17/18/21 compare cloud effects on different cargo. 24/25/26 isolate signaling a door, configuring a door, and configuring a lever. 28/29/30 compare timed passage, timed processing, and postponement. 31/32 contrast renewable solid and liquid supply across a rewind.\n\n");
        text.Append("| Room | Challenge emphasis | Task geometry | Reusable situation | Qualification |\n|---|---|---|---|---|\n");
        foreach(var s in Specs())text.Append("| "+s.name+" | "+s.challenge+" | "+s.geometry+" | "+s.idea+" | "+s.caveat+" |\n");
        text.Append("\n## Validation\n\n"+(passed?"PASS: all 35 completed by automated real-time Unity Play Mode input using actual Rover.OnPress/OnRelease/OnDrop and the existing reset controller. Tests verified every authored cell, prefab links and shared config, rocket requests, simultaneous arm batches, timer windows and postponement, and rocket persistence across mandatory resets. No gameplay rule was replaced by a symbolic simulation.\n":"Runtime validation pending.\n"));
        text.Append("\nThis verifies an executable intended route, not uniqueness of the solution, mandatory use of every direction, absence of every possible softlock, or four-human enjoyment. Parallelism is an opportunity rather than a strict lock. Exchange Cargo, Exchange Contents and Postpone the Switch intentionally permit alternatives. Human playtests should compare route feel, legibility, cooperation and timing targets. Keep each literal prefab exclusive to one future production pool and never request more completions than unique rooms.\n\n");
        text.Append("## Tested routes\n\nCoordinates are (column, row), zero-based from the top left. A/B mean orange/blue. `act` navigates to a valid stance and releases the indicated arm on the object; `go` moves to an empty cell. `parallel` extends all listed arms concurrently. `reset` invokes the cooperative hold countdown and asserts delivery persistence. Navigation is planned over current grid occupancy; interactions and movement execute in real frames.\n\n");
        foreach(var s in Specs())
        {
            text.Append("- **"+s.name+"**: "+string.Join(" -> ",s.commands.Select(c=>c.op=="act"||c.op=="postpone"?c.op+" "+c.dir+" ("+c.x+","+c.y+")":c.op=="parallel"?"parallel "+string.Join("/",Enum.GetValues(typeof(Direction)).Cast<Direction>().Where(d=>(c.x&(1<<(int)d))!=0)):c.op=="reset"||c.op=="drop"?c.op+(c.op=="drop"?" "+c.dir:""):c.op=="delay"?"wait "+c.seconds+"s":c.op+" ("+c.x+","+c.y+")"))+".\n");
        }
        File.WriteAllText(Folder+"/README.md",text.ToString());
    }

    static int roomIndex,commandIndex,stage;
    static double deadline,delayUntil;
    static RoomLoader loader;static LevelManager manager;static Room room;static Rover rover;
    static readonly Queue<Cmd> queue=new Queue<Cmd>();
    static Cmd running;static RoverHand hand;static bool released;static Vector2Int origin;
    static List<RoverHand> parallelHands;static HashSet<RoverHand> parallelReleased;
    
    static BuildBlockRooms()
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
        stage=1;commandIndex=0;queue.Clear();running=null;parallelHands=null;hand=null;
        deadline=EditorApplication.timeSinceStartup+150;
    }
    static void VerifyLayout()
    {
        var s=Specs()[roomIndex];Check(room.Config==DefaultConfig,"Wrong shared config");
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
                Debug.Log("BLOCKS: PASS "+Specs()[roomIndex].name);
                roomIndex++;
                if(roomIndex==Specs().Count)
                {
                    WriteCatalogue(true);Debug.Log("BLOCKS: PASS all "+Specs().Count+" rooms.");
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
            Debug.LogError("BLOCKS: FAIL "+Specs()[roomIndex].name+", command "+commandIndex+", running "+running?.op);
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
            loader.ResetController.TickHold(false,false,1,0);loader.ResetController.TickHold(true,true,1,10);
            Check(loader.CurrentRoom!=before,"Reset did not restore room");RefreshRoom();
            for(int i=0;i<rockets.Length;i++)Check(rockets[i]!=null&&rockets[i].RequiredItems.Count==counts[i]&&rockets[i].transform.IsChildOf(room.transform),"Rocket progress not preserved");
            Check(manager.ElapsedTime>=clock,"Reset refunded score time");
            Check(manager.ObjectiveTracker.AllObjectives.All(o=>o!=null),"Stale objective after reset");
            Debug.Log("BLOCKS: Reset persistence passed "+Specs()[roomIndex].name);stage=3;Finish();return;
        }
        if(running.op=="drop")
        {
            hand=rover.GetComponentsInChildren<RoverHand>().Single(h=>h.Direction==running.dir);
            Check(hand.IsHoldingObject,"Drop without cargo");rover.OnDrop(new[]{running.dir});Check(!hand.IsHoldingObject,"Blocked required drop");Finish();return;
        }
        if(running.op=="parallel")
        {
            parallelHands=rover.GetComponentsInChildren<RoverHand>().Where(h=>(running.x&(1<<(int)h.Direction))!=0).ToList();
            parallelReleased=new HashSet<RoverHand>();foreach(var h in parallelHands)rover.OnPress(h.Direction);return;
        }
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
            Debug.Log("BLOCKS: Timer postponement verified.");
        }
        Finish();
    }
}
