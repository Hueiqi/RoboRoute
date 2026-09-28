using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace RoboRoute
{
    // Opt-in integration run uses isolated saves and the same scene, routes and mission rules.
    public sealed class DemoCapture : MonoBehaviour
    {
        int exceptions;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-roboroute-demo")>=0)
                new GameObject("Delivery integration check").AddComponent<DemoCapture>();
        }
        void OnEnable(){Application.logMessageReceived+=OnLog;}
        void OnDisable(){Application.logMessageReceived-=OnLog;}
        void OnLog(string text,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error)exceptions++;}
        IEnumerator Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=60;
            bool fast=Array.IndexOf(Environment.GetCommandLineArgs(),"-fast-smoke")>=0;
            if(fast)Time.timeScale=3;
            yield return null;
            var shift=FindFirstObjectByType<ShiftController>();var motor=FindFirstObjectByType<RobotMotor>();
            shift.BeginDemo();shift.Career.unlockedMask=0;shift.Career.bestScore=0;shift.Career.bestStars=0;motor.enabled=false;var controller=motor.GetComponent<CharacterController>();
            string folder=Path.Combine(Application.dataPath,"..","Evidence","CozyRun");Directory.CreateDirectory(folder);
            int frames=0;float next=0,start=Time.realtimeSinceStartup;
            while(!shift.State.Complete && Time.realtimeSinceStartup-start<150)
            {
                bool charging=!shift.State.carrying&&shift.State.battery<40;
                var destination=charging?WarehouseBuilder.Dock:shift.State.carrying?WarehouseBuilder.Stations[shift.State.Destination]:WarehouseBuilder.Pickup;
                var route=GridRouter.Find(motor.transform.position,destination);
                if(route.Count==0 && Vector3.Distance(motor.transform.position,destination)>2)break;
                while(route.Count>0&&!shift.State.Complete&&Time.realtimeSinceStartup-start<150)
                {
                    var delta=route.Peek()-motor.transform.position;delta.y=0;
                    if(delta.magnitude<.08f){route.Dequeue();continue;}
                    var before=motor.transform.position;
                    float speed=shift.State.carrying?3.2f:4.5f;
                    if(shift.State.battery>0)controller.Move(delta.normalized*Mathf.Min(delta.magnitude,speed*Time.deltaTime));
                    var moved=motor.transform.position-before;moved.y=0;shift.State.battery=Mathf.Max(0,shift.State.battery-moved.magnitude*.65f);
                    motor.transform.rotation=Quaternion.LookRotation(delta);
                    yield return new WaitForEndOfFrame();
                    if(Time.realtimeSinceStartup>=next)
                    {ScreenCapture.CaptureScreenshot(Path.Combine(folder,frames.ToString("D5")+".png"));frames++;next=Time.realtimeSinceStartup+.15f;}
                }
                if(charging)
                {while(shift.State.battery<95&&!shift.State.Complete&&Time.realtimeSinceStartup-start<150)yield return null;}
                else shift.DemoInteract();
                yield return null;
            }
            bool saved=SaveService.Save(shift.State,out var saveMessage);
            bool loaded=SaveService.Load(out var restored,out var loadMessage);
            bool roundtrip=loaded&&restored.score==shift.State.score&&restored.delivered==shift.State.delivered;
            bool careerSaved=CareerStore.Save(shift.Career);var careerLoaded=CareerStore.Load();bool careerRoundtrip=careerSaved&&careerLoaded.unlockedMask==shift.Career.unlockedMask&&careerLoaded.bestScore==600;
            bool passed=careerRoundtrip&&AchievementRules.Count(shift.Career.unlockedMask)>=6&&shift.State.delivered==6&&shift.State.score==600&&shift.State.IsValid()&&exceptions==0&&saved&&roundtrip;
            string report="Result: "+(passed?"PASS":"FAIL")+"\nUnity: "+Application.unityVersion+
                "\nDelivered: "+shift.State.delivered+"\nMissed: "+shift.State.failed+"\nScore: "+shift.State.score+
                "\nExceptions/errors: "+exceptions+"\nSave/load roundtrip: "+roundtrip+"\nCaptured frames: "+frames+
                "\nCareer persistence: "+careerRoundtrip+"\nStickers collected: "+AchievementRules.Count(shift.Career.unlockedMask)+"\nTime scale: "+Time.timeScale+"\nScripted driver bypasses keyboard. This is not a performance benchmark.";
            File.WriteAllText(Path.Combine(folder,"integration-check.txt"),report);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"complete.png"));
            yield return new WaitForSecondsRealtime(1);
            shift.ShowResultsForCapture();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"results.png"));
            yield return new WaitForSecondsRealtime(1);
            shift.ShowBookForCapture();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"achievements.png"));
            yield return new WaitForSecondsRealtime(1);
            shift.ShowGuideForCapture();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"guide.png"));
            yield return new WaitForSecondsRealtime(2);Application.Quit(passed?0:1);
        }
    }
}
