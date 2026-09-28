using System;
namespace RoboRoute {
[Serializable] public sealed class ShiftState {
 public int version=2, delivered, failed, score; public bool carrying;
 public float battery=100, elapsed, missionTime, totalDeliveryTime;
 public const int Goal=6; public const float Deadline=75;
 public bool Complete => delivered+failed>=Goal;
 public int Destination => (delivered+failed)%3;
 public float SuccessRate => delivered+failed==0?0:100f*delivered/(delivered+failed);
 public void Tick(float dt) { if(Complete || dt<0 || float.IsNaN(dt)||float.IsInfinity(dt)) return; elapsed+=dt; missionTime+=dt; if(missionTime>=Deadline) { failed++; carrying=false; missionTime=0; } }
 public bool Deliver(int station) { if(Complete||!carrying||station!=Destination) return false; carrying=false; delivered++; score+=100; totalDeliveryTime+=missionTime; missionTime=0; return true; }
 public bool IsValid() { return version==2 && delivered>=0 && failed>=0 && delivered+failed<=Goal && score==delivered*100 && Finite(battery)&&battery>=0&&battery<=100 && Finite(elapsed)&&elapsed>=0 && Finite(missionTime)&&missionTime>=0&&missionTime<Deadline && Finite(totalDeliveryTime)&&totalDeliveryTime>=0&&totalDeliveryTime<=elapsed && !(Complete&&carrying); }
 static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
}}
