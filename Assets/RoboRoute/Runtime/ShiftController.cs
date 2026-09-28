using UnityEngine;
using System.Collections.Generic;

namespace RoboRoute
{
    public sealed class ShiftController : MonoBehaviour
    {
        public ShiftState State { get; private set; } = new ShiftState();
        public bool Paused { get; private set; }
        RobotMotor robot;
        GameObject cargo;
        FeedbackAudio audioFeedback;
        AutonomousRobot[] couriers;
        string message="A little parcel. A lovely journey. Collect your first delivery.";
        bool confirmReset, tutorial=true, demoActive, wasPaused, showResults=true;
        float frameMs;
        public CareerProfile Career { get; private set; }
        readonly Queue<string> unlockQueue=new Queue<string>();
        string unlockMessage="";
        float unlockTime;
        int tutorialPage;
        bool bookOpen, chargeEligible;
        bool ModalActive => tutorial || confirmReset || bookOpen || (State.Complete && showResults);
        static readonly string[] Parcels={"Jam jars","A tiny telescope","Flower seeds","A robot birthday cake","Library books","A very small hat"};
        string ParcelName => Parcels[Mathf.Min(5,State.delivered+State.failed)];

        void Start()
        {
            Paused=true;Career=CareerStore.Load();
            audioFeedback=gameObject.AddComponent<FeedbackAudio>();
            robot=WarehouseBuilder.Build(transform,this,out cargo);
            couriers=new AutonomousRobot[2];
            for(int i=0;i<2;i++)
            {
                var bot=new GameObject("RR-0"+(i+2)+" Autonomous");
                bot.transform.position=new Vector3(-11+i*2,.1f,-5);bot.transform.SetParent(transform);
                couriers[i]=bot.AddComponent<AutonomousRobot>();couriers[i].Initialize(this,i+1);
            }
            new GameObject("Your glowing route target").AddComponent<ObjectiveBeacon>().Initialize(this);
        }
        bool Near(Vector3 point)
        {var position=robot.transform.position;position.y=0;return Vector3.Distance(position,point)<2;}
        void Update()
        {
            frameMs=Mathf.Lerp(frameMs,Time.unscaledDeltaTime*1000,.05f);
            unlockTime=Mathf.Max(0,unlockTime-Time.unscaledDeltaTime);
            if(unlockTime<=0 && unlockQueue.Count>0){unlockMessage=unlockQueue.Dequeue();unlockTime=5;}
            if(!ModalActive&&!demoActive&&!State.Complete&&RobotMotor.PausePressed())Paused=!Paused;
            if(Paused || State.Complete)return;
            int previousFailed=State.failed;State.Tick(Time.deltaTime);
            if(State.failed>previousFailed){audioFeedback.Play(2);message="That parcel missed its window. A fresh delivery is ready.";}
            if(Near(WarehouseBuilder.Dock))
            {
                if(State.battery<80)chargeEligible=true;
                State.battery=Mathf.Min(100,State.battery+20*Time.deltaTime);
                if(chargeEligible&&State.battery>=95){chargeEligible=false;State.RecordCharge();ProcessAchievements();message="Battery full of sunshine. You're ready to roll!";}
            }
            else chargeEligible=false;
            if(State.Complete)ProcessAchievements();
            if(!demoActive && RobotMotor.InteractPressed())Interact();
            cargo.SetActive(State.carrying);
        }
        void Interact()
        {
            if(State.Complete || Paused)return;
            if(!State.carrying && Near(WarehouseBuilder.Pickup))
            {
                if(!State.TryPickup())return;audioFeedback.Play(0);
                cargo.GetComponent<Renderer>().sharedMaterial.color=WarehouseBuilder.Colors[State.Destination];cargo.SetActive(true);
                robot.GetComponentInChildren<CozyMotion>().Celebrate();
                message=ParcelName+" tucked in! Head to Bay "+(char)('A'+State.Destination)+".";return;
            }
            for(int i=0;i<3;i++)if(Near(WarehouseBuilder.Stations[i]))
            {
                if(State.Deliver(i))
                {
                    cargo.SetActive(false);audioFeedback.Play(1);
                    CozyMotion.Burst(robot.transform.position,transform,this,WarehouseBuilder.Colors[i]);
                    foreach(var motion in robot.GetComponentsInChildren<CozyMotion>())motion.Celebrate();
                    ProcessAchievements();SaveService.Save(State,out var saved);
                    message=(State.Complete?"Lovely work. Your village shift is finished! ":"Special delivery! +100 points. Streak: "+State.currentStreak+". ")+saved;
                }
                else{audioFeedback.Play(2);message=State.carrying?"This is a different bay. Your parcel belongs at Bay "+(char)('A'+State.Destination)+".":"Your hands are empty. Visit the parcel pickup.";}
                return;
            }
            message="Roll onto the pickup pad or your delivery bay, then press E.";
        }
        public void BeginDemo(){tutorial=false;Paused=false;demoActive=true;showResults=false;}
        public void ShowBookForCapture(){bookOpen=true;Paused=true;}
        public void ShowGuideForCapture(){bookOpen=false;tutorial=true;tutorialPage=0;Paused=true;}
        public void ShowResultsForCapture(){tutorial=false;bookOpen=false;showResults=true;Paused=false;}
        void ProcessAchievements()
        {
            int previousScore=Career.bestScore,previousStars=Career.bestStars;
            int earned=Career.Apply(State);
            for(int i=0;i<8;i++)if((earned&(1<<i))!=0)unlockQueue.Enqueue(AchievementRules.Names[i]);
            if(earned!=0||previousScore!=Career.bestScore||previousStars!=Career.bestStars)
                if(!CareerStore.Save(Career))message="Your stickers are here for this session, but could not be saved to disk.";
        }
        public void DemoInteract(){Interact();}
        void Load()
        {
            if(!SaveService.Load(out var loaded,out message))return;
            State=loaded;robot.ReturnToDock();cargo.SetActive(State.carrying);
            cargo.GetComponent<Renderer>().sharedMaterial.color=WarehouseBuilder.Colors[State.Destination];Paused=false;showResults=true;chargeEligible=false;ProcessAchievements();
        }
        void ResetShift()
        {
            State=new ShiftState();robot.ReturnToDock();cargo.SetActive(false);Paused=false;confirmReset=false;showResults=true;
            chargeEligible=false;foreach(var courier in couriers)courier.ResetForShift();
            message="A fresh day in the village. Your sticker collection stays with you.";
        }
        void OnGUI()
        {
            if(robot==null)return;
            CozyHud.Begin();var ink=CozyHud.Ink;var paper=CozyHud.Paper;var muted=CozyHud.Muted;var teal=CozyHud.Teal;
            CozyHud.Text(new Rect(32,24,350,53),"RoboRoute",39,ink,true);
            CozyHud.Text(new Rect(34,79,350,24),"THE LITTLE DELIVERY VILLAGE",13,muted,true);
            GUI.enabled=!ModalActive&&!demoActive;
            if(CozyHud.Button(new Rect(382,37,282,56),"Sticker book  "+AchievementRules.Count(Career.unlockedMask)+" / 8")){wasPaused=Paused;Paused=true;bookOpen=true;}
            GUI.enabled=true;
            CozyHud.Metric(698,"DELIVERY",Mathf.Min(6,State.delivered+State.failed+1).ToString("00")+" / 06",teal);
            CozyHud.Metric(918,"DESTINATION",State.Complete?"ALL DONE":"BAY "+(char)('A'+State.Destination),WarehouseBuilder.Colors[State.Destination]);
            CozyHud.Metric(1138,"TIME LEFT",State.Complete?"--:--":System.TimeSpan.FromSeconds(Mathf.CeilToInt(ShiftState.Deadline-State.missionTime)).ToString(@"mm\:ss"),WarehouseBuilder.Colors[1]);
            CozyHud.Metric(1358,"YOUR SCORE",State.score.ToString("000"),WarehouseBuilder.Colors[2]);

            CozyHud.Card(new Rect(30,145,268,257),paper);
            CozyHud.Text(new Rect(52,165,220,22),demoActive?"DEMO IN PROGRESS":Paused?"TAKING A BREATHER":"YOUR NEXT LITTLE MISSION",12,muted,true);
            CozyHud.Text(new Rect(52,204,222,64),State.Complete?"Beautifully\ndelivered.":State.carrying?"Off to Bay "+(char)('A'+State.Destination)+"!":"A parcel\nis waiting.",28,ink,true);
            CozyHud.Text(new Rect(52,286,222,68),State.Complete?"Your shift is complete. See your delivery results below.":State.carrying?"Follow the glowing dots to Bay "+(char)('A'+State.Destination)+". Press E to deliver.":"Next: "+ParcelName+". Follow the glowing dots to PICKUP, then press E.",17,muted);
            CozyHud.Panel(new Rect(52,365,222,4),WarehouseBuilder.Hex("E4E9D9"),2);
            CozyHud.Panel(new Rect(52,365,Mathf.Max(4,222*(State.delivered+State.failed)/6f),4),teal,2);

            CozyHud.Card(new Rect(30,422,268,156),paper);
            CozyHud.Text(new Rect(52,442,220,25),"ROBOT BATTERY",12,muted,true);
            CozyHud.Text(new Rect(52,466,218,36),State.battery.ToString("0")+"%",30,State.battery<20?WarehouseBuilder.Colors[1]:ink,true);
            for(int i=0;i<10;i++)CozyHud.Panel(new Rect(52+i*22.4f,512,18,14),State.battery>i*10?teal:WarehouseBuilder.Hex("E1E8D9"),4);
            CozyHud.Text(new Rect(52,541,222,24),Near(WarehouseBuilder.Dock)?"Recharging at the garden dock":"Recharge on the mint landing",13,muted);

            CozyHud.Card(new Rect(30,598,268,211),paper);
            CozyHud.Text(new Rect(52,618,220,22),"VILLAGE COURIERS",12,muted,true);
            for(int i=0;i<couriers.Length;i++)
            {
                CozyHud.Panel(new Rect(52,654+i*51,10,10),WarehouseBuilder.Colors[i+1],5);
                CozyHud.Text(new Rect(74,645+i*51,198,23),i==0?"PIP / coral courier":"SUNNY / yellow courier",15,ink,true);
                CozyHud.Text(new Rect(74,668+i*51,198,22),couriers[i].Status+"  ·  "+couriers[i].Deliveries+" delivered",13,muted);
            }
            CozyHud.Text(new Rect(52,775,220,23),State.SuccessRate.ToString("0")+"% success  ·  "+(State.delivered>0?State.totalDeliveryTime/State.delivered:0).ToString("0.0")+"s avg",13,muted);

            CozyHud.Card(new Rect(326,834,1238,48),paper);
            CozyHud.Text(new Rect(345,846,1202,28),message,16,ink);
            CozyHud.Card(new Rect(30,902,1534,70),paper);
            CozyHud.Text(new Rect(52,920,300,35),"WASD move   E action   ESC pause",16,ink,true);
            GUI.enabled=!ModalActive&&!demoActive;
            if(CozyHud.Button(new Rect(400,916,138,42),Paused?"Resume":"Pause",true))Paused=!Paused;
            if(CozyHud.Button(new Rect(550,916,110,42),"Save"))SaveService.Save(State,out message);
            if(CozyHud.Button(new Rect(672,916,110,42),"Load"))Load();
            if(CozyHud.Button(new Rect(794,916,225,42),"Recover  (+30 sec)"))
            {if(!State.Complete){robot.ReturnToDock();State.Recover();ProcessAchievements();cargo.SetActive(State.carrying);message="Back at the garden dock. Take a moment to recharge.";}}
            if(CozyHud.Button(new Rect(1031,916,142,42),"New shift")){wasPaused=Paused;Paused=true;confirmReset=true;}
            if(CozyHud.Button(new Rect(1185,916,100,42),"Guide")){wasPaused=Paused;Paused=true;tutorialPage=0;tutorial=true;}
            CozyHud.Text(new Rect(1300,925,244,24),frameMs.ToString("0.0")+" ms  ·  "+System.TimeSpan.FromSeconds(State.elapsed).ToString(@"mm\:ss")+" shift",14,muted,false,TextAnchor.MiddleRight);
            GUI.enabled=true;
            if(!ModalActive)
            {
                string prompt=State.Complete?"Shift complete. Open your sticker book or start a new shift.":State.battery<25?"LOW BATTERY: follow the glowing dots to RECHARGE.":Near(WarehouseBuilder.Pickup)&&!State.carrying?"Pick up "+ParcelName+"  [E]":State.carrying&&Near(WarehouseBuilder.Stations[State.Destination])?"Deliver your parcel  [E]":State.carrying?"NEXT: Bay "+(char)('A'+State.Destination)+". Match the letter and press E.":"NEXT: PICKUP. Follow the glowing dots.";
                GUI.enabled=!Paused&&!demoActive&&!State.Complete;
                if(CozyHud.Button(new Rect(520,772,895,45),prompt,true))Interact();GUI.enabled=true;
                if(unlockTime>0)
                {
                    CozyHud.Card(new Rect(1000,130,560,88),paper);
                    CozyHud.Text(new Rect(1022,142,512,22),"NEW STICKER UNLOCKED",12,teal,true);
                    CozyHud.Text(new Rect(1022,171,512,32),unlockMessage,23,ink,true);
                }
            }
            if(ModalActive)DrawModal();
        }
        void DrawModal()
        {
            var ink=CozyHud.Ink;var paper=CozyHud.Paper;var muted=CozyHud.Muted;var teal=CozyHud.Teal;
            GUI.color=new Color(.13f,.25f,.26f,.5f);GUI.DrawTexture(new Rect(0,0,1600,1000),Texture2D.whiteTexture);GUI.color=Color.white;
            if(bookOpen)
            {
                CozyHud.Card(new Rect(350,130,930,725),paper);
                CozyHud.Text(new Rect(387,160,855,45),"Your delivery sticker book",31,ink,true);
                CozyHud.Text(new Rect(387,213,855,26),AchievementRules.Count(Career.unlockedMask)+" / 8 collected. Stickers stay unlocked across shifts. Best score: "+Career.bestScore,16,muted);
                for(int i=0;i<8;i++)
                {
                    float x=387+(i%2)*432,y=262+(i/2)*125;bool earned=(Career.unlockedMask&(1<<i))!=0;
                    CozyHud.Panel(new Rect(x,y,407,110),earned?WarehouseBuilder.Hex("DCEFE5"):WarehouseBuilder.Hex("EEEFE4"));
                    CozyHud.Text(new Rect(x+16,y+13,376,29),(earned?"COLLECTED  ":"LOCKED  ")+AchievementRules.Names[i],17,ink,true);
                    CozyHud.Text(new Rect(x+16,y+51,376,48),AchievementRules.Descriptions[i],15,muted);
                }
                if(CozyHud.Button(new Rect(387,786,839,44),"Back to my delivery",true)){bookOpen=false;Paused=wasPaused;}
                return;
            }
            CozyHud.Card(new Rect(450,220,700,560),paper);
            if(tutorial)
            {
                string[] titles={"Meet Sprout, your courier","Pick up. Match. Deliver.","Stay charged. Find your stickers."};
                string[] steps={"Move with WASD or the arrow keys.\nThe glowing dots show where to go next.\nPress ESC or Pause whenever you need a break.","1. Drive onto the PICKUP pad and press E.\n2. Check the BAY letter at the top of the screen.\n3. Drive to that bay and press E again.\nYou have 75 seconds from dispatch for each job.","Low battery? Visit RECHARGE and wait.\nOne parcel at a time; six jobs finish the shift.\nCollect stickers for speed, care and a perfect day.\nOpen the sticker book to see every challenge."};
                CozyHud.Text(new Rect(493,255,614,25),"QUICK GUIDE  "+(tutorialPage+1)+" / 3",13,teal,true);
                CozyHud.Text(new Rect(493,305,614,75),titles[tutorialPage],31,ink,true);
                CozyHud.Text(new Rect(493,407,614,210),steps[tutorialPage],20,muted);
                if(CozyHud.Button(new Rect(493,661,298,52),tutorialPage==2?"Let's deliver!":"Next step",true))
                {if(tutorialPage<2)tutorialPage++;else{tutorial=false;Paused=false;}}
                if(CozyHud.Button(new Rect(809,661,298,52),"I know the controls")){tutorial=false;Paused=wasPaused;}
            }
            else if(confirmReset)
            {
                CozyHud.Text(new Rect(493,262,614,48),"A fresh delivery day?",32,ink,true);
                CozyHud.Text(new Rect(493,366,614,165),"Your stickers and personal bests stay unlocked. Current unsaved shift progress will be lost. Your previous shift save stays until the next save or delivery.",21,muted);
                if(CozyHud.Button(new Rect(493,661,298,52),"Start new shift",true))ResetShift();
                if(CozyHud.Button(new Rect(809,661,298,52),"Keep this shift")){confirmReset=false;Paused=wasPaused;}
            }
            else
            {
                CozyHud.Text(new Rect(493,252,614,25),"SHIFT COMPLETE  /  "+State.Stars+" OF 3 STARS",14,teal,true);
                CozyHud.Text(new Rect(493,299,614,53),State.Rank,31,ink,true);
                CozyHud.Text(new Rect(493,380,614,190),State.delivered+" delivered   /   "+State.failed+" missed   /   "+State.score+" points\nBest streak: "+State.bestStreak+"   /   Stickers: "+AchievementRules.Count(Career.unlockedMask)+" of 8\nPersonal best: "+Career.bestScore+" points\n\n"+(State.Stars==3?"You made the village smile. Try collecting every sticker!":"Try again: recharge early and match the bay letter."),20,muted);
                if(CozyHud.Button(new Rect(493,661,298,52),"Another lovely shift",true))ResetShift();
                if(CozyHud.Button(new Rect(809,661,298,52),"See my stickers")){showResults=false;bookOpen=true;wasPaused=false;Paused=true;}
            }
        }
    }
}
