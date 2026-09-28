using System.Collections.Generic;
using UnityEngine;
namespace RoboRoute {
// Uniform-cost grid search. Inflated rack bounds provide clearance for the robot body.
public static class GridRouter {
 public static bool Walkable(Vector2Int p) { if(Mathf.Abs(p.x)>12||Mathf.Abs(p.y)>9)return false; for(int x=-4;x<=4;x+=8) for(int z=-3;z<=3;z+=6) if(Mathf.Abs(p.x-x)<=2&&Mathf.Abs(p.y-z)<=2)return false; return true; }
 public static Queue<Vector3> Find(Vector3 start,Vector3 end) {
 var a=new Vector2Int(Mathf.RoundToInt(start.x),Mathf.RoundToInt(start.z)); var b=new Vector2Int(Mathf.RoundToInt(end.x),Mathf.RoundToInt(end.z));
 var result=new Queue<Vector3>(); if(!Walkable(a)||!Walkable(b))return result;
 var open=new Queue<Vector2Int>(); var prev=new Dictionary<Vector2Int,Vector2Int>(); open.Enqueue(a);prev[a]=a;
 var dirs=new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
 while(open.Count>0) {var p=open.Dequeue();if(p==b)break;foreach(var d in dirs){var n=p+d;if(Walkable(n)&&!prev.ContainsKey(n)){prev[n]=p;open.Enqueue(n);}}}
 if(!prev.ContainsKey(b))return result; var path=new Stack<Vector3>();for(var p=b;p!=a;p=prev[p])path.Push(new Vector3(p.x,.1f,p.y));while(path.Count>0)result.Enqueue(path.Pop());return result;
 }
}
public sealed class AutonomousRobot:MonoBehaviour {
 public enum Mode { Pickup, Delivery, Charging }
 public Mode Status {get;private set;} public int Deliveries {get;private set;} public float Battery=100;
 RobotMotor player; ShiftController shift; Queue<Vector3> route; GameObject cargo; int bay, initialBay; float wait;
 public void Initialize(ShiftController owner,int index) {shift=owner;player=FindFirstObjectByType<RobotMotor>();bay=index;initialBay=index;var color=WarehouseBuilder.Colors[index];cargo=WarehouseBuilder.BuildRobot(transform,color,owner);cargo.SetActive(false);SetTarget(Mode.Pickup); }
 public void ResetForShift(){Battery=100;Deliveries=0;wait=0;bay=initialBay;transform.position=new Vector3(-11+(initialBay-1)*2,.1f,-5);cargo.SetActive(false);SetTarget(Mode.Pickup);}
 void SetTarget(Mode mode){Status=mode;route=GridRouter.Find(transform.position,mode==Mode.Pickup?WarehouseBuilder.Pickup:mode==Mode.Charging?WarehouseBuilder.Dock:WarehouseBuilder.Stations[bay]);}
 void Update(){if(shift==null||shift.Paused||shift.State.Complete)return;
 if(route.Count>0){var target=route.Peek(); var delta=target-transform.position; if(delta.magnitude<.04f){route.Dequeue();return;} if(player!=null&&Vector3.Distance(transform.position+delta.normalized*.8f,player.transform.position)<1.25f)return; var step=Mathf.Min(delta.magnitude,2.2f*Time.deltaTime);transform.position+=delta.normalized*step;transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*8);Battery-=step*.35f;return;}
 if(Status==Mode.Charging){Battery=Mathf.Min(100,Battery+20*Time.deltaTime);if(Battery>=100)SetTarget(Mode.Pickup);return;}
 wait+=Time.deltaTime;if(wait<1)return;wait=0;
 if(Status==Mode.Pickup){cargo.SetActive(true);SetTarget(Mode.Delivery);}else{Deliveries++;cargo.SetActive(false);bay=(bay+1)%3;SetTarget(Battery<40?Mode.Charging:Mode.Pickup);}
 }
}}


