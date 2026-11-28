using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class GardenRouteRun
{
    static string[] routes; static int roomIndex, step, phase, frame=-1;
    static double deadline;
    static string[] commands;
    static LevelManager manager; static RoomLoader loader; static Rover rover;
    static RoverHand[] hands; static Vector2Int[] targets; static string[] ops; static bool[] released;
    static GardenRouteRun(){EditorApplication.update+=Tick;}
    public static void Run(){
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        SessionState.SetBool("dated.play",true);
        EditorApplication.isPlaying=true;
    }
    static void Tick(){
        if(!SessionState.GetBool("dated.play",false)||!Application.isPlaying||frame==Time.frameCount)return;
        frame=Time.frameCount;
        try{
            if(routes==null){
                manager=UnityEngine.Object.FindObjectOfType<LevelManager>();loader=UnityEngine.Object.FindObjectOfType<RoomLoader>();
                if(manager==null||loader==null||loader.CurrentRoom==null)return;
                UnityEngine.Object.FindObjectOfType<LobbyManager>().IsRoomTesting=true;
                manager.BeginRoomTestSession(); routes=File.ReadAllLines("routes.txt");Time.captureDeltaTime=1f/40f;
            }
            if(phase==0){
                var parts=routes[roomIndex].Split('|');commands=parts[1].Split(';');step=0;
                var room=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Levels/dated/"+parts[0]+".prefab").GetComponent<Room>();
                if(RoomDuplicateUtility.FindOverlaps(room).Count>0)throw new Exception("overlap");
                manager.StartLevel(LevelData.CreateRoomTest(room));loader.CurrentRoom.SetReflections(false,false);
                phase=1;deadline=EditorApplication.timeSinceStartup+15;return;
            }
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("timed out");
            if(phase==1){
                rover=loader.CurrentRoom.GetComponentInChildren<Rover>();if(!rover.CanReceiveInput)return;
                phase=2;
            }
            if(phase==2){
                if(step==commands.Length){
                    if(!manager.IsRoomTestFinished)return;
                    if(manager.CompletedRoomCount!=1)throw new Exception("level not completed");
                    Debug.Log("ROUTE PASS "+routes[roomIndex].Split('|')[0]+" "+manager.ElapsedTime.ToString("0.00")+"s; "+commands.Length+" gestures");
                    if(++roomIndex==routes.Length){SessionState.EraseBool("dated.play");EditorApplication.Exit(0);return;}
                    phase=0;return;
                }
                var acts=commands[step].Split('+');
                hands=new RoverHand[acts.Length];targets=new Vector2Int[acts.Length];ops=new string[acts.Length];released=new bool[acts.Length];
                for(int i=0;i<acts.Length;i++){
                    var a=acts[i].Split(',');ops[i]=a[0];targets[i]=new Vector2Int(int.Parse(a[1]),int.Parse(a[2]));
                    var direction=(Direction)int.Parse(a[3]);
                    hands[i]=rover.GetComponentsInChildren<RoverHand>().Single(h=>h.Direction==direction);
                    if(a[0]=="drop"){rover.OnDrop(new[]{direction});released[i]=true;}
                    else if(a[0]!="wait")rover.OnPress(direction);
                }
                phase=3;deadline=EditorApplication.timeSinceStartup+10;return;
            }
            for(int i=0;i<hands.Length;i++){
                if(ops[i]=="wait"){released[i]=rover.LevelGrid.Objects.GetObject(targets[i]).GetComponent<Door>().IsOpen;continue;}
                if(!released[i] && rover.LevelGrid.WorldToCell(hands[i].HandPosition)==targets[i]){
                    rover.OnRelease(hands[i].Direction);released[i]=true;
                }
            }
            if(rover.IsMoving||released.Any(r=>!r)||hands.Any(h=>h.HandState!=HandState.None))return;
            for(int i=0;i<hands.Length;i++){
                if(ops[i]=="move" && rover.LevelGrid.WorldToCell(rover.transform.position)!=targets[i])throw new Exception("wrong position");
                if(ops[i]=="grab"&&!hands[i].IsHoldingObject || (ops[i]=="plant"||ops[i]=="drop")&&hands[i].IsHoldingObject)throw new Exception("wrong cargo");
            }
            phase=2;step++;deadline=EditorApplication.timeSinceStartup+10;
        }catch(Exception e){Debug.LogError("ROUTE FAIL index="+roomIndex+" step="+step+" command="+(commands!=null&&step<commands.Length?commands[step]:"end")+" "+e);SessionState.EraseBool("dated.play");EditorApplication.Exit(1);}
    }
}
