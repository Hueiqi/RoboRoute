using System;
using UnityEngine;
namespace RoboRoute.Editor
{
    public static class AchievementChecks
    {
        public static int Run()
        {
            int count=0;Action<bool,string> test=(ok,label)=>{if(!ok)throw new Exception("Achievement check: "+label);count++;};
            var s=new ShiftState();test(s.TryPickup(),"Pickup succeeds");test(!s.TryPickup(),"Capacity is one");
            test(!s.Deliver(1)&&s.wrongBayAttempts==1&&s.carrying&&s.score==0,"Wrong bay records attempt without reward");
            s.Tick(20);test(s.Deliver(0),"Correct bay after correction");test((AchievementRules.Evaluate(s)&5)==5,"First parcel and speed at 20 seconds");
            var slow=new ShiftState();slow.TryPickup();slow.Tick(20.01f);slow.Deliver(0);test((AchievementRules.Evaluate(slow)&4)==0,"Speed boundary excludes slower job");
            var late=new ShiftState();late.TryPickup();late.Tick(65);late.Deliver(0);test((AchievementRules.Evaluate(late)&128)!=0,"Last ten seconds inclusive");
            var early=new ShiftState();early.TryPickup();early.Tick(64.99f);early.Deliver(0);test((AchievementRules.Evaluate(early)&128)==0,"Early delivery is not last second");
            var perfect=new ShiftState();for(int i=0;i<6;i++){perfect.TryPickup();perfect.Tick(10);perfect.Deliver(i%3);}
            test(perfect.Complete&&perfect.Stars==3&&perfect.bestStreak==6,"Gold rank and streak");
            test((AchievementRules.Evaluate(perfect)&115)==115,"First streak explorer perfect careful badges");
            test(!perfect.TryPickup()&&!perfect.Recover(),"Completed shifts reject pickup and recovery");
            perfect.wrongBayAttempts=1;test((AchievementRules.Evaluate(perfect)&64)==0,"Wrong bay blocks careful badge");
            perfect.wrongBayAttempts=0;perfect.recoveries=1;test((AchievementRules.Evaluate(perfect)&64)==0,"Recovery blocks careful badge");
            var streak=new ShiftState();for(int i=0;i<3;i++){streak.TryPickup();streak.Tick(1);streak.Deliver(i);}
            streak.Tick(75);test(streak.currentStreak==0&&streak.bestStreak==3,"Timeout resets current streak only");
            test((AchievementRules.Evaluate(streak)&2)!=0,"Earned streak stays eligible");
            s=new ShiftState();s.RecordCharge();test((AchievementRules.Evaluate(s)&8)!=0,"Charging badge");
            var profile=new CareerProfile();test(profile.Apply(perfect)!=0,"New achievements returned");
            test(profile.Apply(perfect)==0,"Reload cannot duplicate unlocks");
            int savedMask=profile.unlockedMask;profile.Apply(new ShiftState());test(profile.unlockedMask==savedMask&&profile.bestScore==600,"New shift keeps career rewards");
            var copy=JsonUtility.FromJson<CareerProfile>(JsonUtility.ToJson(profile));test(copy.IsValid()&&copy.unlockedMask==profile.unlockedMask,"Career JSON roundtrip");
            copy.unlockedMask=256;test(!copy.IsValid(),"Reject unknown achievement bits");
            var old=JsonUtility.FromJson<ShiftState>("{\"version\":2,\"delivered\":1,\"failed\":0,\"score\":100,\"battery\":80,\"elapsed\":10,\"missionTime\":0,\"totalDeliveryTime\":5}");
            test(old.IsValid()&&old.bestStreak==0,"Existing version-2 saves remain readable");
            old.wrongBayAttempts=-1;test(!old.IsValid(),"Reject negative achievement counter");
            s=new ShiftState();s.Tick(74);test(s.Recover()&&s.failed==1&&s.recoveries==1&&s.missionTime==0,"Recovery applies timed penalty");
            return count;
        }
    }
}
