using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace RoboRoute {
// Opt-in repeatable gameplay smoke run. Never runs during ordinary play.
public sealed class DemoCapture:MonoBehaviour {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-roboroute-demo")>=0)new GameObject("Demo evidence runner").AddComponent<DemoCapture>();}
 IEnumerator Start(){Application.runInBackground=true; yield return null;var shift=FindFirstObjectByType<ShiftController>();var motor=FindFirstObjectByType<RobotMotor>();shift.BeginDemo();motor.enabled=false;var cc=motor.GetComponent<CharacterController>();int frames=0;float next=0, sum=0;int samples=0;string folder=Path.Combine(Application.dataPath,"..","Evidence");Directory.CreateDirectory(folder);Directory.CreateDirectory(Path.Combine(folder,"Frames"));
 float started=Time.realtimeSinceStartup; while(!shift.State.Complete && Time.realtimeSinceStartup-started<180){bool charging=!shift.State.carrying && shift.State.battery<40; var destination=charging?WarehouseBuilder.Dock:shift.State.carrying?WarehouseBuilder.Stations[shift.State.Destination]:WarehouseBuilder.Pickup;var route=GridRouter.Find(motor.transform.position,destination);while(route.Count>0&&!shift.State.Complete && Time.realtimeSinceStartup-started<180){var point=route.Peek();var delta=point-motor.transform.position;delta.y=0;if(delta.magnitude<.08f){route.Dequeue();continue;}var distance=Mathf.Min(delta.magnitude,4*Time.deltaTime);cc.Move(delta.normalized*distance);motor.transform.rotation=Quaternion.LookRotation(delta);shift.State.battery=Mathf.Max(0,shift.State.battery-distance*.65f);sum+=Time.unscaledDeltaTime;samples++;yield return new WaitForEndOfFrame();if(Time.unscaledTime>=next){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Frames",frames.ToString("D5")+".png"));frames++;next=Time.unscaledTime+.1f;} }if(charging){while(shift.State.battery<95 && !shift.State.Complete){yield return null;}}else shift.DemoInteract();yield return null;}
 File.WriteAllText(Path.Combine(folder,"runtime-smoke.txt"),"Unity "+Application.unityVersion+"\nDelivered: "+shift.State.delivered+"\nScore: "+shift.State.score+"\nMean sampled frame ms: "+(1000*sum/Mathf.Max(1,samples))+"\nFrames: "+frames+"\nThis scripted driver bypasses keyboard input; manual control requires separate QA.");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"completed.png"));yield return new WaitForSeconds(1);Application.Quit(shift.State.delivered==6?0:1);
 }
}}



