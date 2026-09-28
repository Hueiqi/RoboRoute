using System;
using System.IO;
using UnityEngine;
namespace RoboRoute
{
    public static class AchievementRules
    {
        public static readonly string[] Names={"First class parcel","Three in a row","Speedy sprout","Powered by plants","Village explorer","Perfect delivery day","Careful courier","Just in thyme"};
        public static readonly string[] Descriptions={"Make your first delivery.","Deliver 3 jobs in a row without a timeout.","Finish one job within 20 seconds of dispatch.","Recharge from below 80% to at least 95%.","Deliver to all three bays in one shift.","Complete a shift with all 6 jobs delivered.","Finish 6 deliveries with no wrong bay or recovery.","Deliver during the final 10 seconds of a job."};
        public static int Evaluate(ShiftState s)
        {
            if(s==null||!s.IsValid())return 0;
            int mask=0;
            if(s.delivered>0)mask|=1;
            if(s.bestStreak>=3)mask|=2;
            if(s.fastestDelivery>0&&s.fastestDelivery<=20)mask|=4;
            if(s.rechargeCount>0)mask|=8;
            if(s.visitedBaysMask==7)mask|=16;
            if(s.Complete&&s.delivered==6)mask|=32;
            if(s.Complete&&s.delivered==6&&s.wrongBayAttempts==0&&s.recoveries==0)mask|=64;
            if(s.lastSecondDelivery)mask|=128;
            return mask;
        }
        public static int Count(int mask){int n=0;for(int i=0;i<8;i++)if((mask&(1<<i))!=0)n++;return n;}
    }
    [Serializable]
    public sealed class CareerProfile
    {
        public int version=1, unlockedMask, bestScore, bestStars;
        public bool IsValid()=>version==1&&unlockedMask>=0&&unlockedMask<=255&&bestScore>=0&&bestScore<=600&&bestScore%100==0&&bestStars>=0&&bestStars<=3;
        // Monotonic unlocks and personal bests prevent reloading a save from farming rewards.
        public int Apply(ShiftState state)
        {
            if(state==null||!state.IsValid())return 0;
            int fresh=AchievementRules.Evaluate(state)&~unlockedMask;unlockedMask|=fresh;
            bestScore=Math.Max(bestScore,state.score);if(state.Complete)bestStars=Math.Max(bestStars,state.Stars);return fresh;
        }
    }
    public static class CareerStore
    {
        public static string PathName=>Path.Combine(Application.persistentDataPath,Array.IndexOf(Environment.GetCommandLineArgs(),"-roboroute-demo")>=0?"roboroute-smoke-career.json":"roboroute-career.json");
        public static CareerProfile Load()
        {
            try{if(File.Exists(PathName)){var p=JsonUtility.FromJson<CareerProfile>(File.ReadAllText(PathName));if(p!=null&&p.IsValid())return p;Debug.LogWarning("Career data was invalid; starting an empty sticker book.");}}
            catch(Exception ex){Debug.LogWarning("Career load: "+ex.Message);}return new CareerProfile();
        }
        public static bool Save(CareerProfile profile)
        {
            try
            {
                if(profile==null||!profile.IsValid())return false;
                string temp=PathName+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(profile,true));
                if(File.Exists(PathName))File.Replace(temp,PathName,PathName+".bak");else File.Move(temp,PathName);return true;
            }
            catch(Exception ex){Debug.LogWarning("Career save: "+ex.Message);return false;}
        }
    }
}
