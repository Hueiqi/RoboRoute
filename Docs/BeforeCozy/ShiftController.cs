using UnityEngine;

namespace RoboRoute
{
    public sealed class ShiftController : MonoBehaviour
    {
        public ShiftState State { get; private set; } = new ShiftState();
        public bool Paused { get; private set; }
        RobotMotor robot;
        GameObject cargo;
        string message="Collect a package at PICKUP. Deliver to the assigned bay.";
        bool confirmReset;
        GUIStyle title, heading, body; FeedbackAudio audioFeedback; bool tutorial=true, demoActive; float frameMs;

        void Start() { Paused=true; audioFeedback=gameObject.AddComponent<FeedbackAudio>(); robot=WarehouseBuilder.Build(transform,this,out cargo); for(int i=0;i<2;i++) { var bot=new GameObject("RR-0"+(i+2)+" Autonomous"); bot.transform.position=new Vector3(-11+i*2,.1f,-5); bot.transform.SetParent(transform); bot.AddComponent<AutonomousRobot>().Initialize(this,i); } }
        bool Near(Vector3 point) { var p=robot.transform.position; p.y=0; return Vector3.Distance(p,point)<2; }
        void Update()
        {
            frameMs=Mathf.Lerp(frameMs,Time.unscaledDeltaTime*1000,.05f); if(Paused || State.Complete) return;
            int previousFailed=State.failed; State.Tick(Time.deltaTime); if(State.failed>previousFailed) {audioFeedback.Play(2); message="Deadline missed. Next job dispatched.";}
            if(Near(WarehouseBuilder.Dock)) State.battery=Mathf.Min(100,State.battery+20*Time.deltaTime);
            if(RobotMotor.InteractPressed()) Interact();
            cargo.SetActive(State.carrying);
        }
        void Interact()
        {
            if(!State.carrying && Near(WarehouseBuilder.Pickup))
            {
                State.carrying=true; audioFeedback.Play(0);
                cargo.GetComponent<Renderer>().sharedMaterial.color=WarehouseBuilder.Colors[State.Destination];
                message="Package secured. Deliver to BAY "+(char)('A'+State.Destination)+".";
                return;
            }
            for(int i=0;i<3;i++) if(Near(WarehouseBuilder.Stations[i]))
            {
                if(State.Deliver(i))
                {
                    audioFeedback.Play(1); SaveService.Save(State,out var saved);
                    message=(State.Complete ? "Shift complete. Review your results. " : "Delivery accepted. Return to PICKUP. ")+saved;
                }
                else message=State.carrying ? "Wrong station. Use BAY "+(char)('A'+State.Destination)+"." : "Collect a package first.";
                return;
            }
            message="Move onto PICKUP or your assigned bay, then press E.";
        }
        public void BeginDemo() { tutorial=false; Paused=false; demoActive=true; }
        public void DemoInteract() { Interact(); }
        void Load()
        {
            if(SaveService.Load(out var loaded,out message))
            {
                State=loaded; robot.ReturnToDock();
                cargo.SetActive(State.carrying);
                cargo.GetComponent<Renderer>().sharedMaterial.color=WarehouseBuilder.Colors[State.Destination];
                Paused=false;
            }
        }
        void OnGUI()
        {
            if(robot==null) return;
            if(title==null)
            {
                title=new GUIStyle(GUI.skin.label){fontSize=28,fontStyle=FontStyle.Bold}; title.normal.textColor=WarehouseBuilder.Teal;
                heading=new GUIStyle(GUI.skin.label){fontSize=17,fontStyle=FontStyle.Bold}; heading.normal.textColor=Color.white;
                body=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true}; body.normal.textColor=new Color(.8f,.86f,.9f);
            }
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/800f,1));
            GUI.Box(new Rect(18,18,315,700),GUIContent.none);
            GUI.enabled=!tutorial && !confirmReset && !demoActive; GUILayout.BeginArea(new Rect(34,28,281,680));
            GUILayout.Label("ROBOROUTE",title);
            GUILayout.Label("WAREHOUSE OPERATIONS / 01",body);
            GUILayout.Space(18);
            GUILayout.Label(State.Complete ? "SHIFT COMPLETE" : Paused ? "PAUSED" : "RR-01  /  ONLINE",heading);
            GUILayout.Label("Deliveries     "+State.delivered+" / "+ShiftState.Goal,body);
            GUILayout.Label("Score             "+State.score,body); GUILayout.Label("Deadline         "+Mathf.CeilToInt(ShiftState.Deadline-State.missionTime)+"s",body); GUILayout.Label("Success           "+State.SuccessRate.ToString("0")+"%   /   Missed "+State.failed,body); GUILayout.Label("Mean delivery  "+(State.delivered==0?0:State.totalDeliveryTime/State.delivered).ToString("0.0")+"s",body);
            GUILayout.Label("Shift time       "+System.TimeSpan.FromSeconds(State.elapsed).ToString(@"mm\:ss"),body);
            GUILayout.Label("Battery           "+State.battery.ToString("0")+"%",body);
            var bar=GUILayoutUtility.GetRect(260,10);
            GUI.color=new Color(.2f,.25f,.3f); GUI.DrawTexture(bar,Texture2D.whiteTexture);
            GUI.color=State.battery<20 ? Color.yellow : WarehouseBuilder.Teal;
            GUI.DrawTexture(new Rect(bar.x,bar.y,bar.width*State.battery/100,bar.height),Texture2D.whiteTexture); GUI.color=Color.white;
            GUILayout.Space(16);
            GUILayout.Label("CURRENT OBJECTIVE",heading);
            GUILayout.Label(State.Complete ? "Shift finished. Start a new shift to replay." : State.carrying ? "Deliver cargo to BAY "+(char)('A'+State.Destination)+". Press E on the coloured pad." : "Drive to the white PICKUP pad. Press E to collect cargo.",body);
            GUILayout.Space(12);
            GUILayout.Label("WASD / arrows   Move\nE   Pick up / deliver\nTeal pad   Automatic charging",body);
            GUILayout.Space(12);
            if(GUILayout.Button(Paused ? "Resume shift" : "Pause",GUILayout.Height(30))) Paused=!Paused;
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Save",GUILayout.Height(30))) SaveService.Save(State,out message);
            if(GUILayout.Button("Load",GUILayout.Height(30))) Load();
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Recover to charger (+30 sec)",GUILayout.Height(30)))
            { if(!State.Complete) {robot.ReturnToDock(); State.Tick(30); message="Robot recovered. Wait on the dock to recharge.";} }
            if(GUILayout.Button("New shift",GUILayout.Height(30))) { confirmReset=true; Paused=true; }
            GUILayout.EndArea(); GUI.enabled=true;
            GUI.Label(new Rect(355,20,850,28),"FLEET / 3 ROBOTS     •     FIFO DISPATCH     •     "+frameMs.ToString("0.0")+" ms / frame",body); GUI.Box(new Rect(350,728,910,54),GUIContent.none);
            GUI.Label(new Rect(368,738,874,42),message,body);
            if(tutorial) { GUI.Box(new Rect(400,200,600,330),GUIContent.none); GUI.Label(new Rect(435,225,530,50),"WELCOME TO ROBOROUTE",title); GUI.Label(new Rect(435,290,530,150),"1. Drive with WASD or arrow keys.\n2. Press E on PICKUP to load one package.\n3. Deliver to the assigned BAY with E within 75 seconds.\n4. Recharge on the teal pad. Six jobs finish your shift.\nAutonomous couriers share the warehouse. Save anytime.",body); if(GUI.Button(new Rect(435,460,530,45),"Start my shift")) {tutorial=false; Paused=false;} }
            if(confirmReset)
            {
                GUI.Box(new Rect(420,270,440,190),GUIContent.none);
                GUI.Label(new Rect(445,290,390,60),"Start a new shift? Unsaved progress will be lost. Your saved file remains until the next save or delivery.",body);
                if(GUI.Button(new Rect(445,380,175,40),"Start new shift"))
                { State=new ShiftState(); robot.ReturnToDock(); cargo.SetActive(false); Paused=false; confirmReset=false; message="New shift ready. Collect your first package."; }
                if(GUI.Button(new Rect(635,380,175,40),"Cancel")) { confirmReset=false; Paused=false; }
            }
        }
    }
}





